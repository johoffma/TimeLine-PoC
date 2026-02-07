using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TimeLine_PoC.Models;

namespace TimeLine_PoC.Events
{
    public abstract class Event
    {
        internal Event(string MeteringPointId, DateTime validityDate)
        {
            CreatedAt = DateTime.UtcNow;
            this.MeteringPointId = MeteringPointId;
            ValidityDate = validityDate;
        }
        public DateTime CreatedAt { get; private set; }

        public string MeteringPointId { get; }
        public DateTime ValidityDate { get; }

    }
}
