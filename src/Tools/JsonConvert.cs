/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Tools
{
    public abstract class JsonObject
    {
        public abstract Matrix<double> ToMatrix();
        public abstract override string ToString();
    }
    public class JsonNumber: JsonObject
    {
        public double Value {get; set;}

        public override Matrix<double> ToMatrix()
        {
            return Value.ToMatrix();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public class JsonString: JsonObject
    {
        public string Value {get; set;}

        public override Matrix<double> ToMatrix()
        {
            throw new System.ArgumentException("string cannot be transferred to matrix");
        }

        public override string ToString()
        {
            return "\""+Value+"\"";
        }
    }
    public class JsonList: JsonObject
    {
        public List<JsonObject> Value {get; set;}
        public void Add(JsonObject jsonObject)
        {
            if (Value is null) Value = new List<JsonObject>();
            Value.Add(jsonObject);
        }
        public static JsonList FromList<T>(IEnumerable<T> list) where T: JsonObject
        {
            JsonList jsonList = new JsonList();
            foreach (T val in list)
            {
                jsonList.Add(val);
            }
            return jsonList;
        }

        public override Matrix<double> ToMatrix()
        {
            if (Value.Count == 0)
            {
                throw new System.ArgumentException("Empty list cannot be transferred to matrix");
            }
            if (Value.All(v => v is JsonNumber))
            {
                var numberList = Value.ConvertAll(item => ((JsonNumber)item).Value);
                return Matrix<double>.Build.Dense(1, numberList.Count, numberList.ToArray());
            }
            if (Value.Any(v => !(v is JsonList)))
            {
                throw new System.ArgumentException("Number or list of number is expected.");
            }
            var matrices = Value.ConvertAll(o => o.ToMatrix());
            if (matrices.Any(m => m.RowCount!=1))
            {
                throw new System.ArgumentException("High dimensions value cannot be transferred to matrix");
            }
            if (matrices.Any(m => m.ColumnCount != matrices[0].ColumnCount))
            {
                throw new System.ArgumentException("Column number inconsistency");
            }
            var mat = Matrix<double>.Build.DenseOfMatrix(matrices[0]);
            for (int i=1; i<matrices.Count; i++)
            {
                mat = mat.Stack(matrices[i]);
            }
            return mat;
        }

        public override string ToString()
        {
            return "["+string.Join(",", Value.ConvertAll(item => item.ToString()))+"]";
        }
    }
    public class JsonDict: JsonObject
    {
        public Dictionary<string, JsonObject> Value {get; set;}
        public void Add(string key, JsonObject jsonObject)
        {
            if (Value is null) Value = new Dictionary<string, JsonObject>();
            if (Value.ContainsKey(key)) throw new System.ArgumentException($"key {key} already exists");
            Value.Add(key, jsonObject);
        }

        public override Matrix<double> ToMatrix()
        {
            throw new System.ArgumentException("Dict object cannot be transferred to matrix");
        }

        public override string ToString()
        {
            return "{"+string.Join(",", Value.Select((pair, i) => pair.Key+":"+pair.Value.ToString()))+"}";
        }
    }
    public class JsonConvert
    {
        private int position = 0;
        private string json;
        private char currentChar 
        {
            get 
            {
                if (position >= json.Length) return '\0';
                else return json[position];
            }
        }
        public JsonConvert(string json)
        {
            this.json = json;
        }
        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(currentChar))
            {
                position++;
            }
        }
        private JsonNumber GetJsonNumber()
        {
            int startPosition = position;
            if (currentChar=='-' || currentChar=='+')
            {
                position++;
            }
            while (char.IsDigit(currentChar))
            {
                position++;
            }
            if (currentChar=='.')
            {
                position++;
                while (char.IsDigit(currentChar))
                {
                    position++;
                }
            }
            if (currentChar=='e' || currentChar=='E')
            {
                position++;
                if (currentChar=='-' || currentChar=='+')
                {
                    position++;
                }
                while (char.IsDigit(currentChar))
                {
                    position++;
                }
            }
            return new JsonNumber()
            {
                Value = System.Convert.ToDouble(json.Substring(startPosition,position-startPosition))
            };
        }
        private JsonString GetJsonString()
        {
            char quotation = currentChar;
            position++;
            int startPosition = position;
            while (currentChar!=quotation)
            {
                if (currentChar!='\n' && currentChar!='\r' && currentChar!='\0')
                {
                    position++;
                }
                else
                {
                    throw new System.ArgumentException($"Incomplete string {json.Substring(startPosition, position-startPosition)}...");
                }
            }
            int endPosition = position;
            position++;
            return new JsonString() 
            {
                Value = json.Substring(startPosition, endPosition-startPosition)
            };
        }
        private JsonList GetJsonList()
        {
            position++;
            JsonList jsonList = new JsonList();
            while (currentChar != ']')
            {
                SkipWhitespace();
                if (currentChar=='-' || currentChar=='+' || char.IsDigit(currentChar))
                {
                    jsonList.Add(GetJsonNumber());
                }
                else if (currentChar=='\'' || currentChar=='"')
                {
                    jsonList.Add(GetJsonString());
                }
                else if (currentChar=='[')
                {
                    jsonList.Add(GetJsonList());
                }
                else if (currentChar=='{')
                {
                    jsonList.Add(GetJsonDict());
                }
                else
                {
                    throw new System.ArgumentException($"Incomplete List");
                }
                SkipWhitespace();
                if (currentChar==',')
                {
                    position++;
                }
                if (currentChar=='\0')
                {
                    throw new System.ArgumentException($"Incomplete List");
                }
            }
            position++;
            return jsonList;
        }
        private JsonDict GetJsonDict()
        {
            position++;
            JsonDict jsonDict = new JsonDict();
            SkipWhitespace();
            while (currentChar != '}')
            {
                if (currentChar != '\'' && currentChar != '"')
                {
                    throw new System.ArgumentException($"Incomplete Dict key");
                }
                string key = GetJsonString().Value;
                SkipWhitespace();
                if (currentChar != ':')
                {
                    throw new System.ArgumentException($"Incomplete Dict");
                }
                position++;
                SkipWhitespace();
                if (currentChar=='-' || currentChar=='+' || char.IsDigit(currentChar))
                {
                    jsonDict.Add(key, GetJsonNumber());
                }
                else if (currentChar=='\'' || currentChar=='"')
                {
                    jsonDict.Add(key, GetJsonString());
                }
                else if (currentChar=='[')
                {
                    jsonDict.Add(key, GetJsonList());
                }
                else if (currentChar=='{')
                {
                    jsonDict.Add(key, GetJsonDict());
                }
                else
                {
                    throw new System.ArgumentException($"Incomplete Dict");
                }
                SkipWhitespace();
                if (currentChar==',')
                {
                    position++;
                }
                if (currentChar=='\0')
                {
                    throw new System.ArgumentException($"Incomplete Dict");
                }
                SkipWhitespace();
            }
            position++;
            return jsonDict;
        }
        public static JsonDict Load(string json)
        {
            JsonConvert jsonConvert = new JsonConvert(json);
            jsonConvert.SkipWhitespace();
            return jsonConvert.GetJsonDict();
        }
    }
}