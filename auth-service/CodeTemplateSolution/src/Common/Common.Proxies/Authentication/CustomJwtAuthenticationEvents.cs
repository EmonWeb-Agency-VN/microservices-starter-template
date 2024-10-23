using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Common.Proxies.Authentication
{
    public class CustomJwtAuthenticationEvents : JwtBearerEvents
    {
        public CustomJwtAuthenticationEvents()
        {

        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("IS-TOKEN-EXPIRED", "true");
            }
            return Task.CompletedTask;
        }

        public override Task Challenge(JwtBearerChallengeContext context)
        {
            return base.Challenge(context);
        }

        public override Task Forbidden(ForbiddenContext context)
        {
            return base.Forbidden(context);
        }

        public override Task MessageReceived(MessageReceivedContext context)
        {
            return base.MessageReceived(context);
        }

        public override Task TokenValidated(TokenValidatedContext context)
        {
            return base.TokenValidated(context);
        }
    }
}
