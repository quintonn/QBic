using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WebsiteTemplate.Utilities
{
    public static class Encryption
    {
        // AES block/key size in bits used for key derivation. 128-bit key is used for compatibility.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // Number of iterations for PBKDF2 (Rfc2898). Increased from 1000 to a modern default.
        // Adjust this value according to your deployment's performance/strength needs.
        private const int DerivationIterations = 100000;

        public static string Encrypt(string plainText, string passphrase)
        {
            // Salt and IV are randomly generated for each encryption and prepended to the cipher text
            // so the same salt/IV can be used during decryption.
            var saltStringBytes = GenerateRandomBytes(Keysize / 8);
            var ivStringBytes = GenerateRandomBytes(16); // AES block size is 16 bytes (128 bits)
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, DerivationIterations, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var aes = Aes.Create())
                {
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var encryptor = aes.CreateEncryptor(keyBytes, ivStringBytes))
                    using (var memoryStream = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        // Final bytes: [salt] + [iv] + [ciphertext]
                        var cipherTextBytes = saltStringBytes
                            .Concat(ivStringBytes)
                            .Concat(memoryStream.ToArray())
                            .ToArray();

                        return Convert.ToBase64String(cipherTextBytes);
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passphrase)
        {
            // Stored format: [salt (Keysize/8 bytes)] + [iv (16 bytes)] + [ciphertext]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);

            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(16).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) + 16).ToArray();

            // Try decrypting with the current (stronger) iteration count first.
            try
            {
                using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, DerivationIterations, System.Security.Cryptography.HashAlgorithmName.SHA256))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var aes = Aes.Create())
                    {
                        aes.BlockSize = 128;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;

                        using (var decryptor = aes.CreateDecryptor(keyBytes, ivStringBytes))
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        using (var streamReader = new StreamReader(cryptoStream))
                        {
                            var plaintext = streamReader.ReadToEnd();
                            return plaintext;
                        }
                    }
                }
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Decryption failed with the current iteration count -> attempt legacy iteration count for backward compatibility.
            }

            // Fallback: try legacy (older) iteration count so previously-encrypted data can still be decrypted after upgrade.
            const int LegacyDerivationIterations = 1000;
            using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, LegacyDerivationIterations, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var aes = Aes.Create())
                {
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor(keyBytes, ivStringBytes))
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (var streamReader = new StreamReader(cryptoStream))
                    {
                        var plaintext = streamReader.ReadToEnd();
                        return plaintext;
                    }
                }
            }
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            var randomBytes = new byte[length];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
