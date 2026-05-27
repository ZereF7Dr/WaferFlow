using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using WaferFlow.Database.Connection;
using WaferFlow.Models;

namespace WaferFlow.Database.Repositories
{
    public class UserRepository
    {
        public User GetByLogin(string login)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT u.*, r.RoleName 
                    FROM [Users] u
                    JOIN [UserRoles] r ON u.RoleId = r.RoleId
                    WHERE u.Login = @Login";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Login", login);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        public User GetById(int userId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT u.*, r.RoleName 
                    FROM [Users] u
                    JOIN [UserRoles] r ON u.RoleId = r.RoleId
                    WHERE u.UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<User> GetAll()
        {
            var users = new List<User>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT u.*, r.RoleName 
                    FROM [Users] u
                    JOIN [UserRoles] r ON u.RoleId = r.RoleId
                    ORDER BY u.UserId";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(MapToUser(reader));
                    }
                }
                foreach (var user in users)
                {
                    user.DisplayInfo = $"{user.FullName} | {GetRoleNameForDisplay(user.RoleName)} | {user.Login}";
                }

            }
            return users;
        }

        private string GetRoleNameForDisplay(string role)
        {
            switch (role)
            {
                case "Operator": return "Оператор";
                case "Engineer": return "Инженер";
                case "Manager": return "Менеджер";
                case "Admin": return "Админ";
                default: return role;
            }
        }

        public bool Update(User user)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE [Users] 
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        Patronymic = @Patronymic,
                        Email = @Email,
                        AvatarData = @AvatarData,
                        AvatarContentType = @AvatarContentType,
                        RoleId = @RoleId,
                        ShiftId = @ShiftId
                    WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Patronymic", user.Patronymic ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvatarData", user.AvatarData ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvatarContentType", user.AvatarContentType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@ShiftId", user.ShiftId ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateLastLogin(int userId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "UPDATE [Users] SET LastLogin = GETDATE() WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateAvatar(int userId, byte[] avatarData, string contentType)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "UPDATE [Users] SET AvatarData = @AvatarData, AvatarContentType = @AvatarContentType WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@AvatarData", avatarData ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvatarContentType", contentType ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateProfile(int userId, string firstName, string lastName, string patronymic, string email)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE [Users] 
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        Patronymic = @Patronymic,
                        Email = @Email
                    WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@FirstName", firstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", lastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Patronymic", patronymic ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public int Add(User user, string passwordHash)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO [Users] ([Login], [FirstName], [LastName], [Patronymic], [PasswordHash], [Email], [AvatarData], [AvatarContentType], [RoleId], [ShiftId])
                    VALUES (@Login, @FirstName, @LastName, @Patronymic, @PasswordHash, @Email, @AvatarData, @AvatarContentType, @RoleId, @ShiftId);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Login", user.Login);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Patronymic", user.Patronymic ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvatarData", user.AvatarData ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvatarContentType", user.AvatarContentType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@ShiftId", user.ShiftId ?? (object)DBNull.Value);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private User MapToUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                Login = reader["Login"].ToString(),
                FirstName = reader["FirstName"]?.ToString(),
                LastName = reader["LastName"]?.ToString(),
                Patronymic = reader["Patronymic"]?.ToString(),
                PasswordHash = reader["PasswordHash"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                AvatarData = reader["AvatarData"] != DBNull.Value ? (byte[])reader["AvatarData"] : null,
                AvatarContentType = reader["AvatarContentType"]?.ToString(),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                RoleName = reader["RoleName"]?.ToString(),
                ShiftId = reader["ShiftId"] != DBNull.Value ? Convert.ToInt32(reader["ShiftId"]) : (int?)null,
                LastLogin = reader["LastLogin"] != DBNull.Value ? Convert.ToDateTime(reader["LastLogin"]) : (DateTime?)null
            };
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "UPDATE [Users] SET PasswordHash = @PasswordHash WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int userId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM [Users] WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}