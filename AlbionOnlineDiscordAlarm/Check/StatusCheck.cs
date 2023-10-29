using Newtonsoft.Json;

namespace AlbionAlarmDiscord.Check;

public class StatusCheck
{
    [JsonProperty("status")]
    public string Status { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }

    public StatusCheck(string status, string message)
    {
        Status = status;
        Message = message;
    }
}