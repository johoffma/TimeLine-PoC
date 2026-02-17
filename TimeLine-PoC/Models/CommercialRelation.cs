using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLine_PoC.Models
{
    public class CommercialRelation : Period<MeteringPoint>
    {
        public CommercialRelation(MeteringPoint meteringPoint, DateTime createdAt, DateTime validFrom, string energySupplierId, Reason reason)
            : base(meteringPoint, createdAt)
        {
            ValidFrom = validFrom;
            EnergySupplierId = energySupplierId ?? throw new ArgumentNullException(nameof(energySupplierId));
            Reason = reason;

            EnergySupplierPeriods = new List<EnergySupplierPeriod>();
        }

        public DateTime ValidFrom { get; }
        public string EnergySupplierId { get; }
        public Reason Reason { get; }

        // collection of EnergySupplierPeriod instances belonging to this commercial relation
        public List<EnergySupplierPeriod> EnergySupplierPeriods { get; }

        // Sorted view: by ValidFrom ascending, tie-breaker: CreatedAt descending (newest created first)
        internal List<EnergySupplierPeriod> GetSortedEnergySupplierPeriods()
            => EnergySupplierPeriods.OrderBy(p => p.ValidFrom).ThenByDescending(p => p.CreatedAt).ToList();

        public EnergySupplierPeriod? GetPrevious(EnergySupplierPeriod current)
        {
            var sorted = GetSortedEnergySupplierPeriods();
            var idx = sorted.IndexOf(current);
            if (idx <= 0) return null;
            return sorted[idx - 1];
        }

        public EnergySupplierPeriod? GetNext(EnergySupplierPeriod current)
        {
            var sorted = GetSortedEnergySupplierPeriods();
            var idx = sorted.IndexOf(current);
            if (idx == -1 || idx >= sorted.Count - 1) return null;
            return sorted[idx + 1];
        }

        // Returns the ValidFrom of the next EnergySupplierPeriod; DateTime.MaxValue if this is the last period.
        // Throws if current not found.
        public DateTime GetNextValidFrom(EnergySupplierPeriod current)
        {
            var sorted = GetSortedEnergySupplierPeriods();
            var idx = sorted.IndexOf(current);
            if (idx == -1) throw new InvalidOperationException("EnergySupplierPeriod not found.");
            if (idx == sorted.Count - 1) return DateTime.MaxValue;
            return sorted[idx + 1].ValidFrom;
        }
    }
}
