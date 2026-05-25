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
    /// Interaction logic for DoctorWindow.xaml
    /// </summary>
    public partial class DoctorWindow : Window
    {
        VisitService visitService=new VisitService();
        DoctorService doctorService=new DoctorService();
        DoctorDTO d;
        public DoctorWindow(DoctorDTO doctor)
        {
            InitializeComponent();
            d= doctor;
            LoadInitialData();
        }
        private async void LoadInitialData()
        {
            try
            {
                // 1. טעינת לו"ז להיום (Join)
                var todaySchedule = await visitService.GetDoctorScheduleToday(d.DoctorId);
                lstDocSchedule.ItemsSource = todaySchedule;

                // פונקציה זו מחזירה מידע הכולל שמות לקוחות ופרטים מורחבים
                List<AppointmentDTO> detailedVisits = await visitService.GetDetailedAppointments(dpDetailedDate.SelectedDate ?? DateTime.Now);
                dgAllDoctorVisits.ItemsSource = detailedVisits;
                    
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת נתונים: " + ex.Message);
            }
        }

        private async void Calc_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtMult.Text, out int multiplier))
            {
                try
                {
                    // חישוב בונוס עם ה-Overload הפשוט (אופציה א' שדיברנו עליה)
                    int bonus = await doctorService.GetDoctorBonus(d.DoctorId, multiplier);
                    lblResult.Text = $"בונוס מצטבר: {bonus:N0} ₪";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("שגיאה בחישוב: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("נא להזין מספר תקין במכפיל.");
            }
        }

        private async void BtnCountVisits_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var stats = await visitService.GetCountVisitToDoctor(d.DoctorId);
                // השמת האובייקט בתוך רשימה כדי שה-DataGrid יוכל להציג אותו
                dgDoctorStats.ItemsSource = new List<CountVisitToDoctor> { stats };
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בשליפת סטטיסטיקות: " + ex.Message);
            }
        }
        private async void BtnShowDetailed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime selectedDate = dpDetailedDate.SelectedDate ?? DateTime.Now;

                // קריאה לשירות לקבלת התורים המפורטים
                var detailedVisits = await visitService.GetDetailedAppointments(selectedDate);

                // עדכון ה-DataGrid וסינון לפי ה-ID של הרופא הנוכחי
                dgAllDoctorVisits.ItemsSource = detailedVisits;
                    //.Where(v => v.DoctorName\ == d.DoctorId)
                    //.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת התורים המפורטים: " + ex.Message);
            }
        }
    }
}

