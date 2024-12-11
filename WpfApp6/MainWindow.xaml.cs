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
        private List<Word> words;
        private Word currentWord;
        private DispatcherTimer timer;
        private int timeLeft;
        private int correctCount = 0;
        private int incorrectCount = 0;
        private int score = 0;

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
            words.AddRange(DatabaseHelper.GetRandomWords(10));
            LoadNextQuestion();
        }

        private void LoadNextQuestion()
        {
            if (words.Count == 0)
            {
                LoadNextBatch();
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
                incorrectCount++;
                UpdateScore(-5); // Büntetés a kifutott időért
                LoadNextQuestion();
            }
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            var selectedButton = sender as Button;

            if (selectedButton.Content.ToString() == currentWord.Translation)
            {
                correctCount++;
                UpdateScore(10);
                MessageBox.Show("Helyes válasz!", "Gratulálok!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                incorrectCount++;
                UpdateScore(-5);
                MessageBox.Show($"Hibás! A helyes válasz: {currentWord.Translation}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadNextQuestion();
        }

        private void UpdateScore(int points)
        {
            score += points;
            ScoreText.Text = $"Pontszám: {score}";
            StatsText.Text = $"Statisztika: Helyes: {correctCount} | Hibás: {incorrectCount}";
        }
    }
}
