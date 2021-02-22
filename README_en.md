<!-- Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# Ligral

![LOGO](https://sued-wind.cc/static/img/ligral/ligral.png)

Ligral is a textual simulation language as an alternative to Simulink. The goal of ligral is to describe simulation models with a set of statements instead of a graphic implementation. The syntax of ligral ensures equally powerful ability of model description. Meanwhile, ligral provides various solvers to solve the ode problems given by the interpreter.

<p>
<a href="https://github.com/jry-zheng/ligral/releases/v0.2.1"><img src="https://img.shields.io/badge/Version-0.2.1-brightgreen"></img></a>
<a href='https://github.com/JRY-Zheng/ligral/stargazers'><img src='https://img.shields.io/github/stars/JRY-Zheng/ligral' alt='star'></img></a>
</p>

## How to Install

We now have released [v0.2.2](https://github.com/jry-zheng/ligral/releases/v0.2.2-beta) which is available at GitHub. Alternatively, you may also want to clone the source to your workspace and build ligral manually.

    git clone https://github.com/jry-zheng/ligral.git
    cd ligral
    dotnet build
    ./bin/Debug/netcoreapp3.1/ligral

For detailed installation guide see [Quick Start](doc/quick-start/README.md)。

## Usage

The main function of ligral absolutely is run a simulation project. The project name, usually a `.lig` file name, should be the first positional parameter, like `ligral path/file.lig`. Besides, many parameters are supported:

|   Parameters        | Functions           |
|   --          | --            |
|  [ProjectFileName] |  Simulation project name, usually a `.lig` file or a `.lig.json` file.      |
|  -s, --step-size [StepSize]    | The step size of simulation, works only with fixed step solvers.   |
|  -t, --stop-time [StopTime]    | The total time when the solver should stop. |
|  -j, --json [bool]?    | Toggle the input to be `.lig.json` file. Ligral parse the project as `.lig` file if this is not toggled |
|  -o, --output [Folder] | Redirect the output folder. |

The other commands that ligral supports include `doc`, `trim`, `lin` and `exm`, run `ligral [command] --help` to see more about the usage.

## Dependency

If `InnerPlotter` is enabled and `Scope` or `PhaseDiagram` is used in your project, you need python 3 to let them work. Packages numpy、matplotlib、pandas are also required. 

## Examples

Following is a [mass-spring-damper system](examples/mass-spring-damper/main.lig) implemented by ligral.

    # Define a route named MassSpringDamper
    route MassSpringDamper(m, k, d, x0, v0; F; x, v)
        F-k*x-d*v -> Gain{value:1/m} -> Integrator{initial:v0} -> v;
        v -> Integrator{initial:x0} -> x;
    end

    # Define a step signal
    Step[F]{start:3, level:5};

    # Instantiate a MassSpringDamper object
    MassSpringDamper[sys]{m:0.1, k:10, d:0.3, x0:1, v0: 0};

    # Link models
    F -> sys;
    sys:x[position] -> Scope;
    (sys:x, sys:v[velocity]) -> PhaseDiagram;

    # Configure the solver
    conf step_size = 0.001;
    conf stop_time = 10;

    # Redirect the output folder
    conf output_folder = 'out';

![plots!!](web/img/mass-spring-damper.gif)

More examples see [examples](examples/).

## Syntax

See detailed syntax in [User Guide](doc/user-guide/README.md), which are written in Chinese currently. If you are interested in ligral but cannot read Chinese, please contact me via [zhengjry@outlook.com](mailto:zhengjry@outlook.com). Followings are completed:

- [Terms](doc/user-guide/terms.md)
- [Configure statements](doc/user-guide/config.md)
- [How to declare a constant?](doc/user-guide/const.md)
- [How to declare a node?](doc/user-guide/node.md)
- [How to link nodes?](doc/user-guide/link.md)
- [Matrices in ligral](doc/user-guide/matrix.md)
- [What is route?](doc/user-guide/route.md)
- [Reference packages](doc/user-guide/import.md)
- [What is signature?](doc/user-guide/signature.md)

## Development

We are working hard to write [Development Guide](doc/dev-guide/README.md). Now the followings are available with Chinese version.

- [Syntax design](doc/dev-guide/syntax.md)
- [Interpreter Implementation](doc/dev-guide/interpreter.md)
- ...

If you found a bug please submit an [issue](https://gitee.com/junruoyu-zheng/ligral/issues).

Development plan:

- Add unit test;
- Add `if-else`, `for`, `while` statements and functions syntax for pre- and post-simulation process; 
- Unify the data signal to be a matrix, which means a scalar will be regarded as a 1 by 1 matrix;
- Fix bugs
- ...
