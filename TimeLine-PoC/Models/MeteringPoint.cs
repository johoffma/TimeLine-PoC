using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Events;

namespace TimeLine_PoC.Models
{
    public class MeteringPoint
    {
        public MeteringPoint()
        {
            MPPs = new List<MeteringPointPeriod>();
            CRs = new List<CommercialRelation>();
        }
        public List<MeteringPointPeriod> MPPs { get; }
        public List<CommercialRelation> CRs { get; }

        // Centralized sorted view:
        // Order by ValidFrom ascending, then by CreatedAt so that
        // when ValidFrom values are equal the period with the lowest CreatedAt comes first.
        private List<MeteringPointPeriod> GetSortedPeriods()
            => MPPs.OrderBy(p => p.ValidFrom).ThenBy(p => p.CreatedAt).ToList();

        public MeteringPointPeriod? GetPrevious(MeteringPointPeriod current)
        {
            var sortedPeriods = GetSortedPeriods();
            var currentIndex = sortedPeriods.IndexOf(current);
            if (currentIndex <= 0)
            {
                return null;
            }
            return sortedPeriods[currentIndex - 1];
        }

        public MeteringPointPeriod? GetNext(MeteringPointPeriod current)
        {
            var sortedPeriods = GetSortedPeriods();
            var currentIndex = sortedPeriods.IndexOf(current);
            if (currentIndex == -1 || currentIndex >= sortedPeriods.Count - 1)
                return null;
            return sortedPeriods[currentIndex + 1];
        }

        public DateTime GetNextValidFrom(MeteringPointPeriod current)
        {
            var sortedPeriods = GetSortedPeriods();
            var currentIndex = sortedPeriods.IndexOf(current);
            if (currentIndex == -1)
            {
                throw new InvalidOperationException("Period not found.");
            }
            if (currentIndex == sortedPeriods.Count - 1)
                return DateTime.MaxValue;
            return sortedPeriods[currentIndex + 1].ValidFrom;
        }

        // --- CommercialRelation helpers (new) ---
        private List<CommercialRelation> GetSortedCommercialRelations()
            => CRs.OrderBy(cr => cr.ValidFrom).ThenByDescending(cr => cr.CreatedAt).ToList();

        public CommercialRelation? GetPrevious(CommercialRelation current)
        {
            var sorted = GetSortedCommercialRelations();
            var idx = sorted.IndexOf(current);
            if (idx <= 0) return null;
            return sorted[idx - 1];
        }

        public CommercialRelation? GetNext(CommercialRelation current)
        {
            var sorted = GetSortedCommercialRelations();
            var idx = sorted.IndexOf(current);
            if (idx == -1 || idx >= sorted.Count - 1) return null;
            return sorted[idx + 1];
        }

        public DateTime GetNextValidFrom(CommercialRelation current)
        {
            var sorted = GetSortedCommercialRelations();
            var idx = sorted.IndexOf(current);
            if (idx == -1) throw new InvalidOperationException("CommercialRelation not found.");
            if (idx == sorted.Count - 1) return DateTime.MaxValue;
            return sorted[idx + 1].ValidFrom;
        }

        public void Apply(CreateMeteringPointEvent input)
        {
            var mpp = new MeteringPointPeriod(
                this,
                input.CreatedAt,
                input.ValidityDate,
                input.ConnectionState,
                input.AddressLine,
                input.Resolution);
            MPPs.Add(mpp);
        }

        public void Apply(ConnectMeteringPointEvent input)
        {
            var mpp = new MeteringPointPeriod(
                this,
                input.CreatedAt,
                input.ValidityDate,
                 connectionState: "Connected");
            MPPs.Add(mpp);
        }

        public void Apply(UpdateMeteringPointEvent input)
        {
            var mpp = new MeteringPointPeriod(
                this,
                input.CreatedAt,
                input.ValidityDate,
                null,
                input.AddressLine,
                input.Resolution);
            MPPs.Add(mpp);
        }

        public void Apply(DisconnectMeteringPointEvent input)
        {
            var mpp = new MeteringPointPeriod(
                this,
                input.CreatedAt,
                input.ValidityDate,
                connectionState: "Disconnected");
            MPPs.Add(mpp);
        }

