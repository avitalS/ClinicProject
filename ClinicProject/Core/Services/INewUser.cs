using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Services
{
    public interface INewUser
    {
        public bool SendEmail(string email, string subject, string body);
        public void SendPass(ClientDTO client);
        public void SendWelcome(ClientDTO client);
    }
}
