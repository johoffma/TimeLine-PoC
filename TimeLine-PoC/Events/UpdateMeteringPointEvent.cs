using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Events
{
    public class UpdateMeteringPointEvent : Event
    {
        public UpdateMeteringPointEvent(
            string meteringPointId,
            DateTime validityDate,
            string? addressLine = null,
            string? resolution = null) : base(meteringPointId, validityDate)
        {
            AddressLine = addressLine;
            Resolution = resolution;
        }

        public string? AddressLine { get; }
        public string? Resolution { get; }
    }
}
