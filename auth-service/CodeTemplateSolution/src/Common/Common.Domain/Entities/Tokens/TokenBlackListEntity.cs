namespace Common.Domain.Entities.Tokens
{
    public class TokenBlackListEntity : AuditableEntities
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid SessionId { get; set; }
    }
}
