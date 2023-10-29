using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace AlbionAlarmDiscord;

public class DiscordBot
{
    private readonly DiscordSocketClient client;

    private readonly ILogger _logger;

    private readonly string token;
    private readonly ulong channelId;

    private SocketGuild? guild;
    private SocketTextChannel? textChannel;

    public bool Connected => client.ConnectionState == ConnectionState.Connected;

    public event Action? BotReady;

    public DiscordBot(string token, ulong channelId, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(this.GetType());

        this.token = token;
        this.channelId = channelId;

        client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            AlwaysDownloadUsers = false,
            AlwaysDownloadDefaultStickers = false,
            AlwaysResolveStickers = false,
            DefaultRetryMode = RetryMode.AlwaysRetry,
            MessageCacheSize = 0,
        });

        client.GuildAvailable += Client_GuildAvailable;
    }

    private Task Client_GuildAvailable(SocketGuild arg)
    {
        _logger.LogInformation("GuildAvailable");

        if (textChannel == null)
        {
            guild = arg;
            textChannel = arg.GetTextChannel(channelId);
            BotReady?.Invoke();
        }

        return Task.CompletedTask;
    }

    public async Task ConnectAsync()
    {
        await client.LoginAsync(TokenType.Bot, token);

        await client.StartAsync();
    }

    public async Task SendAlarmAsync(string alarm)
    {
        if (guild == null)
            throw new NullReferenceException($"{nameof(guild)} is null.");

        if (textChannel == null)
            throw new NullReferenceException($"{nameof(textChannel)} is null.");

        string message = $"[{DateTimeOffset.UtcNow:HH:mm:ss}] {guild.EveryoneRole} {alarm}";

        await textChannel.SendMessageAsync(message);
    }
}