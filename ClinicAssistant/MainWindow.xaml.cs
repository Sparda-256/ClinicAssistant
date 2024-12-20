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

            try
            {
                DatabaseFacade dbFacade = new DatabaseFacade();
                int patientId = dbFacade.AddPatient(fullName, age, gender);

                SymptomChooseWindow symptomWindow = new SymptomChooseWindow(patientId);
                symptomWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при работе с базой данных: " + ex.Message);
            }
        }

    }
}