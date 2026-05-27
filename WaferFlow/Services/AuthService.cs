using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;

namespace WaferFlow.Services
{
    public class AuthService
    {
        private UserRepository _userRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// Проверка логина и пароля
        /// </summary>
        public User Authenticate(string login, string password)
        {
            var user = _userRepository.GetByLogin(login);

            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                _userRepository.UpdateLastLogin(user.UserId);
                user.LastLogin = DateTime.Now;
                return user;
            }

            return null;
        }

        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            string inputHash = ComputeSha256Hash(inputPassword);
            return inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }

        public string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
