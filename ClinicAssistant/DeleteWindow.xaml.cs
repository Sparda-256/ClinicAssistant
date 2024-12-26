using System;
using System.Collections.Generic;
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
        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki8;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        //private readonly string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";

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

        private void LoadAnswers(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT a.AnswerID, fq.Question, a.Answer 
                                 FROM Answers a
                                 INNER JOIN FollowUpQuestions fq ON a.QuestionID = fq.QuestionID";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE a.Answer LIKE @search OR fq.Question LIKE @search";
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

        #region Обработчики событий одиночного удаления

        /// <summary>
        /// Удаление одного симптома
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

            // Подтверждение
            if (MessageBox.Show("Вы уверены, что хотите удалить этот симптом и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Удаляем все данные, связанные с данным SymptomID
                    DeleteSymptomByID(symptomID, conn, transaction);

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
        /// Удаление одного наводящего вопроса
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

            // Подтверждение
            if (MessageBox.Show("Вы уверены, что хотите удалить этот вопрос и все связанные ответы?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    DeleteQuestionByID(questionID, conn, transaction);

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
        /// Удаление одного ответа
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

            // Подтверждение
            if (MessageBox.Show("Вы уверены, что хотите удалить этот ответ и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    DeleteAnswerByID(answerID, conn, transaction);

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
        /// Удаление одного диагноза
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

            // Подтверждение
            if (MessageBox.Show("Вы уверены, что хотите удалить этот диагноз и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    DeleteDiagnosisByID(diagnosisID, conn, transaction);

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
        /// Удаление одного врача
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

            // Подтверждение
            if (MessageBox.Show("Вы уверены, что хотите удалить этого врача и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    DeleteDoctorByID(doctorID, conn, transaction);

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

        #region Обработчики событий массового удаления

        /// <summary>
        /// Массовое удаление симптомов
        /// </summary>
        private void DeleteSelectedSymptoms_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SymptomsDataGrid.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного симптома для удаления.",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранные симптомы и все связанные с ними записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            // Собираем ID всех выбранных симптомов
            List<int> symptomIdsToDelete = new List<int>();
            foreach (var item in selectedItems)
            {
                if (item is DataRowView rowView)
                {
                    if (int.TryParse(rowView["SymptomID"].ToString(), out int symptomID))
                    {
                        symptomIdsToDelete.Add(symptomID);
                    }
                }
            }

            if (symptomIdsToDelete.Count == 0)
            {
                MessageBox.Show("Невозможно определить идентификаторы выбранных симптомов.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int id in symptomIdsToDelete)
                    {
                        DeleteSymptomByID(id, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные симптомы успешно удалены со всеми связанными записями.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadSymptoms(SearchSymptomTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при массовом удалении симптомов: " + ex.Message,
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Массовое удаление наводящих вопросов
        /// </summary>
        private void DeleteSelectedQuestions_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = QuestionsDataGrid.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного вопроса для удаления.",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранные вопросы и все связанные ответы?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            List<int> questionIdsToDelete = new List<int>();
            foreach (var item in selectedItems)
            {
                if (item is DataRowView rowView)
                {
                    if (int.TryParse(rowView["QuestionID"].ToString(), out int questionID))
                    {
                        questionIdsToDelete.Add(questionID);
                    }
                }
            }

            if (questionIdsToDelete.Count == 0)
            {
                MessageBox.Show("Невозможно определить идентификаторы выбранных вопросов.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int qId in questionIdsToDelete)
                    {
                        DeleteQuestionByID(qId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные вопросы успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadFollowUpQuestions(SearchQuestionTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при массовом удалении вопросов: " + ex.Message,
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Массовое удаление ответов
        /// </summary>
        private void DeleteSelectedAnswers_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = AnswersDataGrid.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного ответа для удаления.",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранные ответы и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            List<int> answerIdsToDelete = new List<int>();
            foreach (var item in selectedItems)
            {
                if (item is DataRowView rowView)
                {
                    if (int.TryParse(rowView["AnswerID"].ToString(), out int answerID))
                    {
                        answerIdsToDelete.Add(answerID);
                    }
                }
            }

            if (answerIdsToDelete.Count == 0)
            {
                MessageBox.Show("Невозможно определить идентификаторы выбранных ответов.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int aId in answerIdsToDelete)
                    {
                        DeleteAnswerByID(aId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные ответы успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadAnswers(SearchAnswerTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при массовом удалении ответов: " + ex.Message,
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Массовое удаление диагнозов
        /// </summary>
        private void DeleteSelectedDiagnoses_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DiagnosesDataGrid.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного диагноза для удаления.",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранные диагнозы и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            List<int> diagIdsToDelete = new List<int>();
            foreach (var item in selectedItems)
            {
                if (item is DataRowView rowView)
                {
                    if (int.TryParse(rowView["DiagnosisID"].ToString(), out int diagnosisID))
                    {
                        diagIdsToDelete.Add(diagnosisID);
                    }
                }
            }

            if (diagIdsToDelete.Count == 0)
            {
                MessageBox.Show("Невозможно определить идентификаторы выбранных диагнозов.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int dId in diagIdsToDelete)
                    {
                        DeleteDiagnosisByID(dId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные диагнозы успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadDiagnoses(SearchDiagnosisTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при массовом удалении диагнозов: " + ex.Message,
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Массовое удаление врачей
        /// </summary>
        private void DeleteSelectedDoctors_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DoctorsDataGrid.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного врача для удаления.",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранных врачей и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            List<int> doctorIdsToDelete = new List<int>();
            foreach (var item in selectedItems)
            {
                if (item is DataRowView rowView)
                {
                    if (int.TryParse(rowView["DoctorID"].ToString(), out int doctorID))
                    {
                        doctorIdsToDelete.Add(doctorID);
                    }
                }
            }

            if (doctorIdsToDelete.Count == 0)
            {
                MessageBox.Show("Невозможно определить идентификаторы выбранных врачей.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int docId in doctorIdsToDelete)
                    {
                        DeleteDoctorByID(docId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные врачи успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    LoadDoctors(SearchDoctorTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при массовом удалении врачей: " + ex.Message,
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Вспомогательные методы удаления

        /// <summary>
        /// Удаление симптома и связанных с ним данных по ID
        /// </summary>
        private void DeleteSymptomByID(int symptomID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаление из PatientSymptoms
            string deletePatientSymptoms = "DELETE FROM PatientSymptoms WHERE SymptomID = @SymptomID";
            using (SqlCommand cmd = new SqlCommand(deletePatientSymptoms, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                cmd.ExecuteNonQuery();
            }

            // Получаем все связанные вопросы
            string getQuestions = "SELECT QuestionID FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
            DataTable questionsTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getQuestions, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(questionsTable);
            }

            // Для каждого вопроса удаляем ответы
            foreach (DataRow row in questionsTable.Rows)
            {
                int questionID = Convert.ToInt32(row["QuestionID"]);
                DeleteAnswersByQuestionID(questionID, conn, transaction);
            }

            // Удаляем из FollowUpQuestions
            string deleteFollowUpQuestions = "DELETE FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
            using (SqlCommand cmd = new SqlCommand(deleteFollowUpQuestions, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                cmd.ExecuteNonQuery();
            }

            // Удаляем сам симптом
            string deleteSymptom = "DELETE FROM Symptoms WHERE SymptomID = @SymptomID";
            using (SqlCommand cmd = new SqlCommand(deleteSymptom, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаление вопроса и связанных ответов по QuestionID
        /// </summary>
        private void DeleteQuestionByID(int questionID, SqlConnection conn, SqlTransaction transaction)
        {
            // Получаем все связанные Answers
            string getAnswers = "SELECT AnswerID FROM Answers WHERE QuestionID = @QuestionID";
            DataTable answersTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(answersTable);
            }

            // Для каждого ответа удаляем связанные записи
            foreach (DataRow row in answersTable.Rows)
            {
                int answerID = Convert.ToInt32(row["AnswerID"]);
                DeleteAnswerRelatedRecords(answerID, conn, transaction);
            }

            // Удаляем все ответы для данного вопроса
            string deleteAnswers = "DELETE FROM Answers WHERE QuestionID = @QuestionID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                cmd.ExecuteNonQuery();
            }

            // Удаляем сам вопрос
            string deleteQuestion = "DELETE FROM FollowUpQuestions WHERE QuestionID = @QuestionID";
            using (SqlCommand cmd = new SqlCommand(deleteQuestion, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаление ответа и связанных с ним записей
        /// </summary>
        private void DeleteAnswerByID(int answerID, SqlConnection conn, SqlTransaction transaction)
        {
            // Сначала удаляем связанные записи из AnswerDiagnoses и PatientAnswers
            DeleteAnswerRelatedRecords(answerID, conn, transaction);

            // Удаляем сам ответ
            string deleteAnswer = "DELETE FROM Answers WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswer, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаление диагноза и связанных с ним записей
        /// </summary>
        private void DeleteDiagnosisByID(int diagnosisID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаляем из AnswerDiagnoses
            string deleteAnswerDiagnoses = "DELETE FROM AnswerDiagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswerDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                cmd.ExecuteNonQuery();
            }

            // Удаляем из DoctorDiagnoses
            string deleteDoctorDiagnoses = "DELETE FROM DoctorDiagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deleteDoctorDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                cmd.ExecuteNonQuery();
            }

            // Удаляем из PatientDiagnoses
            string deletePatientDiagnoses = "DELETE FROM PatientDiagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deletePatientDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                cmd.ExecuteNonQuery();
            }

            // Удаляем сам диагноз
            string deleteDiagnosis = "DELETE FROM Diagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deleteDiagnosis, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаление врача и связанных с ним записей
        /// </summary>
        private void DeleteDoctorByID(int doctorID, SqlConnection conn, SqlTransaction transaction)
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
        }

        /// <summary>
        /// Удаление связанных записей из AnswerDiagnoses и PatientAnswers
        /// </summary>
        private void DeleteAnswerRelatedRecords(int answerID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаляем из AnswerDiagnoses
            string deleteAnswerDiagnoses = "DELETE FROM AnswerDiagnoses WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswerDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                cmd.ExecuteNonQuery();
            }

            // Удаляем из PatientAnswers
            string deletePatientAnswers = "DELETE FROM PatientAnswers WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deletePatientAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаление ответов по QuestionID (используется при удалении симптома и его связанных вопросов)
        /// </summary>
        private void DeleteAnswersByQuestionID(int questionID, SqlConnection conn, SqlTransaction transaction)
        {
            // Получаем все ответы по данному QuestionID
            string getAnswers = "SELECT AnswerID FROM Answers WHERE QuestionID = @QuestionID";
            DataTable answersTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(answersTable);
            }

            // Для каждого ответа удаляем связанные записи
            foreach (DataRow row in answersTable.Rows)
            {
                int answerID = Convert.ToInt32(row["AnswerID"]);
                DeleteAnswerRelatedRecords(answerID, conn, transaction);
            }

            // Удаляем сами ответы
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