using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Events;
using TimeLine_PoC.Models;

namespace TimeLine_PoC
{
    public class Scenario_MoveIn_ChangeSupplier : Scenario
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


            ApplyAndVisualize(mp1, new MoveInEvent(
                "mp1",
                new DateTime(2024, 1, 1),
                "Supplier1",
                Reason.PrimaryMoveIn,
                "Anne"
                ));

            ApplyAndVisualize(mp1, new UpdateCustomerEvent(
                "mp1",
                new DateTime(2024, 1, 1),
                "Supplier1",
                "Anne",
                "Address 1"
                ));

            ApplyAndVisualize(mp1, new ChangeSupplierEvent(
                "mp1",
                new DateTime(2024, 1, 15),
                "Supplier2",
                "Anne"
                ));

            ApplyAndVisualize(mp1, new ChangeSupplierEvent(
                "mp1",
                new DateTime(2024, 2, 1),
                "Supplier3",
                "Anne"
                ));

            ApplyAndVisualize(mp1, new MoveInEvent(
                "mp1",
                new DateTime(2024, 2, 15),
                "Supplier4",
                Reason.SecondaryMoveIn,
                "Bente"
                ));

            ApplyAndVisualize(mp1, new UpdateCustomerEvent(
                "mp1",
                new DateTime(2024, 2, 15),
                "Supplier4",
                null,
                "Her bor Bente"
                ));


            ApplyAndVisualize(mp1, new MoveInEvent(
                "mp1",
                new DateTime(2024, 1, 27),
                "Supplier5",
                Reason.PrimaryMoveIn,
                "Carsten"
                ));
        }
    }
}
