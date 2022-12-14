using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QBic.Authentication
{
    /// <summary>
    /// Authorization middleware. Relies on jwt bearer token being present in the requst.
    /// This middleware will only populate current user identity on httpcontext if it's able.
    /// No login or lookups are done.
    /// This is for when the login server is a separeate server, and will allow us to still authenticate using the token only.
    /// </summary>
    public class JwtAuthorizationMiddleware
    {
        private readonly RequestDelegate Next;

        private readonly JsonSerializerOptions SerializerOptions;
        private ILogger Logger { get; set; }

        public JwtAuthorizationMiddleware(RequestDelegate next, ILogger<JwtAuthorizationMiddleware> logger)
        {
            Logger = logger;

            Next = next;

            SerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public Task Invoke(HttpContext context)
        {
            //var identityResolvers = context.RequestServices.GetServices<IJwtIdentityResolver>();
            var providers = context.RequestServices.GetServices<IJwtAuthenticationProvider>();

            // See if authorization header is present, validate it, and set current user identity
            if (context.Request.Headers.TryGetValue("Authorization", out StringValues tokens))
            {
                if (tokens.Count > 0)
                {
                    var accessToken = tokens.First().Replace("Bearer", "").Trim();
                    if (!String.IsNullOrWhiteSpace(accessToken) && accessToken != "null")
                    {
                        var tokenValidation = ValidateToken(accessToken, providers);
                        if (tokenValidation.Valid == false)
                        {
                            // If i return error here, the non-authorize requests also fail if there is a token present.

                            // don't return error, might not require auth
                            // Maybe this should be here. Maybe token validation does other things too..
                        }
                    }
                }
            }

            return Next(context);
        }

        private VerificationResult TryParseToken(string token, IEnumerable<IJwtAuthenticationProvider> providers)
        {
            var handler = new JwtSecurityTokenHandler();

            //var validationParams = JwtValidation.GetValidationParameters(providers);
            //TODO: Do i want to choose all providers, or select ones that match the path or something?
            var validationParams = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = providers.Select(o => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(o.SecretKey))).ToList(),
                ValidateIssuer = true,
                ValidIssuers = providers.Select(o => o.Issuer).ToList(),
                ValidateAudience = true,
                ValidAudiences = providers.Select(o => o.Audience).ToList(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                var valToken = handler.ValidateToken(token, validationParams, out SecurityToken outToken);

                if (valToken.Identity.IsAuthenticated)
                {
                    foreach (var optionProvider in providers)
                    {
                        if (valToken.HasClaim("client", optionProvider.ClientId))
                        {
                            return VerificationResult.Success(valToken.Identity);
                        }
                    }

                    return VerificationResult.Error("Invalid bearer token provided, invalid clientId", StatusCodes.Status401Unauthorized);
                }
                else
                {
                    return VerificationResult.Error("Bearer token is no longer valid", StatusCodes.Status401Unauthorized);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return VerificationResult.Error("Bearer token is no longer valid", StatusCodes.Status401Unauthorized);
            }
            catch (SecurityTokenException)
            {
                return VerificationResult.Error("Unknown error while processing token", StatusCodes.Status401Unauthorized);
            }
            catch (ArgumentException)
            {
                /* This happens when token = "undefined" or some other invalid token value */
                return VerificationResult.Error("Unknown error while processing token", StatusCodes.Status401Unauthorized);
            }
        }

        private VerificationResult ValidateToken(string token, IEnumerable<IJwtAuthenticationProvider> providers)
        {
            if (String.IsNullOrWhiteSpace(token) || token.Trim().ToLower() == "null")
            {
                return VerificationResult.Error("Empty or null access token provided");
            }
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanValidateToken)
            {
                return TryParseToken(token, providers);
            }
            else
            {
                return VerificationResult.Error("Unable to validate token");
            }
        }
    }
}
