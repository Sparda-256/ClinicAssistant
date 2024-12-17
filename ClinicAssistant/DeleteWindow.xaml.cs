using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    /// <summary>
    /// Логика взаимодействия для DeleteWindow.xaml
    /// </summary>
    public partial class DeleteWindow : Window
    {
        // Строка подключения к базе данных
        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki4;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        public DeleteWindow()
        {
            InitializeComponent();
            // Загрузка данных при инициализации окна
            LoadSymptoms();
            LoadFollowUpQuestions();
            LoadAnswers();
            LoadDiagnoses();
            LoadDoctors();
        }

        #region Загрузка данных в DataGrid

        /// <summary>
        /// Загрузка симптомов с возможностью поиска
        /// </summary>
        private void LoadSymptoms(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SymptomID, SymptomName FROM Symptoms";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE SymptomName LIKE @search";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    SymptomsDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        /// <summary>
        /// Загрузка наводящих вопросов с возможностью поиска
        /// </summary>
        private void LoadFollowUpQuestions(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT fq.QuestionID, fq.Question, s.SymptomName 
                                 FROM FollowUpQuestions fq
                                 INNER JOIN Symptoms s ON fq.SymptomID = s.SymptomID";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE fq.Question LIKE @search";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    QuestionsDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        /// <summary>
        /// Загрузка ответов с возможностью поиска
        /// </summary>
        private void LoadAnswers(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT a.AnswerID, fq.Question, a.Answer 
                                 FROM Answers a
                                 INNER JOIN FollowUpQuestions fq ON a.QuestionID = fq.QuestionID";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE a.Answer LIKE @search";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    AnswersDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        /// <summary>
        /// Загрузка диагнозов с возможностью поиска
        /// </summary>
        private void LoadDiagnoses(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DiagnosisID, DiagnosisName FROM Diagnoses";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE DiagnosisName LIKE @search";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DiagnosesDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        /// <summary>
        /// Загрузка врачей с возможностью поиска
        /// </summary>
        private void LoadDoctors(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DoctorID, FullName FROM Doctors";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE FullName LIKE @search";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DoctorsDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        #endregion

        #region Обработчики событий поиска

        private void SearchSymptomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSymptoms(SearchSymptomTextBox.Text.Trim());
        }

        private void SearchQuestionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadFollowUpQuestions(SearchQuestionTextBox.Text.Trim());
        }

        private void SearchAnswerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAnswers(SearchAnswerTextBox.Text.Trim());
        }

        private void SearchDiagnosisTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDiagnoses(SearchDiagnosisTextBox.Text.Trim());
        }

        private void SearchDoctorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDoctors(SearchDoctorTextBox.Text.Trim());
        }

        #endregion

        #region Обработчики событий удаления

        /// <summary>
        /// Удаление Симптома и связанных записей
        /// </summary>
        private void DeleteSymptom_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int symptomID))
            {
                MessageBox.Show("Неверный идентификатор симптома.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Вы уверены, что хотите удалить этот симптом и все связанные записи?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Удаление из PatientSymptoms
                    string deletePatientSymptoms = "DELETE FROM PatientSymptoms WHERE SymptomID = @SymptomID";
                    using (SqlCommand cmd = new SqlCommand(deletePatientSymptoms, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                        cmd.ExecuteNonQuery();
                    }

                    // Получение всех связанных QuestionID для удаления из FollowUpQuestions и Answers
                    string getQuestions = "SELECT QuestionID FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
                    DataTable questionsTable = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(getQuestions, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(questionsTable);
                    }

                    foreach (DataRow row in questionsTable.Rows)
                    {
                        int questionID = Convert.ToInt32(row["QuestionID"]);

                        // Удаление из Answers и связанных таблиц
                        DeleteAnswersByQuestionID(questionID, conn, transaction);
                    }

                    // Удаление из FollowUpQuestions
                    string deleteFollowUpQuestions = "DELETE FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
                    using (SqlCommand cmd = new SqlCommand(deleteFollowUpQuestions, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаление самого симптома
                    string deleteSymptom = "DELETE FROM Symptoms WHERE SymptomID = @SymptomID";
                    using (SqlCommand cmd = new SqlCommand(deleteSymptom, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Симптом и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSymptoms(SearchSymptomTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении симптома: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Удаление Наводящего Вопроса и связанных записей
        /// </summary>
        private void DeleteFollowUpQuestion_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int questionID))
            {
                MessageBox.Show("Неверный идентификатор вопроса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Вы уверены, что хотите удалить этот вопрос и все связанные ответы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Получение всех связанных AnswerID для удаления из Answers и связанных таблиц
                    string getAnswers = "SELECT AnswerID FROM Answers WHERE QuestionID = @QuestionID";
                    DataTable answersTable = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(getAnswers, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", questionID);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(answersTable);
                    }

                    foreach (DataRow row in answersTable.Rows)
                    {
                        int answerID = Convert.ToInt32(row["AnswerID"]);
                        // Удаление из AnswerDiagnoses и PatientAnswers
                        DeleteAnswerRelatedRecords(answerID, conn, transaction);
                    }

                    // Удаление из Answers
                    string deleteAnswers = "DELETE FROM Answers WHERE QuestionID = @QuestionID";
                    using (SqlCommand cmd = new SqlCommand(deleteAnswers, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", questionID);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаление самого вопроса
                    string deleteQuestion = "DELETE FROM FollowUpQuestions WHERE QuestionID = @QuestionID";
                    using (SqlCommand cmd = new SqlCommand(deleteQuestion, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", questionID);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Вопрос и связанные ответы успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadFollowUpQuestions(SearchQuestionTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении вопроса: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Удаление Ответа и связанных записей
        /// </summary>
        private void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int answerID))
            {
                MessageBox.Show("Неверный идентификатор ответа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Вы уверены, что хотите удалить этот ответ и все связанные записи?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Удаление связанных записей из AnswerDiagnoses и PatientAnswers
                    DeleteAnswerRelatedRecords(answerID, conn, transaction);

                    // Удаление самого ответа
                    string deleteAnswer = "DELETE FROM Answers WHERE AnswerID = @AnswerID";
                    using (SqlCommand cmd = new SqlCommand(deleteAnswer, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@AnswerID", answerID);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Ответ и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAnswers(SearchAnswerTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении ответа: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Удаление Диагноза и связанных записей
        /// </summary>
        private void DeleteDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int diagnosisID))
            {
                MessageBox.Show("Неверный идентификатор диагноза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Вы уверены, что хотите удалить этот диагноз и все связанные записи?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Удаление из AnswerDiagnoses
                    string deleteAnswerDiagnoses = "DELETE FROM AnswerDiagnoses WHERE DiagnosisID = @DiagnosisID";
                    using (SqlCommand cmd = new SqlCommand(deleteAnswerDiagnoses, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаление из DoctorDiagnoses
                    string deleteDoctorDiagnoses = "DELETE FROM DoctorDiagnoses WHERE DiagnosisID = @DiagnosisID";
                    using (SqlCommand cmd = new SqlCommand(deleteDoctorDiagnoses, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаление из PatientDiagnoses
                    string deletePatientDiagnoses = "DELETE FROM PatientDiagnoses WHERE DiagnosisID = @DiagnosisID";
                    using (SqlCommand cmd = new SqlCommand(deletePatientDiagnoses, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаление самого диагноза
                    string deleteDiagnosis = "DELETE FROM Diagnoses WHERE DiagnosisID = @DiagnosisID";
                    using (SqlCommand cmd = new SqlCommand(deleteDiagnosis, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Диагноз и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDiagnoses(SearchDiagnosisTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении диагноза: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Удаление Врача и связанных записей
        /// </summary>
        private void DeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int doctorID))
            {
                MessageBox.Show("Неверный идентификатор врача.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            if (MessageBox.Show("Вы уверены, что хотите удалить этого врача и все связанные записи?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Удаление из DoctorDiagnoses
                    string deleteDoctorDiagnoses = "DELETE FROM DoctorDiagnoses WHERE DoctorID = @DoctorID";
                    using (SqlCommand cmd = new SqlCommand(deleteDoctorDiagnoses, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@DoctorID", doctorID);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаление самого врача
                    string deleteDoctor = "DELETE FROM Doctors WHERE DoctorID = @DoctorID";
                    using (SqlCommand cmd = new SqlCommand(deleteDoctor, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@DoctorID", doctorID);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Врач и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDoctors(SearchDoctorTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении врача: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Удаление связанных записей для AnswerID
        /// </summary>
        private void DeleteAnswerRelatedRecords(int answerID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаление из AnswerDiagnoses
            string deleteAnswerDiagnoses = "DELETE FROM AnswerDiagnoses WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswerDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                cmd.ExecuteNonQuery();
            }

            // Удаление из PatientAnswers
            string deletePatientAnswers = "DELETE FROM PatientAnswers WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deletePatientAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаление связанных записей для QuestionID (используется при удалении симптома)
        /// </summary>
        private void DeleteAnswersByQuestionID(int questionID, SqlConnection conn, SqlTransaction transaction)
        {
            // Получение всех связанных AnswerID
            string getAnswers = "SELECT AnswerID FROM Answers WHERE QuestionID = @QuestionID";
            DataTable answersTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(answersTable);
            }

            foreach (DataRow row in answersTable.Rows)
            {
                int answerID = Convert.ToInt32(row["AnswerID"]);
                DeleteAnswerRelatedRecords(answerID, conn, transaction);
            }

            // Удаление из Answers
            string deleteAnswers = "DELETE FROM Answers WHERE QuestionID = @QuestionID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
