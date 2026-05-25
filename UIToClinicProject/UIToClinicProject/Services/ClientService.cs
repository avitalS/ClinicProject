using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;

namespace UIToClinicProject.Services
{
    public class ClientService
    {
        private readonly HttpClient _httpClient;
        public ClientService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new
             Uri("https://localhost:7231/api/Client/");
        }

        public async Task<List<ClientDTO>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<List<ClientDTO>>($"GetAll");
        }

        public async Task<ClientDTO> GetById(int id)
        {
            // שליחת הבקשה לשרת
            var response = await _httpClient.GetAsync($"GetById/{id}");

            // בדיקה אם השרת החזיר תשובה מוצלחת (200 OK)
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ClientDTO>();
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
       
        public async Task<bool> Update(ClientDTO c)
        {
            // שליחת הנתונים לשרת
            var response = await _httpClient.PutAsJsonAsync("Update", c);
            // במקום bool.Parse, פשוט נבדוק אם השרת החזיר הצלחה (קוד 200)
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> Delete(int id)
        {
            try
            {
                // 1. שליחת בקשת DELETE מתאימה לשרת עם ה-id בתוך ה-URL
                // שים לב: המבנה הוא בדיוק כמו ב-Controller: "api/Client/Delete/5"
                HttpResponseMessage response = await _httpClient.DeleteAsync($"Delete/{id}");

                // 2. בדיקה האם השרת החזיר קוד הצלחה (סטטוס 200-299)
                if (response.IsSuccessStatusCode)
                {
                    // קריאת ה-body שחזר מהשרת (במקרה שלך השרת מחזיר true/false בתוך Ok(result))
                    string contentString = await response.Content.ReadAsStringAsync();

                    if (bool.TryParse(contentString, out bool isDeleted))
                    {
                        return isDeleted;
                    }
                    return true; // אם הסטטוס הצליח אך הקידוד שונה
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // טיפול ייעודי במקרה שהלקוח לא נמצא (למשל נמחק כבר על ידי משתמש אחר)
                    throw new Exception("הלקוח לא נמצא במערכת (ייתכן ונמחק כבר).");
                }
                else
                {
                    // טיפול בכל קוד שגיאה אחר (כמו שגיאה 500)
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"השרת החזיר שגיאה: {response.StatusCode}. פרטים: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                // תפיסת שגיאות רשת/תקשורת פיזית מול השרת
                throw new Exception("שגיאת תקשורת: לא ניתן לגשת לשרת. ודא שה-API פועל.", httpEx);
            }
            catch (Exception ex)
            {
                // תפיסת כל שגיאה כללית אחרת והעברתה הלאה ל-UI
                throw new Exception($"נכשלה פעולת המחיקה: {ex.Message}", ex);
            }
        }
        public async Task<bool> Add(ClientDTO c)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync($"Add", c);

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

        public async Task<int> GetAge(int id)
        {
            return await _httpClient.GetFromJsonAsync<int>($"GetAge/{id}");
        }
        public async Task<bool> ImportFromExcel(string filePath)
        {
            // 1. יצירת אובייקט שמדמה שליחת טופס עם קובץ (כמו ב-Swagger)
            using (var content = new MultipartFormDataContent())
            {
                // 2. פתיחת הקובץ שנבחר ב-WPF לקריאה
                var fileStream = System.IO.File.OpenRead(filePath);
                var streamContent = new StreamContent(fileStream);

                // הגדרת סוג התוכן של אקסל
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                // 3. הוספת הקובץ לטופס - השם "file" חייב להיות תואם בדיוק לשם הפרמטר ב-Controller (IFormFile file)
                content.Add(streamContent, "file", System.IO.Path.GetFileName(filePath));

                // 4. שליחת בקשת ה-POST לנתיב המעודכן בשרת
                // ודאי שכתובת ה-BaseAddress של ה-HttpClient שלך מוגדרת נכון (למשל פונה ל-api/Client/)
                var response = await _httpClient.PostAsync("Import", content);

                // 5. בדיקה האם השרת החזיר תשובה חיובית (True)
                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    return bool.TryParse(resultString, out bool isSuccess) && isSuccess;
                }

                return false;
            }
        }
    }
    }

    
