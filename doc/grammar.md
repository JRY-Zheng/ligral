# Grammar

~~~ syntax
Program: Statements
Statements: Statement*
Statement: (ProgramConfig|Define|Using|Import|Chain) SEMIC|Route
ProgramConfig: CONF ID FROM (TRUE|FALSE|STRING|ValueExpr)
Define: ASSIGN ID FROM ValueExpr
ValueExpr: ValueFactor((PLUS|MINUS)ValueFactor)*
ValueFactor: ValueEntity((MUL|DIV)ValueEntity)*
ValueEntity: Value(CARET Value)*
Value: (PLUS|MINUS)Value|LPAR ValueExpr RPAR|DIGIT|Matrix|Pointer
Matrix: LBRK Row(SEMIC Row)*SEMIC?RBRK
Row: (ValueExpr COMMA?)*
Pointer:ID(DOT ID)*
Chain: NodeExpr(GOTO NodeExpr)*
NodeExpr: NodeFactor((PLUS|MINUS)NodeFactor)*
NodeFactor: NodeEntity((MUL|DIV)NodeEntity)*
NodeEntity: Node(CARET Node)*
Node: (PLUS|MINUS)Node|LPAR Bus RPAR|DIGIT|Matrix|Selector
Bus: Chain(COMMA Chain)*
Selector: Block(COLON ID)?
Block: Declare Configure?
Declare: Pointer(LBRK ID RBRK)?
Configure: LBRC(Parameter|BlockParameter)(COMMA(Parameter|BlockParameter))*RBRC
Parameter: ID COLON(STRING|ValueExpr)
BlockParameter: ID COLON Block
Route: ROUTE Inherit LPAR RouteParameters?SEMIC RoutePorts?SEMIC RoutePorts?RPAR Statements END
Inherit: ID(COLON ID)?
RouteParameters: RouteParameter(COMMA RouteParameter)*
RouteParameter: ID(COLON ID)?(FROM ValueExpr)
RoutePorts: ID(COMMA ID)*
Using: USING ID(DOT ID)*
Import: Import ID(DOT ID)*
~~~