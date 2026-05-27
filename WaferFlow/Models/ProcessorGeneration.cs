using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Models
{
    public class ProcessorGeneration
    {
        public int GenerationId { get; set; }
        public string GenerationName { get; set; }
        public string Manufacturer { get; set; }
        public string Series { get; set; }
        public int ReleaseYear { get; set; }
        public decimal NodeSize { get; set; }// Техпроцесс
        public string TargetMarket { get; set; }
        public int? TDP { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }

        public string DisplayName => $"{GenerationName} ({NodeSize}нм)";

        public int Rating { get; set; }

        public string DisplayNameWithRating
        {
            get
            {
                if (Rating > 0)
                    return $"{GenerationName} | {Rating}%";
                return DisplayName;
            }
        }

        public string DisplayInfo { get; set; }
    }
}
