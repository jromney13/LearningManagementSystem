using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace _5GuysLMS.Data
{
    public class Hasher
    {

        public static SaltedHash getSaltedHash(string passwordInput) {

            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: passwordInput,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));


            return new SaltedHash(hash, Convert.ToBase64String(salt));
        }

        public static string getSaltedHashSaltProvided(string inputString, string inputSalt) {

            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                   password: inputString,
                   salt: Convert.FromBase64String(inputSalt),
                   prf: KeyDerivationPrf.HMACSHA256,
                   iterationCount: 100000,
                   numBytesRequested: 256 / 8));

            return hash;

        }



    }
}
