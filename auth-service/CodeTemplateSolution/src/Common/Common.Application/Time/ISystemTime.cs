namespace Common.Application.Time
{
    public interface ISystemTime
    {
        DateTimeOffset UtcNow { get; }
        string DateTimeFormat { get; }
    }
}
