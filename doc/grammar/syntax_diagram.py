# Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import os
from railroad import *

folder = os.path.dirname(__file__)

originalZeroOrMore = ZeroOrMore
def ZeroOrMore(item, sep=None):
    if sep:
        return originalZeroOrMore(item, sep)
    else:
        return OneOrMore(Skip(), item)

def draw(name, *args):
    d = Diagram(Start('simple', name), *args)
    with open(os.path.join(folder, f'{name}.svg'), 'w') as f:
        d.writeSvg(f.write)
    # print(f'~~~\n\n~~~\n![{name}]({name}.svg)\n')

draw("Program", NonTerminal("Statements"))
draw("Statements", OneOrMore(Skip(), NonTerminal("Statement")))
draw("Statement", Choice(4,
    NonTerminal("ProgramConfig"),
    NonTerminal("Define"),
    NonTerminal("Using"),
    NonTerminal("Import"),
    Sequence(NonTerminal("Chain"), "SEMIC"),
    NonTerminal("Signature"),
    NonTerminal("Route")
))
draw("ProgramConfig", Sequence("CONF", "ID", "FROM",
    Choice(3, "TRUE", "FALSE", "STRING", NonTerminal("ValueExpr")), "SEMIC"
))
draw("Define", Sequence("ASSIGN", "ID", "FROM", NonTerminal("ValueExpr"), "SEMIC"))
draw("ValueExpr", OneOrMore(NonTerminal("ValueFactor"), Choice(0, "PLUS", "MINUS")))
draw("ValueFactor", OneOrMore(NonTerminal("ValueEntity"), Choice(0, "MUL", "DIV")))
draw("ValueEntity", OneOrMore(NonTerminal("Value"), "CARET"))
draw("Value", Choice(1,
    Sequence(Choice(0, "PLUS", "MINUS"), NonTerminal("Value")),
    Sequence("LPAR", NonTerminal("ValueExpr"), "RPAR"),
    "DIGIT", NonTerminal("Matrix"), NonTerminal("Pointer")
))
draw("Matrix", Sequence("LBRK", OneOrMore(NonTerminal("Row"), "SEMIC"), "RBRK"))
draw("Row", OneOrMore(NonTerminal("ValueExpr"), "COMMA"))
draw("Pointer", OneOrMore("ID", "DOT"))
draw("Chain", OneOrMore(NonTerminal("NodeExpr"), "GOTO"))
draw("NodeExpr", OneOrMore(NonTerminal("NodeFactor"), Choice(0, "PLUS", "MINUS")))
draw("NodeFactor", OneOrMore(NonTerminal("NodeEntity"), Choice(0, "MUL", "DIV")))
draw("NodeEntity", OneOrMore(NonTerminal("Node"), "CARET"))
draw("Node", Choice(1,
    Sequence(Choice(0, "PLUS", "MINUS"), NonTerminal("Node")),
    Sequence("LPAR", NonTerminal("Bus"), "RPAR"),
    "DIGIT", NonTerminal("MatrixMux"), NonTerminal("MatrixDeMux"), NonTerminal("Selector")
))
draw("Bus", OneOrMore(NonTerminal("Chain"), "COMMA"))
draw("MatrixMux", Sequence("LBRK", OneOrMore(NonTerminal("RowMux"), "SEMIC"), "RBRK"))
draw("RowMux", OneOrMore(NonTerminal("NodeExpr"), "COMMA"))
draw("MatrixDeMux", Sequence("LBRK", OneOrMore(NonTerminal("RowDeMux"), "SEMIC"), "RBRK"))
draw("RowDeMux", OneOrMore(NonTerminal("Selector"), "COMMA"))
draw("Selector", Sequence(NonTerminal("Block"), Optional(Sequence("COLON", NonTerminal("Port")))))
draw("Port", Sequence("ID", Optional(Sequence("LBRK", "ID", "RBRK"))))
draw("Block", Sequence(NonTerminal("Declare"), Optional(NonTerminal("Configure"))))
draw("Declare", Sequence(NonTerminal("Pointer"), Optional(
    Sequence("LBRK", "ID", "RBRK")
)))
draw("Configure", Sequence("LBRC", 
    OneOrMore(Choice(0, NonTerminal("Parameter"), NonTerminal("BlockParameter")), "COMMA"), "RBRC"
))
draw("Parameter", Sequence("ID", "COLON", Choice(1, "STRING", NonTerminal("ValueExpr"))))
draw("BlockParameter", Sequence("ID", "COLON", NonTerminal("Block")))
draw("Signature", Stack(Sequence("SIGNATURE", "ID", "LPAR"), 
    Sequence(Optional(NonTerminal("RoutePorts")), "SEMIC"), 
    Sequence(Optional(NonTerminal("RoutePorts")), "RPAR", "SEMIC")
))
draw("Route", Stack(Sequence("ROUTE", NonTerminal("Inherit"), "LPAR"), 
    Sequence(Optional(NonTerminal("RouteParameters")),"SEMIC"), 
    Sequence(Optional(NonTerminal("RoutePorts")), "SEMIC"), 
    Optional(NonTerminal("RoutePorts")), 
    Sequence("RPAR", NonTerminal("Statements"), "END")
))
draw("Inherit", Sequence("ID", Optional(Sequence("COLON", "ID"))))
draw("RouteParameters", OneOrMore(NonTerminal("RouteParameter"), "COMMA"))
draw("RouteParameter", Sequence("ID", Stack(Optional(Sequence("COLON", "ID")), 
    Optional(Sequence("FROM", NonTerminal("ValueExpr"))))
))
draw("RoutePorts", OneOrMore("ID", "COMMA"))
draw("Using", Sequence("USING", ZeroOrMore("DOT"), OneOrMore("ID", "DOT"),
    Optional(Sequence("COLON", "ID")), "SEMIC"
))
draw("Import", Sequence("IMPORT", ZeroOrMore("DOT"), OneOrMore("ID", "DOT"), 
    Optional(Sequence("COLON", OneOrMore("ID", "COMMA"))), "SEMIC"
))