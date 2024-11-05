using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.IO;

class Server
{
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private static readonly string pathFile = @"D:\text.csv"; // Добавлен относительный путь к файлу

    public static async Task Main(string[] args)
    {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logs.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;

            UdpClient udpClient = new UdpClient(8080);
            logger.Info("Сервер запущен и ожидает запросов.");

            try
            {
                while (true)
                {
                    var receivedResult = await udpClient.ReceiveAsync();
                    string[] request = Encoding.UTF8.GetString(receivedResult.Buffer).Split(":");
                    logger.Info($"Получен запрос: {request}");

                    string response = ProcessRequest(request);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);

                    await udpClient.SendAsync(responseBytes, responseBytes.Length, receivedResult.RemoteEndPoint);
                    logger.Info("Ответ отправлен клиенту.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
    }

    private static string ProcessRequest(string[] request)
    {
        int num_str_in_file = CountLinesInFile(@"D:\text.csv");
        if (request[0] == "get_all_data")
        {
            logger.Info("Запрос на получение всех данных.");
            return GetAllData();
        }
        else if (request[0] == "get_str")
        {
            int recordId;
            if (int.TryParse(request[1], out recordId) && recordId <= num_str_in_file && recordId > 0)
            {
                logger.Info($"Запрос на получение записи с номером {recordId}.");
                return GetRecord(recordId);
            }
            else
            {
                logger.Warn($"Запись с номером {request[1]} не существует");
                return "Неверный формат данных";
            }
                
        }
        else if (request[0] == "del_str")
        {
            int recordId;
            if (int.TryParse(request[1], out recordId) && recordId <= num_str_in_file && recordId > 0)
            {
                logger.Info($"Запрос на получение записи с номером {recordId}.");
                return GetRecord(recordId);
            }
            else
            {
                logger.Warn($"Запись с номером {request[1]} не существует");
                return "Неверный формат данных";
            }
        }
        else if (request[0] == "add_str")
        {
            string recordData = request[1];
            logger.Info($"Запрос на добавление новой записи: {recordData}.");
            return AddRecordToFile(recordData);
        }
        else
        {
            logger.Warn("Неизвестный запрос.");
            return "Unknown command";
        }
    }

    public static int CountLinesInFile(string filePath)
    {
        int lineCount = 0;

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (sr.ReadLine() != null)
                {
                    lineCount++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }

        return lineCount;
    }

    public static string DeleteRecord(int line_num)
    {
        List<string> lines = new List<string>();

        using (StreamReader sr = new StreamReader(pathFile))
        {
            string str;
            while ((str = sr.ReadLine()) != null)
            {
                lines.Add(str);
            }
        }

        if (line_num <= 0 || line_num > lines.Count)
        {
            return "Ошибка индекса строки";
        }

        lines.RemoveAt(line_num - 1);

        using (StreamWriter writer = new StreamWriter(pathFile))
        {
            foreach (string line in lines)
            {
                writer.WriteLine(line);
            }
        }
        return "Строка удалена";
    }

    public static string GetAllData()
    {
        StringBuilder response = new StringBuilder();

        using (StreamReader sr = new StreamReader(pathFile))
        {
            string str;
            while ((str = sr.ReadLine()) != null)
            {
                response.AppendLine(str);
            }
        }
        return response.ToString();
    }

    public static string GetRecord(int recordId)
    {
        string response = "";

        using (StreamReader sr = new StreamReader(pathFile))
        {
            for (int i = 0; i < recordId - 1; i++)
            {
                sr.ReadLine();
            }
            response = sr.ReadLine();
        }
        return response ?? "Запись не найдена";
    }

    public static string AddRecordToFile(string recordData)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(pathFile, true))
            {
                sw.WriteLine(recordData);
            }
            return "Данные успешно записаны в файл.";
        }
        catch (Exception ex)
        {
            return $"Произошла ошибка при записи в файл: {ex.Message}";
        }
    }
}