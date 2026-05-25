using Core.DTOs;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _clientService.GetAll());
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            ClientDTO client = await _clientService.GetById(id);
            if (client == null)
                return NotFound();
            return Ok(client);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] ClientDTO clientDto)
        {
            if (await _clientService.GetById(clientDto.ClientId) != null)
                return Conflict();
            if (clientDto.DateOfBirth > DateTime.Now)
                return BadRequest("invalid date");
            if (clientDto.Phone.Length != 10)
                return BadRequest("invalid phone");
            bool result = await _clientService.Add(clientDto);
            if (!result)
                return BadRequest("Could not add client.");
            return Ok(true);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] ClientDTO clientDto)
        {
            bool result = await _clientService.Update(clientDto);
            if (!result)
                return NotFound("Client not found or update failed.");
            return Ok(result);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _clientService.GetById(id) == null)
                return NotFound();
            bool result = await _clientService.Delete(id);
            if (!result)
                return NotFound("Client not found.");
            return Ok(result);
        }

        [HttpGet("GetAge/{id}")]
        public async Task<IActionResult> GetAge(int id)
        {
            if (await _clientService.GetById(id) == null)
                return NotFound();
            return Ok(await _clientService.GetAge(id));
        }

        [HttpPost("Import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("לא התקבל קובץ.");

            // יצירת נתיב זמני ייחודי עם סיומת .xlsx במקום .tmp
            var uniqueFileName = $"{Guid.NewGuid()}.xlsx";
            var tempPath = Path.Combine(Path.GetTempPath(), uniqueFileName);

            try
            {
                // שמירת הקובץ שהועלה אל הנתיב הזמני עם הסיומת התקנית
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // קריאה לפונקציית הלוגיקה שלך שכעת תזהה את הסיומת בהצלחה
                bool isSuccess = await _clientService.ImportClientsFromExcel(tempPath);

                if (isSuccess)
                    return Ok(true);

                return BadRequest("הייבוא נכשל בלוגיקה הפנימית.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"קריסה פנימית: {ex.Message}");
            }
            finally
            {
                // מחיקת הקובץ הזמני מהשרת בסיום
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }
            }
        }


    }

}
