using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class ClientRepository:IClientRepository
    {
        private readonly DataContext _context;
        public ClientRepository(DataContext dataContext)
        {
            _context = dataContext;
        }
        public async Task<List<Client>> GetAll()
        {
            return await _context.Clients.ToListAsync();
        }

        public async Task<List<Client>> GetClientsWithUrgentVisits()
        {
            return await _context.Clients.
                   Include(x=>x.Visits.Where(v=>v.Priority==3)).ToListAsync();
        }

        public async Task<Client> GetById(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<bool> Add(Client client)
        {
            try
            {
                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> Update(Client client)
        {
            try
            {
                Client c = await _context.Clients.FindAsync(client.ClientId);
                c.FullName = client.FullName;
                c.Phone = client.Phone;
                c.City = client.City;
                c.DateOfBirth = client.DateOfBirth;
                c.Pass = client.Pass;
                c.Email = client.Email;
                //entry
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Client c = await _context.Clients.FindAsync(id);
                _context.Clients.Remove(c);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }
        public async Task<int> GetAge(int idc)
        {
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT dbo.GetAge(@id)";
            var param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.Value = idc;
            cmd.Parameters.Add(param);

            var res = await cmd.ExecuteScalarAsync();
            return res == null ? 0 : (int)(res);
        }
        
    }
}
