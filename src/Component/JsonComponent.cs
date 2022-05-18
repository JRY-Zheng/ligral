/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json.Serialization;
using System.Collections.Generic;
namespace Ligral.Component
{
    struct JParameter
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("value")]
        public object Value {get; set;}
    }
    struct JOutPort
    {
        [JsonPropertyName("name")]
        public string OutPortName {get; set;}
        [JsonPropertyName("destination")]
        public JInPort[] Destinations {get; set;}
    }
    struct JInPort
    {
        [JsonPropertyName("id")]
        public string Id {get; set;}
        [JsonPropertyName("in-port")]
        public string InPortName {get; set;}
    }
    struct JModel
    {
        [JsonPropertyName("id")]
        public string Id {get; set;}
        [JsonPropertyName("type")]
        public string Type {get; set;}
        [JsonPropertyName("parameters")]
        public JParameter[] Parameters {get; set;}
        [JsonPropertyName("out-ports")]
        public JOutPort[] OutPorts {get; set;}
    }
    struct JRoute
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("inherit")]
        public string Inherit {get; set;}
        [JsonPropertyName("params")]
        public List<JRouteParam> Params {get; set;}
        [JsonPropertyName("in-ports")]
        public List<JRouteInPort> InPorts {get; set;}
        [JsonPropertyName("out-ports")]
        public List<JRouteOutPort> OutPorts {get; set;}
        [JsonPropertyName("models")]
        public JModel[] Models {get; set;}
    }
    struct JRouteParam
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("type")]
        public string Type {get; set;}
        [JsonPropertyName("default")]
        public object DefaultValue {get; set;}
    }
    struct JRouteInPort
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("nullable")]
        public bool Nullable {get; set;}
        [JsonPropertyName("default")]
        public object Default {get; set;}
        [JsonPropertyName("input-id")]
        public string InputID {get; set;}
    }
    struct JRouteOutPort
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("output-id")]
        public string OutputID {get; set;}
    }
    struct JConfig
    {
        [JsonPropertyName("item")]
        public string Item {get; set;}
        [JsonPropertyName("value")]
        public object Value {get; set;}
    }
    struct JProject
    {
        [JsonPropertyName("settings")]
        public JConfig[] Settings {get; set;}
        [JsonPropertyName("models")]
        public JModel[] Models {get; set;}
        [JsonPropertyName("routes")]
        public JRoute[] Routes {get; set;}
    }
}