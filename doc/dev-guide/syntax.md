<!-- Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# 语法解析

在用户文档你可能已经了解过 Ligral 的语法，但你可能不明白我为什么要这么设计，因为 Ligral 有许多非常新鲜的、独特的语法。这篇文章就来讲讲我设计 Ligral 的思路，以便让你们更快地融入开发者这一角色。后半部分介绍了目前的语法解析、解释上运用的方法、技巧和碰到的一些难题，以及对未来语法开发的展望。

## 语法的设计思路

语法设计是 Ligral 的源动力，因为 Ligral 的诞生就是为了解决在搭建仿真模型时使用 Simulink 的槽点。

实现一个基于文本语言的仿真程序有很多种实现方式，比如可以用 C++ 编写。事实上只要你实现了$f(x,u,t)$这个函数，调用求解器求解，就是一个仿真程序了。但是为什么大家都用 Simulink 来生成代码，而不是自己手动写呢？答案就是为了方便、直观。C++、Python 等语言不是为了仿真而设计的，这些语言功能固然强大，但是代价就是使用起来也较为繁琐，需要自己处理很多细节。一个假想的 python 程序如下所示：

    def f(x, u, t):
        return 1
    
    solver = Solver()
    solver.config(...)
    solver.solve(f, lambda x: x)
    x, y, t = solver.results

    # 处理 x 和 y，比如绘图


如果你将 Simulink 工程保存为`.mdl`文件，你可以用文本编辑器打开。实际上我们可以称这个文本文件里的代码为一种“MDL”语言，它和你在 Simulink 界面上看到的图形是完全等效的。MDL 语言和 Ligral 语言类似，是一种描述性的语言，也具有很多 C++ 这类命令式的语言所不具备的优点，比如无需考虑实现的细节。它的语法大致如下：

    Model {
        StartTime         "0.0"
        StopTime          "100"
        ...
        System {
            Block {
                BlockType         Constant
                Name              "Constant1"
                Value              "1"
                ...
            }
            Block {
                BlockType         Integrator
                Name              "Integrator1"
            }
            Line {
                SrcBlock              "Constant1"
                SrcPort               1
                DstBlock              "Integrator1"
                DstPort               1
                ...
            }
        }
    }

可以看到这是一种非常典型的声明语法，它首先描述了一些设置项，然后声明了所使用的模块，最后再定义了模块之间的连接。与之相似的，Ligral 开放给第三方的接口格式`.lig.json`也采用了类似的语法：

    {
        "settings": [
            {
                "item": "step_size",
                "value": 0.1
            },
            {
                "item": "stop_time",
                "value": 10
            }
        ],
        "models": [
            {
                "id": "Constant1",
                "type": "Constant",
                "parameters": [
                    {
                        "name": "value",
                        "value": 1
                    }
                ],
                "out-ports": [
                    {
                        "name": "value",
                        "destination": [
                            {
                                "id": "Integrator1",
                                "in-port": "input"
                            }
                        ]
                    }
                ]
            },
            {
                "id": "Integrator1",
                "type": "Integrator",
                "parameters": [],
                "out-ports": []
            }
        ]
    }

可是这样的文本仿真语言，比起图形语言真的有任何优势吗？从直观角度而言，它们差得远了；从编写的容易程度而言，Simulink 中你只要拖放两个模块，然后将它们连起来，这些文本语言却要写这么多，甚至还不如命令式的语言，真是令人望而生畏！说服 Simulink 用户去使用这样的语言是不现实的。但如果你使用 Ligral，只需要下面一行语句：

    1 -> Integrator;

把枯燥的、重复的工作交给解释器，用户只要把握住最核心的框图，就能写出一个仿真程序。

**因此，Ligral 语法设计从一开始就聚焦在如何方便用户。**

### 调用时声明

常见的命令式语言中，声明语句和调用语句是分开的。如果需要在多处调用一个节点，必须声明一个标识符，伪码如下：

    constant1 = Constant(1)
    constant1.connect(node1)
    constant1.connect(node2)

