using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WaferFlow.Database.Connection;
using WaferFlow.Models;

namespace WaferFlow.Database.Repositories
{
    public class ClassificationHistoryRepository
    {
        public void Add(ClassificationHistory history)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO [ClassificationHistory] (
                        WaferId, EngineerId, Decision, AssignedGenerationId, 
                        RejectCategoryId, Reason, DecisionTime
                    ) VALUES (
                        @WaferId, @EngineerId, @Decision, @AssignedGenerationId,
                        @RejectCategoryId, @Reason, @DecisionTime
                    )";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", history.WaferId);
                    cmd.Parameters.AddWithValue("@EngineerId", history.EngineerId);
                    cmd.Parameters.AddWithValue("@Decision", history.Decision);
                    cmd.Parameters.AddWithValue("@AssignedGenerationId", history.AssignedGenerationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RejectCategoryId", history.RejectCategoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reason", history.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DecisionTime", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ClassificationHistory> GetAll()
        {
            var historyList = new List<ClassificationHistory>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [ClassificationHistory] ORDER BY DecisionTime DESC";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historyList.Add(new ClassificationHistory
                        {
                            HistoryId = Convert.ToInt32(reader["HistoryId"]),
                            WaferId = Convert.ToInt32(reader["WaferId"]),
                            EngineerId = Convert.ToInt32(reader["EngineerId"]),
                            Decision = reader["Decision"].ToString(),
                            AssignedGenerationId = reader["AssignedGenerationId"] != DBNull.Value ? Convert.ToInt32(reader["AssignedGenerationId"]) : (int?)null,
                            RejectCategoryId = reader["RejectCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["RejectCategoryId"]) : (int?)null,
                            Reason = reader["Reason"]?.ToString(),
                            DecisionTime = Convert.ToDateTime(reader["DecisionTime"])
                        });
                    }
                }
            }
            return historyList;
        }

        public List<RejectCategory> GetRejectCategories()
        {
            var categories = new List<RejectCategory>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM [RejectCategories]";

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new RejectCategory
                        {
                            CategoryId = Convert.ToInt32(reader["CategoryId"]),
                            CategoryName = reader["CategoryName"].ToString(),
                            Description = reader["Description"]?.ToString(),
                            IsRepairable = Convert.ToBoolean(reader["IsRepairable"])
                        });
                    }
                }
            }
            return categories;
        }

        public ClassificationHistory GetByWaferId(int waferId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string query = @"
            SELECT TOP 1 * FROM [ClassificationHistory] 
            WHERE WaferId = @WaferId 
            ORDER BY DecisionTime DESC";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@WaferId", waferId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ClassificationHistory
                            {
                                HistoryId = Convert.ToInt32(reader["HistoryId"]),
                                WaferId = Convert.ToInt32(reader["WaferId"]),
                                EngineerId = Convert.ToInt32(reader["EngineerId"]),
                                Decision = reader["Decision"].ToString(),
                                AssignedGenerationId = reader["AssignedGenerationId"] != DBNull.Value ? Convert.ToInt32(reader["AssignedGenerationId"]) : (int?)null,
                                RejectCategoryId = reader["RejectCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["RejectCategoryId"]) : (int?)null,
                                Reason = reader["Reason"]?.ToString(),
                                DecisionTime = Convert.ToDateTime(reader["DecisionTime"])
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}