        public void Apply(ReConnectMeteringPointEvent input)
        {
            var mpp = new MeteringPointPeriod(
                this,
                input.CreatedAt,
                input.ValidityDate, 
                connectionState: "Connected");
            MPPs.Add(mpp);
        }

        public void Apply(MoveInEvent input)
        {
            // Remove CommercialRelations with ValidFrom > input.ValidityDate while their Reason is
            // SecondaryMoveIn or ChangeOfSupplier. Stop when no more candidates or when a PrimaryMoveIn is encountered.
            while (true)
            {
                var candidate = GetSortedCommercialRelations().FirstOrDefault(cr => cr.ValidFrom > input.ValidityDate);
                if (candidate == null)
                    break;

                if (candidate.Reason == Reason.PrimaryMoveIn)
                    break;

                if (candidate.Reason == Reason.SecondaryMoveIn || candidate.Reason == Reason.ChangeOfSupplier)
                {
                    CRs.Remove(candidate);
                    continue;
                }

                // If an unexpected Reason enum is present, stop to avoid accidental removal.
                break;
            }

            // Create new CommercialRelation for this MoveIn and add an EnergySupplierPeriod under it.
            var cr = new CommercialRelation(
                this,
                input.CreatedAt,
                input.ValidityDate,
                input.EnergySupplierId,
                input.Reason);

            CRs.Add(cr);

            var esp = new EnergySupplierPeriod(cr, input.CreatedAt, input.ValidityDate, customer: input.Customer);
            cr.EnergySupplierPeriods.Add(esp);
        }

        public void Apply(UpdateCustomerEvent input)
        { 
            // Find the CommercialRelation that is active at input.ValidityDate and update it with a new EnergySupplierPeriod.
            var cr = GetSortedCommercialRelations()
                .LastOrDefault(cr => cr.ValidFrom <= input.ValidityDate);
            if (cr == null)
            {
                throw new InvalidOperationException("No active CommercialRelation found for the given ValidityDate.");
            }
            var esp = new EnergySupplierPeriod(cr, input.CreatedAt, input.ValidityDate, customer: input.Customer, customerAddress: input.CustomerAddress);
            cr.EnergySupplierPeriods.Add(esp);
        }

        // Prints periods in chronological order (by ValidFrom) to Console.
        // Overload accepting a TextWriter is provided for testability / redirection.
        public void PrintPeriods() => PrintPeriods(Console.Out);

        public void PrintPeriods(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            var sorted = GetSortedPeriods();
            if (!sorted.Any())
            {
                writer.WriteLine("No periods available.");
                return;
            }

            // Build rows (first row is header) — includes CreatedAt
            var rows = new List<string[]>();
            rows.Add(new[] { "#", "ValidFrom", "ValidTo", "CreatedAt", "ConnectionState", "AddressLine", "Resolution" });

            for (var i = 0; i < sorted.Count; i++)
            {
                var p = sorted[i];
                var validTo = (i == sorted.Count - 1) ? DateTime.MaxValue : sorted[i + 1].ValidFrom;
                var validToText = validTo == DateTime.MaxValue ? "MaxValue" : validTo.ToString("O");

                rows.Add(new[]
                {
                    (i + 1).ToString(),
                    p.ValidFrom.ToString("O"),
                    validToText,
                    p.CreatedAt.ToString("O"),
                    p.ConnectionState,
                    p.AddressLine ?? "<null>",
                    p.Resolution ?? "<null>"
                });
            }

            // Calculate column widths
            var columns = rows[0].Length;
            var widths = new int[columns];
            for (var c = 0; c < columns; c++)
            {
                widths[c] = rows.Max(r => r[c]?.Length ?? 0);
            }

            // Helper to format a row with padding between columns
            string FormatRow(string[] row) =>
                string.Join("  ", Enumerable.Range(0, columns).Select(c => (row[c] ?? string.Empty).PadRight(widths[c])));

            // Write header, separator and data rows
            writer.WriteLine(FormatRow(rows[0]));
            writer.WriteLine(string.Join("  ", Enumerable.Range(0, columns).Select(c => new string('-', widths[c]))));
            for (var r = 1; r < rows.Count; r++)
            {
                writer.WriteLine(FormatRow(rows[r]));
            }

            Console.WriteLine();
            Console.WriteLine("Commercial relations (and underlying EnergySupplierPeriods):");
            PrintCommercialRelationsAndEnergySupplierPeriods();

        }

