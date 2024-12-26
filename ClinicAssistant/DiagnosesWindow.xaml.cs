using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;

namespace ClinicAssistant
{
    public partial class DiagnosesWindow : Window
    {
        private readonly int patientId;
        private readonly DatabaseFacade dbFacade = new DatabaseFacade();

        //private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki4;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        private readonly string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";

        public ObservableCollection<DiagnosisData> Diagnoses { get; set; } = new ObservableCollection<DiagnosisData>();

        public DiagnosesWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            DiagnosesDataGrid.ItemsSource = Diagnoses;
            LoadDiagnosesAsync();
        }

        private async void LoadDiagnosesAsync()
        {
            try
            {
                var diagnoses = await dbFacade.GetDiagnosesAsync(patientId);

                if (diagnoses.Count == 0)
                {
                    DoctorInfoTextBlock.Text = "Не найдено диагнозов.";
                    return;
                }

                int totalMatches = 0;
                foreach (var diagnosis in diagnoses)
                {
                    totalMatches += diagnosis.Matches;
                }

                await dbFacade.ClearExistingDiagnosesAsync(patientId);

                foreach (var (diagnosisId, name, matches) in diagnoses)
                {
                    double percentage = totalMatches > 0 ? (double)matches / totalMatches * 100 : 0;
                    Diagnoses.Add(new DiagnosisData
                    {
                        DiagnosisID = diagnosisId,
                        Name = name,
                        Matches = matches,
                        Percentage = percentage
                    });

                    await dbFacade.SaveDiagnosisAsync(patientId, diagnosisId, (int)Math.Round(percentage));
                }

                var topDiagnosisId = diagnoses[0].DiagnosisID;
                await LoadDoctorsForDiagnosisAsync(topDiagnosisId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDoctorsForDiagnosisAsync(int diagnosisId)
        {
            try
            {
                var doctors = await dbFacade.GetDoctorsForDiagnosisAsync(diagnosisId);

                if (doctors.Count == 0)
                {
                    DoctorInfoTextBlock.Text = "Врачи не назначены для данного диагноза.";
                    return;
                }

                DoctorInfoTextBlock.Text = string.Join("\n", doctors.ConvertAll(d => $"ФИО: {d.FullName}, Кабинет: {d.OfficeNumber}"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке информации о враче: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var diagnosisDisplayList = Diagnoses
                    .Take(4)
                    .Select(d => $"{d.Name} - {d.Percentage:F2}%")
                    .ToList();
                DiagnosesList.ItemsSource = diagnosisDisplayList;

                double diagnosesHeight = 110 + (diagnosisDisplayList.Count * 25); // 25px на диагноз

                TicketDoctor.SetValue(Canvas.TopProperty, diagnosesHeight + 30);
                TicketOffice.SetValue(Canvas.TopProperty, diagnosesHeight + 70);

                var doctorDetails = DoctorInfoTextBlock.Text.Split('\n').FirstOrDefault()?.Split(',');
                TicketDoctor.Text = $"Врач: {doctorDetails?.ElementAtOrDefault(0)?.Trim() ?? "Не назначен"}";
                TicketOffice.Text = $"{doctorDetails?.ElementAtOrDefault(1)?.Trim() ?? "Не указан"}";

                TicketCanvas.Visibility = Visibility.Visible;
                TicketCanvas.UpdateLayout();
                TicketCanvas.Measure(new System.Windows.Size(TicketCanvas.Width, TicketCanvas.Height));
                TicketCanvas.Arrange(new Rect(new System.Windows.Size(TicketCanvas.Width, TicketCanvas.Height)));

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)TicketCanvas.Width, (int)TicketCanvas.Height, 96d, 96d, PixelFormats.Pbgra32);
                renderBitmap.Render(TicketCanvas);

                string filePath = $"Талон_{patientId}.png";
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fileStream);
                }

                TicketCanvas.Visibility = Visibility.Collapsed;

                MessageBox.Show($"Талон успешно сохранён: {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении талона: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public class DiagnosisData
        {
            public int DiagnosisID { get; set; }
            public string Name { get; set; }
            public int Matches { get; set; }
            public double Percentage { get; set; }
        }

        public class DoctorData
        {
            public string FullName { get; set; }
            public string OfficeNumber { get; set; }
        }

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }
    }
}