# Ligral

<a href='https://gitee.com/junruoyu-zheng/ligral/stargazers'><img src='https://gitee.com/junruoyu-zheng/ligral/badge/star.svg?theme=gray' alt='star'></img></a>

Ligral是一个基于文本的仿真框图语言，旨在替代Simulink，编写、运行仿真程序。

## 语法

### 节点与连接

新建一个节点`Node`，`[ ]`内可选地定义节点的变量名。

~~~
Node | Node[node1]
~~~

为节点`Gain`添加参数`value`，多个参数可用“`,`”隔开。

~~~
Gain{value:1}
~~~

连接节点的输入输出，使用连接语句（chain）。支持顺序多次连接，行末需要加“`;`”。已经定义的节点可以通过变量名来引用。

~~~
node1 -> Gain{value:1} -> Integrator;
~~~

多输入输出节点的连接，可以使用“chain list”表达式：

~~~
(Constant{value:1}, Constant{value:2}) -> Calculator{type:'+'}
~~~

上述表达式亦可简写成`1+2`，四则运算符比`->`具有更高优先级。Chain list中每一项均可以是一个连接语句，但不需要以“`;`”结尾。

连接语句还可以基于端口（port），例如`ThresholdSwitch`的输入有三个端口，可以写作：

~~~
node1 -> ThresholdSwitch[switch]:condition;
1 -> switch:first; 
2 -> switch:second;
~~~

### 定义语句

常量定义由以下语句完成（*注意：`digit` 关键字可能于近期替换*），目前仅支持scalar，后期会增加vector和matrix。

~~~
digit var_name <- 1;
~~~

节点定义由以下语句完成：

~~~
route NodeName(parameters;in_ports;out_ports)
    some_chains;
end
~~~

其中`parameters`是该节点所需的参数，参数之间由“`,`”分隔。如果参数有默认值，则其为可选参数，否则是必选参数。若参数未指定类型，则参数是scalar，否则该参数必须是指定类型或其继承类型。

~~~
# parameters:
param1, param2:Node, param3<-2
~~~

如果指定一个类型继承另一个类型，可以写作：

~~~
route NodeName:Gain(parameters;input;output)...
~~~

指定继承关系后，子类型的输入输出端口必须和父类型一致，但参数可以不一致，如`Gain`的输入输出端口分别是`input`和`output`。指定了继承关系并不意味着继承了信号处理逻辑，如需用到父类的逻辑，可以在内部调用父类。

### 引用

导入另一个`.lig`文件的全部内容，使用`import`语句：

~~~
import module;
~~~

以命名空间的方式引用，使用`using`语句：

~~~
using module;
module.MyNode -> Scope;
~~~

## 用法

使用以下语句运行`.lig`文件：

~~~
ligral path/file.lig
~~~

支持以下参数：

|||
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

![code](https://pic4.zhimg.com/80/v2-a452963e3aa710b0aac72775deedc4ef_720w.jpg?source=c8b7c179)

![plots](https://pic4.zhimg.com/80/v2-b648e7e15562f2ccdc6fe87c21cbc873_720w.jpg?source=c8b7c179)