        private void PrintCommercialRelationsAndEnergySupplierPeriods()
        {
            // Sort CommercialRelations by ValidFrom asc, CreatedAt desc (newest for ties)
            var sortedCr = CRs
                .OrderBy(cr => cr.ValidFrom)
                .ThenByDescending(cr => cr.CreatedAt)
                .ToList();

            if (!sortedCr.Any())
            {
                Console.WriteLine("  No commercial relations.");
                return;
            }

            // Build CR table (use CR.ValidTo property)
            var crRows = new List<string[]>();
            crRows.Add(new[] { "#", "ValidFrom", "ValidTo", "CreatedAt", "EnergySupplierId", "Reason" });

            for (var i = 0; i < sortedCr.Count; i++)
            {
                var cr = sortedCr[i];
                var crValidTo = cr.ValidTo;
                var crValidToText = crValidTo == DateTime.MaxValue ? "MaxValue" : crValidTo.ToString("O");

                crRows.Add(new[]
                {
                    (i + 1).ToString(),
                    cr.ValidFrom.ToString("O"),
                    crValidToText,
                    cr.CreatedAt.ToString("O"),
                    cr.EnergySupplierId ?? "<null>",
                    cr.Reason.ToString()
                });
            }

            FormatTable(Console.Out, crRows);
            Console.WriteLine();

            // Build EnergySupplierPeriod table (flat view) with reference to parent CR index
            var espRows = new List<string[]>();
            // Added Customer and CustomerAddress columns
            espRows.Add(new[] { "#", "CR#", "ValidFrom", "ValidTo", "CreatedAt", "Customer", "CustomerAddress" });

            var espIndex = 0;
            for (var crIndex = 0; crIndex < sortedCr.Count; crIndex++)
            {
                var cr = sortedCr[crIndex];
                var sortedEsps = cr.EnergySupplierPeriods
                    .OrderBy(e => e.ValidFrom)
                    .ThenBy(e => e.CreatedAt)
                    .ToList();

                for (var j = 0; j < sortedEsps.Count; j++)
                {
                    var esp = sortedEsps[j];
                    var espValidToNullable = esp.ValidTo; // property already clips to CR.ValidTo
                    var espValidToText = espValidToNullable == null
                        ? "<null>"
                        : (espValidToNullable.Value == DateTime.MaxValue ? "MaxValue" : espValidToNullable.Value.ToString("O"));

                    espIndex++;

                    var customerText = (esp as dynamic).Customer as string ?? "<null>";
                    var customerAddressText = (esp as dynamic).CustomerAddress as string ?? "<null>";

                    espRows.Add(new[]
                    {
                        espIndex.ToString(),
                        (crIndex + 1).ToString(),
                        esp.ValidFrom.ToString("O"),
                        espValidToText,
                        esp.CreatedAt.ToString("O"),
                        customerText,
                        customerAddressText
                    });
                }
            }

            if (espRows.Count == 1)
            {
                Console.WriteLine("  No EnergySupplierPeriods.");
            }
            else
            {
                FormatTable(Console.Out, espRows);
            }
        }

        private void FormatTable(System.IO.TextWriter writer, List<string[]> rows)
        {
            if (rows == null || rows.Count == 0) return;

            var columns = rows[0].Length;
            var widths = new int[columns];
            for (var c = 0; c < columns; c++)
            {
                widths[c] = rows.Max(r => r[c]?.Length ?? 0);
            }

            string FormatRow(string[] row) =>
                string.Join("  ", Enumerable.Range(0, columns).Select(c => (row[c] ?? string.Empty).PadRight(widths[c])));

            writer.WriteLine(FormatRow(rows[0]));
            writer.WriteLine(string.Join("  ", Enumerable.Range(0, columns).Select(c => new string('-', widths[c]))));
            for (var r = 1; r < rows.Count; r++)
            {
                writer.WriteLine(FormatRow(rows[r]));
            }
        }
    }
}
