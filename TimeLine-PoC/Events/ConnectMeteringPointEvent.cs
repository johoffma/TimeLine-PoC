using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Events
{
    public class ConnectMeteringPointEvent : Event
    {
        public ConnectMeteringPointEvent(
            string meteringPointId,
            DateTime validityDate
            ) : base(meteringPointId, validityDate)
        { 
        }
    }
}
