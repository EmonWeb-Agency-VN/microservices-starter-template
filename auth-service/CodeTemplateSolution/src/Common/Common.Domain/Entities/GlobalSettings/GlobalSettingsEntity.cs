namespace Common.Domain.Entities.GlobalSettings
{
    public class GlobalSettingsEntity : AuditableEntities
    {
        public Guid Id { get; set; }

        public GlobalType Type { get; set; }

        public string Detail { get; set; } = string.Empty;
    }

    public enum GlobalType
    {
        None = 0,
        AuthenticationSetting = 1,
        BeforeTimeout = 2,
    }
}
