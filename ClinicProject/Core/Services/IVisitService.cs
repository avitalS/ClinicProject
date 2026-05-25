using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace Core.Services
{
    public interface IVisitService
    {
        Task<List<VisitDTO>> GetAll();
        Task<VisitDTO> GetById(int id);
        Task<bool> Add(VisitDTO visitDto);
        Task<bool> Update(VisitDTO visitDto);
        Task<bool> Delete(int id);
        Task<SortedList<DateTime, VisitDTO>> GetFutureVisitsByIdClient(int idc);
        Task<DateTime?> GetLastVisitToClient(int idc);
        Task<CountVisitToDoctor> GetCountVisitToDoctor(int id);
        Task<PriorityCity> GetPriorityCity(DateTime date);
        Task<List<ClinicLoadDTO>>GetClinicLoad();
        Task<List<AppointmentDTO>> GetDetailedAppointments(DateTime d);
        Task<List<string>> GetDoctorScheduleToday(int doctorId);
        Task<List<VisitDTO>> GetVisitByidClient(int id);

    }
}
