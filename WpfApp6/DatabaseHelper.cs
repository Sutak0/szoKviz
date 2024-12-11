using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace WpfApp6
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = "Data Source=words.db;Version=3;";

        public static void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string tableCommand = @"
                CREATE TABLE IF NOT EXISTS Words (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WordText TEXT NOT NULL,
                    Translation TEXT NOT NULL,
                    CorrectCount INTEGER DEFAULT 0,
                    IncorrectCount INTEGER DEFAULT 0
                )";
                SQLiteCommand createTable = new SQLiteCommand(tableCommand, connection);
                createTable.ExecuteNonQuery();
            }
        }

        public static void ImportWordsFromFile(string filePath)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('-');
                    if (parts.Length == 2)
                    {
                        var word = parts[0].Trim();
                        var translation = parts[1].Trim();
                        var insertCommand = new SQLiteCommand("INSERT INTO Words (WordText, Translation) VALUES (@word, @translation)", connection);
                        insertCommand.Parameters.AddWithValue("@word", word);
                        insertCommand.Parameters.AddWithValue("@translation", translation);
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public static List<Word> GetRandomWords(int count)
        {
            var words = new List<Word>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT * FROM Words ORDER BY RANDOM() LIMIT @count", connection);
                command.Parameters.AddWithValue("@count", count);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    words.Add(new Word
                    {
                        Id = reader.GetInt32(0),
                        WordText = reader.GetString(1),
                        Translation = reader.GetString(2)
                    });
                }
            }
            return words;
        }
    }
}
