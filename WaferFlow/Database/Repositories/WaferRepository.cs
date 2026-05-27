using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaferFlow.Database.Connection;
using WaferFlow.Models;

namespace WaferFlow.Database.Repositories
{
    internal class WaferRepository
    {
        public List<Wafer> GetAll()
        {
            var wafers = new List<Wafer>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT w.*, 
                           u.FirstName + ' ' + u.LastName AS OperatorName,
                           e.EquipmentName,
                           pg.GenerationName,
                           rc.CategoryName AS RejectCategoryName
                    FROM [Wafers] w
                    LEFT JOIN [Users] u ON w.OperatorId = u.UserId
                    LEFT JOIN [Equipment] e ON w.EquipmentId = e.EquipmentId
                    LEFT JOIN [ProcessorGenerations] pg ON w.GenerationId = pg.GenerationId
                    LEFT JOIN [RejectCategories] rc ON w.RejectCategoryId = rc.CategoryId
                    ORDER BY w.WaferId DESC";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        wafers.Add(MapToWafer(reader));
                    }
                }
            }
            return wafers;
        }

        public List<Wafer> GetByStatus(string status)
        {
            var wafers = new List<Wafer>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT w.*, 
                           u.FirstName + ' ' + u.LastName AS OperatorName,
                           e.EquipmentName,
                           pg.GenerationName,
                           rc.CategoryName AS RejectCategoryName
                    FROM [Wafers] w
                    LEFT JOIN [Users] u ON w.OperatorId = u.UserId
                    LEFT JOIN [Equipment] e ON w.EquipmentId = e.EquipmentId
                    LEFT JOIN [ProcessorGenerations] pg ON w.GenerationId = pg.GenerationId
                    LEFT JOIN [RejectCategories] rc ON w.RejectCategoryId = rc.CategoryId
                    WHERE w.Status = @Status
                    ORDER BY w.Priority ASC, w.MeasurementTime DESC";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", status);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            wafers.Add(MapToWafer(reader));
                        }
                    }
                }
            }
            return wafers;
        }

        public Wafer GetById(int waferId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT w.*, 
                           u.FirstName + ' ' + u.LastName AS OperatorName,
                           e.EquipmentName,
                           pg.GenerationName,
                           rc.CategoryName AS RejectCategoryName
                    FROM [Wafers] w
                    LEFT JOIN [Users] u ON w.OperatorId = u.UserId
                    LEFT JOIN [Equipment] e ON w.EquipmentId = e.EquipmentId
                    LEFT JOIN [ProcessorGenerations] pg ON w.GenerationId = pg.GenerationId
                    LEFT JOIN [RejectCategories] rc ON w.RejectCategoryId = rc.CategoryId
                    WHERE w.WaferId = @WaferId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", waferId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToWafer(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<Wafer> GetByBatch(string batchNumber)
        {
            var wafers = new List<Wafer>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT w.*, 
                           u.FirstName + ' ' + u.LastName AS OperatorName,
                           e.EquipmentName,
                           pg.GenerationName,
                           rc.CategoryName AS RejectCategoryName
                    FROM [Wafers] w
                    LEFT JOIN [Users] u ON w.OperatorId = u.UserId
                    LEFT JOIN [Equipment] e ON w.EquipmentId = e.EquipmentId
                    LEFT JOIN [ProcessorGenerations] pg ON w.GenerationId = pg.GenerationId
                    LEFT JOIN [RejectCategories] rc ON w.RejectCategoryId = rc.CategoryId
                    WHERE w.BatchNumber = @BatchNumber
                    ORDER BY w.WaferId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@BatchNumber", batchNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            wafers.Add(MapToWafer(reader));
                        }
                    }
                }
            }
            return wafers;
        }

        public int Add(Wafer wafer)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO [Wafers] (
                        SerialNumber, BatchNumber, Diameter, Thickness, 
                        Resistivity, DislocationDensity, Curvature, DopingUniformity,
                        ProductionDate, OperatorId, EquipmentId, Status, Priority,
                        MeasurementTime
                    ) VALUES (
                        @SerialNumber, @BatchNumber, @Diameter, @Thickness,
                        @Resistivity, @DislocationDensity, @Curvature, @DopingUniformity,
                        @ProductionDate, @OperatorId, @EquipmentId, @Status, @Priority,
                        @MeasurementTime
                    );
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SerialNumber", wafer.SerialNumber);
                    cmd.Parameters.AddWithValue("@BatchNumber", wafer.BatchNumber);
                    cmd.Parameters.AddWithValue("@Diameter", wafer.Diameter);
                    cmd.Parameters.AddWithValue("@Thickness", wafer.Thickness);
                    cmd.Parameters.AddWithValue("@Resistivity", wafer.Resistivity ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DislocationDensity", wafer.DislocationDensity ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Curvature", wafer.Curvature ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DopingUniformity", wafer.DopingUniformity ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProductionDate", wafer.ProductionDate);
                    cmd.Parameters.AddWithValue("@OperatorId", wafer.OperatorId);
                    cmd.Parameters.AddWithValue("@EquipmentId", wafer.EquipmentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", "Measured");
                    cmd.Parameters.AddWithValue("@Priority", wafer.Priority);
                    cmd.Parameters.AddWithValue("@MeasurementTime", DateTime.Now);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool Update(Wafer wafer)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE [Wafers] 
                    SET Status = @Status,
                        GenerationId = @GenerationId,
                        RejectCategoryId = @RejectCategoryId,
                        ClassificationTime = @ClassificationTime
                    WHERE WaferId = @WaferId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", wafer.WaferId);
                    cmd.Parameters.AddWithValue("@Status", wafer.Status);
                    cmd.Parameters.AddWithValue("@GenerationId", wafer.GenerationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RejectCategoryId", wafer.RejectCategoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ClassificationTime", wafer.ClassificationTime ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int waferId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM [Wafers] WHERE WaferId = @WaferId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", waferId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public string GenerateSerialNumber()
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM [Wafers]";

                using (var cmd = new SqlCommand(query, connection))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"WAF-{DateTime.Now:yyyyMMdd}-{count:D4}";
                }
            }
        }

        public List<string> GetBatches()
        {
            var batches = new List<string>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT DISTINCT BatchNumber FROM [Wafers] ORDER BY BatchNumber DESC";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        batches.Add(reader["BatchNumber"].ToString());
                    }
                }
            }
            return batches;
        }

        private Wafer MapToWafer(SqlDataReader reader)
        {
            return new Wafer
            {
                WaferId = Convert.ToInt32(reader["WaferId"]),
                SerialNumber = reader["SerialNumber"].ToString(),
                BatchNumber = reader["BatchNumber"].ToString(),
                Diameter = Convert.ToDecimal(reader["Diameter"]),
                Thickness = Convert.ToDecimal(reader["Thickness"]),
                Resistivity = reader["Resistivity"] != DBNull.Value ? Convert.ToDecimal(reader["Resistivity"]) : (decimal?)null,
                DislocationDensity = reader["DislocationDensity"] != DBNull.Value ? Convert.ToDecimal(reader["DislocationDensity"]) : (decimal?)null,
                Curvature = reader["Curvature"] != DBNull.Value ? Convert.ToDecimal(reader["Curvature"]) : (decimal?)null,
                DopingUniformity = reader["DopingUniformity"] != DBNull.Value ? Convert.ToDecimal(reader["DopingUniformity"]) : (decimal?)null,
                ProductionDate = Convert.ToDateTime(reader["ProductionDate"]),
                OperatorId = Convert.ToInt32(reader["OperatorId"]),
                OperatorName = reader["OperatorName"]?.ToString(),
                EquipmentId = reader["EquipmentId"] != DBNull.Value ? Convert.ToInt32(reader["EquipmentId"]) : (int?)null,
                EquipmentName = reader["EquipmentName"]?.ToString(),
                GenerationId = reader["GenerationId"] != DBNull.Value ? Convert.ToInt32(reader["GenerationId"]) : (int?)null,
                GenerationName = reader["GenerationName"]?.ToString(),
                Status = reader["Status"].ToString(),
                Priority = Convert.ToInt32(reader["Priority"]),
                RejectCategoryId = reader["RejectCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["RejectCategoryId"]) : (int?)null,
                RejectCategoryName = reader["RejectCategoryName"]?.ToString(),
                MeasurementTime = Convert.ToDateTime(reader["MeasurementTime"]),
                ClassificationTime = reader["ClassificationTime"] != DBNull.Value ? Convert.ToDateTime(reader["ClassificationTime"]) : (DateTime?)null
            };
        }

        public Wafer GetBySerialNumber(string serialNumber)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
            SELECT w.*, 
                   u.FirstName + ' ' + u.LastName AS OperatorName,
                   e.EquipmentName,
                   pg.GenerationName,
                   rc.CategoryName AS RejectCategoryName
            FROM [Wafers] w
            LEFT JOIN [Users] u ON w.OperatorId = u.UserId
            LEFT JOIN [Equipment] e ON w.EquipmentId = e.EquipmentId
            LEFT JOIN [ProcessorGenerations] pg ON w.GenerationId = pg.GenerationId
            LEFT JOIN [RejectCategories] rc ON w.RejectCategoryId = rc.CategoryId
            WHERE w.SerialNumber = @SerialNumber";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToWafer(reader);
                        }
                    }
                }
            }
            return null;
        }
    }
}