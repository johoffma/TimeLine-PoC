using System;

namespace TimeLine_PoC.Models
{
    public abstract class Period<TParent>
    {
        protected Period(TParent parent, DateTime createdAt)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            CreatedAt = createdAt;
        }

        public DateTime CreatedAt { get; private set; }

        internal TParent Parent { get; }
    }
}