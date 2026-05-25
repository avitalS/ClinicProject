using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace Core.Services
{
    public delegate int SalaryBonusCalculator(int years, int multiplier);
    public interface IDoctorService
    {
        Task<List<DoctorDTO>> GetAll(int page, int pageSize);
        Task<DoctorDTO> GetById(int id);
        Task<bool> Add(DoctorDTO doctorDto);
        Task<bool> Update(DoctorDTO doctorDto);
        Task<bool> Delete(int id);
        Task<List<DoctorDTO>> GetDoctorsbySpecialization(string spec);
        Task<List<DoctorDTO>> GetDoctorsByCity(string city);
        Task<bool> ExportAvgHoursToExcel(string path);
        Task<int> GetDoctorBonus(int id, int multiplier, SalaryBonusCalculator calc);
        int CalculateBonusLogic(int years, int multiplier);

    }
}
