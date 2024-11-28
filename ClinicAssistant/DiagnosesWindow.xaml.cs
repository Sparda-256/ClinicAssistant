using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace ClinicAssistant
{
    public partial class DiagnosesWindow : Window
    {
        private int patientId;

        public DiagnosesWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            LoadDiagnoses();
        }

        // Загрузка диагнозов и информации о враче
        private void LoadDiagnoses()
        {
            string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Получаем возможные диагнозы на основе ответов пациента
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

                    SqlDataReader reader = diagnosesCommand.ExecuteReader();
                    List<DiagnosisData> diagnoses = new List<DiagnosisData>();
                    int totalMatches = 0;

                    while (reader.Read())
                    {
                        int diagnosisId = (int)reader["DiagnosisID"];
                        string diagnosisName = reader["DiagnosisName"].ToString();
                        int answerMatches = (int)reader["AnswerMatches"];

                        // Собираем общий счет совпадений для вычисления процента
                        totalMatches += answerMatches;
                        diagnoses.Add(new DiagnosisData { DiagnosisID = diagnosisId, Name = diagnosisName, Matches = answerMatches });
                    }
                    reader.Close();

                    // Вывод диагнозов с процентным соотношением
                    foreach (var diagnosis in diagnoses)
                    {
                        double percentage = totalMatches > 0 ? (double)diagnosis.Matches / totalMatches * 100 : 0;
                        TextBlock diagnosisTextBlock = new TextBlock
                        {
                            Text = $"{diagnosis.Name}: {percentage:F2}%",
                            FontSize = 14,
                            Margin = new Thickness(0, 5, 0, 5)
                        };
                        DiagnosesPanel.Items.Add(diagnosisTextBlock);
                    }

                    // Получение информации о враче для наиболее вероятного диагноза
                    if (diagnoses.Count > 0)
                    {
                        int mostLikelyDiagnosisId = diagnoses[0].DiagnosisID;

                        string doctorQuery = @"
                            SELECT doc.FullName, doc.OfficeNumber
                            FROM DoctorDiagnoses dd
                            JOIN Doctors doc ON dd.DoctorID = doc.DoctorID
                            WHERE dd.DiagnosisID = @DiagnosisID";
                        SqlCommand doctorCommand = new SqlCommand(doctorQuery, connection);
                        doctorCommand.Parameters.AddWithValue("@DiagnosisID", mostLikelyDiagnosisId);

                        SqlDataReader doctorReader = doctorCommand.ExecuteReader();
                        if (doctorReader.Read())
                        {
                            string doctorName = doctorReader["FullName"].ToString();
                            string officeNumber = doctorReader["OfficeNumber"].ToString();
                            DoctorInfoTextBlock.Text = $"ФИО: {doctorName}, Кабинет: {officeNumber}";
                        }
                        doctorReader.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Вспомогательный класс для хранения данных о диагнозах
        private class DiagnosisData
        {
            public int DiagnosisID { get; set; }
            public string Name { get; set; }
            public int Matches { get; set; }
        }
    }
}