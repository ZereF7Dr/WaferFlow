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
    public class GenerationRepository
    {
        public List<ProcessorGeneration> GetAllActive()
        {
            var generations = new List<ProcessorGeneration>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM [ProcessorGenerations] 
                    WHERE IsActive = 1 
                    ORDER BY Priority ASC, ReleaseYear DESC";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        generations.Add(MapToGeneration(reader));
                    }
                }
            }
            return generations;
        }

        public List<ProcessorGeneration> GetAll()
        {
            var generations = new List<ProcessorGeneration>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [ProcessorGenerations] ORDER BY ReleaseYear DESC, Priority ASC";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        generations.Add(MapToGeneration(reader));
                    }
                }
            }
            return generations;
        }

        public ProcessorGeneration GetById(int generationId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [ProcessorGenerations] WHERE GenerationId = @GenerationId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GenerationId", generationId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToGeneration(reader);
                        }
                    }
                }
            }
            return null;
        }

        public int Add(ProcessorGeneration generation)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO [ProcessorGenerations] (
                        GenerationName, Manufacturer, Series, ReleaseYear, 
                        NodeSize, TargetMarket, TDP, IsActive, Priority
                    ) VALUES (
                        @GenerationName, @Manufacturer, @Series, @ReleaseYear,
                        @NodeSize, @TargetMarket, @TDP, @IsActive, @Priority
                    );
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GenerationName", generation.GenerationName);
                    cmd.Parameters.AddWithValue("@Manufacturer", generation.Manufacturer);
                    cmd.Parameters.AddWithValue("@Series", generation.Series);
                    cmd.Parameters.AddWithValue("@ReleaseYear", generation.ReleaseYear);
                    cmd.Parameters.AddWithValue("@NodeSize", generation.NodeSize);
                    cmd.Parameters.AddWithValue("@TargetMarket", generation.TargetMarket);
                    cmd.Parameters.AddWithValue("@TDP", generation.TDP ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", generation.IsActive);
                    cmd.Parameters.AddWithValue("@Priority", generation.Priority);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool Update(ProcessorGeneration generation)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE [ProcessorGenerations] 
                    SET GenerationName = @GenerationName,
                        Manufacturer = @Manufacturer,
                        Series = @Series,
                        ReleaseYear = @ReleaseYear,
                        NodeSize = @NodeSize,
                        TargetMarket = @TargetMarket,
                        TDP = @TDP,
                        IsActive = @IsActive,
                        Priority = @Priority
                    WHERE GenerationId = @GenerationId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GenerationId", generation.GenerationId);
                    cmd.Parameters.AddWithValue("@GenerationName", generation.GenerationName);
                    cmd.Parameters.AddWithValue("@Manufacturer", generation.Manufacturer);
                    cmd.Parameters.AddWithValue("@Series", generation.Series);
                    cmd.Parameters.AddWithValue("@ReleaseYear", generation.ReleaseYear);
                    cmd.Parameters.AddWithValue("@NodeSize", generation.NodeSize);
                    cmd.Parameters.AddWithValue("@TargetMarket", generation.TargetMarket);
                    cmd.Parameters.AddWithValue("@TDP", generation.TDP ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", generation.IsActive);
                    cmd.Parameters.AddWithValue("@Priority", generation.Priority);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int generationId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM [ProcessorGenerations] WHERE GenerationId = @GenerationId";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GenerationId", generationId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private ProcessorGeneration MapToGeneration(SqlDataReader reader)
        {
            return new ProcessorGeneration
            {
                GenerationId = Convert.ToInt32(reader["GenerationId"]),
                GenerationName = reader["GenerationName"].ToString(),
                Manufacturer = reader["Manufacturer"].ToString(),
                Series = reader["Series"].ToString(),
                ReleaseYear = Convert.ToInt32(reader["ReleaseYear"]),
                NodeSize = Convert.ToDecimal(reader["NodeSize"]),
                TargetMarket = reader["TargetMarket"].ToString(),
                TDP = reader["TDP"] != DBNull.Value ? Convert.ToInt32(reader["TDP"]) : (int?)null,
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                Priority = Convert.ToInt32(reader["Priority"])
            };
        }
    }
}
