//Copyright (c) 2010 Josip Medved <jmedved@jmedved.com>

//2010-05-17: New version.
//2013-03-23: Obsoleted in favor of Password class.


using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Medo.Security.Cryptography {

    /// <summary>
    /// Generates passwords based on password, username and salt.
    /// </summary>
    [Obsolete("Use Medo.Security.Cryptography.Password instead.")]
    public static class PasswordHash {

        private static readonly System.Random _random = new System.Random();
        private const byte _iterationPower = 13; //iterationCount = 2 ^ iterationPower
        private const int _saltSize = 9;
        private const int _keySize = 20;


        /// <summary>
        /// Returns encoded password.
        /// </summary>
        /// <param name="password">Password to encode.</param>
        public static string EncodePassword(string password) {
            return EncodePassword(password, null);
        }

        /// <summary>
        /// Returns encoded password and user name.
        /// </summary>
        /// <param name="password">Password.</param>
        /// <param name="userName">User name.</param>
        public static string EncodePassword(string password, string userName) {
            if (password == null) { password = string.Empty; }
            if (userName == null) { userName = string.Empty; }

            var passwordBuffer = UTF8Encoding.UTF8.GetBytes(password + "\0" + userName.ToUpperInvariant());

            var saltBuffer = new byte[_saltSize];
            _random.NextBytes(saltBuffer);

            var hashBuffer = new List<byte>();
            hashBuffer.Add(System.Math.Min(_iterationPower, (byte)30));
            hashBuffer.AddRange(saltBuffer);

            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordBuffer, saltBuffer, (int)System.Math.Pow(2, _iterationPower))) {
                hashBuffer.AddRange(pbkdf2.GetBytes(_keySize));
            }

            return Convert.ToBase64String(hashBuffer.ToArray());
        }

        /// <summary>
        /// Returns true if hash matches password.
        /// </summary>
        /// <param name="hash">Hashed password.</param>
        /// <param name="password">Password.</param>
        public static bool CheckPassword(string hash, string password) {
            return CheckPassword(hash, password, null);
        }

        /// <summary>
        /// Returns true if hash matches password and user name.
        /// </summary>
        /// <param name="hash">Hashed password and user name.</param>
        /// <param name="password">Password.</param>
        /// <param name="userName">User name.</param>
        public static bool CheckPassword(string hash, string password, string userName) {
            if (hash == null) { return false; }
            if (password == null) { password = string.Empty; }
            if (userName == null) { userName = string.Empty; }

            byte[] hashBuffer;
            try {
                hashBuffer = Convert.FromBase64String(hash);
                if (hashBuffer.Length != (1 + _saltSize + _keySize)) { return false; }
            } catch (FormatException) {
                return false;
            }

            int iterationPower = hashBuffer[0];
            if (iterationPower > 30) { return false; }

            var saltBuffer = new byte[_saltSize];
            Array.Copy(hashBuffer, 1, saltBuffer, 0, saltBuffer.Length);

            var passwordBuffer = UTF8Encoding.UTF8.GetBytes(password + "\0" + userName.ToUpperInvariant());

            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordBuffer, saltBuffer, (int)System.Math.Pow(2, iterationPower))) {
                var hashBuffer2 = pbkdf2.GetBytes(_keySize);
                for (int i = 0; i < _keySize; ++i) {
                    if (hashBuffer[1 + _saltSize + i] != hashBuffer2[i]) { return false; }
                }
            }

            return true;
        }

    }

}
