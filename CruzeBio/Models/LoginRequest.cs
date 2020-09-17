using System;
using System.Text.Json.Serialization;

namespace CruzeBio.Models
{
    /// <summary>
    /// A partial representation of an issue object from the GitHub API
    /// </summary>
    public class LoginRequest
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
    }
}