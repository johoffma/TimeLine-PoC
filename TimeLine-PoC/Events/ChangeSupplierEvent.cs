using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Models;

namespace TimeLine_PoC.Events
{
    public class ChangeSupplierEvent : Event
    {
        public ChangeSupplierEvent(
            string meteringPointId,
            DateTime validityDate,
            string energySupplierId,
            string customer) : base(meteringPointId, validityDate)
        {
            EnergySupplierId = energySupplierId;
            Customer = customer;
        }

        public string EnergySupplierId { get; }
        public string Customer { get; }
    }
}
