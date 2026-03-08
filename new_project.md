Authentication design for new_project
=================================

This document describes a secure, modern authentication design for a new web application that:
- Gives full control over the user schema and persistence (NHibernate)
- Uses up-to-date hashing/KDF and authenticated encryption
- Issues short-lived JWT access tokens and server-side opaque refresh tokens with rotation
- Minimizes dependencies while using well-tested crypto libraries where appropriate

Goals
-----
- Secure password storage using a modern memory-hard KDF (Argon2id) with clearly defined parameters
- Stateless short-lived access tokens (JWT) for API authorization
- Server-controlled refresh tokens (opaque, hashed in DB) with rotation and replay detection
- Encrypted storage of any sensitive fields (secrets, 2FA seed, external tokens) using AEAD (AES-GCM)
- Simple, testable flows for register/login/refresh/logout/password-reset/2FA
- Key management and rotation guidance

High-level architecture
-----------------------
- Authentication primitives:
  - IUser/IUserStore implemented with NHibernate (reuse pattern from QBicUserStore)
  - UserManager (ASP.NET Identity primitives) may be used with a custom IUserStore and a custom IPasswordHasher
  - Custom `IPasswordHasher<TUser>` that wraps Argon2id (or other approved KDF)

- Tokens & sessions:
  - Access tokens: JWT (signed), short lifetime (recommended 5–15 minutes)
  - Refresh tokens: opaque random token returned to client; store only hash (SHA-256) in DB; rotate on use

- Secrets & encryption:
  - Use AES-GCM for symmetric authenticated encryption of stored secrets
  - Protect keys with environment/secret manager (Key Vault, Vault, AWS/Google secret manager)
  - Use ASP.NET DataProtection only for short-lived data protection if you persist its keyring across instances

Cryptography choices & formats
-----------------------------
1) Password hashing (recommended): Argon2id
   - Params (example baseline):
     - salt: 16 bytes (128 bits)
     - output hash: 32 bytes (256 bits)
     - memory: 64 MiB (65536 KB)
     - iterations (time cost): 3-4
     - parallelism: 1-2
   - Store password hash string in a self-describing format, e.g. Argon2 encoded form:
     $argon2id$v=19$m=65536,t=4,p=2$<base64-salt>$<base64-hash>
   - Implementation: use Konscious.Security.Cryptography.Argon2 or libsodium wrapper. Implement IPasswordHasher<T> to use this.

2) Password hashing (alternative): PBKDF2-HMAC-SHA256
   - If Argon2 not desired, PBKDF2 with SHA-256 and a large iteration count (>= 200k on typical server) and 16-byte salt.
   - Prefer Argon2 when possible.

3) Access token signing: Asymmetric (RS256 or ES256) recommended
   - Use RSA (RS256) or ECDSA (ES256) with key IDs (kid) in JWT header for rotation.
   - Keep private key in secret manager; publish public keys (JWKS) if tokens might be validated by other services.
   - If you must use symmetric keys, use HS256 but manage secrets carefully; asymmetric keys are preferred for rotation and separation of signing/verification.

4) Refresh tokens: opaque + hashed
   - Generate token: cryptographically-random byte sequence (e.g. 32–64 bytes), base64url encode for transport.
   - Store: SHA-256(tokenPlain) (or HMAC-SHA256 using a server secret) in DB, not the plaintext token.
   - DB row: token hash, user id, device info/ip (optional), createdAt, expiresAt, revokedAt, replacedByTokenHash (for rotation chain), lastUsedAt.
   - Rotation: when client uses a refresh token:
     1. Compute hash and find DB row.
     2. If not found or revoked/expired => reject.
     3. Issue a new refresh token, insert its hash, mark the old token revoked (store replacedByTokenHash). Return new refresh token to client.
     4. If an old token is seen after rotation (replay), treat as compromise: revoke all refresh tokens for user and require re-login.

5) Sensitive field encryption (AES-GCM)
   - For any stored secrets (2FA seed, external provider tokens, SMTP passwords) use AES-GCM with a unique nonce per encryption.
   - Blob format (versioned): [version:1][nonce:12][ciphertext][tag:16]
   - Store key in secret manager; rotate by re-encrypting data (or storing key metadata and supporting multiple keys for decrypt).

Database schema (recommended)
----------------------------
- Users
  - Id (GUID PK)
  - UserName, NormalizedUserName
  - Email, NormalizedEmail, EmailConfirmed (bool)
  - PasswordHash (string; self-describing algorithm+params)
  - SecurityStamp (string; random) — used to force token invalidation when changed
  - TwoFactorEnabled (bool)
  - LockoutEnd (datetime nullable), AccessFailedCount (int)
  - CreatedAt, UpdatedAt, LastLoginAt
  - Additional app-specific columns

- Roles
  - Id, Name, NormalizedName

- UserRoles
  - UserId, RoleId

