using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.Data.SQLite;
using System;
using System.Data;
using System.Threading;

namespace digitacaoauto
{
    class Program
    {
        static void Main()
        {
            using IWebDriver driver = new EdgeDriver();
            driver.Manage().Window.Maximize();

            driver.Navigate().GoToUrl("https://10fastfingers.com/typing-test/portuguese");

            // Temporizador
            DateTime endTime = DateTime.Now.AddMinutes(1);

            while (DateTime.Now < endTime)
            {
                //Extrai o texto da classe "highlight" dentro da div
                string highlightedText = driver.FindElement(By.ClassName("highlight")).Text;

                //Digita o texto localizado
                IWebElement inputField = driver.FindElement(By.Id("inputfield"));
                inputField.SendKeys(highlightedText + " ");
                Console.WriteLine("Palavra digitada: " + highlightedText);

                //Tempo de espera entre os inputs
                Thread.Sleep(500);
            }
            
            //Tempo de prevenção para que a tabela se torne visível
            Thread.Sleep(5000);

            //Obtém os elementos da tabela e extrai as informações que estão nela
            IWebElement table = driver.FindElement(By.Id("result-table"));
            string wpm = table.FindElement(By.Id("wpm")).Text;
            string keystrokes = table.FindElement(By.Id("keystrokes")).Text;
            string accuracy = table.FindElement(By.Id("accuracy")).Text;
            string correctWords = table.FindElement(By.Id("correct")).Text;
            string wrongWords = table.FindElement(By.Id("wrong")).Text;
            Console.WriteLine("WPM: " + wpm);
            Console.WriteLine("Keystrokes: " + keystrokes);
            Console.WriteLine("Accuracy: " + accuracy);
            Console.WriteLine("Correct words: " + correctWords);
            Console.WriteLine("Wrong words: " + wrongWords);

            //Cria uma tabela local para armazenar os dados temporariamente
            DataTable resultsTable = new DataTable();
            resultsTable.Columns.Add("WPM", typeof(string));
            resultsTable.Columns.Add("Keystrokes", typeof(string));
            resultsTable.Columns.Add("Accuracy", typeof(string));
            resultsTable.Columns.Add("CorrectWords", typeof(string));
            resultsTable.Columns.Add("WrongWords", typeof(string));
            resultsTable.Rows.Add(wpm, keystrokes, accuracy, correctWords, wrongWords);

            driver.Quit();

            //Salva os dados no banco de dados SQLite
            SaveToSQLite(resultsTable);

            //Mostra os dados do banco de dados
            ShowDataFromSQLite();
        }
        static void SaveToSQLite(DataTable dataTable)
        {
            //Conexão com o banco de dados SQLite
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=results.db;Version=3;"))
            {
                connection.Open();

                //Criação da tabela se ela não existir e inserindo os dados dentro dela
                using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Results (WPM TEXT, Keystrokes TEXT, Accuracy TEXT, CorrectWords TEXT, WrongWords TEXT);", connection))
                {
                    command.ExecuteNonQuery();
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO Results (WPM, Keystrokes, Accuracy, CorrectWords, WrongWords) VALUES (@WPM, @Keystrokes, @Accuracy, @CorrectWords, @WrongWords);", connection))
                    {
                        command.Parameters.AddWithValue("@WPM", row["WPM"]);
                        command.Parameters.AddWithValue("@Keystrokes", row["Keystrokes"]);
                        command.Parameters.AddWithValue("@Accuracy", row["Accuracy"]);
                        command.Parameters.AddWithValue("@CorrectWords", row["CorrectWords"]);
                        command.Parameters.AddWithValue("@WrongWords", row["WrongWords"]);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        static void ShowDataFromSQLite()
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=results.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Results;", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("\nDados armazenados no banco de dados:");

                        while (reader.Read())
                        {
                            Console.WriteLine($"WPM: {reader["WPM"]}, Keystrokes: {reader["Keystrokes"]}, Accuracy: {reader["Accuracy"]}, CorrectWords: {reader["CorrectWords"]}, WrongWords: {reader["WrongWords"]}");
                        }
                    }
                }
            }
        }
    }
}
