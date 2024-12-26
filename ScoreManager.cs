using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Tetris
{
    public class ScoreEntry
    {
        public int Score { get; set; }
        public DateTime Date { get; set; }

        public static readonly string LeaderboardDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tetris");
        public static readonly string LeaderboardFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tetris", "leaderboard.json");


        private void EnsureDirectoryExists()
        {
            string directoryPath = Path.GetDirectoryName(LeaderboardFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public void SaveScore(int score)
        {
            try
            {
                EnsureDirectoryExists();
                List<ScoreEntry> scores = LoadScores();

                scores.Add(new ScoreEntry { Score = score, Date = DateTime.Now });

                scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();

                string json = JsonSerializer.Serialize(scores);
                File.WriteAllText(LeaderboardFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving score: {ex.Message}");
            }
        }

        private List<ScoreEntry> LoadScores()
        {
            try
            {
                EnsureDirectoryExists();

                if (!File.Exists(LeaderboardFilePath))
                {
                    return new List<ScoreEntry>();
                }

                string json = File.ReadAllText(LeaderboardFilePath);
                return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading scores: {ex.Message}");
                return new List<ScoreEntry>();
            }
        }
    }
}
