using Newtonsoft.Json;

namespace AlbionAlarmDiscord.Check;

public class StatusCheck
{
    [JsonProperty("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonProperty("current_status")]
    public string CurrentStatus { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("comment")]
    public string Comment { get; set; }

    public StatusCheck(DateTimeOffset createdAt, string currentStatus, string message, string comment)
    {
        CreatedAt = createdAt;
        CurrentStatus = currentStatus;
        Message = message;
        Comment = comment;
    }
}