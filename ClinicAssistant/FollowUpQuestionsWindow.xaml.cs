using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class FollowUpQuestionsWindow : Window
    {
        private readonly int patientId;
        private readonly string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        public ObservableCollection<QuestionViewModel> Questions { get; set; } = new ObservableCollection<QuestionViewModel>();

        public FollowUpQuestionsWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            this.DataContext = this;
            LoadQuestionsAsync();
        }

        private async void LoadQuestionsAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string symptomQuery = "SELECT SymptomID FROM PatientSymptoms WHERE PatientID = @PatientID";
                    SqlCommand symptomCommand = new SqlCommand(symptomQuery, connection);
                    symptomCommand.Parameters.AddWithValue("@PatientID", patientId);

                    List<int> symptomIds = new List<int>();
                    using (SqlDataReader symptomReader = await symptomCommand.ExecuteReaderAsync())
                    {
                        while (await symptomReader.ReadAsync())
                        {
                            symptomIds.Add(symptomReader.GetInt32(symptomReader.GetOrdinal("SymptomID")));
                        }
                    }

                    if (symptomIds.Count == 0)
                    {
                        MessageBox.Show("У пациента нет связанных симптомов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                        return;
                    }

                    string questionQuery = @"
                        SELECT QuestionID, SymptomID, Question 
                        FROM FollowUpQuestions 
                        WHERE SymptomID IN (" + string.Join(",", symptomIds) + ")";

                    SqlCommand questionCommand = new SqlCommand(questionQuery, connection);
                    List<QuestionModel> questions = new List<QuestionModel>();

                    using (SqlDataReader questionReader = await questionCommand.ExecuteReaderAsync())
                    {
                        while (await questionReader.ReadAsync())
                        {
                            questions.Add(new QuestionModel
                            {
                                QuestionID = questionReader.GetInt32(questionReader.GetOrdinal("QuestionID")),
                                SymptomID = questionReader.GetInt32(questionReader.GetOrdinal("SymptomID")),
                                QuestionText = questionReader.GetString(questionReader.GetOrdinal("Question"))
                            });
                        }
                    }

                    if (questions.Count == 0)
                    {
                        MessageBox.Show("Нет наводящих вопросов для выбранных симптомов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                        return;
                    }

                    foreach (var question in questions)
                    {
                        string answerQuery = "SELECT AnswerID, Answer FROM Answers WHERE QuestionID = @QuestionID";
                        SqlCommand answerCommand = new SqlCommand(answerQuery, connection);
                        answerCommand.Parameters.AddWithValue("@QuestionID", question.QuestionID);

                        List<AnswerModel> answers = new List<AnswerModel>();
                        using (SqlDataReader answerReader = await answerCommand.ExecuteReaderAsync())
                        {
                            while (await answerReader.ReadAsync())
                            {
                                answers.Add(new AnswerModel
                                {
                                    AnswerID = answerReader.GetInt32(answerReader.GetOrdinal("AnswerID")),
                                    AnswerText = answerReader.GetString(answerReader.GetOrdinal("Answer"))
                                });
                            }
                        }

                        if (answers.Count == 0)
                        {
                            continue;
                        }

                        Questions.Add(new QuestionViewModel
                        {
                            QuestionID = question.QuestionID,
                            QuestionText = question.QuestionText,
                            Answers = new ObservableCollection<AnswerModel>(answers)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке вопросов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }


        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var question in Questions)
            {
                if (question.SelectedAnswerID == null)
                {
                    MessageBox.Show($"Пожалуйста, ответьте на вопрос: \"{question.QuestionText}\"", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string insertQuery = "INSERT INTO PatientAnswers (PatientID, AnswerID) VALUES (@PatientID, @AnswerID)";
                            SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction);
                            insertCommand.Parameters.Add("@PatientID", System.Data.SqlDbType.Int).Value = patientId;
                            insertCommand.Parameters.Add("@AnswerID", System.Data.SqlDbType.Int);

                            foreach (var question in Questions)
                            {
                                insertCommand.Parameters["@AnswerID"].Value = question.SelectedAnswerID.Value;
                                await insertCommand.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                        }
                        catch (Exception exInner)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Ошибка при сохранении ответов: " + exInner.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                DiagnosesWindow diagnosesWindow = new DiagnosesWindow(patientId);
                diagnosesWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class QuestionModel
    {
        public int QuestionID { get; set; }
        public int SymptomID { get; set; }
        public string QuestionText { get; set; }
    }

    public class AnswerModel
    {
        public int AnswerID { get; set; }
        public string AnswerText { get; set; }
    }

    public class QuestionViewModel : DependencyObject
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public ObservableCollection<AnswerModel> Answers { get; set; }

        public int? SelectedAnswerID
        {
            get { return (int?)GetValue(SelectedAnswerIDProperty); }
            set { SetValue(SelectedAnswerIDProperty, value); }
        }

        public static readonly DependencyProperty SelectedAnswerIDProperty =
            DependencyProperty.Register("SelectedAnswerID", typeof(int?), typeof(QuestionViewModel), new PropertyMetadata(null));
    }
}