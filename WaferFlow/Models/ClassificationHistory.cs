using System;

namespace WaferFlow.Models
{
    public class ClassificationHistory
    {
        public int HistoryId { get; set; }
        public int WaferId { get; set; }
        public int EngineerId { get; set; }
        public string Decision { get; set; }
        public int? AssignedGenerationId { get; set; }
        public int? RejectCategoryId { get; set; }
        public string Reason { get; set; }
        public DateTime DecisionTime { get; set; }
    }
}