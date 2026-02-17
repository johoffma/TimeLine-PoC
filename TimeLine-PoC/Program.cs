// See https://aka.ms/new-console-template for more information
using System;
using TimeLine_PoC.Models;
using TimeLine_PoC.Events;
using TimeLine_PoC;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var mp1 = new MeteringPoint();

        Visualizer.ApplyAndVisualize(mp1, new CreateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 1),
            "New",
            "Address 1",
            "PH1H"));


        Visualizer.ApplyAndVisualize(mp1, new MoveInEvent(
            "mp1",
            new DateTime(2024, 1, 1),
            "Supplier1",
            Reason.PrimaryMoveIn,
            "Anne"
            ));

        Visualizer.ApplyAndVisualize(mp1, new UpdateCustomerEvent(
            "mp1",
            new DateTime(2024, 1, 1),
            "Supplier1",
            "Anne",
            "Address 1"
            ));


        Visualizer.ApplyAndVisualize(mp1, new MoveInEvent(
            "mp1",
            new DateTime(2024, 2, 2),
            "Supplier1",
            Reason.SecondaryMoveIn,
            "Bente"
            ));


        Visualizer.ApplyAndVisualize(mp1, new MoveInEvent(
            "mp1",
            new DateTime(2024, 2, 1),
            "Supplier2",
            Reason.PrimaryMoveIn,
            "Carsten"
            ));

        Visualizer.ApplyAndVisualize(mp1, new MoveInEvent(
            "mp1",
            new DateTime(2023, 10, 1),
            "Supplier3",
            Reason.PrimaryMoveIn,
            "Dorte"
            ));

        /*
        Visualizer.ApplyAndVisualize(mp1, new ConnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 15)));

        Visualizer.ApplyAndVisualize(mp1, new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 2, 1),
            resolution: "PH15H"));

        Visualizer.ApplyAndVisualize(mp1, new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 1, 29),
            addressLine: "Address 2"));

        Visualizer.ApplyAndVisualize(mp1, new DisconnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 3, 1)));

        Visualizer.ApplyAndVisualize(mp1, new ReConnectMeteringPointEvent(
            "mp1",
            new DateTime(2024, 3, 1)));

        Visualizer.ApplyAndVisualize(mp1, new UpdateMeteringPointEvent(
            "mp1",
            new DateTime(2024, 2, 15),
            addressLine: "Address 3"));

        */
    }

}