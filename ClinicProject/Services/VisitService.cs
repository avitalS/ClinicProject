using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Repositories;
using Core.Services;
using Data.Repositories;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Services
{
    public class VisitService:IVisitService
    {
        private readonly IVisitRepository _visitRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public VisitService(IVisitRepository repository,
            IClientRepository clientRepository
            , IMapper mapper)
        {
            _visitRepository = repository;
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<List<VisitDTO>> GetAll()
        {
            var visits = await _visitRepository.GetAll();
            return _mapper.Map<List<VisitDTO>>(visits);
        }

        public async Task<VisitDTO> GetById(int id)
        {
            var visit = await _visitRepository.GetById(id);
            return _mapper.Map<VisitDTO>(visit);
        }

        public async Task<bool> Add(VisitDTO visitDto)
        {
            var allVisits = await _visitRepository.GetAll();
            var doctorVisits = allVisits.Where(v => v.DoctorId == visitDto.DoctorId &&
                                                    v.DateAndHour.Date == visitDto.DateAndHour.Date).ToList();

            // 2. חישוב טווח הזמן של התור החדש
            DateTime newStart = visitDto.DateAndHour;
            DateTime newEnd = newStart.AddMinutes(visitDto.Duration); // ה-Duration מגיע מה-WPF

            // 3. בדיקת חפיפה מול תורים קיימים
            foreach (var existingVisit in doctorVisits)
            {
                DateTime existingStart = existingVisit.DateAndHour;
                // שימוש ב-Duration הקיים במסד, או ברירת מחדל של 10 דקות אם השדה ריק
                int existingDuration = existingVisit.Duration > 0 ? existingVisit.Duration : 10;
                DateTime existingEnd = existingStart.AddMinutes(existingDuration);

                // לוגיקת חפיפה: (התחלה א' לפני סיום ב') וגם (התחלה ב' לפני סיום א')
                if (newStart < existingEnd && existingStart < newEnd)
                {
                    // קיימת התנגשות בזמנים!
                    return false;
                }
            }

            // אם אין התנגשות, שומרים את התור
            Visit visit = _mapper.Map<Visit>(visitDto);
            return await _visitRepository.Add(visit);
        }
        

        public async Task<bool> Update(VisitDTO visitDto)
        {
            var visit = _mapper.Map<Visit>(visitDto);
            return await _visitRepository.Update(visit);
        }

        public async Task<bool> Delete(int id)
        {
            return await _visitRepository.Delete(id);
        }


        public async Task<SortedList<DateTime, VisitDTO>> GetFutureVisitsByIdClient(int idc)
        {
            SortedList<DateTime, VisitDTO> sl = new SortedList<DateTime, VisitDTO>();
            List<Visit> vl = await _visitRepository.GetAll();
            List<Visit> futureVisits = vl.FindAll(v => v.ClientId == idc && v.DateAndHour > DateTime.Now).ToList();
            foreach (Visit v in futureVisits)
            {
                sl.Add(v.DateAndHour, _mapper.Map<VisitDTO>(v));
            }
            return sl;
        }

        public async Task<DateTime?> GetLastVisitToClient(int idc)
        {
            return await _visitRepository.GetLastVisitToClient(idc);
        }

        public async Task<CountVisitToDoctor> GetCountVisitToDoctor(int idc)
        {
            return await _visitRepository.GetCountVisitToDoctor(idc);
        }

        public async Task<PriorityCity> GetPriorityCity(DateTime date)
        {
            return await _visitRepository.GetPriorityCity(date);
        }

        //LINQ?
        public async Task<List<AppointmentDTO>> GetDetailedAppointments(DateTime d)
        {
            List<Visit>  visits = await _visitRepository.GetAllWithDetails();

            return visits.FindAll(x=>x.DateAndHour.Date==d).Select(v => new AppointmentDTO
            {
                PatientName = v.Client.FullName,
                DoctorName = v.Doctor.FirstName+" "+v.Doctor.LastName,
                Specialization = v.Doctor.Specialization,
                Time = v.DateAndHour
            }).ToList();
        }

        //group by
        //סך ביקורים כללי לכל התמחות
        public async Task<List<ClinicLoadDTO>> GetClinicLoad()
        {
            List<Visit> lv = await _visitRepository.GetAllWithDetails();

            return lv.GroupBy(v => v.Doctor.Specialization)
                     .Select(group => new ClinicLoadDTO
                     {
                         Specialty = group.Key,
                         Count = group.Count()
                     })
                     .ToList();
        }

        //join
        public async Task<List<string>> GetDoctorScheduleToday(int doctorId)
        {
            List<Visit> visits =await _visitRepository.GetAll();
            List<Client> clients = await _clientRepository.GetAll();

            var query = from v in visits
                        join c in clients on v.ClientId equals c.ClientId
                        where v.DoctorId == doctorId 
                        && v.DateAndHour.Date == DateTime.Today 
                        select "המטופל: " + c.FullName + " " +  " בשעה: " + v.DateAndHour.ToShortTimeString();

            return  query.ToList();
        }

        public async Task<List<VisitDTO>> GetVisitByidClient(int id)
        {
            List<Visit> vl = await _visitRepository.GetAll();
            return  _mapper.Map<List<VisitDTO>>(vl.FindAll(x=>x.ClientId==id));
        }
    }
}
