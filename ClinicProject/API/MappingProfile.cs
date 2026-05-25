using Core.DTOs;
using Core.Entities;
using AutoMapper;

namespace API
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Client, ClientDTO>();
            CreateMap<ClientDTO, Client>();
            CreateMap<Doctor, DoctorDTO>();
            CreateMap<DoctorDTO, Doctor>(); 
            CreateMap<Visit, VisitDTO>(); 
            CreateMap<VisitDTO, Visit>(); 
        }
    }
}
