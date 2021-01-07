/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Ligral.Syntax;
using Ligral.Component;

namespace Ligral
{
    [System.Serializable]
    public class LigralException : System.Exception
    {
        public LigralException() { }
        public LigralException(string message) : base(message) { }
        public LigralException(string message, System.Exception inner) : base(message, inner) { }
        protected LigralException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            string errorMessage = $"Ligral Exception";
            if (Message!="")
            {
                return $"{errorMessage}: {Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }

    [System.Serializable]
    internal class SyntaxException : LigralException
    {
        private Token ErrorToken;
        public SyntaxException(Token token, string message="") : base(message) 
        {
            ErrorToken = token;
        }
        public override string ToString()
        {
            string errorMessage = $"Invalid Syntax ({ErrorToken.Value}) in file {Interpreter.GetInstance().CurrentFileName}:line {ErrorToken.Line} column {ErrorToken.Column}";
            if (Message!="")
            {
                return $"{errorMessage}\n{Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }

    [System.Serializable]
    public class SemanticException : LigralException
    {
        private Token ErrorToken;
        // private string errorMessage;
        internal SemanticException(Token token, string message="") : base(message)
        {
            ErrorToken = token;
            // errorMessage = message;
            // this.Data.Add("Token", $"Invalid Semantics at line {ErrorToken.Line} column {ErrorToken.Column} ({ErrorToken.Value})");
        }
        public override string ToString()
        {
            string errorMessage = $"Invalid Semantics ({ErrorToken.Value}) in file {Interpreter.GetInstance().GetFileNameByIndex(ErrorToken.File)}:line {ErrorToken.Line} column {ErrorToken.Column}";
            if (Message!="")
            {
                return $"{errorMessage}\n{Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }

    [System.Serializable]
    public class ModelException : LigralException
    {
        private Model errorModel;
        // private string errorMessage;
        public ModelException(Model model, string message="") : base(message)
        {
            errorModel = model;
            // errorMessage = message;
            // this.Data.Add("Token", $"Invalid Semantics at line {ErrorToken.Line} column {ErrorToken.Column} ({ErrorToken.Value})");
        }
        public override string ToString()
        {
            string errorMessage = $"Error occurred in {errorModel.Name}";
            if (Message!="")
            {
                return $"{errorMessage}: {Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }

    [System.Serializable]
    public class SettingException : LigralException
    {
        private string errorSetting;
        private object errorValue;
        // private string errorMessage;
        public SettingException(string settingName, object value, string message="") : base(message)
        {
            errorSetting = settingName;
            errorValue = value;
        }
        public override string ToString()
        {
            string errorMessage = $"Invalid setting {errorSetting} = {errorValue}";
            if (Message!="")
            {
                return $"{errorMessage}: {Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }
    [System.Serializable]
    public class OptionException : LigralException
    {
        private string errorOption;
        // private string errorMessage;
        public OptionException(string optionName, string message="") : base(message)
        {
            errorOption = optionName;
        }
        public override string ToString()
        {
            string errorMessage = $"Invalid option {errorOption}";
            if (Message!="")
            {
                return $"{errorMessage}: {Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }
    [System.Serializable]
    public class CSVFormatError : LigralException
    {
        public CSVFormatError(string message) : base(message)
        {

        }
        public override string ToString()
        {
            string errorMessage = $"Wrong CSV format";
            if (Message!="")
            {
                return $"{errorMessage}: {Message}";
            }
            else
            {
                return errorMessage;
            }
        }
    }
}