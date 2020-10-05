# Grammar

~~~
Program: Statements
~~~
![Program](Program.svg)

~~~
Statements: Statement*
~~~
![Statements](Statements.svg)

~~~
Statement: (ProgramConfig|Define|Using|Import|Chain) SEMIC|Route
~~~
![Statement](Statement.svg)

~~~
ProgramConfig: CONF ID FROM (TRUE|FALSE|STRING|ValueExpr)
~~~
![ProgramConfig](ProgramConfig.svg)

~~~
Define: ASSIGN ID FROM ValueExpr
~~~
![Define](Define.svg)

~~~
ValueExpr: ValueFactor((PLUS|MINUS)ValueFactor)*
~~~
![ValueExpr](ValueExpr.svg)

~~~
ValueFactor: ValueEntity((MUL|DIV)ValueEntity)*
~~~
![ValueFactor](ValueFactor.svg)

~~~
ValueEntity: Value(CARET Value)*
~~~
![ValueEntity](ValueEntity.svg)

~~~
Value: (PLUS|MINUS)Value|LPAR ValueExpr RPAR|DIGIT|Matrix|Pointer
~~~
![Value](Value.svg)

~~~
Matrix: LBRK Row(SEMIC Row)*SEMIC?RBRK
~~~
![Matrix](Matrix.svg)

~~~
Row: (ValueExpr COMMA?)*
~~~
![Row](Row.svg)

~~~
Pointer:ID(DOT ID)*
~~~
![Pointer](Pointer.svg)

~~~
Chain: NodeExpr(GOTO NodeExpr)*
~~~
![Chain](Chain.svg)

~~~
NodeExpr: NodeFactor((PLUS|MINUS)NodeFactor)*
~~~
![NodeExpr](NodeExpr.svg)

~~~
NodeFactor: NodeEntity((MUL|DIV)NodeEntity)*
~~~
![NodeFactor](NodeFactor.svg)

~~~
NodeEntity: Node(CARET Node)*
~~~
![NodeEntity](NodeEntity.svg)

~~~
Node: (PLUS|MINUS)Node|LPAR Bus RPAR|DIGIT|Matrix|Selector
~~~
![Node](Node.svg)

~~~
Bus: Chain(COMMA Chain)*
~~~
![Bus](Bus.svg)

~~~
Selector: Block(COLON ID)?
~~~
![Selector](Selector.svg)

~~~
Block: Declare Configure?
~~~
![Block](Block.svg)

~~~
Declare: Pointer(LBRK ID RBRK)?
~~~
![Declare](Declare.svg)

~~~
Configure: LBRC(Parameter|BlockParameter)(COMMA(Parameter|BlockParameter))*RBRC
~~~
![Configure](Configure.svg)

~~~
Parameter: ID COLON(STRING|ValueExpr)
~~~
![Parameter](Parameter.svg)

~~~
BlockParameter: ID COLON Block
~~~
![BlockParameter](BlockParameter.svg)

~~~
Route: ROUTE Inherit LPAR RouteParameters?SEMIC RoutePorts?SEMIC RoutePorts?RPAR Statements END
~~~
![Route](Route.svg)

~~~
Inherit: ID(COLON ID)?
~~~
![Inherit](Inherit.svg)

~~~
RouteParameters: RouteParameter(COMMA RouteParameter)*
~~~
![RouteParameters](RouteParameters.svg)

~~~
RouteParameter: ID(COLON ID)?(FROM ValueExpr)?
~~~
![RouteParameter](RouteParameter.svg)

~~~
RoutePorts: ID(COMMA ID)*
~~~
![RoutePorts](RoutePorts.svg)

~~~
Using: USING ID(DOT ID)*
~~~
![Using](Using.svg)

~~~
Import: Import ID(DOT ID)*
~~~
![Import](Import.svg)