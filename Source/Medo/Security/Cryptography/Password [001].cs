//Copyright (c) 2013 Josip Medved <jmedved@jmedved.com>

//2013-03-26: Initial version.


using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Medo.Security.Cryptography {

    /// <summary>
    /// Various password generation algorithms.
    /// Based on:
    ///     http://www.akkadia.org/drepper/SHA-crypt.txt
    ///     http://www.freebsd.org/cgi/cvsweb.cgi/~checkout~/src/lib/libcrypt/crypt.c?rev=1.2
    ///     http://httpd.apache.org/docs/2.2/misc/password_encryptions.html
    /// </summary>
    public static class Password {

        private static UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);
        private static RandomNumberGenerator Rng = RandomNumberGenerator.Create();
        private static readonly char[] Base64Characters = new char[] { '.', '/', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        private const Int32 MinimumSaltSize = 0;
        private const Int32 MaximumSaltSize = 16;
        private const Int32 MinimumIterationCount = 1000;
        private const Int32 MaximumIterationCount = 999999999;

        private const Int32 Md5DefaultIterationCount = 1000;
        private const Int32 Md5ApacheDefaultIterationCount = 1000;
        private const Int32 Sha256DefaultIterationCount = 5000;
        private const Int32 Sha512DefaultIterationCount = 5000;


        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">Password to hash. It is converted to bytes using UTF8.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null.</exception>
        public static String Create(String password) {
            return Create(password, 16, PasswordAlgorithm.Sha512);
        }

        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">Password to hash. It is converted to bytes using UTF8.</param>
        /// <param name="algorithm">Algorithm to use.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown algorithm.</exception>
        public static String Create(String password, PasswordAlgorithm algorithm) {
            if ((algorithm == PasswordAlgorithm.MD5) || (algorithm == PasswordAlgorithm.MD5Apache)) {
                return Create(password, 8, algorithm);
            } else {
                return Create(password, 16, algorithm);
            }
        }

        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">Password to hash. It is converted to bytes using UTF8.</param>
        /// <param name="saltSize">Size of salt. It is kept between 0 and 16 characters without throwing exception.</param>
        /// <param name="algorithm">Algorithm to use.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown algorithm.</exception>
        public static String Create(String password, Int32 saltSize, PasswordAlgorithm algorithm) {
            return Create(password, saltSize, algorithm, 0);
        }

        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">Password to hash. It is converted to bytes using UTF8.</param>
        /// <param name="saltSize">Size of salt. It is kept between 0 and 16 characters without throwing exception.</param>
        /// <param name="algorithm">Algorithm to use.</param>
        /// <param name="iterationCount">Number of iterations. If value is 0 then default value is used.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown algorithm.</exception>
        public static String Create(String password, Int32 saltSize, PasswordAlgorithm algorithm, Int32 iterationCount) {
            if (password == null) { throw new ArgumentNullException("password", "Password cannot be null."); }

            if (saltSize < Password.MinimumSaltSize) { saltSize = Password.MinimumSaltSize; }
            if (saltSize > Password.MaximumSaltSize) { saltSize = Password.MaximumSaltSize; }

            var salt = new byte[saltSize];
            Password.Rng.GetBytes(salt);
            for (int i = 0; i < salt.Length; i++) { //make it an ascii
                salt[i] = (byte)Password.Base64Characters[salt[i] % Password.Base64Characters.Length];
            }

            return Create(Password.Utf8WithoutBom.GetBytes(password), salt, algorithm, iterationCount);
        }

        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">Password to hash.</param>
        /// <param name="algorithm">Algorithm to use.</param>
        /// <param name="salt">Salt. Length of salt is not restricted.</param>
        /// <param name="iterationCount">Number of iterations. If value is 0 then default value is used.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null. -or- Salt cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown algorithm.</exception>
        public static String Create(Byte[] password, Byte[] salt, PasswordAlgorithm algorithm, Int32 iterationCount) {
            if (password == null) { throw new ArgumentNullException("password", "Password cannot be null."); }
            if (salt == null) { throw new ArgumentNullException("salt", "Salt cannot be null."); }

            if (iterationCount != 0) { //silently setup iterationCount to allowable limits (except for default value)
                if (iterationCount < Password.MinimumIterationCount) { iterationCount = Password.MinimumIterationCount; }
                if (iterationCount > Password.MaximumIterationCount) { iterationCount = Password.MaximumIterationCount; }
            }

            if (algorithm == PasswordAlgorithm.Default) { algorithm = PasswordAlgorithm.Sha512; }
            switch (algorithm) {
                case PasswordAlgorithm.MD5: return CreateMd5Basic(password, salt, iterationCount);
                case PasswordAlgorithm.MD5Apache: return CreateMd5Apache(password, salt, iterationCount);
                case PasswordAlgorithm.Sha256: return CreateSha256(password, salt, iterationCount);
                case PasswordAlgorithm.Sha512: return CreateSha512(password, salt, iterationCount);
                default: throw new ArgumentOutOfRangeException("algorithm", "Unknown algorithm.");
            }
        }


        /// <summary>
        /// Verifies password agains hash.
        /// </summary>
        /// <param name="password">Password to check.</param>
        /// <param name="passwordHash">Hashed password.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null.</exception>
        public static Boolean Verify(String password, String passwordHash) {
            if (password == null) { throw new ArgumentNullException("password", "Password cannot be null."); }
            return Verify(Password.Utf8WithoutBom.GetBytes(password), passwordHash);
        }

        /// <summary>
        /// Verifies
        /// </summary>
        /// <param name="password">Password to check.</param>
        /// <param name="passwordHash">Hashed password.</param>
        /// <exception cref="System.ArgumentNullException">Password cannot be null.</exception>
        public static Boolean Verify(Byte[] password, String passwordHash) {
            if (password == null) { throw new ArgumentNullException("password", "Password cannot be null."); }
            if (passwordHash == null) { return false; }

            string id;
            int iterationCount;
            byte[] salt;
            string hash;

            if (!(SplitHashedPassword(passwordHash, out id, out iterationCount, out salt, out hash))) { return false; }

            string hashCalc;
            switch (id) { //algorithm
                case "1": SplitHashedPassword(CreateMd5Basic(password, salt, iterationCount), out hashCalc); break;
                case "apr1": SplitHashedPassword(CreateMd5Apache(password, salt, iterationCount), out hashCalc); break;
                case "5": SplitHashedPassword(CreateSha256(password, salt, iterationCount), out hashCalc); break;
                case "6": SplitHashedPassword(CreateSha512(password, salt, iterationCount), out hashCalc); break;
                default: return false;
            }
            return string.Equals(hash, hashCalc);
        }


        #region SHA 256/512

        private static string CreateSha256(byte[] password, byte[] salt, int iterationCount) {
            if (iterationCount == 0) { iterationCount = Password.Sha256DefaultIterationCount; }
            var c = CreateSha("SHA256", password, salt, iterationCount);

            var sb = new StringBuilder();
            sb.Append("$5$");
            if (iterationCount != Password.Sha256DefaultIterationCount) {
                sb.Append("rounds=" + iterationCount.ToString(CultureInfo.InvariantCulture) + "$");
            }
            sb.Append(ASCIIEncoding.ASCII.GetString(salt));
            sb.Append("$");
            Base64TripetFill(sb, c[00], c[10], c[20]);
            Base64TripetFill(sb, c[21], c[01], c[11]);
            Base64TripetFill(sb, c[12], c[22], c[02]);
            Base64TripetFill(sb, c[03], c[13], c[23]);
            Base64TripetFill(sb, c[24], c[04], c[14]);
            Base64TripetFill(sb, c[15], c[25], c[05]);
            Base64TripetFill(sb, c[06], c[16], c[26]);
            Base64TripetFill(sb, c[27], c[07], c[17]);
            Base64TripetFill(sb, c[18], c[28], c[08]);
            Base64TripetFill(sb, c[09], c[19], c[29]);
            Base64TripetFill(sb, null, c[31], c[30]);
            return sb.ToString();
        }

        private static string CreateSha512(byte[] password, byte[] salt, int iterationCount) {
            if (iterationCount == 0) { iterationCount = Password.Sha512DefaultIterationCount; }
            var c = CreateSha("SHA512", password, salt, iterationCount);

            var sb = new StringBuilder();
            sb.Append("$6$");
            if (iterationCount != Password.Sha512DefaultIterationCount) {
                sb.Append("rounds=" + iterationCount.ToString(CultureInfo.InvariantCulture) + "$");
            }
            sb.Append(ASCIIEncoding.ASCII.GetString(salt));
            sb.Append("$");
            Base64TripetFill(sb, c[00], c[21], c[42]);
            Base64TripetFill(sb, c[22], c[43], c[01]);
            Base64TripetFill(sb, c[44], c[02], c[23]);
            Base64TripetFill(sb, c[03], c[24], c[45]);
            Base64TripetFill(sb, c[25], c[46], c[04]);
            Base64TripetFill(sb, c[47], c[05], c[26]);
            Base64TripetFill(sb, c[06], c[27], c[48]);
            Base64TripetFill(sb, c[28], c[49], c[07]);
            Base64TripetFill(sb, c[50], c[08], c[29]);
            Base64TripetFill(sb, c[09], c[30], c[51]);
            Base64TripetFill(sb, c[31], c[52], c[10]);
            Base64TripetFill(sb, c[53], c[11], c[32]);
            Base64TripetFill(sb, c[12], c[33], c[54]);
            Base64TripetFill(sb, c[34], c[55], c[13]);
            Base64TripetFill(sb, c[56], c[14], c[35]);
            Base64TripetFill(sb, c[15], c[36], c[57]);
            Base64TripetFill(sb, c[37], c[58], c[16]);
            Base64TripetFill(sb, c[59], c[17], c[38]);
            Base64TripetFill(sb, c[18], c[39], c[60]);
            Base64TripetFill(sb, c[40], c[61], c[19]);
            Base64TripetFill(sb, c[62], c[20], c[41]);
            Base64TripetFill(sb, null, null, c[63]);
            return sb.ToString();
        }

        private static byte[] CreateSha(string hashName, byte[] password, byte[] salt, int iterationCount) {
            byte[] hashA;
            using (var digestA = HashAlgorithm.Create(hashName)) { //step 1
                AddDigest(digestA, password); //step 2
                AddDigest(digestA, salt); //step 3

                byte[] hashB;
                using (var digestB = HashAlgorithm.Create(hashName)) { //step 4
                    AddDigest(digestB, password); //step 5
                    AddDigest(digestB, salt);  //step 6
                    AddDigest(digestB, password); //step 7
                    hashB = FinishDigest(digestB); //step 8
                }
                AddRepeatedDigest(digestA, hashB, password.Length); //step 9/10

                var passwordLenght = password.Length;
                while (passwordLenght > 0) { //step 11
                    if ((passwordLenght & 0x01) != 0) { //bit 1
                        AddDigest(digestA, hashB);
                    } else { //bit 0
                        AddDigest(digestA, password);
                    }
                    passwordLenght >>= 1;
                }
                hashA = FinishDigest(digestA); //step 12
            }

            byte[] hashDP;
            using (var digestDP = HashAlgorithm.Create(hashName)) { //step 13
                for (int i = 0; i < password.Length; i++) { //step 14
                    AddDigest(digestDP, password);
                }
                hashDP = FinishDigest(digestDP); //step 15
            }
            var p = ProduceBytes(hashDP, password.Length); //step 16

            byte[] hashDS;
            using (var digestDS = HashAlgorithm.Create(hashName)) { //step 17
                for (int i = 0; i < (16 + hashA[0]); i++) { //step 18
                    AddDigest(digestDS, salt);
                }
                hashDS = FinishDigest(digestDS); //step 19
            }
            var s = ProduceBytes(hashDS, salt.Length); //step 20

            var hashAC = hashA;
            for (int i = 0; i < iterationCount; i++) { //step 21
                using (var digestC = HashAlgorithm.Create(hashName)) { //step 21a
                    if ((i % 2) == 1) { //step 21b
                        AddDigest(digestC, p);
                    } else { //step 21c
                        AddDigest(digestC, hashAC);
                    }
                    if ((i % 3) != 0) { AddDigest(digestC, s); } //step 21d
                    if ((i % 7) != 0) { AddDigest(digestC, p); } //step 21e
                    if ((i % 2) == 1) { //step 21f
                        AddDigest(digestC, hashAC);
                    } else { //step 21g
                        AddDigest(digestC, p);
                    }
                    hashAC = FinishDigest(digestC); //step 21h
                }
            }
            return hashAC;
        }

        #endregion

        #region MD5

        private static string CreateMd5Basic(byte[] password, byte[] salt, int iterationCount) {
            if (iterationCount == 0) { iterationCount = Password.Md5DefaultIterationCount; }
            return CreateMd5(password, salt, iterationCount, "$1$");
        }

        private static string CreateMd5Apache(byte[] password, byte[] salt, int iterationCount) {
            if (iterationCount == 0) { iterationCount = Password.Md5ApacheDefaultIterationCount; }
            return CreateMd5(password, salt, iterationCount, "$apr1$");
        }

        private static string CreateMd5(byte[] password, byte[] salt, int iterationCount, string magic) {
            byte[] hashA;
            using (var digestA = HashAlgorithm.Create("MD5")) { //step 1
                //password+magic+salt
                AddDigest(digestA, password); //step 2
                AddDigest(digestA, ASCIIEncoding.ASCII.GetBytes(magic));
                AddDigest(digestA, salt); //step 3

                byte[] hashB;
                using (var digestB = HashAlgorithm.Create("MD5")) { //step 4
                    AddDigest(digestB, password); //step 5
                    AddDigest(digestB, salt);  //step 6
                    AddDigest(digestB, password); //step 7
                    hashB = FinishDigest(digestB); //step 8
                }
                AddRepeatedDigest(digestA, hashB, password.Length); //step 9/10

                var passwordLenght = password.Length;
                while (passwordLenght > 0) { //step 11
                    if ((passwordLenght & 0x01) != 0) { //bit 1
                        AddDigest(digestA, new byte[] { 0x00 });
                    } else { //bit 0
                        AddDigest(digestA, new byte[] { password[0] });
                    }
                    passwordLenght >>= 1;
                }
                hashA = FinishDigest(digestA); //step 12
            }

            var hashAC = hashA;
            for (int i = 0; i < iterationCount; i++) { //step 21
                using (var digestC = HashAlgorithm.Create("MD5")) { //step 21a
                    if ((i % 2) == 1) { //step 21b
                        AddDigest(digestC, password);
                    } else { //step 21c
                        AddDigest(digestC, hashAC);
                    }
                    if ((i % 3) != 0) { AddDigest(digestC, salt); } //step 21d
                    if ((i % 7) != 0) { AddDigest(digestC, password); } //step 21e
                    if ((i % 2) == 1) { //step 21f
                        AddDigest(digestC, hashAC);
                    } else { //step 21g
                        AddDigest(digestC, password);
                    }
                    hashAC = FinishDigest(digestC); //step 21h
                }
            }

            var c = hashAC;

            var sb = new StringBuilder();
            sb.Append(magic);
            if (iterationCount != Password.Md5DefaultIterationCount) {
                sb.Append("rounds=" + iterationCount.ToString(CultureInfo.InvariantCulture) + "$");
            }
            sb.Append(ASCIIEncoding.ASCII.GetString(salt));
            sb.Append("$");
            Base64TripetFill(sb, c[00], c[06], c[12]);
            Base64TripetFill(sb, c[01], c[07], c[13]);
            Base64TripetFill(sb, c[02], c[08], c[14]);
            Base64TripetFill(sb, c[03], c[09], c[15]);
            Base64TripetFill(sb, c[04], c[10], c[05]);
            Base64TripetFill(sb, null, null, c[11]);
            return sb.ToString();
        }

        #endregion

        #region Helpers

        private static void AddDigest(HashAlgorithm digest, byte[] bytes) {
            if (bytes.Length == 0) { return; }
            var hashLen = digest.HashSize / 8;

            var offset = 0;
            var remaining = bytes.Length;
            while (remaining > 0) {
                digest.TransformBlock(bytes, offset, (remaining >= hashLen) ? hashLen : remaining, null, 0);
                remaining -= hashLen;
                offset += hashLen;
            }
        }

        private static void AddRepeatedDigest(HashAlgorithm digest, byte[] bytes, int length) {
            var hashLen = digest.HashSize / 8;
            while (length > 0) {
                digest.TransformBlock(bytes, 0, (length >= hashLen) ? hashLen : length, null, 0);
                length -= hashLen;
            }
        }

        private static byte[] ProduceBytes(byte[] hash, int lenght) {
            var hashLen = hash.Length;
            var produced = new byte[lenght];
            var offset = 0;
            while (lenght > 0) {
                Buffer.BlockCopy(hash, 0, produced, offset, (lenght >= hashLen) ? hashLen : lenght);
                offset += hashLen;
                lenght -= hashLen;
            }

            return produced;
        }

        private static byte[] FinishDigest(HashAlgorithm digest) {
            digest.TransformFinalBlock(new byte[] { }, 0, 0);
            return digest.Hash;
        }

        private static void Base64TripetFill(StringBuilder sb, byte? byte2, byte? byte1, byte? byte0) {
            if (byte0 != null) {
                sb.Append(Password.Base64Characters[byte0.Value & 0x3F]);
                if (byte1 != null) {
                    sb.Append(Password.Base64Characters[((byte1.Value & 0x0F) << 2) | (byte0.Value >> 6)]);
                    if (byte2 != null) {
                        sb.Append(Password.Base64Characters[((byte2.Value & 0x03) << 4) | (byte1.Value >> 4)]);
                        sb.Append(Password.Base64Characters[byte2.Value >> 2]);
                    } else {
                        sb.Append(Password.Base64Characters[byte1.Value >> 4]);
                    }
                } else {
                    sb.Append(Password.Base64Characters[byte0.Value >> 6]);
                }
            }
        }

        private static bool SplitHashedPassword(string hashedPassword, out string hash) {
            string id;
            int iterationCount;
            byte[] salt;
            return SplitHashedPassword(hashedPassword, out id, out iterationCount, out salt, out hash);
        }

        private static bool SplitHashedPassword(string hashedPassword, out string id, out int iterationCount, out byte[] salt, out string hash) {
            id = null;
            iterationCount = 0;
            salt = null;
            hash = null;

            var parts = hashedPassword.Split('$');
            if (parts.Length < 4) { return false; }

            id = parts[1];
            if (parts[2].StartsWith("rounds=", StringComparison.Ordinal) && (parts.Length >= 5) && (int.TryParse(parts[2].Substring(7), NumberStyles.Integer, CultureInfo.InvariantCulture, out iterationCount))) {
                salt = ASCIIEncoding.ASCII.GetBytes(parts[3]);
                hash = parts[4];
            } else {
                iterationCount = 0;
                salt = ASCIIEncoding.ASCII.GetBytes(parts[2]);
                hash = parts[3];
            }
            return true;
        }

        #endregion

    }


    /// <summary>
    /// Algorithm to use for hasing a password.
    /// </summary>
    public enum PasswordAlgorithm {
        /// <summary>
        /// Default algorithm.
        /// </summary>
        Default = 0,

        /// <summary>
        /// MD-5 algoritm.
        /// </summary>
        MD5 = 10,

        /// <summary>
        /// Apache's MD-5 algoritm.
        /// </summary>
        MD5Apache = 11,

        /// <summary>
        /// SHA-256 algoritm.
        /// </summary>
        Sha256 = 50,

        /// <summary>
        /// SHA-512 algoritm.
        /// </summary>
        Sha512 = 60,
    }
}
