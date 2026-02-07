using System;

namespace TimeLine_PoC.Models
{
    public abstract class Period
    {
        protected Period(MeteringPoint meteringPoint, DateTime createdAt)
        {
            CreatedAt = createdAt;
            MeteringPoint = meteringPoint ?? throw new ArgumentNullException(nameof(meteringPoint));
        }
        public DateTime CreatedAt { get; private set; }

        internal MeteringPoint MeteringPoint { get; }
    }
}