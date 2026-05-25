using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace Core.Repositories
{
    public interface IClientRepository
    {
        Task<List<Client>> GetAll();
        Task<List<Client>> GetClientsWithUrgentVisits();
        Task<Client> GetById(int id);
        Task<bool> Add(Client client);
        Task<bool> Update(Client client);
        Task<bool> Delete(int id);
        Task<int> GetAge(int idc);
    }
}
