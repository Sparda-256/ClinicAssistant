using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    /// <summary>
    /// Логика взаимодействия для SymptomChooseWindow.xaml
    /// </summary>
    public partial class SymptomChooseWindow : Window
    {
        private readonly int patientId;
        private readonly string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        // Коллекция для хранения симптомов
        private List<Symptom> allSymptoms = new List<Symptom>();

        public SymptomChooseWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            LoadSymptoms();
        }

        // Класс для представления симптома
        public class Symptom
        {
            public int SymptomID { get; set; }
            public string SymptomName { get; set; }
            public bool IsSelected { get; set; }
        }

        // Загрузка симптомов из базы данных
        private void LoadSymptoms(string search = "")
        {
            allSymptoms.Clear();
            SymptomsListBox.ItemsSource = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SymptomID, SymptomName FROM Symptoms";

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        query += " WHERE SymptomName LIKE @search";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            command.Parameters.AddWithValue("@search", "%" + search + "%");
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Symptom symptom = new Symptom
                                {
                                    SymptomID = reader.GetInt32(reader.GetOrdinal("SymptomID")),
                                    SymptomName = reader.GetString(reader.GetOrdinal("SymptomName")),
                                    IsSelected = false
                                };
                                allSymptoms.Add(symptom);
                            }
                        }
                    }

                    SymptomsListBox.ItemsSource = allSymptoms;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке симптомов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработка нажатия на кнопку "Продолжить"
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            List<int> selectedSymptomIds = new List<int>();

            foreach (Symptom symptom in allSymptoms)
            {
                if (symptom.IsSelected)
                {
                    selectedSymptomIds.Add(symptom.SymptomID);
                }
            }

            if (selectedSymptomIds.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один симптом.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Запись выбранных симптомов в таблицу PatientSymptoms
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (int symptomId in selectedSymptomIds)
                            {
                                string query = "INSERT INTO PatientSymptoms (PatientID, SymptomID) VALUES (@PatientID, @SymptomID)";
                                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@PatientID", patientId);
                                    command.Parameters.AddWithValue("@SymptomID", symptomId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception exInner)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Ошибка при сохранении симптомов: " + exInner.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Открытие окна FollowUpQuestionsWindow
            FollowUpQuestionsWindow followUpWindow = new FollowUpQuestionsWindow(patientId);
            followUpWindow.Show();
            this.Close();
        }

        // Обработка изменения текста в поле поиска
        private void SearchSymptomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchSymptomTextBox.Text.Trim();
            LoadSymptoms(searchText);
        }
    }
}