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

        public const string LeaderboardFilePath = "leaderboard.json";

        public void SaveScore(int score)
        {
            List<ScoreEntry> scores = LoadScores();

            scores.Add(new ScoreEntry { Score = score, Date = DateTime.Now });

            scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();

            string json = JsonSerializer.Serialize(scores);
            File.WriteAllText(LeaderboardFilePath, json);
        }

        private List<ScoreEntry> LoadScores()
        {
            if (!File.Exists(LeaderboardFilePath))
            {
                return new List<ScoreEntry>();
            }

            string json = File.ReadAllText(LeaderboardFilePath);
            return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
        }
    }
}
