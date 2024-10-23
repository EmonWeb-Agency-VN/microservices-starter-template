using Common.Application.Time;

namespace Common.Application.Infrastructure.Time
{
    public class SystemTime : ISystemTime
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        public string DateTimeFormat => "";
    }
}
