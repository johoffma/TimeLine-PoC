using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Models;

namespace TimeLine_PoC.Events
{
    public class UpdateCustomerEvent : Event
    {
        public UpdateCustomerEvent(
            string meteringPointId,
            DateTime validityDate,
            string energySupplierId,
            string? customer,
            string? customerAddress) : base(meteringPointId, validityDate)
        {
            EnergySupplierId = energySupplierId;
            Customer = customer;
            CustomerAddress = customerAddress;
        }

        public string EnergySupplierId { get; }

        public Reason Reason { get; }
        public string? Customer { get; }
        public string? CustomerAddress { get; }
    }
}
