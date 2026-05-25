using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using ClosedXML.Excel;
using Core.DTOs;
using Core.Entities;
using Core.Repositories;
using Core.Services;
using Data.Repositories;

namespace Services
{
    public delegate int SalaryBonusCalculator(int years, int multiplier);

    public class DoctorService:IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IMapper _mapper;
        public delegate void PrintmessageDel(string message);
        // event PrintmessageDel onSearchCity;


        public DoctorService(IDoctorRepository doctorRepository, IMapper mapper)
        {
            _doctorRepository = doctorRepository;
            _mapper = mapper;
           
        }

        public async Task<List<DoctorDTO>> GetAll(int page , int pageSize)
        {
            var doctors = await _doctorRepository.GetAll(page,pageSize);
            return _mapper.Map<List<DoctorDTO>>(doctors);
        }

        public async Task<DoctorDTO> GetById(int id)
        {
            var doctor = await _doctorRepository.GetById(id);
            return _mapper.Map<DoctorDTO>(doctor);
        }

        public async Task<bool> Add(DoctorDTO doctorDto)
        {
            var doctor = _mapper.Map<Doctor>(doctorDto);
            return await _doctorRepository.Add(doctor);
        }

        public async Task<bool> Update(DoctorDTO doctorDto)
        {
            var doctor = _mapper.Map<Doctor>(doctorDto);
            return await _doctorRepository.Update(doctor);
        }

        public async Task<bool> Delete(int id)
        {
            return await _doctorRepository.Delete(id);
        }

        public async Task<List<DoctorDTO>> GetDoctorsbySpecialization(string spec)
        {
            var doctors = await _doctorRepository.GetDoctorsbySpecialization(spec);
            return  _mapper.Map<List<DoctorDTO>>(doctors);
        }
       
        public async Task<List<DoctorDTO>> GetDoctorsByCity(string city)
        {
            var doctors = await _doctorRepository.GetDoctorsByCity(city);
            return _mapper.Map<List<DoctorDTO>>(doctors);
        }

        public async Task<bool> ExportAvgHoursToExcel(string filePath)
        {
            try
            {
                var data = await _doctorRepository.GetAvgHourToDoctorInMonth();

                if (data == null || data.Count == 0)
                {
                    throw new Exception("No data found in Database to export.");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Reports");
                    worksheet.Cell(1, 1).Value = "Doctor ID";
                    worksheet.Cell(1, 2).Value = "First Name";
                    worksheet.Cell(1, 3).Value = "Last Name";
                    worksheet.Cell(1, 4).Value = "Average Duration";

                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = data[i].DoctorId;
                        worksheet.Cell(i + 2, 2).Value = data[i].FirstName;
                        worksheet.Cell(i + 2, 3).Value = data[i].LastName;
                        worksheet.Cell(i + 2, 4).Value = data[i].AvgDuration;
                    }

                    workbook.SaveAs(filePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Export Error: " + ex.Message);
                return false;
            }
        }


        public int CalculateBonusLogic(int years, int multiplier)
        {
            return years * multiplier;
        }
        //delegate
        public async Task<int> GetDoctorBonus(int id, int multiplier, Core.Services.SalaryBonusCalculator calc)
        {
            Doctor doctor = await _doctorRepository.GetById(id);
            if (doctor == null) 
                return 0;

            return calc(doctor.YearsOfExperience, multiplier);
        }

        
    }
    }

