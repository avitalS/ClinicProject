using Core.DTOs;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("GetAll/{page}/{pageSize}")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            return Ok(await _doctorService.GetAll(page,pageSize));
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            DoctorDTO doctor = await _doctorService.GetById(id);
            if (doctor == null)
                return NotFound();
            return Ok(doctor);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] DoctorDTO doctorDto)
        {
            if (doctorDto.FirstName.Length < 2 || doctorDto.LastName.Length < 2)
                return BadRequest("invalid name");
            if (await _doctorService.GetById(doctorDto.DoctorId) != null)
                return Conflict();
            bool result = await _doctorService.Add(doctorDto);
            if (!result)
                return BadRequest("Could not add doctor.");
            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] DoctorDTO doctorDto)
        {
            bool result = await _doctorService.Update(doctorDto);
            if (!result)
                return NotFound("Doctor not found or update failed.");
            return Ok(true);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _doctorService.GetById(id) == null)
                return NotFound();
            bool result = await _doctorService.Delete(id);
            if (!result)
                return NotFound("Doctor not found.");
            return Ok(result);
        }

        [HttpGet("GetDoctorsbySpecialization/{spec}")]
        public async Task<IActionResult> GetDoctorsbySpecialization(string spec)
        {
            if (string.IsNullOrEmpty(spec))
                return BadRequest("invalid Specialization");
            return Ok(await _doctorService.GetDoctorsbySpecialization(spec));
        }

        [HttpGet("GetDoctorsByCity/{city}")]
        public async Task<IActionResult> GetDoctorsByCity(string city)
        {
            if (string.IsNullOrEmpty(city))
                return BadRequest("invalid city");
            return Ok(await _doctorService.GetDoctorsByCity(city));
        }

        [HttpGet("ExportAvgHours")]
        public async Task<IActionResult> ExportToFile()
        {
            // יצירת נתיב זמני בשרת שבו האקסל ייווצר לרגע
            var tempPath = Path.Combine(Path.GetTempPath(), $"Report_{Guid.NewGuid()}.xlsx");

            try
            {
                // קריאה לפונקציה הקיימת שלך שכותבת את הקובץ לנתיב שנתנו לה
                bool success = await _doctorService.ExportAvgHoursToExcel(tempPath);

                if (!success || !System.IO.File.Exists(tempPath))
                {
                    return BadRequest("השרת לא הצליח להפיק את הנתונים לדו\"ח.");
                }

                // קריאת הקובץ מהנתיב הזמני לתוך מערך של בייטים (Bytes)
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);

                // הגדרת סוג הקובץ (Content-Type) עבור אקסל
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // החזרת הקובץ ישירות לצד הלקוח (WPF)
                return File(fileBytes, contentType, "Doctor_Avg_Hours.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בהפקת הדו\"ח: {ex.Message}");
            }
            finally
            {
                // מחיקת הקובץ הזמני מהשרת
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }
            }
        }

        [HttpGet("GetDoctorBonus/{id}/{amount}")]
        public async Task<IActionResult> GetDoctorBonus(int id, int amount)
        {
            int finalBonus = await _doctorService.GetDoctorBonus(id, amount, _doctorService.CalculateBonusLogic);
            return Ok(finalBonus);
        }
    }
 }

