using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Database.Connection
{
    public static class DatabaseConnection
    {
        private static string _connectionString;

        static DatabaseConnection()
        {
            // Читаем строку подключения из App.config
            _connectionString = ConfigurationManager.ConnectionStrings["SiliconWafersDB"].ConnectionString;
        }

        /// <summary>
        /// Возвращает новое открытое соединение с БД
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}