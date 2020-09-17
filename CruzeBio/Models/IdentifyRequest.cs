using System;
using System.Text.Json.Serialization;

namespace CruzeBio.Models
{
    /// <summary>
    /// A partial representation of an issue object from the GitHub API
    /// </summary>
    public class IdentifyRequest
    {
        [JsonPropertyName("CarrierCode")]
        public string CarrierCode { get; set; }

        [JsonPropertyName("FlightNumber")]
        public string FlightNumber { get; set; }

        [JsonPropertyName("ScheduledEncounterPort")]
        public string ScheduledEncounterPort { get; set; }

        [JsonPropertyName("ScheduledEncounterDate")]
        public string ScheduledEncounterDate { get; set; }

        [JsonPropertyName("PhotoDate")]
        public string PhotoDate { get; set; }

        [JsonPropertyName("DeviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("DepartureGate")]
        public string DepartureGate { get; set; }

        [JsonPropertyName("DepartureTerminal")]
        public string DepartureTerminal { get; set; }

        [JsonPropertyName("Token")]
        public string Token { get; set; }

        [JsonPropertyName("Photo")]
        public string Photo { get; set; }
    }
}