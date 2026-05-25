using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Core.DTOs;
using UIToClinicProject.Services; 

namespace UIToClinicProject
{
    public partial class Main : Window
    {
        // אתחול ה-Service של הלקוחות (שני את השם במידה והגדרת אותו אחרת אצלך בפרויקט)
        private readonly ClientService clientService = new ClientService();
        private readonly DoctorService doctorService = new DoctorService();

        public Main()
        {
            InitializeComponent();
        }

        // --- לוגיקת כניסה למערכת (מעודכנת ומאובטחת) ---
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            // 1. קריאת המזהה מתיבת הטקסט
            string inputId = txtId.Text.Trim();

            if (string.IsNullOrEmpty(inputId))
            {
                MessageBox.Show("אנא הזן מספר מזהה / תעודת זהות.", "קלט חסר", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(inputId, out int id))
            {
                MessageBox.Show("תעודת הזהות חייבת להכיל ספרות בלבד.", "שגיאת פורמט", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. בדיקה האם מדובר במנהל המערכת (קוד 123)
            if (id == 123)
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
                this.Close();
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                // 3. בדיקה האם המזהה שייך לרופא
                // (אם יש לך שירות DoctorService, ננסה לבדוק אם הרופא קיים)
                var doctor = await doctorService.GetById(id);
                if (doctor != null)
                {
                    DoctorWindow doctorWindow = new DoctorWindow(doctor);
                    doctorWindow.Show();
                    //this.Close();
                    return;
                }

                // 4. אם לא אדמין ולא רופא - נבדוק האם מדובר בלקוח (Client)
                ClientDTO client = await clientService.GetById(id);
                if (client != null)
                {
                    MessageBox.Show($"ברוך הבא, {client.FullName}!", "התחברות הצליחה", MessageBoxButton.OK, MessageBoxImage.Information);

                    // פתיחת חלון הלקוח הראשי
                    ClientWindow clientWindow = new ClientWindow(client);
                    clientWindow.Show();

                    //this.Close();
                }
                else
                {
                    MessageBox.Show("מספר מזהה זה אינו רשום במערכת.", "התחברות נכשלה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string innerMessage = ex.InnerException != null ? $"\nפרטים: {ex.InnerException.Message}" : "";
                MessageBox.Show($"שגיאה בתקשורת מול השרת או בעיבוד הנתונים:\n{ex.Message}{innerMessage}",
                                "שגיאת שרת / קליינט", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        // --- לוגיקת רישום לקוח חדש (נשמרה כמו שהייתה) ---
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // פתיחת חלון הרישום/עריכה במצב ריק
            EditClientWindow registerWindow = new EditClientWindow();

            // הגדרת חלון האב כחלון הנוכחי
            registerWindow.Owner = this;

            // הצגת החלון כחלון דיאלוג מודאלי
            if (registerWindow.ShowDialog() == true)
            {
                // אם ההרשמה הצליחה, נשים אוטומטית את תעודת הזהות שהלקוח יצר בתוך תיבת הטקסט
                if (registerWindow.UpdatedClient != null)
                {
                    txtId.Text = registerWindow.UpdatedClient.ClientId.ToString();
                }

                MessageBox.Show("נרשמת למערכת בהצלחה! כעת תוכל ללחוץ על 'כניסה למערכת' כדי להתחבר.", "ההרשמה הושלמה", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}