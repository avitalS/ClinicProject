using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Core.DTOs;
using Core.Entities;

namespace UIToClinicProject.Services
{
    public class VisitService
    {
        private readonly HttpClient _httpClient;
        public VisitService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new
             Uri("https://localhost:7231/api/Visit/");
        }

        public async Task<List<VisitDTO>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<List<VisitDTO>>($"GetAll");
        }
        public async Task<bool> Add(VisitDTO v)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync($"Add", v);

                if (res.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception(await res.Content.ReadAsStringAsync());
                }

                if (res.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new Exception("you are exists");
                }
                return await res.Content.ReadFromJsonAsync<bool>();
            }
            catch (Exception e)
            {

                return false;
            }
        }
        public async Task<bool> Update(VisitDTO v)
        {
            var response = await _httpClient.PostAsJsonAsync("Update", v);
            return (bool.Parse(response.Content.ToString()));
        }
        public async Task<bool> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"id?id={id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<SortedList<DateTime, VisitDTO>> GetFutureVisitsByIdClient(int id)
        {
            return await _httpClient.GetFromJsonAsync<SortedList<DateTime,VisitDTO>>($"GetFutureVisitsByIdClient/{id}");
        }

        public async Task<DateTime?> GetLastVisitToClient(int id)
        {
            try
            {
                // שינוי הטיפול ל-<DateTime?> מאפשר לקבל null מהשרת בצורה בטוחה
                return await _httpClient.GetFromJsonAsync<DateTime?>($"GetLastVisitToClient/{id}");
            }
            catch (Exception ex)
            {
                // במקרה של שגיאת תקשורת, נחזיר null כדי שהתוכנה לא תקרוס ויודפס לוג במידת הצורך
                System.Diagnostics.Debug.WriteLine($"שגיאה בקבלת ביקור אחרון: {ex.Message}");
                return null;
            }
        }

        public async Task<CountVisitToDoctor> GetCountVisitToDoctor(int id)
        {
            return await _httpClient.GetFromJsonAsync<CountVisitToDoctor>($"GetCountVisitToDoctor/{id}");
        }

        public async Task<PriorityCity> GetPriorityCity(DateTime d)
        {
            // 1. המרה לפורמט סטנדרטי (חובה כדי למנוע שגיאת 400/500)
            string formattedDate = d.ToString("yyyy-MM-dd");

            // 2. שימוש במחרוזת המפורמטת בתוך ה-URL
            var response = await _httpClient.GetAsync($"GetPriorityCity?d={formattedDate}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PriorityCity>();
            }

            throw new Exception($"Error: {response.StatusCode}");
        }

        public async Task<List<AppointmentDTO>> GetDetailedAppointments(DateTime d)
        {
            // 1. המרה מפורשת לפורמט שהשרת מצפה לו, כדי למנוע שיבושים של יום וחודש
            string formattedDate = d.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            // 2. שירשור הפרמטר ל-URL בצורה נכונה (כולל סימן שאלה ושם הפרמטר 'd')
            string url = $"GetDetailedAppointments?d={Uri.EscapeDataString(formattedDate)}";

            // 3. ביצוע הקריאה עם ה-URL המעודכן
            return await _httpClient.GetFromJsonAsync<List<AppointmentDTO>>(url);
        }

        public async Task<List<string>> GetDoctorScheduleToday(int id)
        {
            return await _httpClient.GetFromJsonAsync<List<string>>($"GetDoctorScheduleToday/{id}");
        }

        public async Task<List<VisitDTO>> GetVisitByidClient(int id)
        {
            return await _httpClient.GetFromJsonAsync<List<VisitDTO>>($"GetVisitByidClient/{id}");
        }
        public async Task<List<ClinicLoadDTO>> GetClinicLoad()
        {
            return await _httpClient.GetFromJsonAsync<List<ClinicLoadDTO>>($"GetClinicLoad");

        }
    }
}
