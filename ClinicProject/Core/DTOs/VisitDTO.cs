using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    
    public class VisitDTO
    {
        public int VisitId { get; set; }
        public DateTime DateAndHour { get; set; }
        public int DoctorId { get; set; }
        public int ClientId { get; set; }
        public int Priority { get; set; }
        public int Duration { get; set; }
    }
}

