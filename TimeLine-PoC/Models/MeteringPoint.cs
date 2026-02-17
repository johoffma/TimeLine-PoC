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
            var cr = new CommercialRelation(
                this,
                input.CreatedAt,
                input.ValidityDate,
                input.EnergySupplierId,
                Reason.PrimaryMoveIn);

            var esp = new EnergySupplierPeriod(
                cr,
                input.CreatedAt,
                input.ValidityDate);

            cr.EnergySupplierPeriods.Add(esp);
            CRs.Add(cr);
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
        }
    }
}
