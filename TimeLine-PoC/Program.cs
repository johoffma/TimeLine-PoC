// See https://aka.ms/new-console-template for more information
using TimeLine_PoC.Models;
using TimeLine_PoC.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var mp1 = new MeteringPoint();
        mp1.Apply(new CreateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 1),
            "Connected",
            "Address 1",
            "PH1H"));

        mp1.Apply(new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 2, 1),
            resolution: "PH15H"));

        mp1.Apply(new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 29),
            addressLine: "Address 2"));

        mp1.Apply(new DisconnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 3, 1)));

        mp1.Apply(new ReConnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 3, 1)));

        mp1.Apply(new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 2, 15),
            addressLine: "Address 3"));


        mp1.PrintPeriods();

        Console.ReadLine();
    }


}