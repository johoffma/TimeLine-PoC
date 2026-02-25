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
        new Scenario_MPP().Execute();

    }
}