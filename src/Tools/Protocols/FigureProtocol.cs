using System.Text.Json.Serialization;

namespace Ligral.Tools.Protocols
{
    static class FigureProtocol
    {
        public const int DataLabel = 0xff0d;
        public struct Data
        {
            [JsonPropertyName("curve_handle")]
            public short CurveHandle { get; set; }
            [JsonPropertyName("x_value")]
            public double XValue { get; set; }
            [JsonPropertyName("y_value")]
            public double YValue { get; set; }
        }
    }
}