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
using Microsoft.Win32; // נוסף עבור OpenFileDialog ו-SaveFileDialog
using Core.DTOs;
using Core.Entities;
using UIToClinicProject.Services;

namespace UIToClinicProject
{
    public partial class AdminWindow : Window
    {
        VisitService VisitService = new VisitService();
        ClientService ClientService = new ClientService();
        DoctorService DoctorService = new DoctorService();

        public AdminWindow()
        {
            InitializeComponent();
        }

        // --- פונקציית עזר לניקוי והכנת ה-DataGrid (מעודכנת עם כפתור מחיקה) ---
        private void PrepareDataGrid(bool showActionButtons)
        {
            dgAdmin.ItemsSource = null;
            dgAdmin.Columns.Clear();
            dgAdmin.AutoGenerateColumns = true;

            if (showActionButtons)
            {
                // יצירת עמודת פעולות בצורה תכנותית
                DataGridTemplateColumn actionColumn = new DataGridTemplateColumn();
                actionColumn.Header = "פעולות";

                // יצירת StackPanel אופקי שיכיל את שני הכפתורים יחד
                FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
                stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                // 1. יצירת כפתור עריכה
                FrameworkElementFactory btnEditFactory = new FrameworkElementFactory(typeof(Button));
                btnEditFactory.SetValue(Button.ContentProperty, "📝 ערוך");
                btnEditFactory.SetValue(Button.BackgroundProperty, new SolidColorBrush(Colors.Gold));
                btnEditFactory.SetValue(Button.MarginProperty, new Thickness(5, 2, 5, 2));
                btnEditFactory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
                btnEditFactory.SetValue(Button.CursorProperty, Cursors.Hand);
                btnEditFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditClient_Click));

                // 2. יצירת כפתור מחיקה
                FrameworkElementFactory btnDeleteFactory = new FrameworkElementFactory(typeof(Button));
                btnDeleteFactory.SetValue(Button.ContentProperty, "🗑️ מחק");
                btnDeleteFactory.SetValue(Button.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"))); // צבע אדום
                btnDeleteFactory.SetValue(Button.ForegroundProperty, new SolidColorBrush(Colors.White));
                btnDeleteFactory.SetValue(Button.MarginProperty, new Thickness(5, 2, 5, 2));
                btnDeleteFactory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
                btnDeleteFactory.SetValue(Button.CursorProperty, Cursors.Hand);
                btnDeleteFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteClient_Click));

                // הוספת הכפתורים לתוך ה-StackPanel
                stackPanelFactory.AppendChild(btnEditFactory);
                stackPanelFactory.AppendChild(btnDeleteFactory);

                // הגדרת ה-Template לעמודה
                DataTemplate dt = new DataTemplate();
                dt.VisualTree = stackPanelFactory;
                actionColumn.CellTemplate = dt;

