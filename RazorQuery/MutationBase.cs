namespace RazorQuery;


public abstract class MutationBase
{
    public MutationStatus Status { get; protected set; } = MutationStatus.Idle;

    public Exception? Error { get; protected set; } = null;

    public bool IsIdle => Status == MutationStatus.Idle;

    public bool IsPending => Status == MutationStatus.Pending;

    public bool IsSuccess => Status == MutationStatus.Success;

    public bool IsError => Status == MutationStatus.Error;
}

/// <summary>
/// Represents the current state of a mutation.
/// </summary>
public enum MutationStatus
{
    Idle,
    Pending,
    Success,
    Error
}
