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
using Core.Services;
using UIToClinicProject.Services;

namespace UIToClinicProject
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        VisitService VisitService=new VisitService();
        ClientService ClientService=new ClientService();
        DoctorService DoctorService =new DoctorService();
        ClientDTO c;
        public ClientWindow(ClientDTO client)
        {
            InitializeComponent();
            c = client;
            LoadClientData();
            RefreshDashboard();
        }
        private async void LoadClientData()
        {
            // 1. קריאה לפונקציה שמחזירה DateTime? (נאלבל)
            var lastVisit = await VisitService.GetLastVisitToClient(c.ClientId);

            // 2. בדיקה והצגה: מאחר ו-lastVisit הוא בעצמו ה-DateTime, ניגש ישירות ל-Value שלו
            lblLastVisit.Text = lastVisit != null
                ? $"ביקור אחרון: {lastVisit.Value:dd/MM/yyyy}"
                : "אין ביקורים קודמים";
            // מימוש GetAge
            int age = await ClientService.GetAge(c.ClientId);
            lblClientHeader.Text = $"שלום {c.FullName}, גיל: {age}";
        }
        private async void RefreshDashboard()
        {
            // טעינת תורים עתידיים מהסרוויס

            var futureVisitsMap = await VisitService.GetFutureVisitsByIdClient(c.ClientId);
            dgFutureVisits.ItemsSource = futureVisitsMap.ToList(); // המרה ל-List הופכת כל איבר ל-KeyValuePair שניתן להציג

            // טעינת היסטוריה
            var history = await VisitService.GetVisitByidClient(c.ClientId);
            dgVisitHistory.ItemsSource = history;
        }

        // חיפוש לפי התמחות
        private async void BtnSearchBySpec_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSpecialization.SelectedItem is ComboBoxItem selected)
            {
                // שליחת הערך באנגלית שנמצא ב-Tag
                string englishSpec = selected.Tag.ToString();
                var list = await DoctorService.GetDoctorsbySpecialization(englishSpec);
                dgAvailableDoctors.ItemsSource = list;
            }
            else
            {
                MessageBox.Show("נא לבחור התמחות מהרשימה");
            }
        }

        // חיפוש לפי עיר
        private async void BtnSearchByCity_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCitySearch.SelectedItem is ComboBoxItem selected)
            {
                // שליחת הערך באנגלית שנמצא ב-Tag
                string englishCity = selected.Tag.ToString();
                var list = await DoctorService.GetDoctorsByCity(englishCity);
                dgAvailableDoctors.ItemsSource = list;
            }
            else
            {
                MessageBox.Show("נא לבחור עיר מהרשימה");
            }
        }

        private async void BtnBookVisit_Click(object sender, RoutedEventArgs e)
        {
            var selectedDoctor = dgAvailableDoctors.SelectedItem as DoctorDTO;
            var selectedDate = dpVisitDate.SelectedDate;

            if (selectedDoctor == null || selectedDate == null)
            {
                MessageBox.Show("נא לבחור רופא ותאריך.");
                return;
            }

            // חישוב משך זמן ב-WPF לפי ה-Priority שנבחר
            int priority = cmbPriority.SelectedIndex + 1;
            int durationMinutes = priority switch
            {
                1 => 5,
                2 => 10,
                3 => 15,
                _ => 10
            };

            VisitDTO visitDto = new VisitDTO
            {
                ClientId = c.ClientId,
                DoctorId = selectedDoctor.DoctorId,
                DateAndHour = selectedDate.Value,
                Priority = priority,
                Duration = durationMinutes // נשלח לשרת לבדיקת חפיפה
            };

            // השרת יבצע ב-Add את בדיקת החפיפה מול התורים הקיימים
            bool success = await VisitService.Add(visitDto);

            if (success)
            {
                MessageBox.Show($"התור נקבע בהצלחה ל-{durationMinutes} דקות.");
                RefreshDashboard();
            }
            else
            {
                MessageBox.Show("הרופא תפוס בשעה זו. נא בחר שעה אחרת.");
            }
        }
    }
}
