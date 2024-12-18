﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class DiagnosesWindow : Window
    {
        private readonly int patientId;
        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki4;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

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
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string diagnosesQuery = @"
                SELECT d.DiagnosisID, d.DiagnosisName, COUNT(pa.AnswerID) AS AnswerMatches
                FROM PatientAnswers pa
                JOIN AnswerDiagnoses ad ON pa.AnswerID = ad.AnswerID
                JOIN Diagnoses d ON ad.DiagnosisID = d.DiagnosisID
                WHERE pa.PatientID = @PatientID
                GROUP BY d.DiagnosisID, d.DiagnosisName
                ORDER BY AnswerMatches DESC";

                    SqlCommand diagnosesCommand = new SqlCommand(diagnosesQuery, connection);
                    diagnosesCommand.Parameters.AddWithValue("@PatientID", patientId);

                    List<DiagnosisData> diagnoses = new List<DiagnosisData>();
                    int totalMatches = 0;

                    using (SqlDataReader reader = await diagnosesCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int diagnosisId = reader.GetInt32(reader.GetOrdinal("DiagnosisID"));
                            string diagnosisName = reader.GetString(reader.GetOrdinal("DiagnosisName"));
                            int answerMatches = reader.GetInt32(reader.GetOrdinal("AnswerMatches"));

                            totalMatches += answerMatches;
                            diagnoses.Add(new DiagnosisData
                            {
                                DiagnosisID = diagnosisId,
                                Name = diagnosisName,
                                Matches = answerMatches
                            });
                        }
                    }

                    // Clear previous records for the patient in PatientDiagnoses
                    await ClearExistingDiagnosesAsync(connection, patientId);

                    foreach (var diagnosis in diagnoses)
                    {
                        double percentage = totalMatches > 0 ? (double)diagnosis.Matches / totalMatches * 100 : 0;
                        diagnosis.Percentage = percentage;
                        Diagnoses.Add(diagnosis);

                        // Save diagnosis to PatientDiagnoses table
                        await SaveDiagnosisToDatabaseAsync(connection, patientId, diagnosis.DiagnosisID, (int)Math.Round(percentage));
                    }

                    if (diagnoses.Count > 0)
                    {
                        int topDiagnosisId = diagnoses[0].DiagnosisID;
                        await LoadDoctorsForDiagnosisAsync(connection, topDiagnosisId);
                    }
                    else
                    {
                        DoctorInfoTextBlock.Text = "Не найдено диагнозов.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClearExistingDiagnosesAsync(SqlConnection connection, int patientId)
        {
            try
            {
                string deleteQuery = "DELETE FROM PatientDiagnoses WHERE PatientID = @PatientID";
                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@PatientID", patientId);
                await deleteCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при очистке старых данных о диагнозах: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveDiagnosisToDatabaseAsync(SqlConnection connection, int patientId, int diagnosisId, int percentage)
        {
            try
            {
                string insertQuery = @"
            INSERT INTO PatientDiagnoses (PatientID, DiagnosisID, Percentageofdiagnosis)
            VALUES (@PatientID, @DiagnosisID, @Percentageofdiagnosis)";

                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@PatientID", patientId);
                insertCommand.Parameters.AddWithValue("@DiagnosisID", diagnosisId);
                insertCommand.Parameters.AddWithValue("@Percentageofdiagnosis", percentage);

                await insertCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных о диагнозе: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <param name="connection">Открытое подключение к базе данных</param>
        /// <param name="diagnosisId">ID диагноза</param>
        private async Task LoadDoctorsForDiagnosisAsync(SqlConnection connection, int diagnosisId)
        {
            try
            {
                string doctorQuery = @"
                    SELECT doc.FullName, doc.OfficeNumber
                    FROM DoctorDiagnoses dd
                    JOIN Doctors doc ON dd.DoctorID = doc.DoctorID
                    WHERE dd.DiagnosisID = @DiagnosisID";

                SqlCommand doctorCommand = new SqlCommand(doctorQuery, connection);
                doctorCommand.Parameters.AddWithValue("@DiagnosisID", diagnosisId);

                List<DoctorData> doctors = new List<DoctorData>();

                using (SqlDataReader doctorReader = await doctorCommand.ExecuteReaderAsync())
                {
                    while (await doctorReader.ReadAsync())
                    {
                        string doctorName = doctorReader.GetString(doctorReader.GetOrdinal("FullName"));
                        string officeNumber = doctorReader.GetString(doctorReader.GetOrdinal("OfficeNumber"));

                        doctors.Add(new DoctorData
                        {
                            FullName = doctorName,
                            OfficeNumber = officeNumber
                        });
                    }
                }

                if (doctors.Count > 0)
                {
                    string doctorInfo = string.Join("\n", doctors.ConvertAll(d => $"ФИО: {d.FullName}, Кабинет: {d.OfficeNumber}"));
                    DoctorInfoTextBlock.Text = doctorInfo;
                }
                else
                {
                    DoctorInfoTextBlock.Text = "Врачи не назначены для данного диагноза.";
                }
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
    }
}