using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ClinicAssistant
{
    /// <summary>
    /// Логика взаимодействия для SymptomChooseWindow.xaml
    /// </summary>
    public partial class SymptomChooseWindow : Window
    {
        private readonly int patientId;

        //private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki8;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        private readonly string connectionString = "data source = 192.168.147.54; initial catalog = PomoshnikPolicliniki8; persist security info=True;user id =is; password=1;MultipleActiveResultSets=True;App=EntityFramework";

        // Коллекция для хранения симптомов
        private ObservableCollection<Symptom> allSymptoms = new ObservableCollection<Symptom>();

        // Коллекция для хранения выбранных симптомов
        private HashSet<int> selectedSymptomIds = new HashSet<int>();

        public SymptomChooseWindow(int patientId)
        {
            InitializeComponent();
            this.patientId = patientId;
            SymptomsListBox.ItemsSource = allSymptoms;
            LoadSymptoms();
        }

        public class Symptom : INotifyPropertyChanged
        {
            private bool isSelected;

            public int SymptomID { get; set; }
            public string SymptomName { get; set; }

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void LoadSymptoms(string search = "")
        {
            try
            {
                DatabaseFacade dbFacade = new DatabaseFacade();
                var symptoms = dbFacade.GetSymptoms(search);

                allSymptoms.Clear();
                foreach (var (id, name) in symptoms)
                {
                    Symptom symptom = new Symptom
                    {
                        SymptomID = id,
                        SymptomName = name,
                        IsSelected = selectedSymptomIds.Contains(id)
                    };

                    symptom.PropertyChanged += Symptom_PropertyChanged;
                    allSymptoms.Add(symptom);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке симптомов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSymptomIds.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один симптом.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (int symptomId in selectedSymptomIds)
                            {
                                string query = "INSERT INTO PatientSymptoms (PatientID, SymptomID) VALUES (@PatientID, @SymptomID)";
                                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@PatientID", patientId);
                                    command.Parameters.AddWithValue("@SymptomID", symptomId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception exInner)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Ошибка при сохранении симптомов: " + exInner.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            FollowUpQuestionsWindow followUpWindow = new FollowUpQuestionsWindow(patientId);
            followUpWindow.Show();
            this.Close();
        }

        private void SearchSymptomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchSymptomTextBox.Text.Trim();
            LoadSymptoms(searchText);
        }

        private void Symptom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                var symptom = sender as Symptom;
                if (symptom != null)
                {
                    if (symptom.IsSelected)
                    {
                        selectedSymptomIds.Add(symptom.SymptomID);
                    }
                    else
                    {
                        selectedSymptomIds.Remove(symptom.SymptomID);
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var symptom in allSymptoms)
            {
                symptom.PropertyChanged -= Symptom_PropertyChanged;
            }
            base.OnClosed(e);
        }

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }
    }
}
