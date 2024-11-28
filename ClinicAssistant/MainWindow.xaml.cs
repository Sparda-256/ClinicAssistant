using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text;
            if (!int.TryParse(AgeTextBox.Text, out int age))
            {
                MessageBox.Show("Пожалуйста, введите корректный возраст.");
                return;
            }
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(gender))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (fullName == "Админ Админович Админов" && age == 333 && gender == "Женский")
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
                this.Close();
                return;
            }

            string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";
            int patientId;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Patients (FullName, Age, Gender) OUTPUT INSERTED.PatientID VALUES (@FullName, @Age, @Gender)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@Gender", gender);

                    patientId = (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
                    return;
                }
            }

            SymptomChooseWindow symptomWindow = new SymptomChooseWindow(patientId);
            symptomWindow.Show();
            this.Close();
        }

    }
}