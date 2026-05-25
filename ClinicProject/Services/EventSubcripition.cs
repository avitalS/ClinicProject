using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Services;

namespace Services
{
    public class EventSubcripition
    {
        public EventSubcripition(IClientService clientService,INewUser newUser)
        {
            ClientService.OnClientCreated+=newUser.SendPass;
            ClientService.OnClientCreated+=newUser.SendWelcome;
        }
        
    }
}
