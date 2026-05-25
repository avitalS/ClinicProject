using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Core.DTOs;
using Core.Entities;
using Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class VisitRepository:IVisitRepository
    {
        private readonly DataContext _context;
        public VisitRepository(DataContext dataContext)
        {
            _context = dataContext;
        }
        public async Task<List<Visit>> GetAll()
        {
           return await _context.Visits.ToListAsync();
        }

        public async Task<List<Visit>> GetAllWithDetails()
        {
            return await _context.Visits.Include(x=>x.Doctor).Include(c=>c.Client).ToListAsync();
        }
        public async Task<Visit> GetById(int id)
        {
            return await _context.Visits.FindAsync(id);
        }

        public async Task<bool> Add(Visit visit)
        {
            try
            {
                await _context.Visits.AddAsync(visit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public async Task<bool> Update(Visit visit)
        {
            try
            {
                Visit v = await _context.Visits.FindAsync(visit.VisitId);
                v.DateAndHour = visit.DateAndHour;
                v.DoctorId = visit.DoctorId;
                v.ClientId = visit.ClientId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Visit v = await _context.Visits.FindAsync(id);
                if (v == null) return false;
                _context.Visits.Remove(v);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<DateTime?> GetLastVisitToClient(int idc)
        {
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT dbo.GetLastVisitToClient(@id)";

            var param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.Value = idc;
            cmd.Parameters.Add(param);

            var res = await cmd.ExecuteScalarAsync();

            if (res == null || res == DBNull.Value)
            {
                return null; 
            }

            return (DateTime)res;
        }
        public async Task<CountVisitToDoctor> GetCountVisitToDoctor(int id)
        {
            var result = await _context.Set<CountVisitToDoctor>()
                .FromSqlRaw("EXEC GetCountVisitToDoctor @id = @id", new SqlParameter("@id", id))
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task<PriorityCity> GetPriorityCity(DateTime date)
        {
            var dateParam = new SqlParameter("@date", date);
            var countParam = new SqlParameter { ParameterName = "@cnt", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };
            var cityParam = new SqlParameter { ParameterName = "@city", SqlDbType = SqlDbType.NVarChar, Size = 50, Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync("EXEC GetPriorityCity @date, @cnt OUTPUT, @city OUTPUT",
                dateParam, countParam, cityParam);

            return new PriorityCity
            {
                Count = countParam.Value == DBNull.Value ? 0 : (int)countParam.Value,

                City = cityParam.Value == DBNull.Value ? string.Empty : cityParam.Value.ToString()
            };
        }
    }
}
