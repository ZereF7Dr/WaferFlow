using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public string DisplayInfo { get; set; }
    }
}