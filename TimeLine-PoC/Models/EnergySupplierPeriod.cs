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
            _customer = customer;
            _customerAddress = customerAddress;
        }

        public DateTime ValidFrom { get; }

        // Backing fields for inheritance behaviour (nullable means "not set on this period")
        private string? _customer;
        private string? _customerAddress;

        // Internal raw accessors so other ESP instances can inspect stored values without triggering inheritance logic
        internal string? RawCustomer => _customer;
        internal string? RawCustomerAddress => _customerAddress;

        // Generic resolver that walks previous ESPs within the same CommercialRelation to inherit values
        private T? ResolveInherited<T>(Func<EnergySupplierPeriod, T?> rawSelector)
            where T : class
        {
            // local value first
            var local = rawSelector(this);
            if (local != null)
                return local;

            var previous = Parent.GetPrevious(this);
            while (previous != null)
            {
                var val = rawSelector(previous);
                if (val != null)
                    return val;
                previous = Parent.GetPrevious(previous);
            }

            return null;
        }

        // Customer and CustomerAddress inherit from previous EnergySupplierPeriod when not set locally
        public string? Customer => _customer ?? ResolveInherited(p => p.RawCustomer);
        public string? CustomerAddress => _customerAddress ?? ResolveInherited(p => p.RawCustomerAddress);

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
