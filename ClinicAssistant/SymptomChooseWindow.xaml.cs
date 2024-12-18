﻿// SymptomChooseWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki4;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        // Коллекция для хранения симптомов
        private ObservableCollection<Symptom> allSymptoms = new ObservableCollection<Symptom>();

        // Коллекция для хранения выбранных симптомов
        private HashSet<int> selectedSymptomIds = new HashSet<int>();

        public SymptomChooseWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            SymptomsListBox.ItemsSource = allSymptoms;
            LoadSymptoms();
        }

        // Класс для представления симптома
        public class Symptom : INotifyPropertyChanged
        {
            private bool isSelected;

            public int SymptomID { get; set; }
            public string SymptomName { get; set; }

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Загрузка симптомов из базы данных
        private void LoadSymptoms(string search = "")
        {
            try
            {
                allSymptoms.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
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
                                int id = reader.GetInt32(reader.GetOrdinal("SymptomID"));
                                string name = reader.GetString(reader.GetOrdinal("SymptomName"));

                                Symptom symptom = new Symptom
                                {
                                    SymptomID = id,
                                    SymptomName = name,
                                    IsSelected = selectedSymptomIds.Contains(id)
                                };

                                // Подписка на изменение свойства
                                symptom.PropertyChanged += Symptom_PropertyChanged;

                                allSymptoms.Add(symptom);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке симптомов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработка нажатия на кнопку "Продолжить"
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
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

        // Обработка изменения свойства IsSelected у симптома
        private void Symptom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                var symptom = sender as Symptom;
                if (symptom != null)
                {
                    if (symptom.IsSelected)
                    {
                        selectedSymptomIds.Add(symptom.SymptomID);
                    }
                    else
                    {
                        selectedSymptomIds.Remove(symptom.SymptomID);
                    }
                }
            }
        }

        // Отписка от событий при закрытии окна (опционально)
        protected override void OnClosed(EventArgs e)
        {
            foreach (var symptom in allSymptoms)
            {
                symptom.PropertyChanged -= Symptom_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}
