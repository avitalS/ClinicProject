using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;

namespace UIToClinicProject.Services
{
    public class DoctorService
    {
        private readonly HttpClient _httpClient;
        public DoctorService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new
             Uri("https://localhost:7231/api/Doctor/");
        }

        public async Task<List<DoctorDTO>> GetAll(int page, int pageSize)
        {
            return await _httpClient.GetFromJsonAsync<List<DoctorDTO>>($"GetAll/{page}/{pageSize}");
        }
        public async Task<DoctorDTO> GetById(int id)
        {
            // שליחת הבקשה לשרת
            var response = await _httpClient.GetAsync($"GetById/{id}");

            // בדיקה אם השרת החזיר תשובה מוצלחת (200 OK)
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DoctorDTO>();
            }

            // טיפול בשגיאת 404 - הנתון לא נמצא בשרת
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // כאן ניתן להחזיר null או לזרוק שגיאה מותאמת אישית
                return null;
            }

            // טיפול בשגיאות אחרות (כמו שגיאת שרת 500)
            throw new Exception($"Error fetching client: {response.ReasonPhrase}");
        }

        
        public async Task<List<DoctorDTO>> GetDoctorsbySpecialization(string spec)
        {
            return await _httpClient.GetFromJsonAsync<List<DoctorDTO>>($"GetDoctorsbySpecialization/{spec}");
        }
        
        public async Task<List<DoctorDTO>> GetDoctorsByCity(string city)
        {
            return await _httpClient.GetFromJsonAsync<List<DoctorDTO>>($"GetDoctorsByCity/{city}");
        }

        public async Task<int> GetDoctorBonus(int id,int amount)
        {
            return await _httpClient.GetFromJsonAsync<int>($"GetDoctorBonus/{id}/{amount}");
        }
        public async Task<bool> ExportToFile(string savePath)
        {
            try
            {
                // תיקון נתיב ה-URL: פנייה ל-AdminController כפי שהוגדר בשרת
                // במידה וה-BaseAddress של ה-HttpClient שלך כבר מכיל את הקידומת api/, רשמי רק: "Admin/ExportAvgHours"
                var response = await _httpClient.GetAsync("ExportAvgHours", HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                // בדיקה האם חזר JSON (טקסט "false") במקום קובץ בינארי
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType != null && contentType.Contains("json"))
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (responseString.Trim().ToLower() == "false")
                    {
                        return false;
                    }
                }

                // כתיבת הסטרים לקובץ המקומי
                using (var streamToRead = await response.Content.ReadAsStreamAsync())
                {
                    using (var streamToWrite = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await streamToRead.CopyToAsync(streamToWrite);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"נכשלה הורדת הקובץ או כתיבתו לדיסק. פרטים: {ex.Message}", ex);
            }
        }
    }
}
