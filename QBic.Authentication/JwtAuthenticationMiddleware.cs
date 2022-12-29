using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QBic.Authentication
{
    /// <summary>
    /// Authentication middleware.
    /// Used to perform logins and to refresh expired tokens.
    /// Not responsible for validating current access tokens. That is done my JwtAuthenticationMiddleware.
    /// </summary>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate Next;

        private readonly JsonSerializerOptions SerializerOptions;
        private ILogger Logger { get; set; }

        public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
        {
            Logger = logger;

            Next = next;

            SerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public async Task Invoke(HttpContext context)
        {
            var providers = context.RequestServices.GetServices<IJwtAuthenticationProvider>();

            var provider = providers?.Where(x => x.Path == context.Request.Path).SingleOrDefault();

            if (provider == null)
            {
                await Next(context);
                return;
            }

            if (provider.AllowInsecureHttp == false && context.Request.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase))
            {
                await Error(context, $"Calls to {provider.Path} only supports HTTPS requests", StatusCodes.Status405MethodNotAllowed);
                return;
            }

            if (!context.Request.Method.Equals("POST"))
            {
                await Error(context, "Bad request. Only POST requests allowed", StatusCodes.Status405MethodNotAllowed);
                return;
            }

            var clientId = "";
            var grantType = "";
            var username = "";
            var password = "";
            var refreshToken = "";

            if (context.Request.HasFormContentType)
            {
                clientId = context.Request.Form["client_id"];
                grantType = context.Request.Form["grant_type"];
                username = context.Request.Form["username"];
                password = context.Request.Form["password"];
                refreshToken = context.Request.Form["refresh_token"].FirstOrDefault();
            }
            else
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    var data = reader.ReadToEndAsync().Result;
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        var json = JsonDocument.Parse(data);
                        clientId = json.RootElement.GetProperty("client_id").GetString();
                        grantType = json.RootElement.GetProperty("grant_type").GetString();
                        username = json.RootElement.GetProperty("username").GetString();
                        password = json.RootElement.GetProperty("password").GetString();
                        refreshToken = json.RootElement.GetProperty("refresh_token").GetString();
                    }
                }
            }

            if (clientId != provider.ClientId)
            {
                await Error(context, "Invalid client id provided", StatusCodes.Status401Unauthorized);
                return;
            }

            // I could probably handle external tokens here too, e.g. after login using facebook, gmail, microsoft, etc.

            if (grantType == "password")
            {
                var validCredentials = await provider.VerifyPassword(username, password);

                if (validCredentials == false)
                {
                    await Error(context, "Incorrect username or password", StatusCodes.Status401Unauthorized);
                    return;
                }

                var verification = await provider.VerifyUserCanLogin(username);
                if (verification.Valid == false)
                {
                    await Error(context, verification.ErrorMessage, verification.StatusCode);
                    return;
                }

                var claims = await provider.GetAdditionalClaims(username);

                await GenerateToken(context, username, claims, provider); //TODO: Maybe the token subject should be the user's ID ??
                return;
            }
            else if (grantType == "refresh_token")
            {
                // User is using refresh token to get a new access & refresh token.
                if (String.IsNullOrWhiteSpace(refreshToken))
                {
                    await Error(context, "No refresh token provided while trying to perform token refresh", StatusCodes.Status400BadRequest);
                    return;
                }

                var handler = new JwtSecurityTokenHandler();
                if (handler.CanValidateToken)
                {
                    // Does refresh token exist
                    var tokenExists = await provider.FindRefreshToken(refreshToken);
                    if (tokenExists == false)
                    {
                        await Error(context, "Refresh token is no longer valid", StatusCodes.Status401Unauthorized);
                        return;
                    }

                    // Delete token so it can't be reused
                    await provider.DeleteRefreshToken(refreshToken);

                    //Validate the token
                    var tokenValidation = ValidateToken(refreshToken, providers);
                    if (tokenValidation.Valid == false)
                    {
                        await Error(context, tokenValidation.ErrorMessage, tokenValidation.StatusCode);
                        return;
                    }
                    else
                    {
                        var identity = tokenValidation.ResultObject as IIdentity;
                        await GenerateToken(context, identity.Name, new List<Claim>(), provider);
                        return;
                    }
                }
                else
                {
                    await Error(context, "Unable to validate refresh token", StatusCodes.Status500InternalServerError);
                    return;
                }
            }
            else
            {
                await Error(context, "Invalid grant_type provided", StatusCodes.Status400BadRequest);
                return;
            }

            //await Error(context, "Unable to process token request", StatusCodes.Status500InternalServerError);
            //return;
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
                    foreach (var provider in providers)
                    {
                        if (valToken.HasClaim("client", provider.ClientId))
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
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException)
            {
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

        //private async Task<ClaimsIdentity> CreateIdentity(string userName, IEnumerable<Claim> additionalClaims, JwtAuthenticationProvider optionsProvider)
        //{
        //    var claims = CreateClaims(userName, optionsProvider);

        //    claims.Union(additionalClaims);

        //    return new ClaimsIdentity(new GenericIdentity(userName, "Token"), claims);
        //}

        private async Task Error(HttpContext context, string error, int statusCode = StatusCodes.Status400BadRequest)
        {
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(error);
        }

        private IEnumerable<Claim> CreateClaims(string username, IJwtAuthenticationProvider provider)
        {
            var claims = new List<Claim>();
            claims.AddRange(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, provider.NonceGenerator()), // JWT ID
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // issued at
                new Claim(ClaimTypes.Name, username), // needed because JWtSecurityTokenHandler converts claimtype
                new Claim("client", provider.ClientId)
            });

            return claims;
        }

        private async Task GenerateToken(HttpContext context, string identityName, IEnumerable<Claim> additionalClaims, IJwtAuthenticationProvider provider)
        {
            var now = DateTime.UtcNow;
            var claims = CreateClaims(identityName, provider);

            claims.Union(additionalClaims);

            var accessToken = CreateToken(claims, now, now.Add(provider.AccessTokenExpiration), provider);
            var refreshToken = CreateToken(claims, now, now.Add(provider.RefreshTokenExpiration), provider);

            await provider.StoreNewRefreshTokenAsync(refreshToken);

            var response = new
            {
                access_token = accessToken,
                expires_in = (int)provider.AccessTokenExpiration.TotalSeconds,
                refresh_token = refreshToken
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json"; //MediaTypeNames.Application.Json (json not yet available)
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
        }

        private string CreateToken(IEnumerable<Claim> claims, DateTime notBefore, DateTime tokenExpiration, IJwtAuthenticationProvider optionsProvider)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(optionsProvider.SecretKey));
            var signingCredentials = new SigningCredentials(signingKey, optionsProvider.SigningAlgorithm);

            var token = new JwtSecurityToken(
                  issuer: optionsProvider.Issuer,
                  audience: optionsProvider.Audience,
                  claims: claims,
                  notBefore: notBefore,
                  expires: tokenExpiration,
                  signingCredentials: signingCredentials);

            var handler = new JwtSecurityTokenHandler();
            var encodedToken = handler.WriteToken(token);

            return encodedToken;
        }
    }
}
