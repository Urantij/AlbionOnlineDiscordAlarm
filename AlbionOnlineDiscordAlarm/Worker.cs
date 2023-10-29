using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlbionAlarmDiscord.Check;
using Microsoft.Extensions.Logging;

namespace AlbionAlarmDiscord;

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

    private readonly DiscordBot bot;
    private readonly Checker checker;

    public Worker(DiscordBot bot, Checker checker, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(this.GetType());

        this.bot = bot;
        this.checker = checker;
    }

    public async Task StartAsync()
    {
        bot.BotReady += BotReady;

        await bot.ConnectAsync();
    }

    private void BotReady()
    {
        bot.BotReady -= BotReady;

        Task.Run(EndlessLoop);
    }

    private async Task EndlessLoop()
    {
        StatusCheck? lastCheck = null;

        while (true)
        {
            StatusCheck? check = await checker.CheckAsync();

            if (check?.Status == null)
            {
                await Task.Delay(badRequestWaitTime);
                continue;
            }

            if (check.Status != lastCheck?.Status)
            {
                if (check.Status.Equals("online", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[{time}] онлайн.", DateTime.UtcNow.ToString("HH:mm:ss"));
                    await bot.SendAlarmAsync("Сервер онлайн!");
                }
                else if (check.Status.Equals("offline", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[{time}] офлайн.", DateTime.UtcNow.ToString("HH:mm:ss"));
                    await bot.SendAlarmAsync("Сервер офлайн!");
                }
                else if (check.Status.Equals("starting", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[{time}] запускается.", DateTime.UtcNow.ToString("HH:mm:ss"));
                    await bot.SendAlarmAsync("Сервер запускается!");
                }
            }

            lastCheck = check;

            if (check.Status.Equals("online", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(preOfflineWaitTime);
            }
            else if (check.Status.Equals("offline", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(preStartingWaitTime);
            }
            else if (check.Status.Equals("starting", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(preOnlineWaitTime);
            }
            else
            {
                _logger.LogError("Неизвестный статус в сообщении. {status} ({message})", check.Status, check.Message);

                await Task.Delay(badRequestWaitTime);
            }
        }
    }
}
