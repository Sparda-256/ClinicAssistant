﻿using System;
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

        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki2;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        //private readonly string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki8; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";

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

                var topDiagnoses = diagnoses
                  .GroupBy(d => d.Matches)
                  .OrderByDescending(g => g.Key)
                  .First()
                  .Select(d => d.DiagnosisID)
                  .ToList();

                await LoadDoctorsForDiagnosisAsync(topDiagnoses);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDoctorsForDiagnosisAsync(List<int> topDiagnosisIds)
        {
            try
            {
                // Dictionary to store doctor specialties and their cumulative percentage
                var specialtyPercentage = new Dictionary<string, double>();

                foreach (var diagnosisId in topDiagnosisIds)
                {
                    var doctors = await dbFacade.GetDoctorsForDiagnosisAsync(diagnosisId);

                    foreach (var doctor in doctors)
                    {
                        if (specialtyPercentage.ContainsKey(doctor.Specialty))
                        {
                            specialtyPercentage[doctor.Specialty] += 1.0 / topDiagnosisIds.Count * 100;
                        }
                        else
                        {
                            specialtyPercentage[doctor.Specialty] = 1.0 / topDiagnosisIds.Count * 100;
                        }
                    }
                }

                if (!specialtyPercentage.Any())
                {
                    DoctorInfoTextBlock.Text = "Врачи не назначены для данных диагнозов.";
                    return;
                }

                // Determine the leader based on percentage
                var maxPercentage = specialtyPercentage.Values.Max();
                var topSpecialties = specialtyPercentage
                    .Where(s => Math.Abs(s.Value - maxPercentage) < 0.01) // Account for floating-point precision
                    .Select(s => s.Key)
                    .ToList();

                string selectedSpecialty;

                if (topSpecialties.Count == 1)
                {
                    // Clear leader
                    selectedSpecialty = topSpecialties.First();
                }
                else if (topSpecialties.Contains("Терапевт"))
                {
                    // If there's a tie, prioritize "Терапевт"
                    selectedSpecialty = "Терапевт";
                }
                else
                {
                    // Fallback to the first specialty in the list
                    selectedSpecialty = topSpecialties.First();
                }

                // Fetch doctor details for the selected specialty
                var fallbackDoctor = await dbFacade.GetDoctorBySpecialtyAsync(selectedSpecialty);

                if (fallbackDoctor.HasValue)
                {
                    DoctorInfoTextBlock.Text = $"ФИО: {fallbackDoctor.Value.FullName}, Кабинет: {fallbackDoctor.Value.OfficeNumber}";
                    await dbFacade.UpdatePredictedDoctorAsync(patientId, fallbackDoctor.Value.DoctorID);
                }

                else
                {
                    DoctorInfoTextBlock.Text = "Не удалось найти врача по выбранной специальности.";
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке информации о враче: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

                double diagnosesHeight = 110 + (diagnosisDisplayList.Count * 25);

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

                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Path.Combine(exeDirectory, "..", "..");
                string talonyDirectory = Path.Combine(projectDirectory, "Талоны");

                Directory.CreateDirectory(talonyDirectory);

                string filePath = Path.Combine(talonyDirectory, $"Талон_{patientId}.png");

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