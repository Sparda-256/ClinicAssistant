using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class DiagnosisDoctorLinkWindow : Window
    {
        private string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki4;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";
        private List<dynamic> diagnoses = new List<dynamic>();
        private List<dynamic> doctors = new List<dynamic>();

        public DiagnosisDoctorLinkWindow()
        {
            InitializeComponent();
            LoadDiagnoses();
            LoadDoctors();
        }

        // Загрузка данных о диагнозах
        private void LoadDiagnoses()
        {
            diagnoses.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT DiagnosisID, DiagnosisName FROM Diagnoses", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    diagnoses.Add(new
                    {
                        DiagnosisID = reader["DiagnosisID"],
                        DiagnosisName = reader["DiagnosisName"].ToString()
                    });
                }
                reader.Close();
            }
            DiagnosisListView.ItemsSource = diagnoses;
        }

        // Загрузка данных о врачах
        private void LoadDoctors()
        {
            doctors.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT DoctorID, FullName FROM Doctors", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    doctors.Add(new
                    {
                        DoctorID = reader["DoctorID"],
                        DoctorName = reader["FullName"].ToString()
                    });
                }
                reader.Close();
            }
            DoctorListView.ItemsSource = doctors;
        }

        // Поиск диагнозов
        private void DiagnosisSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = DiagnosisSearchBox.Text.ToLower();
            DiagnosisListView.ItemsSource = diagnoses
                .Where(d => d.DiagnosisName.ToLower().Contains(query))
                .ToList();
        }

        // Поиск врачей
        private void DoctorSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = DoctorSearchBox.Text.ToLower();
            DoctorListView.ItemsSource = doctors
                .Where(d => d.DoctorName.ToLower().Contains(query))
                .ToList();
        }

        // Сохранение связи диагноза и врача
        private void SaveDiagnosisDoctorLink_Click(object sender, RoutedEventArgs e)
        {
            var selectedDiagnosis = DiagnosisListView.SelectedItem;
            var selectedDoctor = DoctorListView.SelectedItem;

            if (selectedDiagnosis == null || selectedDoctor == null)
            {
                MessageBox.Show("Выберите диагноз и врача.");
                return;
            }

            int diagnosisID = (int)selectedDiagnosis.GetType().GetProperty("DiagnosisID").GetValue(selectedDiagnosis);
            int doctorID = (int)selectedDoctor.GetType().GetProperty("DoctorID").GetValue(selectedDoctor);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "INSERT INTO DoctorDiagnoses (DiagnosisID, DoctorID) VALUES (@DiagnosisID, @DoctorID)",
                    connection);
                command.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                command.Parameters.AddWithValue("@DoctorID", doctorID);

                command.ExecuteNonQuery();
                MessageBox.Show("Связь добавлена успешно!");
                this.Close();
            }
        }
    }
}
