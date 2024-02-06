using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Укажите путь к файлу данных в качестве аргумента командной строки.");
            return;
        }
        string filePath = args[0];
        try
        {
            List<DatabaseConnection> data = ReadDataFromFile(filePath);
            if (data.Count > 0)
            {
                List<DatabaseConnection> validData = ValidateData(data);
                SaveValidData(validData);
                List<DatabaseConnection>[] splitData = SplitData(validData, 5);
                SaveSplitData(splitData);
            }
            Console.WriteLine("Программа выполнена успешно.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }

    static List<DatabaseConnection> ReadDataFromFile(string filePath)
    {
        List<DatabaseConnection> data = new List<DatabaseConnection>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            DatabaseConnection currentConnection = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("["))
                {
                    currentConnection = new DatabaseConnection();
                    currentConnection.Title = line.Trim('[', ']').Trim();
                }
                else if (line.StartsWith("Connect="))
                {
                    currentConnection.ConnectParameter = line.Replace("Connect=", "").Trim();
                    data.Add(currentConnection);
                }
            }
        }

        return data;
    }

    static List<DatabaseConnection> ValidateData(List<DatabaseConnection> data)
    {
        List<DatabaseConnection> validData = new List<DatabaseConnection>();
        foreach (var connection in data)
        {
            if (connection.IsValid())
            {
                validData.Add(connection);
            }
            else
            {
                SaveInvalidData(connection);
            }
        }
        return validData;
    }

    static void SaveValidData(List<DatabaseConnection> validData)
    {
        if (validData.Count > 0)
        {
            File.WriteAllLines("valid_data.txt", validData.Select(connection => connection.ToString()));
        }
    }

    static void SaveInvalidData(DatabaseConnection invalidConnection)
    {
        File.AppendAllText("bad_data.txt", $"Invalid data: {invalidConnection}\n");
    }

    static List<DatabaseConnection>[] SplitData(List<DatabaseConnection> data, int parts)
    {
        int batchSize = (int)Math.Ceiling((double)data.Count / parts);
        return data.Select((value, index) => new { value, index })
                   .GroupBy(x => x.index / batchSize)
                   .Select(group => group.Select(x => x.value).ToList())
                   .ToArray();
    }

    static void SaveSplitData(List<DatabaseConnection>[] splitData)
    {
        for (int i = 0; i < splitData.Length; i++)
        {
            File.WriteAllLines($"base_{i + 1}.txt", splitData[i].Select(connection => connection.ToString()));
        }
    }
}

class DatabaseConnection
{
    public string Title { get; set; }
    public string ConnectParameter { get; set; }
    public bool IsValid()
    {
        //Доп проверки...
        return !string.IsNullOrEmpty(ConnectParameter);
    }
    public override string ToString()
    {
        return $"[{Title}]\nConnect={ConnectParameter}\n";
    }
}
