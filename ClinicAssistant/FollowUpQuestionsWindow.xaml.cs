using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    /// <summary>
    /// Логика взаимодействия для FollowUpQuestionsWindow.xaml
    /// </summary>
    public partial class FollowUpQuestionsWindow : Window
    {
        private readonly int patientId;
        private readonly DatabaseFacade dbFacade = new DatabaseFacade();
        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki2;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";
        //private readonly string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki8; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";

        // Коллекция для хранения вопросов
        public ObservableCollection<QuestionViewModel> Questions { get; set; } = new ObservableCollection<QuestionViewModel>();

        public FollowUpQuestionsWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            this.DataContext = this;
            LoadQuestionsAsync();
        }

        /// <summary>
        /// Загрузка вопросов на основе выбранных симптомов пациента
        /// </summary>
        private async void LoadQuestionsAsync()
        {
            try
            {
                // Получение связанных симптомов
                var symptomIds = dbFacade.GetNewPatientSymptoms(patientId);

                if (symptomIds.Count == 0)
                {
                    MessageBox.Show("У пациента нет связанных симптомов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    return;
                }

                // Получение вопросов для симптомов
                var questions = dbFacade.GetQuestionsBySymptoms(symptomIds);

                if (questions.Count == 0)
                {
                    MessageBox.Show("Нет наводящих вопросов для выбранных симптомов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    return;
                }

                // Загрузка ответов для каждого вопроса
                foreach (var question in questions)
                {
                    var answers = dbFacade.GetAnswersByQuestion(question.QuestionID);

                    if (answers.Count == 0)
                    {
                        continue; // Пропускаем вопросы без ответов
                    }

                    Questions.Add(new QuestionViewModel
                    {
                        QuestionID = question.QuestionID,
                        SymptomName = question.SymptomName,
                        QuestionText = question.QuestionText,
                        Answers = new ObservableCollection<AnswerModel>(answers)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке вопросов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Обработка нажатия на кнопку "Продолжить"
        /// </summary>
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, что все вопросы имеют выбранные ответы
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
                // Сохранение ответов в базе данных
                var selectedAnswerIds = new List<int>();
                foreach (var question in Questions)
                {
                    if (question.SelectedAnswerID.HasValue)
                    {
                        selectedAnswerIds.Add(question.SelectedAnswerID.Value);
                    }
                }

                dbFacade.SaveNewPatientAnswers(patientId, selectedAnswerIds);

                // Открытие окна DiagnosesWindow
                DiagnosesWindow diagnosesWindow = new DiagnosesWindow(patientId);
                diagnosesWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Модель данных для вопроса
        /// </summary>
        public class QuestionModel
        {
            public int QuestionID { get; set; }
            public int SymptomID { get; set; }
            public string SymptomName { get; set; }
            public string QuestionText { get; set; }
        }

        /// <summary>
        /// Модель данных для ответа
        /// </summary>
        public class AnswerModel
        {
            public int AnswerID { get; set; }
            public string AnswerText { get; set; }
        }

        /// <summary>
        /// ViewModel для отображения вопроса в интерфейсе
        /// </summary>
        public class QuestionViewModel : DependencyObject
        {
            public int QuestionID { get; set; }
            public string SymptomName { get; set; } // Имя симптома для группировки
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

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }
    }
}
