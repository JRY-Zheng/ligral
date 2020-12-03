using System.Text.Json.Serialization;

namespace Ligral.Component
{
    struct ParameterDocument
    {
        [JsonPropertyName("fig")]
        public bool Required { get; set; }
        public string Type { get; set; }
    }
    struct ModelDocument
    {
        public string Type { get; set; }
        public ParameterDocument[] Parameters { get; set; }
        public string[] InPorts { get; set; }
        public string[] OutPorts { get; set; }
    }
}