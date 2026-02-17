using TimeLine_PoC.Models;

public class MeteringPointPeriod : Period<MeteringPoint>
{
    public MeteringPointPeriod(
        MeteringPoint meteringPoint,
        DateTime createdAt,
        DateTime validFrom,
        string? connectionState,
        string? addressLine = null,
        string? resolution = null) : base(meteringPoint, createdAt)
    {
        ValidFrom = validFrom;
        ConnectionState = connectionState;
        AddressLine = addressLine;
        Resolution = resolution;
    }

    // Back-compatibility helper (keeps existing code readable)
    internal MeteringPoint MeteringPoint => Parent;

    public DateTime ValidFrom { get; }
    public DateTime? ValidTo
    {
        get
        {
            return Parent.GetNextValidFrom(this);
        }
    }

    // Backing fields (nullable to represent "not set on this period")
    private string? _connectionState;
    private string? _addressLine;
    private string? _resolution;

    // Internal raw accessors so other period instances can read the raw stored value
    internal string? RawConnectionState => _connectionState;
    internal string? RawAddressLine => _addressLine;
    internal string? RawResolution => _resolution;

    // Generic resolver for inherited reference-type properties
    private T? ResolveInherited<T>(Func<MeteringPointPeriod, T?> rawSelector)
        where T : class
    {
        // local value first
        var local = rawSelector(this);
        if (local != null)
        {
            return local;
        }

        // walk previous periods until a non-null raw value is found
        var previous = Parent.GetPrevious(this);
        while (previous != null)
        {
            var val = rawSelector(previous);
            if (val != null)
            {
                return val;
            }
            previous = Parent.GetPrevious(previous);
        }

        return null;
    }

    // Variant that throws when no value can be resolved (used for required properties)
    private T ResolveInheritedOrThrow<T>(Func<MeteringPointPeriod, T?> rawSelector, string message)
        where T : class
    {
        var result = ResolveInherited(rawSelector);
        if (result == null)
        {
            throw new InvalidOperationException(message);
        }
        return result;
    }

    // ConnectionState is required for the first period => non-nullable property
    public string? ConnectionState
    {
        get => _connectionState ?? ResolveInheritedOrThrow(p => p.RawConnectionState, "Connection state must be set for the first period.");
        private set => _connectionState = value;
    }

    // AddressLine and Resolution can be inherited from previous periods and are optional
    public string? AddressLine
    {
        get => _addressLine ?? ResolveInherited(p => p.RawAddressLine);
        private set => _addressLine = value;
    }

    public string? Resolution
    {
        get => _resolution ?? ResolveInherited(p => p.RawResolution);
        private set => _resolution = value;
    }
}