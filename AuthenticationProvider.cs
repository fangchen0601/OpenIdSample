using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CallBackHandler
{
    public class AuthenticationProvider
    {
        public string OpenIdConfigUrl { get; }

        private OpenIdConnectConfiguration openIdConfiguration;

        private ILogger logger;

        public AuthenticationProvider(string openIdConfigUrl, ILogger logger)
        {
            this.OpenIdConfigUrl = openIdConfigUrl;
            this.logger = logger;
        }

        public async Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequest request, string[] validIssuers, string validAudience)
        {
            var token = request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                return new RequestValidationResult(false);
            }

            var tokenString = token.Replace("Bearer ", "");

            IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>(
                        this.OpenIdConfigUrl,
                        new OpenIdConnectConfigurationRetriever());

            this.openIdConfiguration = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuers = validIssuers,
                ValidAudience = validAudience,
                IssuerSigningKeys = this.openIdConfiguration.SigningKeys,
            };
            ClaimsPrincipal claimsPrincipal;
            try
            {
                // Now validate the token. If the token is not valid for any reason, an exception will be thrown by the method
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                claimsPrincipal = handler.ValidateToken(tokenString, validationParameters, out _);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to validate token for client, {ex}");
                return new RequestValidationResult(false);
            }

            return new RequestValidationResult(true);
        }
    }
}
