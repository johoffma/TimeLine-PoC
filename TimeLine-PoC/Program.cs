// See https://aka.ms/new-console-template for more information
using System;
using TimeLine_PoC.Models;
using TimeLine_PoC.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var mp1 = new MeteringPoint();

        ApplyAndVisualize(mp1, new CreateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 1),
            "New",
            "Address 1",
            "PH1H"));


        ApplyAndVisualize(mp1, new ConnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 15)));

        ApplyAndVisualize(mp1, new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 2, 1),
            resolution: "PH15H"));

        ApplyAndVisualize(mp1, new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 29),
            addressLine: "Address 2"));

        ApplyAndVisualize(mp1, new DisconnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 3, 1)));

        ApplyAndVisualize(mp1, new ReConnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 3, 1)));

        ApplyAndVisualize(mp1, new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 2, 15),
            addressLine: "Address 3"));
    }

    private static void ApplyAndVisualize(MeteringPoint mp, Event ev)
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