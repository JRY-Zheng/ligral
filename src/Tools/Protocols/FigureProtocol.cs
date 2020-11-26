using System.Text.Json.Serialization;

namespace Ligral.Tools.Protocols
{
    static class FigureProtocol
    {
        public const int DataLabel = 0xffd0;
        public struct Data
        {
            [JsonPropertyName("curve_handle")]
            public int CurveHandle { get; set; }
            [JsonPropertyName("x_value")]
            public double XValue { get; set; }
            [JsonPropertyName("y_value")]
            public double YValue { get; set; }
        }
        public const int CurveLabel = 0xffc0;
        public struct Curve
        {
            [JsonPropertyName("curve_handle")]
            public int CurveHandle {get; set; }
            [JsonPropertyName("row")]
            public int RowNO {get; set; }
            [JsonPropertyName("col")]
            public int ColumnNO {get; set; }
        }
        public const int FigureConfigLabel = 0xffb0;
        public struct FigureConfig
        {
            [JsonPropertyName("title")]
            public string Title {get; set; }
            [JsonPropertyName("rows")]
            public int RowsCount {get; set; }
            [JsonPropertyName("cols")]
            public int ColumnsCount {get; set; }
            [JsonPropertyName("grid")]
            public bool Grid {get; set; }
        }
        public const int PlotConfigLabel = 0xffa0;
        public struct PlotConfig
        {
            [JsonPropertyName("xlabel")]
            public string XLabel {get; set; }
            [JsonPropertyName("ylabel")]
            public string YLabel {get; set; }
            [JsonPropertyName("row")]
            public int RowNO {get; set; }
            [JsonPropertyName("col")]
            public int ColumnNO {get; set; }
        }
    }
}