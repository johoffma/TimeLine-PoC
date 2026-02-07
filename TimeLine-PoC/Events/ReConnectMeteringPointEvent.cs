using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Events
{
    public class ReConnectMeteringPointEvent : Event
    {
        public ReConnectMeteringPointEvent(
            string meteringPointId,
            DateTime validityDate
            ) : base(meteringPointId, validityDate)
        { 
        }
    }
}
