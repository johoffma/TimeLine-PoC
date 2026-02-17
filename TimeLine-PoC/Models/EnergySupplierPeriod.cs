using System;

namespace TimeLine_PoC.Models
{
    public class EnergySupplierPeriod : Period<CommercialRelation>
    {
        public EnergySupplierPeriod(
            CommercialRelation commercialRelation,
            DateTime createdAt,
            DateTime validFrom,
            string? customer = null,
            string? customerAddress = null) : base(commercialRelation, createdAt)
        {
            ValidFrom = validFrom;
            Customer = customer;    
            CustomerAddress = customerAddress;
        }

        public DateTime ValidFrom { get; }
        public string? Customer { get; }
        public string? CustomerAddress { get; } 

        // ValidTo is the earlier of:
        // - the next EnergySupplierPeriod.ValidFrom within the same CommercialRelation
        // - the CommercialRelation.ValidTo (so it never extends beyond its owning CR)
        public DateTime? ValidTo
        {
            get
            {
                var nextEspValidFrom = Parent.GetNextValidFrom(this); // may be DateTime.MaxValue
                var crValidTo = Parent.ValidTo; // may be DateTime.MaxValue

                var min = nextEspValidFrom < crValidTo ? nextEspValidFrom : crValidTo;
                return min == DateTime.MaxValue ? (DateTime?)DateTime.MaxValue : min;
            }
        }
    }
}
