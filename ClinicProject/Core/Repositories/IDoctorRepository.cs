using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace Core.Repositories
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAll(int page, int pageSize);
        Task<Doctor> GetById(int id);
        Task<bool> Add(Doctor doctor);
        Task<bool> Update(Doctor doctor);
        Task<bool> Delete(int id);
        Task<List<Doctor>> GetDoctorsbySpecialization(string spec);
        Task<List<Doctor>> GetDoctorsByCity(string city);
        Task<List<AvgHourToDoctorInMonth>> GetAvgHourToDoctorInMonth();
    }
}
