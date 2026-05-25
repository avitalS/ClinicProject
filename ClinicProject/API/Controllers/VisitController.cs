using Core.DTOs;
using Core.Entities;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //created,badrequest,notfound
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;
        private readonly IClientService _clientService;
        private readonly IDoctorService _doctorService;

        public VisitController(IVisitService visitService,
            IClientService clientService,
            IDoctorService doctorService)
        {
            _visitService = visitService;
            _clientService = clientService;
            _doctorService = doctorService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result =await _visitService.GetAll();
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            VisitDTO visit = await _visitService.GetById(id);
            if (visit == null)
                return NotFound();
            return Ok(visit);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] VisitDTO visitDto)
        {
            if (await _clientService.GetById(visitDto.ClientId) == null)
                return NotFound("client doesnt exist");
            if (await _doctorService.GetById(visitDto.DoctorId) == null)
                return NotFound("doctorid doesnt exist");
            if (visitDto.DateAndHour.Date < DateTime.Today)
                return BadRequest("can not add visit for past days");
            bool result = await _visitService.Add(visitDto);
            if (!result)
                return BadRequest("Could not add visit.");
            return Ok(result);

        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] VisitDTO visitDto)
        {
            bool result = await _visitService.Update(visitDto);
            if (result)
                return Ok(true);
            return NotFound("Visit not found or update failed.");
        }

        [HttpDelete("Delete{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _visitService.GetById(id) == null)
                return NotFound("Visit not found.");
            bool result = await _visitService.Delete(id);
            if (!result)
                return BadRequest("deletete faild");
            return Ok(result);
        }

        [HttpGet("GetFutureVisitsByIdClient/{id}")]
        public async Task<IActionResult> GetFutureVisitsByIdClient(int id)
        {
            if (id < 0 || id.ToString().Length < 9)
                return BadRequest("invalid id");
            if (await _clientService.GetById(id) == null)
                return NotFound();
            var list = await _visitService.GetFutureVisitsByIdClient(id);
            return Ok(list); // List<VisitDTO> ישירות
        }

        [HttpGet("GetLastVisitToClient/{id}")]
        public async Task<IActionResult> GetLastVisitToClient(int id)
        {
            if(await _clientService.GetById(id) == null)
                return NotFound("there is no this client");
            return Ok(await _visitService.GetLastVisitToClient(id));
        }

        [HttpGet("GetCountVisitToDoctor/{id}")]
        public async Task<IActionResult> GetCountVisitToDoctor(int id)
        {
            if (await _doctorService.GetById(id) == null)
                return NotFound("there is no this client");
            return Ok(await _visitService.GetCountVisitToDoctor(id));
        }

        [HttpGet("GetPriorityCity")]
        public async Task<IActionResult> GetPriorityCity(DateTime d)
        {
            return Ok(await _visitService.GetPriorityCity(d));
        }

        [HttpGet("GetClinicLoad")]
        public async Task<IActionResult> GetClinicLoad()
        {
            return Ok(await _visitService.GetClinicLoad());
        }

        [HttpGet("GetDetailedAppointments")]
        public async Task<IActionResult> GetDetailedAppointments(DateTime d)
        {
            return Ok(await _visitService.GetDetailedAppointments(d));
        }

        [HttpGet("GetDoctorScheduleToday/{id}")]
        public async Task<IActionResult> GetDoctorScheduleToday(int id)
        {
            if (await _doctorService.GetById(id) == null)
                return NotFound();
            return Ok(await _visitService.GetDoctorScheduleToday(id));
        }

        [HttpGet("GetVisitByidClient/{id}")]
        public async Task<IActionResult> GetVisitByidClient(int id)
        {
            if (await _clientService.GetById(id) == null)
                return NotFound();
            return Ok(await _visitService.GetVisitByidClient(id));
        }

    }
}