using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Events;
using TimeLine_PoC.Models;

namespace TimeLine_PoC
{
    public static class Visualizer
    {
        public static void ApplyAndVisualize(MeteringPoint mp, Event ev)
        {
            if (mp == null) throw new ArgumentNullException(nameof(mp));
            if (ev == null) throw new ArgumentNullException(nameof(ev));

            Console.Clear();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Applying {ev.GetType().Name} (CreatedAt: {ev.CreatedAt:O})");
            Console.WriteLine($"  MeteringPointId: {ev.MeteringPointId}");
            Console.WriteLine($"  ValidityDate   : {ev.ValidityDate:O}");

            switch (ev)
            {
                case CreateMeteringPointEvent c:
                    Console.WriteLine($"  ConnectionState: {c.ConnectionState}");
                    Console.WriteLine($"  AddressLine    : {c.AddressLine ?? "<null>"}");
                    Console.WriteLine($"  Resolution     : {c.Resolution ?? "<null>"}");
                    mp.Apply(c);
                    break;

                case UpdateMeteringPointEvent u:
                    Console.WriteLine($"  AddressLine    : {u.AddressLine ?? "<null>"}");
                    Console.WriteLine($"  Resolution     : {u.Resolution ?? "<null>"}");
                    mp.Apply(u);
                    break;

                case DisconnectMeteringPointEvent d:
                    mp.Apply(d);
                    break;

                case ReConnectMeteringPointEvent r:
                    mp.Apply(r);
                    break;

                case ConnectMeteringPointEvent co:
                    mp.Apply(co);
                    break;

                case MoveInEvent mi:
                    Console.WriteLine($"  AddressLine    : {mi.EnergySupplierId ?? "<null>"}");
                    Console.WriteLine($"  Resolution     : {mi.Reason.ToString() ?? "<null>"}");
                    mp.Apply(mi);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported event type: {ev.GetType().FullName}");
            }

            Console.WriteLine("Applied.");
            Console.WriteLine();
            Console.WriteLine("Current periods (ordered by ValidFrom):");
            mp.PrintPeriods();

            Console.WriteLine();
            Console.WriteLine("Commercial relations (and underlying EnergySupplierPeriods):");
            PrintCommercialRelationsAndEnergySupplierPeriods(mp);

            Console.ReadLine();
        }

        private static void PrintCommercialRelationsAndEnergySupplierPeriods(MeteringPoint mp)
        {
            if (mp == null) throw new ArgumentNullException(nameof(mp));

            // Sort CommercialRelations by ValidFrom asc, CreatedAt desc (newest for ties)
            var sortedCr = mp.CRs
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
            espRows.Add(new[] { "#", "CR#", "ValidFrom", "ValidTo", "CreatedAt" });

            var espIndex = 0;
            for (var crIndex = 0; crIndex < sortedCr.Count; crIndex++)
            {
                var cr = sortedCr[crIndex];
                var sortedEsps = cr.EnergySupplierPeriods
                    .OrderBy(e => e.ValidFrom)
                    .ThenByDescending(e => e.CreatedAt)
                    .ToList();

                for (var j = 0; j < sortedEsps.Count; j++)
                {
                    var esp = sortedEsps[j];
                    var espValidToNullable = esp.ValidTo; // property already clips to CR.ValidTo
                    var espValidToText = espValidToNullable == null
                        ? "<null>"
                        : (espValidToNullable.Value == DateTime.MaxValue ? "MaxValue" : espValidToNullable.Value.ToString("O"));

                    espIndex++;
                    espRows.Add(new[]
                    {
                        espIndex.ToString(),
                        (crIndex + 1).ToString(),
                        esp.ValidFrom.ToString("O"),
                        espValidToText,
                        esp.CreatedAt.ToString("O")
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

        private static void FormatTable(System.IO.TextWriter writer, List<string[]> rows)
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
