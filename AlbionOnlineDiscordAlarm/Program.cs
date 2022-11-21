using System;
using System.Threading.Tasks;
using AlbionAlarmDiscord.Check;
using Microsoft.Extensions.Logging;

namespace AlbionAlarmDiscord;

class Program
{
    const string configPath = "config.ini";

    static async Task Main(string[] appArgs)
    {
        Console.WriteLine("Hello World!");

        ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole(c => c.TimestampFormat = "HH:mm:ss"));
        ILogger logger = loggerFactory.CreateLogger("Main");

        if (!File.Exists(configPath))
        {
            logger.LogCritical("Конфига нет. ({path})", configPath);
            // Логгер может не успеть оставить сообщение.
            // Не знаю, сколько ему нужно времени, но 10мс точно должно.
            await Task.Delay(10);
            return;
        }

        Dictionary<string, string> configLines = File.ReadAllLines(configPath)
        .Where(l => !string.IsNullOrEmpty(l))
        .Select(line =>
        {
            string[] split = line.Split('=');

            return (split[0], split[1]);
        }).ToDictionary(key => key.Item1, value => value.Item2);

        string botToken = configLines["BotToken"];
        ulong channelId = ulong.Parse(configLines["ChannelId"]);

        DiscordBot bot = new(botToken, channelId, loggerFactory);
        Checker checker = new(loggerFactory);

        Worker worker = new(bot, checker, loggerFactory);

        await worker.StartAsync();

        while (true)
        {
            logger.LogInformation(";)");
            Console.ReadLine();
        }
    }
}