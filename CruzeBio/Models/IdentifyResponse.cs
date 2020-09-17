using System;
using System.Text.Json.Serialization;

namespace CruzeBio.Models
{
    /// <summary>
    /// A partial representation of an issue object from the GitHub API
    /// </summary>
    public class IdentifyResponse
    {
        [JsonPropertyName("ExecutionTime")]
        public decimal ExecutionTime { get; set; }

        [JsonPropertyName("ScheduledEncounterPort")]
        public string ScheduledEncounterPort { get; set; }

        [JsonPropertyName("Result")]
        public string Result { get; set; }

        [JsonPropertyName("UID")]
        public string UID { get; set; }
    }
}