using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlbionOnlineDiscordAlarm.Check;
using Discord.Webhook;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineDiscordAlarm;

public class Worker
{
    /// <summary>
    /// Ждём дольше, если не удалось получить статус.
    /// </summary>
    private static readonly TimeSpan badRequestWaitTime = TimeSpan.FromMinutes(5);
    /// <summary>
    /// Когда ждём, что сервер станет офлайн.
    /// </summary>
    private static readonly TimeSpan preOfflineWaitTime = TimeSpan.FromMinutes(1);
    /// <summary>
    /// Когда ждём, что сервер станет стартовать.
    /// </summary>
    private static readonly TimeSpan preStartingWaitTime = TimeSpan.FromSeconds(30);
    /// <summary>
    /// Когда ждём, что сервер станет онлайн.
    /// </summary>
    private static readonly TimeSpan preOnlineWaitTime = TimeSpan.FromSeconds(5);

    private readonly ILogger _logger;

    private readonly DiscordBot? bot;
    private readonly DiscordWebhookClient? discordWebhookClient;
    private readonly Checker checker;
    private readonly Lines lines;

    public Worker(DiscordBot? bot, DiscordWebhookClient? discordWebhookClient, Checker checker, Lines lines, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(this.GetType());

        this.bot = bot;
        this.discordWebhookClient = discordWebhookClient;
        this.checker = checker;
        this.lines = lines;
    }

    public async Task StartAsync()
    {
        if (bot != null)
        {
            bot.BotReady += BotReady;

            await bot.ConnectAsync();
        }

        _ = Task.Run(EndlessLoop);
    }

    private void BotReady()
    {
        bot!.BotReady -= BotReady;
    }

    private async Task EndlessLoop()
    {
        Status? lastStatus = null;

        while (true)
        {
            StatusCheck? check = await checker.CheckAsync();

            if (check?.Status == null)
            {
                _logger.LogWarning("Нет статуса.");
                await Task.Delay(badRequestWaitTime);
                continue;
            }

            Status status = check.Status.ToLowerInvariant() switch
            {
                "online" => Status.Online,
                "offline" or "500" => Status.Offline,
                "starting" => Status.Starting,

                _ => Status.Unknown
            };

            if (status == Status.Unknown)
            {
                _logger.LogError("Неизвестный статус в сообщении. {status} ({message})", check.Status, check.Message);

                await Task.Delay(badRequestWaitTime);
                continue;
            }

            if (status != lastStatus)
            {
                lastStatus = status;

                if (status == Status.Online)
                {
                    _logger.LogInformation("Онлайн.");
                    await NotifyAsync(lines.OnlineText);

                    await Task.Delay(preOfflineWaitTime);
                }
                else if (status == Status.Offline)
                {
                    _logger.LogInformation("Офлайн.");
                    await NotifyAsync(lines.OfflineText);

                    await Task.Delay(preStartingWaitTime);
                }
                else if (status == Status.Starting)
                {
                    _logger.LogInformation("Запускается.");
                    await NotifyAsync(lines.StartingText);

                    await Task.Delay(preOnlineWaitTime);
                }
            }
        }
    }

    private Task NotifyAsync(string text)
    {
        List<Task> tasks = new();

        if (discordWebhookClient != null)
        {
            tasks.Add(discordWebhookClient.SendMessageAsync(text));
        }

        if (bot != null && bot.Connected)
        {
            tasks.Add(bot.SendAlarmAsync(text));
        }

        return Task.WhenAll(tasks);
    }
}
