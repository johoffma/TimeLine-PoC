using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Events
{
    public class CreateMeteringPointEvent : Event
    {
        public CreateMeteringPointEvent(
            string meteringPointId,
            DateTime validityDate,
            string connectionState,
            string? addressLine = null,
            string? resolution = null
        ) : base(meteringPointId, validityDate)
        {
            ConnectionState = connectionState;

            AddressLine = addressLine;
            Resolution = resolution;
        }

        public string ConnectionState { get; }
        public string? AddressLine { get; }
        public string? Resolution { get; }

    }
}
