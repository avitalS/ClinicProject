using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Services
{
    public interface IClientService
    {
        static event Action<ClientDTO> OnClientCreated;
        Task<List<ClientDTO>> GetAll();
        Task<ClientDTO> GetById(int id);
        Task<bool> Add(ClientDTO clientDto);
        Task<bool> Update(ClientDTO clientDto);
        Task<bool> Delete(int id);
        Task<int> GetAge(int idc);
        Task<bool> ImportClientsFromExcel(string filePath);
    }
}
