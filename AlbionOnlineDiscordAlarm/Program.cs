using System;
using System.Threading.Tasks;
using AlbionAlarmDiscord.Check;
using Discord.Webhook;
using Microsoft.Extensions.Logging;

namespace AlbionAlarmDiscord;

class Program
{
    const string defaultAskUrl = "https://serverstatus.albiononline.com/";
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
            string[] split = line.Split('=', 2);

            return (split[0], split[1]);
        }).ToDictionary(key => key.Item1, value => value.Item2);

        DiscordBot? bot = null;
        if (configLines.TryGetValue("BotToken", out string? botToken))
        {
            logger.LogInformation("Используем дискорд бота.");

            ulong channelId = ulong.Parse(configLines["ChannelId"]);

            bot = new(botToken, channelId, loggerFactory);
        }

        DiscordWebhookClient? discordWebhookClient = null;
        if (configLines.TryGetValue("WebhookUrl", out string? webhookUrl))
        {
            logger.LogInformation("Используем дискорд вебхук.");

            discordWebhookClient = new(webhookUrl);
        }

        string askUrl = configLines.GetValueOrDefault("Url") ?? defaultAskUrl;
        Checker checker = new(askUrl, loggerFactory);

        Worker worker = new(bot, discordWebhookClient, checker, loggerFactory);

        await worker.StartAsync();

        while (true)
        {
            logger.LogInformation(";)");
            Console.ReadLine();
        }
    }
}