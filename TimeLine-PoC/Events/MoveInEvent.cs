using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Events
{
    public class MoveInEvent : Event
    {
        public MoveInEvent(
            string meteringPointId,
            DateTime validityDate,
            string energySupplierId
            ) : base(meteringPointId, validityDate)
        {
            EnergySupplierId = energySupplierId;
        }

        public string EnergySupplierId { get; }
    }
}
