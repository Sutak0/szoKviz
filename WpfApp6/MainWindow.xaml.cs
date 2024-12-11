using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp6
{
    public partial class MainWindow : Window
    {
        private List<Word> words;       // A teljes szólista
        private Word currentWord;       // Jelenlegi kérdés
        private DispatcherTimer timer;  // Időzítő
        private int timeLeft;           // Időzítő számláló
        private int correctCount = 0;   // Helyes válaszok száma
        private int incorrectCount = 0; // Hibás válaszok száma

        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            DatabaseHelper.ImportWordsFromFile("Resources/eng_hun.txt");
            StartQuiz();
        }

        private void StartQuiz()
        {
            words = new List<Word>();
            LoadNextBatch();
        }

        private void LoadNextBatch()
        {
            // Új kérdéssor betöltése
            words.AddRange(DatabaseHelper.GetRandomWords(10));
            LoadNextQuestion();
        }

        private void LoadNextQuestion()
        {
            if (words.Count == 0)
            {
                LoadNextBatch();
            }

            if (words.Count == 0)
            {
                MessageBox.Show("Nincs több kérdés az adatbázisban.", "Vége", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            currentWord = words[0];
            words.RemoveAt(0);

            QuestionText.Text = $"Mi a(z) '{currentWord.WordText}' jelentése?";

            var options = DatabaseHelper.GetRandomWords(3)
                                        .Select(w => w.Translation)
                                        .ToList();
            options.Add(currentWord.Translation);
            options = options.OrderBy(o => Guid.NewGuid()).ToList();

            Option1.Content = options[0];
            Option2.Content = options[1];
            Option3.Content = options[2];
            Option4.Content = options[3];

            StartTimer();
        }

        private void StartTimer()
        {
            timeLeft = 10;
            TimerBar.Value = 100;

            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            TimerBar.Value = (timeLeft / 10.0) * 100;

            if (timeLeft <= 0)
            {
                timer.Stop();
                MessageBox.Show("Lejárt az idő!", "Idő", MessageBoxButton.OK, MessageBoxImage.Warning);
                incorrectCount++;
                UpdateStats();
                LoadNextQuestion();
            }
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            var selectedButton = sender as Button;

            if (selectedButton.Content.ToString() == currentWord.Translation)
            {
                MessageBox.Show("Helyes válasz!", "Gratulálok!", MessageBoxButton.OK, MessageBoxImage.Information);
                correctCount++;
            }
            else
            {
                MessageBox.Show($"Hibás! A helyes válasz: {currentWord.Translation}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                incorrectCount++;
            }

            UpdateStats();
            LoadNextQuestion();
        }

        private void UpdateStats()
        {
            StatsText.Text = $"Statisztika: Helyes: {correctCount} | Hibás: {incorrectCount}";
        }
    }
}