而在 Ligral 中，声明和调用允许同时进行。为了实现这一点，Ligral 放弃了常见的`=`号赋值这一做法，而采用了用一对`[ ]`来命名。

    Constant[constant1]{value:1} -> node1;
    constant1 -> node2;

### 全连接语法

在图形语言中，从端口连线到端口是很自然的一件事。但是在文本语言中，如果每次连接都显式指定端口，就会很繁琐。在 MDL 语言和`.lig.json`的语法中都是显式指定端口的，因为这样程序解析起来比较容易，但就苦了用户们（当然这两个语言本来也不是直接面向用户的）。Ligral 提供了全连接语法，只要上下游模块之间输出和输入端口一一对应，就可以之间连接而无需指定端口，这在单输入输出节点（最常见的情形）之间连接尤为方便。比如上面的代码`constant1 -> node2`就没有指定端口，Ligral 自动将对应的端口一一连接。

### 自动装箱

在图形语言中引入一个常数，就拖入一个常数模块，但在文本语言中写`Constant{value:1}`就显得过于厚重。为此，Ligral 提供了一个自动装箱的语法，在连接语句中识别到数字的就会自动将数字装箱成一个常数模块，从而简化语句为`1 -> node1`。

### 操作符

在图形语言中，双目运算是通过运算模块实现的，如果“直译”成 Ligral 语言就是：

    (1, 1) -> Calculator{type: '+'};

注意这里使用了自动装箱，但语句还是略显繁琐。Ligral 提供了一系列操作符，并在识别到操作符时自动构建`Calculator`模块，将语句简化为`1+1`。

### 空节点

图形语言的优势在于能表达错综复杂的关系，笔即二维图形的表达能力远超过一维（文字就是一种一维的表达）。但成也萧何，败也萧何，正因表达能力太强，才导致工程文件难以维护，因为人的思维其实是线性的。

在 Ligral 中，为了解决表达能力的问题，并且在保持语法简洁性，引入了空节点这一概念。空节点实际上是一个模块，直接转发输入到输出。这允许用户在一个线性的连接任意位置打上标记，并在另一个语句（甚至同一个语句）中引用。例如表示一个环：

     ╭───╮           ┌─────┐          ╭───╮
     │ u ├───►(x)───►│ 1/s ├────┬────►│ x │
     ╰───╯     ▲-    └─────┘    │     ╰───╯
               │    ┌───┐       │
               └────┤ k │◄──────┘
                    └───┘   

利用空节点来标记很轻松就能用 Ligral 表达：

    u-k*x -> Integrator -> x;

空节点在 Ligral 中非常常见，因此我们简化了空节点的声明语法。完整的声明应该是`Node[x]`，但在解释器里，凡是出现没有声明过的标识符一律处理成空节点。但这么做有一个问题，在于当用户不小心拼错标识符名称，或者本该声明标识符但是没有声明，该标识符会被识别成一个新的空节点，从而报出意料之外的错误（通常是节点未连接）。这个问题是需要解决的，但目前没有很好的处理办法。

### 矩阵构建语法

在 Simulink 中，从标量构建矩阵是由`Mux`模块实现的；反之，从矩阵解构到标量是通过`DeMux`模块。Ligral 与之对应的模块是`Stack`系列和`Split`系列（Ligral 对行和列两个方向作了区分）。目前最后一步采用`Split`而非`VSplit`的原因是标量和矩阵尚未统一，`VSplit`分解后的数据仍是矩阵而非标量。

    (1, 2) -> HStack -> row1;
    (3, 4) -> HStack -> row2;
    (row1, row2) -> VStack -> mat;

    mat -> HSplit -> (col1, col2);
    col1 -> Split -> (item11, item21);
    col2 -> Split -> (item12, item22);

但在文本语言中，矩阵的操作还能再简化一些。

    [1, 2; 3, 4] -> mat -> [item11, item12; item21, item22];

这样更符合文本语言使用者的使用习惯。