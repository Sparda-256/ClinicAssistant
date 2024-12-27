using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class AnswerDiagnosisLinkWindow : Window
    {
        private string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki8; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";

        //private string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki8;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";
        private List<dynamic> answers = new List<dynamic>();
        private List<dynamic> diagnoses = new List<dynamic>();

        public AnswerDiagnosisLinkWindow()
        {
            InitializeComponent();
            LoadAnswers();
            LoadDiagnoses();
        }

        // Метод загрузки ответов из базы данных
        private void LoadAnswers()
        {
            answers.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "SELECT a.AnswerID, a.Answer, fq.Question " +
                    "FROM Answers a " +
                    "JOIN FollowUpQuestions fq ON a.QuestionID = fq.QuestionID",
                    connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    answers.Add(new
                    {
                        AnswerID = reader["AnswerID"],
                        DisplayText = $"{reader["Question"]} - {reader["Answer"]}"
                    });
                }
                reader.Close();
            }
            AnswerListView.ItemsSource = answers;
        }

        // Метод загрузки диагнозов из базы данных
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

        // Метод поиска ответов
        private void AnswerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = AnswerSearchBox.Text.ToLower();
            AnswerListView.ItemsSource = answers
                .Where(a => a.DisplayText.ToLower().Contains(query))
                .ToList();
        }

        // Метод поиска диагнозов
        private void DiagnosisSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = DiagnosisSearchBox.Text.ToLower();
            DiagnosisListView.ItemsSource = diagnoses
                .Where(d => d.DiagnosisName.ToLower().Contains(query))
                .ToList();
        }

        // Метод сохранения связи ответа и диагноза
        private void SaveAnswerDiagnosisLink_Click(object sender, RoutedEventArgs e)
        {
            var selectedAnswer = AnswerListView.SelectedItem;
            var selectedDiagnosis = DiagnosisListView.SelectedItem;

            if (selectedAnswer == null || selectedDiagnosis == null)
            {
                MessageBox.Show("Выберите ответ и диагноз.");
                return;
            }

            int answerID = (int)selectedAnswer.GetType().GetProperty("AnswerID").GetValue(selectedAnswer);
            int diagnosisID = (int)selectedDiagnosis.GetType().GetProperty("DiagnosisID").GetValue(selectedDiagnosis);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "INSERT INTO AnswerDiagnoses (AnswerID, DiagnosisID) VALUES (@AnswerID, @DiagnosisID)",
                    connection);
                command.Parameters.AddWithValue("@AnswerID", answerID);
                command.Parameters.AddWithValue("@DiagnosisID", diagnosisID);

                command.ExecuteNonQuery();
                MessageBox.Show("Связь добавлена успешно!");
            }
        }

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}