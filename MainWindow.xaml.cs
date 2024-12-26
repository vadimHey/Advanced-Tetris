using Tetris;
using System;
using System.Text.Json;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        public MainWindow()
        {
            InitializeComponent();
            LoadLeaderboard();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            PlayWindow playWindow = new PlayWindow();

            playWindow.Top = this.Top;
            playWindow.Left = this.Left;

            playWindow.ShowDialog();
        }

        private void GameManagement_Click(object sender, RoutedEventArgs e)
        {
            GameManagementWindow gameManagementWindow = new GameManagementWindow();

            gameManagementWindow.Top = this.Top;
            gameManagementWindow.Left = this.Left;

            gameManagementWindow.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();

            aboutWindow.Top = this.Top;
            aboutWindow.Left = this.Left;

            aboutWindow.ShowDialog();
        }

        private void LoadLeaderboard()
        {
            if (!File.Exists(ScoreEntry.LeaderboardFilePath))
            {
                return;
            }

            string json = File.ReadAllText(ScoreEntry.LeaderboardFilePath);
            var scores = JsonSerializer.Deserialize<List<ScoreEntry>>(json);
            var sortedScores = scores?.OrderByDescending(s => s.Score).ToList();

            if (sortedScores == null || sortedScores.Count == 0)
            {
                LeaderboardList.Items.Add("Рекордов счета нет");
            }
            else
            {
                foreach (var score in sortedScores)
                {
                    LeaderboardList.Items.Add($"Score: {score.Score} - {score.Date:yyyy-MM-dd HH:mm}");
                }
            }
        }

        private void ClearLeaderboard()
        {
            LeaderboardList.Items.Clear();
            File.WriteAllText(ScoreEntry.LeaderboardFilePath, "[]");
        }

        private void ClearLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы точно хотите очистить таблицу рекордов?",
                "Подтверждение очистки таблицы",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClearLeaderboard();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}