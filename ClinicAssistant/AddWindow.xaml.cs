using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    public partial class AddWindow : Window
    {
        private string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;user id=is;password=1;encrypt=False;";

        public AddWindow()
        {
            InitializeComponent();
            LoadSymptoms();
            LoadQuestions();
        }

        private void LoadSymptoms()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT SymptomID, SymptomName FROM Symptoms", connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    SymptomComboBox.ItemsSource = dataTable.DefaultView;
                    SymptomComboBox.DisplayMemberPath = "SymptomName";
                    SymptomComboBox.SelectedValuePath = "SymptomID";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки симптомов: " + ex.Message);
                }
            }
        }

        private void LoadQuestions()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT QuestionID, Question FROM FollowUpQuestions", connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    QuestionComboBox.ItemsSource = dataTable.DefaultView;
                    QuestionComboBox.DisplayMemberPath = "Question";
                    QuestionComboBox.SelectedValuePath = "QuestionID";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки вопросов: " + ex.Message);
                }
            }
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
            if (SymptomComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите симптом.");
                return;
            }

            int symptomID = (int)SymptomComboBox.SelectedValue;
            string questionText = QuestionTextTextBox.Text.Trim();

            if (string.IsNullOrEmpty(questionText))
            {
                MessageBox.Show("Введите текст наводящего вопроса.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
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
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления наводящего вопроса: " + ex.Message);
                }
            }
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите вопрос.");
                return;
            }

            int questionID = (int)QuestionComboBox.SelectedValue;
            string answerText = AnswerTextTextBox.Text.Trim();

            if (string.IsNullOrEmpty(answerText))
            {
                MessageBox.Show("Введите текст ответа.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO Answers (QuestionID, Answer) VALUES (@QuestionID, @Answer)", connection);
                    command.Parameters.AddWithValue("@QuestionID", questionID);
                    command.Parameters.AddWithValue("@Answer", answerText);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Ответ успешно добавлен.");
                    AnswerTextTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления ответа: " + ex.Message);
                }
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