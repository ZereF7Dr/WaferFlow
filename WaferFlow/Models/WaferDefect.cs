using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Models
{
    public class WaferDefect
    {
        public int DefectId { get; set; }
        public int WaferId { get; set; }
        public int? DefectTypeId { get; set; }
        public string DefectTypeName { get; set; }
        public decimal X_Position { get; set; }
        public decimal Y_Position { get; set; }
        public int Severity { get; set; }// Степень повреждения 1-10
        public int DamageLevel { get; set; }// Уровень серьёзности 1-10

        public string DisplayInfo => $"{DefectTypeName} | Повреждение: {Severity}/10 | X:{X_Position} Y:{Y_Position}";
    }
}
