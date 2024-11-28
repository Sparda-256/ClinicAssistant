using System;
using System.Data.SqlClient;
using System.Windows;

namespace ClinicAssistant
{
    public partial class AnswerDiagnosisLinkWindow : Window
    {
        private string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        public AnswerDiagnosisLinkWindow()
        {
            InitializeComponent();
            LoadAnswers();
            LoadDiagnoses();
        }

        private void LoadAnswers()
        {
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
                    AnswerComboBox.Items.Add(new
                    {
                        AnswerID = reader["AnswerID"],
                        DisplayText = $"{reader["Question"]} - {reader["Answer"]}"
                    });
                }
                reader.Close();
            }

            AnswerComboBox.DisplayMemberPath = "DisplayText";
            AnswerComboBox.SelectedValuePath = "AnswerID";
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
                    DiagnosisComboBox.Items.Add(new { DiagnosisID = reader["DiagnosisID"], DiagnosisName = reader["DiagnosisName"].ToString() });
                }
                reader.Close();
            }

            DiagnosisComboBox.DisplayMemberPath = "DiagnosisName";
            DiagnosisComboBox.SelectedValuePath = "DiagnosisID";
        }


        private void SaveAnswerDiagnosisLink_Click(object sender, RoutedEventArgs e)
        {
            if (AnswerComboBox.SelectedItem == null || DiagnosisComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите ответ и диагноз.");
                return;
            }

            var selectedAnswer = AnswerComboBox.SelectedItem;
            var selectedDiagnosis = DiagnosisComboBox.SelectedItem;

            int answerID = (int)selectedAnswer.GetType().GetProperty("AnswerID").GetValue(selectedAnswer);
            int diagnosisID = (int)selectedDiagnosis.GetType().GetProperty("DiagnosisID").GetValue(selectedDiagnosis);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO AnswerDiagnoses (AnswerID, DiagnosisID) VALUES (@AnswerID, @DiagnosisID)", connection);
                command.Parameters.AddWithValue("@AnswerID", answerID);
                command.Parameters.AddWithValue("@DiagnosisID", diagnosisID);

                command.ExecuteNonQuery();
                MessageBox.Show("Связь добавлена успешно!");
                this.Close();
            }
        }
    }
}
