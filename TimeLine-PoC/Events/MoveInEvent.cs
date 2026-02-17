using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Models;

namespace TimeLine_PoC.Events
{
    public class MoveInEvent : Event
    {
        public MoveInEvent(
            string meteringPointId,
            DateTime validityDate,
            string energySupplierId,
            Reason reason,
            string customer) : base(meteringPointId, validityDate)
        {
            EnergySupplierId = energySupplierId;
            Reason = reason;
            Customer = customer;
        }

        public string EnergySupplierId { get; }

        public Reason Reason { get; }
        public string Customer { get; }
        }
}
