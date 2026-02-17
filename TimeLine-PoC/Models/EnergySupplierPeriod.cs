using System;

namespace TimeLine_PoC.Models
{
    public class EnergySupplierPeriod : Period<CommercialRelation>
    {
        public EnergySupplierPeriod(
            CommercialRelation commercialRelation,
            DateTime createdAt,
            DateTime validFrom) : base(commercialRelation, createdAt)
        {
            ValidFrom = validFrom;
        }

        public DateTime ValidFrom { get; }

        // ValidTo is determined from the owning CommercialRelation's sibling EnergySupplierPeriods
        public DateTime? ValidTo => Parent.GetNextValidFrom(this);
    }
}
