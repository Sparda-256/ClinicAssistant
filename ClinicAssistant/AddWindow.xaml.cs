using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ClinicAssistant
{
    public partial class AddWindow : Window
    {
        //private string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";
        private string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki8;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";
        private List<dynamic> symptoms = new List<dynamic>();
        private List<dynamic> questions = new List<dynamic>();
        public AddWindow()
        {
            InitializeComponent();
            LoadSymptoms();
            LoadQuestions();
        }

        private void LoadSymptoms()
        {
            symptoms.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT SymptomID, SymptomName FROM Symptoms", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    symptoms.Add(new
                    {
                        SymptomID = reader["SymptomID"],
                        SymptomName = reader["SymptomName"].ToString()
                    });
                }
                reader.Close();
            }
            SymptomListView.ItemsSource = symptoms;
        }

        private void LoadQuestions()
        {
            questions.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT QuestionID, Question FROM FollowUpQuestions", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    questions.Add(new
                    {
                        QuestionID = reader["QuestionID"],
                        QuestionText = reader["Question"].ToString()
                    });
                }
                reader.Close();
            }
            QuestionListView.ItemsSource = questions;
        }

        private void SymptomSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SymptomSearchBox.Text.ToLower();
            SymptomListView.ItemsSource = symptoms
                .Where(s => s.SymptomName.ToLower().Contains(query))
                .ToList();
        }

        private void AddSymptom_Click(object sender, RoutedEventArgs e)
        {
            string symptomName = SymptomNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(symptomName))
            {
                MessageBox.Show("Введите название симптома.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO Symptoms (SymptomName) VALUES (@SymptomName)", connection);
                    command.Parameters.AddWithValue("@SymptomName", symptomName);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Симптом успешно добавлен.");
                    SymptomNameTextBox.Clear();
                    LoadSymptoms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления симптома: " + ex.Message);
                }
            }
        }

        private void AddFollowUpQuestion_Click(object sender, RoutedEventArgs e)
        {
            var selectedSymptom = SymptomListView.SelectedItem;
            string questionText = QuestionTextTextBox.Text.Trim();

            if (selectedSymptom == null || string.IsNullOrEmpty(questionText))
            {
                MessageBox.Show("Выберите симптом и введите текст наводящего вопроса.");
                return;
            }

            int symptomID = (int)selectedSymptom.GetType().GetProperty("SymptomID").GetValue(selectedSymptom);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO FollowUpQuestions (SymptomID, Question) VALUES (@SymptomID, @Question)", connection);
                command.Parameters.AddWithValue("@SymptomID", symptomID);
                command.Parameters.AddWithValue("@Question", questionText);
                command.ExecuteNonQuery();
                MessageBox.Show("Наводящий вопрос успешно добавлен.");
                QuestionTextTextBox.Clear();
                LoadQuestions();
            }
        }

        private void QuestionSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = QuestionSearchBox.Text.ToLower();
            QuestionListView.ItemsSource = questions
                .Where(q => q.QuestionText.ToLower().Contains(query))
                .ToList();
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            var selectedQuestion = QuestionListView.SelectedItem;
            string answerText = AnswerTextTextBox.Text.Trim();

            if (selectedQuestion == null || string.IsNullOrEmpty(answerText))
            {
                MessageBox.Show("Выберите наводящий вопрос и введите текст ответа.");
                return;
            }

            int questionID = (int)selectedQuestion.GetType().GetProperty("QuestionID").GetValue(selectedQuestion);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO Answers (QuestionID, Answer) VALUES (@QuestionID, @Answer)", connection);
                command.Parameters.AddWithValue("@QuestionID", questionID);
                command.Parameters.AddWithValue("@Answer", answerText);
                command.ExecuteNonQuery();
                MessageBox.Show("Ответ успешно добавлен.");
                AnswerTextTextBox.Clear();
            }
        }

        private void AddDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            string diagnosisName = DiagnosisNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(diagnosisName))
            {
                MessageBox.Show("Введите название диагноза.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO Diagnoses (DiagnosisName) VALUES (@DiagnosisName)", connection);
                    command.Parameters.AddWithValue("@DiagnosisName", diagnosisName);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Диагноз успешно добавлен.");
                    DiagnosisNameTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления диагноза: " + ex.Message);
                }
            }
        }

        private void AddDoctor_Click(object sender, RoutedEventArgs e)
        {
            string fullName = DoctorFullNameTextBox.Text.Trim();
            string specialty = DoctorSpecialtyTextBox.Text.Trim();
            string officeNumber = DoctorOfficeNumberTextBox.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(specialty) || string.IsNullOrEmpty(officeNumber))
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO Doctors (FullName, Specialty, OfficeNumber) VALUES (@FullName, @Specialty, @OfficeNumber)", connection);
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Specialty", specialty);
                    command.Parameters.AddWithValue("@OfficeNumber", officeNumber);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Врач успешно добавлен.");
                    DoctorFullNameTextBox.Clear();
                    DoctorSpecialtyTextBox.Clear();
                    DoctorOfficeNumberTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления врача: " + ex.Message);
                }
            }
        }

        private void OpenAnswerDiagnosisLinkWindow_Click(object sender, RoutedEventArgs e)
        {
            AnswerDiagnosisLinkWindow linkWindow = new AnswerDiagnosisLinkWindow();
            linkWindow.ShowDialog();
        }

        private void OpenDiagnosisDoctorLinkWindow_Click(object sender, RoutedEventArgs e)
        {
            DiagnosisDoctorLinkWindow linkWindow = new DiagnosisDoctorLinkWindow();
            linkWindow.ShowDialog();
        }

    }
}