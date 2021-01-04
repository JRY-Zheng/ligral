using System.Text.Json.Serialization;

namespace Ligral.Component
{
    struct ParameterDocument
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("required")]
        public bool Required { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
    struct ModelDocument
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("parameters")]
        public ParameterDocument[] Parameters { get; set; }
        [JsonPropertyName("in-ports")]
        public string[] InPorts { get; set; }
        [JsonPropertyName("out-ports")]
        public string[] OutPorts { get; set; }
        [JsonPropertyName("in-port-variable")]
        public bool InPortVariable { get; set; }
        [JsonPropertyName("out-port-variable")]
        public bool OutPortVariable { get; set; }
    }
}