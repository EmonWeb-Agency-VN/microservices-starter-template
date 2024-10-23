using Newtonsoft.Json;
using System.Security.Claims;

namespace Common.Proxies.CustomConverter
{
    public class ClaimsIdentityConverter : JsonConverter<ClaimsIdentity>
    {
        public override void WriteJson(JsonWriter writer, ClaimsIdentity value, JsonSerializer serializer)
        {
            var claims = value.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var dto = new
            {
                value.AuthenticationType,
                Claims = claims
            };
            serializer.Serialize(writer, dto);
        }

        public override ClaimsIdentity ReadJson(JsonReader reader, Type objectType, ClaimsIdentity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dto = serializer.Deserialize<ClaimsIdentityDto>(reader);
            var claims = dto.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
            return new ClaimsIdentity(claims, dto.AuthenticationType);
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
