using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WaferFlow.Database.Connection;
using WaferFlow.Models;

namespace WaferFlow.Database.Repositories
{
    public class DefectRepository
    {
        public List<WaferDefect> GetByWaferId(int waferId)
        {
            var defects = new List<WaferDefect>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT wd.*, dt.DefectName
                    FROM [WaferDefects] wd
                    LEFT JOIN [DefectTypes] dt ON wd.DefectTypeId = dt.DefectTypeId
                    WHERE wd.WaferId = @WaferId
                    ORDER BY wd.DefectId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", waferId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            defects.Add(new WaferDefect
                            {
                                DefectId = Convert.ToInt32(reader["DefectId"]),
                                WaferId = Convert.ToInt32(reader["WaferId"]),
                                DefectTypeId = reader["DefectTypeId"] != DBNull.Value ? Convert.ToInt32(reader["DefectTypeId"]) : (int?)null,
                                DefectTypeName = reader["DefectName"]?.ToString(),
                                X_Position = Convert.ToDecimal(reader["X_Position"]),
                                Y_Position = Convert.ToDecimal(reader["Y_Position"]),
                                Severity = Convert.ToInt32(reader["Severity"])
                            });
                        }
                    }
                }
            }
            return defects;
        }

        public List<DefectType> GetAllDefectTypes()
        {
            var defectTypes = new List<DefectType>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [DefectTypes] ORDER BY DefectName";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        defectTypes.Add(new DefectType
                        {
                            DefectTypeId = Convert.ToInt32(reader["DefectTypeId"]),
                            DefectCode = reader["DefectCode"].ToString(),
                            DefectName = reader["DefectName"].ToString(),
                            Category = reader["Category"]?.ToString(),
                            Description = reader["Description"]?.ToString()
                        });
                    }
                }
            }
            return defectTypes;
        }

        public int Add(WaferDefect defect)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO [WaferDefects] (WaferId, DefectTypeId, X_Position, Y_Position, Severity)
                    VALUES (@WaferId, @DefectTypeId, @X_Position, @Y_Position, @Severity);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", defect.WaferId);
                    cmd.Parameters.AddWithValue("@DefectTypeId", defect.DefectTypeId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@X_Position", defect.X_Position);
                    cmd.Parameters.AddWithValue("@Y_Position", defect.Y_Position);
                    cmd.Parameters.AddWithValue("@Severity", defect.Severity);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool Delete(int defectId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM [WaferDefects] WHERE DefectId = @DefectId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@DefectId", defectId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteByWaferId(int waferId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM [WaferDefects] WHERE WaferId = @WaferId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", waferId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}