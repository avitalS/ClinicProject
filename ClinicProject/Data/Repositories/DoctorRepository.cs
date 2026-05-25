using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class DoctorRepository:IDoctorRepository
    {
        private readonly DataContext _context;
        public DoctorRepository(DataContext dataContext)
        {
            _context = dataContext;
        }
       

        public async Task<List<Doctor>> GetAll(int page, int pageSize )
        {
            int cnt = _context.Doctors.Count();
            if (cnt < (page - 1) * pageSize)
                return await _context.Doctors.ToListAsync();
            var doctors = await _context.Doctors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return doctors;
        }

        public async Task<Doctor> GetById(int id)
        {
            return await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == id);
        }

        public async Task<bool> Add(Doctor doctor)
        {
            try
            {
                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> Update(Doctor doctor)
        {
            try
            {
                Doctor d = await _context.Doctors.FindAsync(doctor.DoctorId);
                if (d == null) return false;
                d.FirstName = doctor.FirstName;
                d.LastName = doctor.LastName;
                d.City = doctor.City;
                d.Specialization = doctor.Specialization;
                //Entry?
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Doctor d = await _context.Doctors.FindAsync(id);
                if (d == null) return false;
                _context.Doctors.Remove(d);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<Doctor>> GetDoctorsbySpecialization(string spec)
        {

            return await _context.Doctors
                .FromSqlRaw("SELECT * FROM dbo.GetDoctorsbySpecialization(@spec)",
                    new SqlParameter("@spec", spec))
                .ToListAsync();
        }
        public async Task<List<Doctor>> GetDoctorsByCity(string city)
        {
            return await _context.Doctors
           .FromSqlRaw("EXEC dbo.GetDoctorsByCity @city",
            new SqlParameter("@city", city))
           .ToListAsync();
        }
        public async Task<List<AvgHourToDoctorInMonth>> GetAvgHourToDoctorInMonth()
        {
            return await _context.Set<AvgHourToDoctorInMonth>()
                 .FromSqlRaw("SELECT * FROM AvgHourToDoctorInMonth")
                 .ToListAsync();
        }
       

    }
}
