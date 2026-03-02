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

        // Backing field for CustomerId
        private Guid? _customerId;

        // Public property exposes inherited behavior: if this instance has no CustomerId,
        // walk previous CommercialRelation instances until a value is found (or null).
        public Guid? CustomerId
        {
            get => _customerId ?? ResolveInheritedCustomerId();
            internal set => _customerId = value;
        }

        // Internal raw accessor allows other instances to inspect stored value without triggering inheritance logic.
        internal Guid? RawCustomerId => _customerId;

        // Energy supplier periods that belong to this commercial relation
        private List<EnergySupplierPeriod> EnergySupplierPeriods { get; }

        // Sorted view: only include ESPs that start before the CR.ValidTo.
        // This prevents navigating to ESPs that lie entirely beyond the CommercialRelation range.
        internal List<EnergySupplierPeriod> GetSortedEnergySupplierPeriods()
            => EnergySupplierPeriods
                .Where(e => e.ValidFrom < this.ValidTo)                      // exclude ESPs starting at/after CR.ValidTo
                .OrderBy(p => p.ValidFrom)
                .ThenByDescending(p => p.CreatedAt)                         // newest created wins on tie
                .ToList();

        public void AddEnergySupplierPeriod(EnergySupplierPeriod period)
        {
            if (period == null) throw new ArgumentNullException(nameof(period));
            if (period.Parent != this) throw new ArgumentException("EnergySupplierPeriod must belong to this CommercialRelation.", nameof(period));
            EnergySupplierPeriods.Add(period);
        }

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

        // Next ValidFrom for EnergySupplierPeriod siblings.
        // If the current period is not part of the filtered set (e.g. starts after CR.ValidTo),
        // consider there is no next -> return DateTime.MaxValue (i.e. no successor within CR).
        public DateTime GetNextValidFrom(EnergySupplierPeriod current)
        {
            var sorted = GetSortedEnergySupplierPeriods();
            var idx = sorted.IndexOf(current);
            if (idx == -1) return DateTime.MaxValue;
            if (idx == sorted.Count - 1) return DateTime.MaxValue;
            return sorted[idx + 1].ValidFrom;
        }

        // ValidTo of this CommercialRelation is determined by the next CommercialRelation on the MeteringPoint
        public DateTime ValidTo
        {
            get
            {
                return Parent.GetNextValidFrom(this);
            }
        }

        // Walk previous CommercialRelation instances on the same MeteringPoint to inherit CustomerId.
        private Guid? ResolveInheritedCustomerId()
        {
            var previous = Parent.GetPrevious(this);
            while (previous != null)
            {
                if (previous.RawCustomerId.HasValue)
                    return previous.RawCustomerId;
                previous = Parent.GetPrevious(previous);
            }
            return null;
        }
    }
}
