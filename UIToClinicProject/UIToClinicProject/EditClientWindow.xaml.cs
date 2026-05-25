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
    public partial class EditClientWindow : Window
    {
        public ClientDTO UpdatedClient { get; set; }
        private bool _isNewMode; // משתנה פנימי לזיהוי המצב (חדש או עריכה)

        // שימי לב ל-client = null: זה מה שפותר את שגיאת הקומפילציה!
        public EditClientWindow(ClientDTO client = null)
        {
            InitializeComponent();

            if (client == null)
            {
                // --- מצב: הרשמה עצמית / לקוח חדש ---
                _isNewMode = true;
                UpdatedClient = new ClientDTO();

                // התאמת הכותרות והטקסטים לחלון החדש
                this.Title = "הרשמה למערכת";
                lblTitle.Text = "יצירת חשבון לקוח חדש";
                btnSave.Content = "📝 סיים הרשמה וסגור";

                txtClientId.IsEnabled = true; // מאפשרים להזין תעודת זהות ללקוח חדש
            }
            else
            {
                // --- מצב: עריכת לקוח קיים ---
                _isNewMode = false;
                UpdatedClient = client;

                // טעינת הנתונים הקיימים לשדות
                txtClientId.Text = client.ClientId.ToString();
                txtClientId.IsEnabled = false; // חוסמים שינוי של תעודת זהות קיימת!

                txtFullName.Text = client.FullName;
                txtCity.Text = client.City;
                txtPhone.Text = client.Phone;
                txtEmail.Text = client.Email;
                txtPass.Text = client.Pass;
                dpBirthDate.SelectedDate = client.DateOfBirth;

                this.Title = "עדכון פרטי לקוח";
                lblTitle.Text = "עריכת פרטי לקוח";
                btnSave.Content = "✅ עדכן שינויים";
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // בדיקת תקינות ומילוי תעודת זהות במצב חדש בלבד
            if (_isNewMode)
            {
                if (int.TryParse(txtClientId.Text, out int clientId))
                {
                    UpdatedClient.ClientId = clientId;
                }
                else
                {
                    MessageBox.Show("נא להזין תעודת זהות תקינה (מספרים בלבד).", "שגיאה קלט", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // עדכון שאר השדות לאובייקט
            UpdatedClient.FullName = txtFullName.Text;
            UpdatedClient.City = txtCity.Text;
            UpdatedClient.Phone = txtPhone.Text;
            UpdatedClient.Email = txtEmail.Text;
            UpdatedClient.Pass = txtPass.Text;

            if (dpBirthDate.SelectedDate.HasValue)
            {
                UpdatedClient.DateOfBirth = dpBirthDate.SelectedDate.Value;
            }

            try
            {
                ClientService clientService = new ClientService();
                bool isSuccess = false;

                if (_isNewMode)
                {
                    // קריאה לפונקציית הוספה ב-Service (ודאי שהשם תואם למה שיש לך ב-ClientService)
                    isSuccess = await clientService.Add(UpdatedClient);
                }
                else
                {
                    // קריאה לפונקציית עדכון קיימת
                    isSuccess = await clientService.Update(UpdatedClient);
                }

                if (isSuccess)
                {
                    this.DialogResult = true; // יסגור את החלון ויחזיר true למסך האב
                }
                else
                {
                    MessageBox.Show(_isNewMode ? "ההרשמה נכשלה בשרת. ודא שתעודת הזהות אינה קיימת כבר." : "העדכון בשרת נכשל. נא לנסות שוב.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בתקשורת עם השרת: " + ex.Message);
            }
        }
    }
}