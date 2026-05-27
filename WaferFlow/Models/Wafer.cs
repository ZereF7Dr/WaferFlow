using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Models
{
    public class Wafer
    {
        public int WaferId { get; set; }
        public string SerialNumber { get; set; }
        public string BatchNumber { get; set; }

        // Физические параметры
        public decimal Diameter { get; set; }
        public decimal Thickness { get; set; }
        public decimal? Resistivity { get; set; }// Сопротивление (Ом·см)
        public decimal? DislocationDensity { get; set; }
        public decimal? Curvature { get; set; }// Кривизна (мкм)
        public decimal? DopingUniformity { get; set; }// Однородность легирования (%)

        // Производственные данные
        public DateTime ProductionDate { get; set; }
        public int OperatorId { get; set; }
        public string OperatorName { get; set; }
        public int? EquipmentId { get; set; }
        public string EquipmentName { get; set; }

        public int? GenerationId { get; set; }
        public string GenerationName { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public int? RejectCategoryId { get; set; }
        public string RejectCategoryName { get; set; }
        public DateTime MeasurementTime { get; set; }
        public DateTime? ClassificationTime { get; set; }


        public string DisplayInfo => $"{SerialNumber} | {BatchNumber} | Приор: {PriorityText}";

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "Measured": return "Измерена";
                    case "Classified": return "Классифицирована";
                    case "Rejected": return "Брак";
                    default: return Status;
                }
            }
        }

        public string PriorityText
        {
            get
            {
                switch (Priority)
                {
                    case 1: return "Срочный";
                    case 2: return "Обычный";
                    case 3: return "Низкий";
                    default: return "Обычный";
                }
            }
        }
    }
}
