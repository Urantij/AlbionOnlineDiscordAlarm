using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AlbionAlarmDiscord.Check;

public class Checker
{
    const string url = "https://api.albionstatus.com/current/";

    private readonly ILogger _logger;

    private readonly HttpClient httpClient;

    public Checker(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(this.GetType());

        httpClient = new();
    }

    public async Task<StatusCheck?> CheckAsync()
    {
        try
        {
            HttpResponseMessage result = await httpClient.GetAsync(url);

            string content = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusCheck[]>(content)![0];
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при получении статуса.");
            return null;
        }
    }
}