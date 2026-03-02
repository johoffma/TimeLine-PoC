using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Events;
using TimeLine_PoC.Models;

namespace TimeLine_PoC
{
    public class Scenario_MPP : Scenario
    {
        public void Execute()
        {
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
    }
}
