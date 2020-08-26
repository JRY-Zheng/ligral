namespace Ligral
{
    [System.Serializable]
    class LigralException : System.Exception
    {
        public LigralException() { }
        public LigralException(string message) : base(message) { }
        public LigralException(string message, System.Exception inner) : base(message, inner) { }
        protected LigralException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    class SyntaxException : LigralException
    {
        private Token ErrorToken;
        public SyntaxException(Token token, string message="") : base(message) 
        {
            ErrorToken = token;
        }
        public override string ToString()
        {
            string errorMessage = $"Invalid Syntax at line {ErrorToken.Line} column {ErrorToken.Column} ({ErrorToken.Value})";
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
    class SemanticException : LigralException
    {
        private Token ErrorToken;
        private string errorMessage;
        public SemanticException(Token token, string message="") : base(message)
        {
            ErrorToken = token;
            errorMessage = message;
            this.Data.Add("Token", $"Invalid Semantics at line {ErrorToken.Line} column {ErrorToken.Column} ({ErrorToken.Value})");
        }
    }
}