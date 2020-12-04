# Ligral

<a href='https://gitee.com/junruoyu-zheng/ligral/stargazers'><img src='https://gitee.com/junruoyu-zheng/ligral/badge/star.svg?theme=gray' alt='star'></img></a>

![LOGO](https://sued-wind.cc/static/img/ligral/ligral.png)

Ligral是一个基于文本的仿真框图语言，旨在替代Simulink，编写、运行仿真程序。

**版本**：v0.2.0

## 语法

### 节点与连接

新建一个节点`Node`，`[ ]`内可选地定义节点的变量名。

    Node | Node[node1]


为节点`Gain`添加参数`value`，多个参数可用“`,`”隔开。

    Gain{value:1}


连接节点的输入输出，使用连接语句（chain）。支持顺序多次连接，行末需要加“`;`”。已经定义的节点可以通过变量名来引用。

    node1 -> Gain{value:1} -> Integrator;


多输入输出节点的连接，可以使用“chain list”表达式：

    (Constant{value:1}, Constant{value:2}) -> Calculator{type:'+'};


Chain list中每一项均可以是一个连接语句，但不需要以“`;`”结尾，如下所示：

    (
        Constant{value:1} -> Gain{value:2}, 
        Constant{value:-2} -> Abs
    ) -> Calculator{type:'+'};


连接语句还可以基于端口（port），例如`ThresholdSwitch`的输入有三个端口，可以写作：

    node1 -> ThresholdSwitch[switch]:condition;
    1 -> switch:first; 
    2 -> switch:second;


### 定义语句

常量定义由以下语句完成（*注意：`digit` 关键字和 `<-` 符号将于下个版本废弃*），目前支持 scalar 和二维的 matrix。

    let a = 1;
    let vec = [1, 2]
    let mat = [-a, 2; vec];


节点定义由以下语句完成：

    route NodeName(parameters;in_ports;out_ports)
        some_chains;
    end


其中`parameters`是该节点所需的参数，参数之间由“`,`”分隔。如果参数有默认值，则其为可选参数，否则是必选参数。若参数未指定类型，则参数是scalar，否则该参数必须是指定类型或其继承类型。

    # parameters:
    param1, param2:Node, param3=2


如果指定一个类型继承另一个类型，可以写作：

    route NodeName:Gain(parameters;input;output)...


指定继承关系后，子类型的输入输出端口必须和父类型一致，但参数可以不一致，如`Gain`的输入输出端口分别是`input`和`output`。指定了继承关系并不意味着继承了信号处理逻辑，如需用到父类的逻辑，可以在内部调用父类。

### 引用

导入另一个`.lig`文件的全部内容，使用`import`语句：

    import module;


以命名空间的方式引用，使用`using`语句：

    using module;
    module.MyNode -> Scope;


### 自动装箱

连接语句中的 `1` 会被自动装箱成 `Constant{value:1}`。此外，如果是一个符号，如

    let a = 1;
    a -> Print;


此处的 `a` 也会被自动装箱。除了数字以外，矩阵也会被自动装箱，但是只有矩阵符号会被装箱，如下两个表达是等价的。

    let a = [1, 2];
    a -> Print;
    Constant{value:a} -> Print;


### 运算符

在常量定义语句中，支持 `+`, `-`, `*`, `/` 和 `^` 等常见的运算符。

在连接语句中，这些运算符将被解析成以下结构：

    let a = 1;

    a + 1 -> Print;
    (a, 1) -> Calculate{type:'+'} -> Print;

    -a -> Print;
    (0, a) -> Calculate{type:'-'} -> Print;


### 矩阵构建解构器

直接在连接语句中定义的“矩阵”则不会被直接装箱，因为该“矩阵”所包含的不一定是常数。通常，在连接语句中的“矩阵”我们称之为矩阵构建解构器，根据其位置（句首、句中、句尾）又分为构建器、构建解构器和解构器。以构建解构器为例，解释器将其解释成如下结构，提供了语法层面的便利。

    let vec = [1, -2];
    vec -> [Node, Abs] -> Print;
    vec -> VSplit -> Split -> (Node, Abs) -> HStack -> VStack -> Print;


其中 `VSplit` 将矩阵按行分解成单行的矩阵，`Split` 将矩阵分解成数字；`HStack` 将元素横向堆叠成矩阵，`VStack` 将元素纵向堆叠成矩阵。构建解构器的本质就是对一个矩阵信号结构后进行逐元素遍历，再重新构建成一个矩阵的过程，因此要求元素行列数与矩阵信号一致，且均为但输入输出的节点。

位于句尾的解构器则允许出现非单输出的节点，即对输出节点数目不做限制，也不会自动构建 Stack 族的节点。位于句首的构建器则更加灵活，还允许出现表达式。

    let mat = [1, 2];
    [mat; 2*mat] -> [_, _; Print, _];


此处的 `_` 是弃元，解释器将其解释成一个特殊的节点，在运行过程中该节点不会执行任何语句，用于声明放弃使用一些信号。上述例子中，构建器内包含一些运算，嵌套了一些矩阵，输出的矩阵信号被解构器解析，并打印左下角的值。

### 设置语句

可以在脚本中对求解器进行配置。目前支持的字段如下所示：

    conf step_size = 0.01;
    conf stop_time = 10;
    conf variable_step = false;
    conf output_folder = 'output';


## 用法

使用以下语句运行`.lig`文件：

    ligral path/file.lig


支持以下参数：

|参数|作用|
|--|--|
|  -o, --output  | 输出重定向至给定文件夹 |
|  -d, --doc     | 展示指定节点的文档 |
|  -D, --docs    | 展示所有节点文档 |
|  -h            | 展示示例程序 |
|  -s, --size    | 设置仿真步长 |
|  -t, --time    | 设置仿真时长 |
|  --help        | 展示帮助 |
|  --version     | 展示版本信息 |

## 依赖

绘图模块`Scope`和`PhaseDiagram`需要 python 3 支持，且需要numpy、matplotlib、pandas包。

## 示例

以下是一个弹簧阻尼质量块系统的仿真。

    route MassSpringDamper(m,k,d,x0,v0; F; x,v)
        F-k*x-d*v -> Gain{value:1/m} -> Integrator{initial:v0} -> v;
        v -> Integrator{initial:x0} -> x;
    end

    SineWave[F]{ampl:10, omega:pi, phi:pi/4};
    MassSpringDamper[sys]{m:10, k:1, d:0.1, x0:0, v0: 0};

    F -> sys;
    sys:x -> Scope{name:'position'};
    (sys:x, sys:v) -> PhaseDiagram{name:'phase plot'};

    conf step_size = 0.001;
    conf stop_time = 10;


![plots](https://pic4.zhimg.com/80/v2-b648e7e15562f2ccdc6fe87c21cbc873_720w.jpg?source=c8b7c179)