- RefreshTokens
  - Id (GUID), UserId, TokenHash (varchar(64) for SHA-256 hex/base64), ExpiresAt, CreatedAt, RevokedAt, ReplacedBy (GUID/null), DeviceInfo, IpAddress, LastUsedAt
  - Index on TokenHash and UserId

- AuthEvents (optional audit)
  - Id, UserId, EventType, IpAddress, DeviceInfo, CreatedAt, Details

Flow diagrams & details
-----------------------
1) Registration
   - Create user with PasswordHash = Argon2(hash)
   - Set EmailConfirmed = false
   - Send email confirmation token generated via DataProtection or a short-lived signed JWT

2) Login (password grant)
   - Validate credentials using Argon2 hash comparison (verify with stored parameters)
   - If valid and user allowed, issue:
     - access_token: JWT signed (RS256), includes sub=userId, iat, exp, aud, iss, jti, optional roles minimal
     - refresh_token: opaque random, store hash in RefreshTokens table
   - Return both tokens to client

3) Refresh
   - Client submits refresh_token
   - Server computes hash, finds DB row, checks not expired/revoked
   - Create new access token and a new refresh token; insert new hash and mark old as revoked with ReplacedBy
   - Return new tokens
   - If old token reuse detected after rotation -> revoke all sessions for user and require re-authentication

4) Logout / Revoke
   - Delete/mark the refresh token as revoked
   - Optionally audit event

5) Password reset
   - Generate single-use token using DataProtection or an expiring signed token (short lifetime), send link to email
   - On use, reset password and update SecurityStamp to invalidate existing tokens, and delete relevant refresh tokens

6) 2FA
   - Support TOTP (RFC6238) with per-user secret stored encrypted (AES-GCM)
   - Provide backup codes (store hashed)

Key management & rotation
-------------------------
- Signing keys (JWT): keep a key store with metadata (kid, createdAt, status). Always publish the active public keys (JWKS) if other services validate your tokens.
- Validation should accept multiple keys (current + previous) to allow rotation while issued tokens expire.
- Refresh token hashing: no rotation required for hashing algorithm, but consider adding HMAC with server secret if you need keyed validation.
- Encryption keys: rotate by re-encrypting data (preferable) or maintain multiple keys and try them on decrypt.
- DataProtection: if used, persist key ring to shared storage across instances.
- Use a secret manager (vault) — do not store private keys/secrets in source or in plaintext config files.

Operational & security practices
--------------------------------
- Enforce HTTPS; reject non-TLS on token endpoints.
- Rate-limit authentication endpoints and implement exponential backoff.
- Implement strong logging for auth events (login success/failure) but do NOT log secrets, tokens, or decrypted values.
- Detect suspicious behavior (many failed logins, token reuse) and alert/revoke sessions.
- Ensure database backups either exclude refresh tokens or that restore procedures rotate/invalidate tokens after restore.
- Provide an endpoint to list active sessions and allow users to revoke them (safe UX practice).

Minimal dependency list
-----------------------
- NHibernate (for persistence)
- Microsoft.AspNetCore.Identity (only for primitives if desired: UserManager, IPasswordHasher interfaces) — you can implement the IUserStore to avoid EF
- Konscious.Security.Cryptography.Argon2 (or libsodium wrapper) for Argon2id
- System.Security.Cryptography (built-in) for AES-GCM, RSA key operations, HMAC, SHA-256
- Microsoft.IdentityModel.Tokens + System.IdentityModel.Tokens.Jwt for JWT handling

Example implementation notes
----------------------------
- Implement a class Argon2PasswordHasher : IPasswordHasher<TUser>
  - Hash: create salt (16 bytes), run Argon2id with chosen params, return encoded string with params
  - Verify: parse stored string, run Argon2 with stored params & salt, constant-time compare

- Refresh token helper:
  - Generate: token = RandomNumberGenerator.GetBytes(48) => base64url
  - Store: SHA256(token) hex/base64 in DB
  - On validation: compute SHA256(token) and lookup

- JWT issuance:
  - Use a signing service that adds `kid` header and signs with the active private key
  - Keep JWT short (5–15 min) and include a `jti` claim for unique token id when you need extra auditing

Migration note (if importing old users)
-------------------------------------
- If you must import users from an old system with different hash/format:
  - Preserve existing hash and store a `HashAlgorithm` metadata column
  - On first successful login, re-hash the password using Argon2 and replace the stored hash
  - Alternatively, require users to reset passwords during first login cycle

Next steps I can implement for you
----------------------------------
1) Scaffold a small starter project showing:
   - NHibernate-backed IUserStore
   - Argon2 `IPasswordHasher` implementation
   - JWT access token issuance (RS256) + refresh token rotation
   - AES-GCM helpers and example encrypted field
2) Provide a migration tool to import old users and re-hash on first login or eagerly

Pick the next step you'd like and I will scaffold the code and README for the sample implementation.

End of document
