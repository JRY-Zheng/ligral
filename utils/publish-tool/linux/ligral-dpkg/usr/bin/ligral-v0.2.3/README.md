Ligral is a textual simulation language as an alternative to Simulink. The goal of ligral is to describe simulation models with a set of statements instead of a graphic implementation. The syntax of ligral ensures equally powerful ability of model description. Meanwhile, ligral provides various solvers to solve the ode problems given by the interpreter.

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