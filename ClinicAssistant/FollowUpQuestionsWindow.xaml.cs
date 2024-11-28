using System;
using System.Data.SqlClient;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class FollowUpQuestionsWindow : Window
    {
        private int patientId;
        private List<QuestionControl> questionControls = new List<QuestionControl>();

        public FollowUpQuestionsWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            LoadQuestions();
        }

        // Загрузка вопросов и вариантов ответов из базы данных
        private void LoadQuestions()
        {
            string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Получаем список симптомов пациента
                    string symptomQuery = "SELECT SymptomID FROM PatientSymptoms WHERE PatientID = @PatientID";
                    SqlCommand symptomCommand = new SqlCommand(symptomQuery, connection);
                    symptomCommand.Parameters.AddWithValue("@PatientID", patientId);
                    SqlDataReader symptomReader = symptomCommand.ExecuteReader();

                    List<int> symptomIds = new List<int>();
                    while (symptomReader.Read())
                    {
                        symptomIds.Add((int)symptomReader["SymptomID"]);
                    }
                    symptomReader.Close();

                    // Получаем наводящие вопросы для каждого симптома
                    foreach (int symptomId in symptomIds)
                    {
                        string questionQuery = "SELECT QuestionID, Question FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
                        SqlCommand questionCommand = new SqlCommand(questionQuery, connection);
                        questionCommand.Parameters.AddWithValue("@SymptomID", symptomId);
                        SqlDataReader questionReader = questionCommand.ExecuteReader();

                        while (questionReader.Read())
                        {
                            int questionId = (int)questionReader["QuestionID"];
                            string questionText = questionReader["Question"].ToString();

                            QuestionControl questionControl = new QuestionControl(questionId, questionText);
                            questionControls.Add(questionControl);
                            QuestionsPanel.Items.Add(questionControl);
                        }
                        questionReader.Close();
                    }

                    // Загрузка вариантов ответов для каждого вопроса
                    foreach (var questionControl in questionControls)
                    {
                        string answerQuery = "SELECT AnswerID, Answer FROM Answers WHERE QuestionID = @QuestionID";
                        SqlCommand answerCommand = new SqlCommand(answerQuery, connection);
                        answerCommand.Parameters.AddWithValue("@QuestionID", questionControl.QuestionId);
                        SqlDataReader answerReader = answerCommand.ExecuteReader();

                        while (answerReader.Read())
                        {
                            int answerId = (int)answerReader["AnswerID"];
                            string answerText = answerReader["Answer"].ToString();

                            ComboBoxItem comboBoxItem = new ComboBoxItem { Content = answerText, Tag = answerId };
                            questionControl.AnswersComboBox.Items.Add(comboBoxItem);
                        }
                        answerReader.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке вопросов: " + ex.Message);
                }
            }
        }

        // Обработка нажатия на кнопку "Продолжить"
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    foreach (var questionControl in questionControls)
                    {
                        ComboBoxItem selectedAnswer = questionControl.AnswersComboBox.SelectedItem as ComboBoxItem;
                        if (selectedAnswer != null)
                        {
                            int answerId = (int)selectedAnswer.Tag;

                            string query = "INSERT INTO PatientAnswers (PatientID, AnswerID) VALUES (@PatientID, @AnswerID)";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.Parameters.AddWithValue("@PatientID", patientId);
                            command.Parameters.AddWithValue("@AnswerID", answerId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении ответов: " + ex.Message);
                    return;
                }
            }

            // Открытие окна DiagnosesWindow
            DiagnosesWindow diagnosesWindow = new DiagnosesWindow(patientId);
            diagnosesWindow.Show();
            this.Close();
        }
    }

    // Вспомогательный класс для представления вопроса и вариантов ответов
    public class QuestionControl : StackPanel
    {
        public int QuestionId { get; private set; }
        public ComboBox AnswersComboBox { get; private set; }

        public QuestionControl(int questionId, string questionText)
        {
            this.QuestionId = questionId;
            TextBlock questionTextBlock = new TextBlock { Text = questionText, Margin = new Thickness(0, 10, 0, 5) };
            this.Children.Add(questionTextBlock);

            AnswersComboBox = new ComboBox { Width = 300 };
            this.Children.Add(AnswersComboBox);
        }
    }
}
