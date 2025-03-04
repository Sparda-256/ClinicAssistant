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
            string fullName = FullNameTextBox.Text.Trim();

            DateTime dateOfBirth;

            if (string.IsNullOrEmpty(fullName) || !fullName.Contains(" "))
            {
                MessageBox.Show("Пожалуйста, введите корректное ФИО.");
                return;
            }

            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(gender))
            {
                MessageBox.Show("Пожалуйста, выберите пол.");
                return;
            }

            if (fullName == "Админ Админович Админов" && gender == "Женский")
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
                this.Close();
                return;
            }

            if (!DateTime.TryParse(DateOfBirthPicker.Text, out dateOfBirth))
            {
                MessageBox.Show("Пожалуйста, введите корректную дату рождения.");
                return;
            }

            try
            {
                DatabaseFacade dbFacade = new DatabaseFacade();
                int patientId = dbFacade.AddNewPatient(fullName, dateOfBirth, gender); // Изменено на AddNewPatient

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