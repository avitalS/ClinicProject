using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace Core.Repositories
{
    public interface IVisitRepository
    {
        Task<List<Visit>> GetAll();
        Task<List<Visit>> GetAllWithDetails();
        Task<Visit> GetById(int id);
        Task<bool> Add(Visit visit);
        Task<bool> Update(Visit visit);
        Task<bool> Delete(int id);
        Task<DateTime?> GetLastVisitToClient(int idc);
        Task<CountVisitToDoctor> GetCountVisitToDoctor(int id);
        Task<PriorityCity> GetPriorityCity(DateTime date);

    }
}
