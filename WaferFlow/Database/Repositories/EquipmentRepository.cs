using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WaferFlow.Database.Connection;
using WaferFlow.Models;

namespace WaferFlow.Database.Repositories
{
    public class EquipmentRepository
    {
        public List<Equipment> GetAll()
        {
            var equipmentList = new List<Equipment>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [Equipment] ORDER BY EquipmentName";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipmentList.Add(MapToEquipment(reader));
                    }
                }
            }
            return equipmentList;
        }

        public Equipment GetById(int equipmentId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [Equipment] WHERE EquipmentId = @EquipmentId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToEquipment(reader);
                        }
                    }
                }
            }
            return null;
        }

        public int Add(Equipment equipment)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO [Equipment] (EquipmentName, SerialNumber, Status)
                    VALUES (@EquipmentName, @SerialNumber, @Status);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EquipmentName", equipment.EquipmentName);
                    cmd.Parameters.AddWithValue("@SerialNumber", equipment.SerialNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", equipment.Status);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool Update(Equipment equipment)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE [Equipment] 
                    SET EquipmentName = @EquipmentName,
                        SerialNumber = @SerialNumber,
                        Status = @Status
                    WHERE EquipmentId = @EquipmentId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EquipmentId", equipment.EquipmentId);
                    cmd.Parameters.AddWithValue("@EquipmentName", equipment.EquipmentName);
                    cmd.Parameters.AddWithValue("@SerialNumber", equipment.SerialNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", equipment.Status);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int equipmentId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM [Equipment] WHERE EquipmentId = @EquipmentId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private Equipment MapToEquipment(SqlDataReader reader)
        {
            return new Equipment
            {
                EquipmentId = Convert.ToInt32(reader["EquipmentId"]),
                EquipmentName = reader["EquipmentName"].ToString(),
                SerialNumber = reader["SerialNumber"]?.ToString(),
                Status = reader["Status"].ToString()
            };
        }
    }
}