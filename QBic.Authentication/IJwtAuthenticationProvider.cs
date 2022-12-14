using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QBic.Authentication
{
    public interface IJwtAuthenticationProvider
    {
        /// <summary>
        /// The relative request path to listen on. 
        /// This is the path to which login [and token refresh] requests should be made.
        /// </summary>
        /// <remarks>The default path is <c>/token</c>.</remarks>
        string Path { get; }// = "/token";

        /// <summary>
        /// Indicates if authentication service may be provided using insecure requests.
        /// Defaults to false.
        /// </summary>
        bool AllowInsecureHttp { get; } //= false;

        /// <summary>
        /// This provider will be used for all requests where this ClientId is present.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        ///  The Issuer (iss) claim for generated tokens.
        /// </summary>
        string Issuer { get; }

        /// <summary>
        /// The Audience (aud) claim for the generated tokens.
        /// </summary>
        string Audience { get; }

        /// <summary>
        /// The expiration time for the generated access tokens.
        /// </summary>
        /// <remarks>The default is five minutes (300 seconds).</remarks>
        TimeSpan AccessTokenExpiration { get; }// = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The expiration time for the generated refresh tokens.
        /// </summary>
        /// <remarks>The default is 1 day.</remarks>
        TimeSpan RefreshTokenExpiration { get; }// = TimeSpan.FromDays(1);

        /// <summary>
        /// The signing key to use when generating tokens.
        /// </summary>
        string SecretKey { get; }

        /// <summary>
        /// The algorithm to use when generating and validating tokens.
        /// Defaults to <see cref="SecurityAlgorithms.HmacSha256"/>
        /// </summary>
        string  SigningAlgorithm{ get; } //= SecurityAlgorithms.HmacSha256;

        /// <summary>
        /// Generates a random value (nonce) for each generated token.
        /// The default nonce is a random GUID.
        /// </summary>
        Func<string> NonceGenerator { get; }// = () => Guid.NewGuid().ToString();  //TODO: only required for Authentication

        /// <summary>
        /// This method can be used to add additional application specific claims.
        /// <para>Note that JWT required claims will already be present.</para>
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<IEnumerable<Claim>> GetAdditionalClaims(string username);

        /// <summary>
        /// This method is called when a login attempt is made.
        /// </summary>
        /// <param name="username">The username being authenticated.</param>
        /// <param name="password">The password being used.</param>
        /// <returns>Return true if the username and password are correct AND the password belongs to the username.</returns>
        Task<bool> VerifyPassword(string username, string password);

        /// <summary>
        /// This method is called when a refresh token is used to obtain new access and refresh tokens and should indicate if the refresh token exists.
        /// <para>When refersh tokens are manually revoked, deleted or block, this method can be used to mark a specifc refresh token as invalid.</para>
        /// </summary>
        /// <param name="token">The refresh token to find.</param>
        /// <returns>Return true if the refresh token still exists and is still valid if any custom validation needs to be done.</returns>
        Task<bool> FindRefreshToken(string token);

        /// <summary>
        /// Once a refresh token is used, this method can be used to delete it so as to prevent it from being used again.
        /// </summary>
        /// <param name="token">The token to delete</param>
        Task DeleteRefreshToken(string token);

        /// <summary>
        /// When a new token is created, either upon logon or refreshing of a token, a refresh token is stored.
        /// <para>Subsequent calls to <see cref="FindRefreshToken(string)"></see> will provide the same <paramref name="token"/> value.</para>
        /// </summary>
        /// <param name="token">The token to store.</param>
        /// <returns>Async task. Typical result can be:
        /// <code>return Task.FromResult(0);</code> 
        /// </returns>
        Task StoreNewRefreshTokenAsync(string token);

        /// <summary>
        /// This method is called to confirm if a user can log in, even with correct password.
        /// <para>This can be used to check for things such as email confirmations, user is blocked or inactive, etc.</para>
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>Return true if the user is allowed to login.</returns>
        Task<VerificationResult> VerifyUserCanLogin(string username);
    }
}
