# 语法

## Program

~~~
Program: Statements
~~~
![Program](Program.svg)

## Statements

~~~
Statements: Statement*
~~~
![Statements](Statements.svg)

## Statement

~~~
Statement: (ProgramConfig|Define|Using|Import|Chain) SEMIC|Route
~~~
![Statement](Statement.svg)

## ProgramConfig

~~~
ProgramConfig: CONF ID FROM (TRUE|FALSE|STRING|ValueExpr)
~~~
![ProgramConfig](ProgramConfig.svg)

## Define

~~~
Define: ASSIGN ID FROM ValueExpr
~~~
![Define](Define.svg)

## ValueExpr

~~~
ValueExpr: ValueFactor((PLUS|MINUS)ValueFactor)*
~~~
![ValueExpr](ValueExpr.svg)

## ValueFactor

~~~
ValueFactor: ValueEntity((MUL|DIV)ValueEntity)*
~~~
![ValueFactor](ValueFactor.svg)

## ValueEntity

~~~
ValueEntity: Value(CARET Value)*
~~~
![ValueEntity](ValueEntity.svg)

## Value

~~~
Value: (PLUS|MINUS)Value|LPAR ValueExpr RPAR|DIGIT|Matrix|Pointer
~~~
![Value](Value.svg)

## Matrix

~~~
Matrix: LBRK Row(SEMIC Row)*RBRK
~~~
![Matrix](Matrix.svg)

## Row

~~~
Row: ValueExpr(COMMA ValueExpr)*
~~~
![Row](Row.svg)

## Pointer

~~~
Pointer:ID(DOT ID)*
~~~
![Pointer](Pointer.svg)

## Chain

~~~
Chain: NodeExpr(GOTO NodeExpr)*
~~~
![Chain](Chain.svg)

## NodeExpr

~~~
NodeExpr: NodeFactor((PLUS|MINUS)NodeFactor)*
~~~
![NodeExpr](NodeExpr.svg)

## NodeFactor

~~~
NodeFactor: NodeEntity((MUL|DIV)NodeEntity)*
~~~
![NodeFactor](NodeFactor.svg)

## NodeEntity

~~~
NodeEntity: Node(CARET Node)*
~~~
![NodeEntity](NodeEntity.svg)

## Node

~~~
Node: (PLUS|MINUS)Node|LPAR Bus RPAR|DIGIT|Matrix|Selector
~~~
![Node](Node.svg)

## Bus

~~~
Bus: Chain(COMMA Chain)*
~~~
![Bus](Bus.svg)

## MatrixMux

~~~
MatrixMux: LBRK RowMux(SEMIC RowMux)*RBRK
~~~
![MatrixMux](MatrixMux.svg)

## RowMux

~~~
RowMux: NodeExpr(COMMA NodeExpr)*
~~~
![RowMux](RowMux.svg)

## MatrixDeMux

~~~
MatrixDeMux: LBRK RowDeMux(SEMIC RowDeMux)*RBRK
~~~
![MatrixDeMux](MatrixDeMux.svg)

## RowDeMux

~~~
RowDeMux: Selector(COMMA Selector)*
~~~
![RowDeMux](RowDeMux.svg)

## Selector

~~~
Selector: Block(COLON ID)?
~~~
![Selector](Selector.svg)

## Port

~~~
Port: ID(LBRK ID LBRK)?
~~~
![Port](Port.svg)

## Block

~~~
Block: Declare Configure?
~~~
![Block](Block.svg)

## Declare

~~~
Declare: Pointer(LBRK ID RBRK)?
~~~
![Declare](Declare.svg)

## Configure

~~~
Configure: LBRC(Parameter|BlockParameter)(COMMA(Parameter|BlockParameter))*RBRC
~~~
![Configure](Configure.svg)

## Parameter

~~~
Parameter: ID COLON(STRING|ValueExpr)
~~~
![Parameter](Parameter.svg)

## BlockParameter

~~~
BlockParameter: ID COLON Block
~~~
![BlockParameter](BlockParameter.svg)

## Route

~~~
Route: ROUTE Inherit LPAR RouteParameters?SEMIC RoutePorts?SEMIC RoutePorts?RPAR Statements END
~~~
![Route](Route.svg)

## Inherit

~~~
Inherit: ID(COLON ID)?
~~~
![Inherit](Inherit.svg)

## RouteParameters

~~~
RouteParameters: RouteParameter(COMMA RouteParameter)*
~~~
![RouteParameters](RouteParameters.svg)

## RouteParameter

~~~
RouteParameter: ID(COLON ID)?(FROM ValueExpr)?
~~~
![RouteParameter](RouteParameter.svg)

## RoutePorts

~~~
RoutePorts: ID(COMMA ID)*
~~~
![RoutePorts](RoutePorts.svg)

## Using

~~~
Using: USING ID(DOT ID)*
~~~
![Using](Using.svg)

## Import

~~~
Import: Import ID(DOT ID)*
~~~
![Import](Import.svg)