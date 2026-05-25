using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
        public class ClientDTO
        {
            public int ClientId { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string City { get; set; }
            public string Email { get; set; }
            public string Pass { get; set; }
            public DateTime DateOfBirth { get; set; }
        }
}

