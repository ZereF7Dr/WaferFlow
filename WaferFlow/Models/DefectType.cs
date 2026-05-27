using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Models
{
    public class DefectType
    {
        public int DefectTypeId { get; set; }
        public string DefectCode { get; set; }
        public string DefectName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}