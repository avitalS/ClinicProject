using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class AppointmentDTO
    {
        public string PatientName { get; set; } 
        public string DoctorName { get; set; }  
        public string Specialization { get; set; } 
        public DateTime Time { get; set; }
    }
}
