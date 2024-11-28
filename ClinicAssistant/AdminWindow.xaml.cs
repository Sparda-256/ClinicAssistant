using System.Windows;

namespace ClinicAssistant
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.Show();


        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteWindow deleteWindow = new DeleteWindow();
            deleteWindow.Show();

        }

        private void EndSessionButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }
    }
}
