<!-- Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# Ligral

![LOGO](https://sued-wind.cc/static/img/ligral/ligral.png)

Ligral是一个基于文本的仿真语言，旨在替代 Simulink 进行仿真，通过与框图等效的文本语言描述仿真对象，并解释/编译而后求解。

<p>
<a href="https://gitee.com/junruoyu-zheng/ligral/releases/v0.2.1"><img src="https://img.shields.io/badge/版本-0.2.1-brightgreen"></img></a>
<a href='https://gitee.com/junruoyu-zheng/ligral/stargazers'><img src='https://gitee.com/junruoyu-zheng/ligral/badge/star.svg?theme=gray' alt='star'></img></a>
<a href="README_en.md"><img src="https://img.shields.io/badge/English-README-blue"></img></a>
</p>

## 安装

目前发布了[v0.2.2](https://gitee.com/junruoyu-zheng/ligral/releases/v0.2.2-beta) 预览版，您也可以克隆本项目到本地编译。

    git clone https://gitee.com/junruoyu-zheng/ligral.git
    cd ligral
    dotnet build
    ./bin/Debug/netcoreapp3.1/ligral

详细安装方法请参考[快速开始](doc/quick-start/README.md)。

## 使用

运行仿真工程需要给出仿真工程文件作为第一个位置参数，该命令支持以下参数：

|   参数        | 作用           |
|   --          | --            |
|  [ProjectFileName] |  仿真工程文件，一般是`.lig`和`.lig.json`文件      |
|  -s, --step-size [StepSize]    | 设置仿真步长，仅在定步长求解器中生效   |
|  -t, --stop-time [StopTime]    | 设置仿真时长 |
|  -j, --json [bool]?    | 指定输入`.lig.json`文件，默认为输入`.lig`文件 |
|  -o, --output [Folder] | 仿真输出重定向至给定文件夹                    |

Ligral 目前支持的命令包括`doc`、`trim`、`lin`和`exm`，可以通过`ligral [command] --help`查看使用说明。

## 依赖

如果启用内部绘图工具（`InnerPlotter`），绘图模块`Scope`和`PhaseDiagram`需要 python 3 支持，且需要numpy、matplotlib、pandas包。

## 示例

以下是一个弹簧阻尼质量块系统的仿真（[代码](examples/mass-spring-damper/main.lig)）。

    # 定义一个路由 MassSpringDamper
    route MassSpringDamper(m, k, d, x0, v0; F; x, v)
        F-k*x-d*v -> Gain{value:1/m} -> Integrator{initial:v0} -> v;
        v -> Integrator{initial:x0} -> x;
    end

    # 定义一个 Step 信号
    Step[F]{start:3, level:5};

    # 实例化一个 MassSpringDamper 对象
    MassSpringDamper[sys]{m:0.1, k:10, d:0.3, x0:1, v0: 0};

    # 连接模块
    F -> sys;
    sys:x[position] -> Scope;
    (sys:x, sys:v[velocity]) -> PhaseDiagram;

    # 设置定步长仿真配置参数
    conf step_size = 0.001;
    conf stop_time = 10;

    # 设置输出文件夹
    conf output_folder = 'out';

![plots!!](web/img/mass-spring-damper.gif)

更多例子参见[examples](examples/)

## 语法

语法参见[用户指引](doc/user-guide/README.md)，目前已编写完成的文档如下：

- [术语定义](doc/user-guide/terms.md)
- [设置语句](doc/user-guide/config.md)
- [声明常量](doc/user-guide/const.md)
- [声明节点](doc/user-guide/node.md)
- [节点连接](doc/user-guide/link.md)
- [矩阵运算](doc/user-guide/matrix.md)
- [路由类型](doc/user-guide/route.md)
- [引用依赖](doc/user-guide/import.md)
- [接口签名](doc/user-guide/signature.md)

## 开发

目前开发文档还在努力编撰中。

- [语法设计](doc/dev-guide/syntax.md)
- [解释器](doc/dev-guide/interpreter.md)
- ...

如有 bug 反馈或其他建议，请提交 [issue](https://gitee.com/junruoyu-zheng/ligral/issues)。

开发计划：

- 增加单元测试
- 增加科学计算语法，进行仿真前处理、后处理
- 统一数据格式为矩阵
- 修复 bugs
- ...
