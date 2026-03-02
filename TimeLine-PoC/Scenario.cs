using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Events;
using TimeLine_PoC.Models;

namespace TimeLine_PoC
{
    public class Scenario
    {
        public void ApplyAndVisualize(MeteringPoint mp, Event ev)
        {
            if (mp == null) throw new ArgumentNullException(nameof(mp));
            if (ev == null) throw new ArgumentNullException(nameof(ev));

            Console.Clear();
            Console.WriteLine("Current periods (ordered by ValidFrom):");
            mp.PrintPeriods();
            Console.WriteLine();

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

                default:
                    throw new InvalidOperationException($"Unsupported event type: {ev.GetType().FullName}");
            }

            Console.WriteLine("Applied.");
            Console.WriteLine();
            Console.WriteLine("Current periods (ordered by ValidFrom):");
            mp.PrintPeriods();
            Console.ReadLine();
        }
    }
}
