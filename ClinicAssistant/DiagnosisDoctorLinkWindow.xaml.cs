using System;
using System.Data.SqlClient;
using System.Windows;

namespace ClinicAssistant
{
    public partial class DiagnosisDoctorLinkWindow : Window
    {
        private string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        public DiagnosisDoctorLinkWindow()
        {
            InitializeComponent();
            LoadDiagnoses();
            LoadDoctors();
        }

        private void LoadDiagnoses()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT DiagnosisID, DiagnosisName FROM Diagnoses", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DiagnosisComboBox.Items.Add(new
                    {
                        DiagnosisID = reader["DiagnosisID"],
                        DiagnosisName = reader["DiagnosisName"]
                    });
                }
                reader.Close();
            }

            DiagnosisComboBox.DisplayMemberPath = "DiagnosisName";
            DiagnosisComboBox.SelectedValuePath = "DiagnosisID";
        }

        private void LoadDoctors()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT DoctorID, FullName FROM Doctors", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DoctorComboBox.Items.Add(new
                    {
                        DoctorID = reader["DoctorID"],
                        DoctorName = reader["FullName"]
                    });
                }
                reader.Close();
            }

            DoctorComboBox.DisplayMemberPath = "DoctorName";
            DoctorComboBox.SelectedValuePath = "DoctorID";
        }


        private void SaveDiagnosisDoctorLink_Click(object sender, RoutedEventArgs e)
        {
            if (DiagnosisComboBox.SelectedItem == null || DoctorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите диагноз и врача.");
                return;
            }

            var selectedDiagnosis = DiagnosisComboBox.SelectedItem;
            var selectedDoctor = DoctorComboBox.SelectedItem;

            int diagnosisID = (int)selectedDiagnosis.GetType().GetProperty("DiagnosisID").GetValue(selectedDiagnosis);
            int doctorID = (int)selectedDoctor.GetType().GetProperty("DoctorID").GetValue(selectedDoctor);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO DoctorDiagnoses (DiagnosisID, DoctorID) VALUES (@DiagnosisID, @DoctorID)", connection);
                command.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                command.Parameters.AddWithValue("@DoctorID", doctorID);

                command.ExecuteNonQuery();
                MessageBox.Show("Связь добавлена успешно!");
                this.Close();
            }
        }
    }
}