                dgAdmin.Columns.Add(actionColumn);
            }
        }

        // פונקציית עזר עצמאית לרענון בטוח של טבלת הלקוחות
        private async Task RefreshClientsGrid()
        {
            try
            {
                PrepareDataGrid(true);
                lblStatus.Text = "מרענן רשימת לקוחות...";
                var clients = await ClientService.GetAll();

                if (clients != null)
                {
                    dgAdmin.ItemsSource = clients.ToList();
                    lblStatus.Text = $"מוצגים {clients.Count()} לקוחות. ניתן לערוך או למחוק פרטים.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת לקוחות: " + ex.Message);
            }
        }

        // --- פונקציית עזר חדשה לרענון בטוח של טבלת הרופאים ---
        private async Task RefreshDoctorsGrid()
        {
            try
            {
                PrepareDataGrid(false);
                lblStatus.Text = "מתחיל בטעינת רשימת רופאים...";

                int page = 1;
                int size = 3;

                List<DoctorDTO> allDoctors = new List<DoctorDTO>();

                // קריאה ראשונית לעמוד הראשון
                var data = await DoctorService.GetAll(page++, size);

                while (data != null && data.Count() != 0)
                {
                    // שמירת הכמות הנוכחית לפני השרשור
                    int previousCount = allDoctors.Count;

                    // סינון כפילויות מקומי: נוסיף רק רופאים שעדיין לא קיימים ברשימה הכללית לפי ה-ID שלהם
                    var newDoctors = data.Where(newDoc => !allDoctors.Any(existingDoc => existingDoc.DoctorId == newDoc.DoctorId)).ToList();

                    // אם אין אף רופא חדש באמת להוסיף, הגענו לסוף!
                    if (newDoctors.Count == 0)
                    {
                        break; // עצירת הלולאה מיד
                    }

                    // שירשור רק של הרופאים החדשים שאינם כפולים
                    allDoctors = allDoctors.Concat(newDoctors).ToList();

                    // עדכון ה-UI
                    dgAdmin.ItemsSource = allDoctors;
                    lblStatus.Text = $"טוען... מוצגים כעת {allDoctors.Count} רופאים.";

                    await Task.Delay(500);

                    // קריאה לעמוד הבא
                    data = await DoctorService.GetAll(page++, size);
                }

                // עדכון סטטוס סופי
                if (allDoctors.Count > 0)
                {
                    lblStatus.Text = $" סך הכל מוצגים {allDoctors.Count} רופאים במערכת.";
                }
                else
                {
                    lblStatus.Text = "לא נמצאו רופאים במערכת.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת רופאים: " + ex.Message);
            }
        }
        // --- לוגיקת כפתור המחיקה של הלקוחות ---
        private async void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            ClientDTO selectedClient = (sender as Button).DataContext as ClientDTO;
            if (selectedClient == null) return;

            MessageBoxResult result = MessageBox.Show(
                $"האם אתה בטוח שברצונך למחוק את הלקוח {selectedClient.FullName} (ת.ז: {selectedClient.ClientId}) מהמערכת?",
                "אישור מחיקה",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes) return;

            try
            {
                lblStatus.Text = "מוחק לקוח מהמערכת...";

                // שליחת בקשת המחיקה ל-Service
                bool isSuccess = await ClientService.Delete(selectedClient.ClientId);

                if (isSuccess)
                {
                    MessageBox.Show("הלקוח נמחק בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshClientsGrid(); // רענון הטבלה
                }
                else
                {
                    lblStatus.Text = "המחיקה נכשלה.";
                    MessageBox.Show("לא ניתן למחוק לקוח זה מכיוון שיש לו תורות או ביקורים פעילים במערכת.", "שגיאת שלמות נתונים", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "שגיאה בביצוע הפעולה.";

                // בדיקה דינמית אם השגיאה שמגיעה מהשרת מכילה רמז על קשר לטבלאות אחרות
                string errorMessage = ex.Message;

                if (errorMessage.Contains("500") || errorMessage.Contains("foreign key") || errorMessage.Contains("constraint"))
                {
                    MessageBox.Show($"שגיאה במחיקת הלקוח:\nלא ניתן למחוק את {selectedClient.FullName} כיוון שהוא מקושר להיסטוריית ביקורים (Visit) במסד הנתונים.",
                                    "שגיאת שלמות נתונים (SQL)", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"שגיאה בתקשורת בזמן המחיקה:\n{errorMessage}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 1. הצגת עיר בעדיפות
        private async void BtnGetPriorityCity_Click(object sender, RoutedEventArgs e)
        {
            if (!dpAdminDate.SelectedDate.HasValue)
            {
                MessageBox.Show("נא לבחור תאריך.");
                return;
            }

            try
            {
                PrepareDataGrid(false);
                DateTime selectedDate = dpAdminDate.SelectedDate.Value;
                lblStatus.Text = $"בודק עיר מועדפת לתאריך {selectedDate:dd/MM/yyyy}...";

                PriorityCity topCity = await VisitService.GetPriorityCity(selectedDate);

                if (topCity != null && !string.IsNullOrEmpty(topCity.City))
                {
                    dgAdmin.ItemsSource = new List<PriorityCity> { topCity };
                    lblStatus.Text = $"העיר בראש סדר עדיפויות: {topCity.City}";
                }
                else
                {
                    lblStatus.Text = "לא נמצאו נתונים לתאריך זה.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בשליפת עיר מועדפת: " + ex.Message);
            }
        }

        // 2. עומס מחלקות
        private async void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrepareDataGrid(false);
                lblStatus.Text = "טוען נתוני עומס מחלקות...";
                List<ClinicLoadDTO> loadData = await VisitService.GetClinicLoad();
                dgAdmin.ItemsSource = loadData;
                lblStatus.Text = "נתוני העומס עודכנו בהצלחה.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת עומס מחלקות: " + ex.Message);
            }
        }

        // 3. הצגת כל הלקוחות
        private async void AllClients_Click(object sender, RoutedEventArgs e)
        {
            await RefreshClientsGrid();
        }

        // --- אירוע הלחיצה החדש עבור כפתור הרופאים ---
        private async void AllDoctors_Click(object sender, RoutedEventArgs e)
        {
            await RefreshDoctorsGrid();
        }

        // 4. לוגיקת כפתור העריכה של לקוח
        private async void EditClient_Click(object sender, RoutedEventArgs e)
        {
            // שליפת הלקוח מהשורה שעליה לחצו
            ClientDTO selectedClient = (sender as Button).DataContext as ClientDTO;

            if (selectedClient == null) return;

            // פתיחת חלון עריכה
            EditClientWindow editWin = new EditClientWindow(selectedClient);

            if (editWin.ShowDialog() == true)
            {
                try
                {
                    lblStatus.Text = "שומר שינויים...";

                    // שמירת תוצאת העדכון מה-Service
                    bool isSuccess = await ClientService.Update(editWin.UpdatedClient);

                    if (isSuccess)
                    {
                        MessageBox.Show("הפרטים עודכנו בהצלחה בשרת!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                        lblStatus.Text = "פרטי לקוח עודכנו בהצלחה.";
                        await RefreshClientsGrid(); // רענון בטוח של הטבלה
                    }
                    else
                    {
                        lblStatus.Text = "העדכון נכשל בשרת.";
                        MessageBox.Show("השרת לא הצלח לעדכן את הנתונים. ודא כי ה-API תקין.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("שגיאה בתקשורת: " + ex.Message);
                }
            }
        }

        // 5. ייבוא לקוחות מאקסל דרך השרת
        private async void ImportClients_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    lblStatus.Text = "מעלה קובץ ומייבא לקוחות...";

                    // קריאה לשירות עם הנתיב המקומי - השירות כבר יהפוך אותו ל-Stream וישלח לשרת
                    bool success = await ClientService.ImportFromExcel(openFileDialog.FileName);

                    if (success)
                    {
                        lblStatus.Text = "הלקוחות יובאו בהצלחה!";
                        MessageBox.Show("הייבוא מהאקסל הושלם בהצלחה.", "ייבוא אקסל", MessageBoxButton.OK, MessageBoxImage.Information);
                        await RefreshClientsGrid();
                    }
                    else
                    {
                        lblStatus.Text = "הייבוא נכשל.";
                        MessageBox.Show("השרת החזיר תשובה שלילית. ודא שהנתונים אינם כפולים.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "שגיאה בתקשורת.";
                    MessageBox.Show("שגיאה במהלך פעולת הייבוא:\n" + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 6. ייצוא דו"ח שעות רופאים מאקסל שהתקבל מהשרת
        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            saveFileDialog.FileName = $"Doctor_Avg_Hours_{DateTime.Now:yyyyMMdd}";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    lblStatus.Text = "מפיק דו''ח שעות מהשרת ומוריד קובץ...";

                    string targetSavePath = saveFileDialog.FileName;

                    // קריאה לשירות המעודכן
                    bool success = await DoctorService.ExportToFile(targetSavePath);

                    if (success)
                    {
                        lblStatus.Text = "הקובץ הופק והורד בהצלחה.";
                        MessageBox.Show($"הדו\"ח הופק ונשמר בהצלחה בנתיב:\n{targetSavePath}", "ייצוא לאקסל", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        lblStatus.Text = "הפקת הדו''ח נכשלה.";
                        MessageBox.Show("השרת החזיר תשובה שלילית (False). ודא שקיימים נתונים מתאימים במסד הנתונים עבור החודש המבוקש.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "שגיאה בהורדת הקובץ.";

                    // בניית הודעת שגיאה מפורטת שתפרק גם את ה-InnerException (כדי לראות את הודעת השרת האמיתית)
                    string fullErrorMessage = ex.Message;

                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        fullErrorMessage += "\n← " + inner.Message;
                        inner = inner.InnerException;
                    }

                    MessageBox.Show("שגיאה במהלך פעולת הייצוא:\n" + fullErrorMessage, "דיאגנוסטיקה ושגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}