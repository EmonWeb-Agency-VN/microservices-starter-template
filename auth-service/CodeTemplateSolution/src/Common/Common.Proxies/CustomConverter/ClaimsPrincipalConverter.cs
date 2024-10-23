using Newtonsoft.Json;
using System.Security.Claims;

namespace Auth.API.CustomConverter
{
    public class ClaimsPrincipalConverter : JsonConverter<ClaimsPrincipal>
    {
        public override void WriteJson(JsonWriter writer, ClaimsPrincipal? value, JsonSerializer serializer)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            var identities = value.Identities.Select(i => new ClaimsIdentityDto
            {
                AuthenticationType = i.AuthenticationType,
                Claims = i.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value }).ToList()
            }).ToList();

            serializer.Serialize(writer, identities);
        }

        public override ClaimsPrincipal ReadJson(JsonReader reader, Type objectType, ClaimsPrincipal existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var identities = serializer.Deserialize<List<ClaimsIdentityDto>>(reader);
            var claimsIdentities = identities.Select(i => new ClaimsIdentity(
                i.Claims.Select(c => new Claim(c.Type, c.Value)).ToList(), i.AuthenticationType)).ToList();

            return new ClaimsPrincipal(claimsIdentities);
        }

        private class ClaimsIdentityDto
        {
            public string AuthenticationType { get; set; }
            public List<ClaimDto> Claims { get; set; }
        }

        private class ClaimDto
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
