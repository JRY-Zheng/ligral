namespace Ligral.Syntax.ASTs
{
    class PortAST : AST
    {
        public WordAST PortId;
        public WordAST PortName;
        public PortAST(WordAST portID, WordAST portName)
        {
            PortId = portID;
            PortName = portName;
        }
        public override Token FindToken()
        {
            return PortId.FindToken();
        }
    }
}