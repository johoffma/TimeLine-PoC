using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Models
{
    public class CommercialRelation : Period
    {
        public CommercialRelation(MeteringPoint meteringPoint, DateTime createdAt, DateTime validFrom, string energySupplierId, Reason reason) : base(meteringPoint, createdAt)
        {
            _validFrom = validFrom;
            energySupplierId = energySupplierId ?? throw new ArgumentNullException(nameof(energySupplierId));
            _energySupplierId = energySupplierId;
            _reason = reason;
        }

        private DateTime _validFrom;
        private Reason _reason;
        private string _energySupplierId;
    }
}
