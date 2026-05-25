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

    public class ClientService:IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        public static event Action<ClientDTO> OnClientCreated;

        public ClientService(IClientRepository repository, IMapper mapper)
        {
            _clientRepository = repository;
            _mapper = mapper;
        }

        public async Task<List<ClientDTO>> GetAll()
        {
            var clients = await _clientRepository.GetAll();
            return _mapper.Map<List<ClientDTO>>(clients);
        }

        public async Task<ClientDTO> GetById(int id)
        {
            var client = await _clientRepository.GetById(id);
            return _mapper.Map<ClientDTO>(client);
        }

        public async Task<bool> Add(ClientDTO clientDto)
        { 
            var client = _mapper.Map<Client>(clientDto);
            OnClientCreated?.Invoke(clientDto);
            return await _clientRepository.Add(client);
        }

        public async Task<bool> Update(ClientDTO clientDto)
        {
            var client = _mapper.Map<Client>(clientDto);
            return await _clientRepository.Update(client);
        }

        public async Task<bool> Delete(int id)
        {
            return await _clientRepository.Delete(id);
        }

        public async Task<int> GetAge(int idc)
        {
            return await _clientRepository.GetAge(idc);
        }

        public async Task<bool> ImportClientsFromExcel(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var client = new Client
                        {
                            // עמודה A - תאריך לידה
                            DateOfBirth = row.Cell(1).GetValue<DateTime>(),

                            // עמודה B - סיסמה
                            Pass = row.Cell(2).GetValue<string>(),

                            // עמודה C - אימייל
                            Email = row.Cell(3).GetValue<string>(),

                            // עמודה D - עיר
                            City = row.Cell(4).GetValue<string>(),

                            // עמודה E - טלפון
                            Phone = row.Cell(5).GetValue<string>(),

                            // עמודה F - שם מלא
                            FullName = row.Cell(6).GetValue<string>(),

                            // עמודה G - תעודת זהות
                            ClientId = row.Cell(7).GetValue<int>()
                        };

                        await _clientRepository.Add(client);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"קריסה פנימית בלוגיקת הייבוא. פרטי השגיאה: {ex.Message}", ex);
            }
        }

    }
}
    