using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    /// <summary>
    /// Логика взаимодействия для DeleteWindow.xaml
    /// </summary>
    public partial class DeleteWindow : Window
    {
        //private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki8;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";
        private readonly string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki8;persist security info=True;user id=is;password=1;MultipleActiveResultSets=True;App=EntityFramework";

        public DeleteWindow()
        {
            InitializeComponent();

            // Загрузка данных при инициализации окна (запускаем асинхронно)
            _ = LoadSymptomsAsync();
            _ = LoadFollowUpQuestionsAsync();
            _ = LoadAnswersAsync();
            _ = LoadDiagnosesAsync();
            _ = LoadDoctorsAsync();
        }

        #region Загрузка данных в DataGrid (Асинхронные версии)

        private async Task LoadSymptomsAsync(string search = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

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

                        DataTable dt = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        SymptomsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке симптомов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadFollowUpQuestionsAsync(string search = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

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

                        DataTable dt = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        QuestionsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке вопросов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAnswersAsync(string search = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

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

                        DataTable dt = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        AnswersDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке ответов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDiagnosesAsync(string search = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

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

                        DataTable dt = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        DiagnosesDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке диагнозов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDoctorsAsync(string search = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

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

                        DataTable dt = new DataTable();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        DoctorsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке врачей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики событий поиска (асинхронные вызовы методов загрузки)

        private async void SearchSymptomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadSymptomsAsync(SearchSymptomTextBox.Text.Trim());
        }

        private async void SearchQuestionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadFollowUpQuestionsAsync(SearchQuestionTextBox.Text.Trim());
        }

        private async void SearchAnswerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadAnswersAsync(SearchAnswerTextBox.Text.Trim());
        }

        private async void SearchDiagnosisTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadDiagnosesAsync(SearchDiagnosisTextBox.Text.Trim());
        }

        private async void SearchDoctorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await LoadDoctorsAsync(SearchDoctorTextBox.Text.Trim());
        }

        #endregion

        #region Обработчики событий одиночного удаления (асинхронные)

        /// <summary>
        /// Удаление одного симптома
        /// </summary>
        private async void DeleteSymptom_Click(object sender, RoutedEventArgs e)
        {
            // Вместо "is not" используем старый синтаксис:
            if (!(sender is Button btn)) return;

            if (!int.TryParse(btn.Tag.ToString(), out int symptomID))
            {
                MessageBox.Show("Неверный идентификатор симптома.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот симптом и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    await DeleteSymptomByIDAsync(symptomID, conn, transaction);

                    transaction.Commit();
                    MessageBox.Show("Симптом и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadSymptomsAsync(SearchSymptomTextBox.Text.Trim());
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
        private async void DeleteFollowUpQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (!int.TryParse(btn.Tag.ToString(), out int questionID))
            {
                MessageBox.Show("Неверный идентификатор вопроса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот вопрос и все связанные ответы?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    await DeleteQuestionByIDAsync(questionID, conn, transaction);

                    transaction.Commit();
                    MessageBox.Show("Вопрос и связанные ответы успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadFollowUpQuestionsAsync(SearchQuestionTextBox.Text.Trim());
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
        private async void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (!int.TryParse(btn.Tag.ToString(), out int answerID))
            {
                MessageBox.Show("Неверный идентификатор ответа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот ответ и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    await DeleteAnswerByIDAsync(answerID, conn, transaction);

                    transaction.Commit();
                    MessageBox.Show("Ответ и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadAnswersAsync(SearchAnswerTextBox.Text.Trim());
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
        private async void DeleteDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (!int.TryParse(btn.Tag.ToString(), out int diagnosisID))
            {
                MessageBox.Show("Неверный идентификатор диагноза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот диагноз и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    await DeleteDiagnosisByIDAsync(diagnosisID, conn, transaction);

                    transaction.Commit();
                    MessageBox.Show("Диагноз и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDiagnosesAsync(SearchDiagnosisTextBox.Text.Trim());
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
        private async void DeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (!int.TryParse(btn.Tag.ToString(), out int doctorID))
            {
                MessageBox.Show("Неверный идентификатор врача.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этого врача и все связанные записи?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    await DeleteDoctorByIDAsync(doctorID, conn, transaction);

                    transaction.Commit();
                    MessageBox.Show("Врач и связанные записи успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDoctorsAsync(SearchDoctorTextBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении врача: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Обработчики событий массового удаления (асинхронные)

        private async void DeleteSelectedSymptoms_Click(object sender, RoutedEventArgs e)
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
            {
                return;
            }

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
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int id in symptomIdsToDelete)
                    {
                        await DeleteSymptomByIDAsync(id, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные симптомы успешно удалены со всеми связанными записями.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LoadSymptomsAsync(SearchSymptomTextBox.Text.Trim());
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

        private async void DeleteSelectedQuestions_Click(object sender, RoutedEventArgs e)
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
            {
                return;
            }

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
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int qId in questionIdsToDelete)
                    {
                        await DeleteQuestionByIDAsync(qId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные вопросы успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LoadFollowUpQuestionsAsync(SearchQuestionTextBox.Text.Trim());
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

        private async void DeleteSelectedAnswers_Click(object sender, RoutedEventArgs e)
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
            {
                return;
            }

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
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int aId in answerIdsToDelete)
                    {
                        await DeleteAnswerByIDAsync(aId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные ответы успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LoadAnswersAsync(SearchAnswerTextBox.Text.Trim());
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

        private async void DeleteSelectedDiagnoses_Click(object sender, RoutedEventArgs e)
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
            {
                return;
            }

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
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int dId in diagIdsToDelete)
                    {
                        await DeleteDiagnosisByIDAsync(dId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные диагнозы успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LoadDiagnosesAsync(SearchDiagnosisTextBox.Text.Trim());
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

        private async void DeleteSelectedDoctors_Click(object sender, RoutedEventArgs e)
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
            {
                return;
            }

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
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (int docId in doctorIdsToDelete)
                    {
                        await DeleteDoctorByIDAsync(docId, conn, transaction);
                    }

                    transaction.Commit();
                    MessageBox.Show("Выбранные врачи успешно удалены.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LoadDoctorsAsync(SearchDoctorTextBox.Text.Trim());
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

        #region Вспомогательные методы удаления (асинхронные)

        private async Task DeleteSymptomByIDAsync(int symptomID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаление из PatientSymptoms
            string deletePatientSymptoms = "DELETE FROM PatientSymptoms WHERE SymptomID = @SymptomID";
            using (SqlCommand cmd = new SqlCommand(deletePatientSymptoms, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Получаем все связанные вопросы
            string getQuestions = "SELECT QuestionID FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
            DataTable questionsTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getQuestions, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    questionsTable.Load(reader);
                }
            }

            // Для каждого вопроса удаляем ответы
            foreach (DataRow row in questionsTable.Rows)
            {
                int questionID = Convert.ToInt32(row["QuestionID"]);
                await DeleteAnswersByQuestionIDAsync(questionID, conn, transaction);
            }

            // Удаляем из FollowUpQuestions
            string deleteFollowUpQuestions = "DELETE FROM FollowUpQuestions WHERE SymptomID = @SymptomID";
            using (SqlCommand cmd = new SqlCommand(deleteFollowUpQuestions, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаляем сам симптом
            string deleteSymptom = "DELETE FROM Symptoms WHERE SymptomID = @SymptomID";
            using (SqlCommand cmd = new SqlCommand(deleteSymptom, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SymptomID", symptomID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DeleteQuestionByIDAsync(int questionID, SqlConnection conn, SqlTransaction transaction)
        {
            // Получаем все связанные Answers
            string getAnswers = "SELECT AnswerID FROM Answers WHERE QuestionID = @QuestionID";
            DataTable answersTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    answersTable.Load(reader);
                }
            }

            // Для каждого ответа удаляем связанные записи
            foreach (DataRow row in answersTable.Rows)
            {
                int answerID = Convert.ToInt32(row["AnswerID"]);
                await DeleteAnswerRelatedRecordsAsync(answerID, conn, transaction);
            }

            // Удаляем все ответы для данного вопроса
            string deleteAnswers = "DELETE FROM Answers WHERE QuestionID = @QuestionID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаляем сам вопрос
            string deleteQuestion = "DELETE FROM FollowUpQuestions WHERE QuestionID = @QuestionID";
            using (SqlCommand cmd = new SqlCommand(deleteQuestion, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DeleteAnswerByIDAsync(int answerID, SqlConnection conn, SqlTransaction transaction)
        {
            // Сначала удаляем связанные записи
            await DeleteAnswerRelatedRecordsAsync(answerID, conn, transaction);

            // Удаляем сам ответ
            string deleteAnswer = "DELETE FROM Answers WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswer, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DeleteDiagnosisByIDAsync(int diagnosisID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаляем из AnswerDiagnoses
            string deleteAnswerDiagnoses = "DELETE FROM AnswerDiagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswerDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаляем из DoctorDiagnoses
            string deleteDoctorDiagnoses = "DELETE FROM DoctorDiagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deleteDoctorDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаляем из PatientDiagnoses
            string deletePatientDiagnoses = "DELETE FROM PatientDiagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deletePatientDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаляем сам диагноз
            string deleteDiagnosis = "DELETE FROM Diagnoses WHERE DiagnosisID = @DiagnosisID";
            using (SqlCommand cmd = new SqlCommand(deleteDiagnosis, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DiagnosisID", diagnosisID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DeleteDoctorByIDAsync(int doctorID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаление из DoctorDiagnoses
            string deleteDoctorDiagnoses = "DELETE FROM DoctorDiagnoses WHERE DoctorID = @DoctorID";
            using (SqlCommand cmd = new SqlCommand(deleteDoctorDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаление самого врача
            string deleteDoctor = "DELETE FROM Doctors WHERE DoctorID = @DoctorID";
            using (SqlCommand cmd = new SqlCommand(deleteDoctor, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DeleteAnswerRelatedRecordsAsync(int answerID, SqlConnection conn, SqlTransaction transaction)
        {
            // Удаляем из AnswerDiagnoses
            string deleteAnswerDiagnoses = "DELETE FROM AnswerDiagnoses WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswerDiagnoses, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                await cmd.ExecuteNonQueryAsync();
            }

            // Удаляем из PatientAnswers
            string deletePatientAnswers = "DELETE FROM PatientAnswers WHERE AnswerID = @AnswerID";
            using (SqlCommand cmd = new SqlCommand(deletePatientAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@AnswerID", answerID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task DeleteAnswersByQuestionIDAsync(int questionID, SqlConnection conn, SqlTransaction transaction)
        {
            // Получаем все ответы по данному QuestionID
            string getAnswers = "SELECT AnswerID FROM Answers WHERE QuestionID = @QuestionID";
            DataTable answersTable = new DataTable();
            using (SqlCommand cmd = new SqlCommand(getAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    answersTable.Load(reader);
                }
            }

            // Для каждого ответа удаляем связанные записи
            foreach (DataRow row in answersTable.Rows)
            {
                int answerID = Convert.ToInt32(row["AnswerID"]);
                await DeleteAnswerRelatedRecordsAsync(answerID, conn, transaction);
            }

            // Удаляем сами ответы
            string deleteAnswers = "DELETE FROM Answers WHERE QuestionID = @QuestionID";
            using (SqlCommand cmd = new SqlCommand(deleteAnswers, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@QuestionID", questionID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        #endregion

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}