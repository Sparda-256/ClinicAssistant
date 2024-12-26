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

            if (!int.TryParse(AgeTextBox.Text, out int age))
            {
                MessageBox.Show("Пожалуйста, введите корректный возраст.");
                return;
            }
            if (age < 18 || age > 100)
            {
                MessageBox.Show("Возраст может быть в диапазоне от 18 до 100 лет.");
                return;
            }

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

            if (fullName == "Админ Админович Админов" && age == 33 && gender == "Женский")
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