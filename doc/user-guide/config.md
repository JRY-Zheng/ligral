<!-- Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# 设置语句

求解一个仿真模型需要用到许多设置参数，比如求解器的选择和配置、输出文件存放位置等。

这些设置语句部分可以在程序运行时通过参数传入，如果没有参数传入，则以 lig 文件中的设置语句为准。如果在 lig 文件中没有进行设置，则会采用默认参数。默认参数记录在 ligral 可执行程序所在文件夹的 default.lig 文件中。如果该文件不存在，则会使用硬编码在 ligral 程序中的设置。

以下列举了目前支持的设置项。

### 步长

**定义：** 该设置项适用于定步长求解器，给定了该求解器的步长。

**默认：** 0.01 秒。

**设置方法：**

- 可以在运行时添加`-s`参数进行设置，例如：`ligral test.lig -s 0.01`，将步长设置为 0.01 秒。
- 可以在 lig 文件中写下`conf step_size = 0.01;`，也可达到同样效果。

### 时长

**定义：** 该设置项适用于所有求解器，给定了该求解器的仿真总时长。

**默认：** 10 秒。

**设置方法：**

- 可以在运行时添加`-t`参数进行设置，例如：`ligral test.lig -t 10`，将总时长设置为 10 秒。
- 可以在 lig 文件中写下`conf stop_time = 10;`，也可达到同样效果。

### 输出文件夹

**定义：** 所有在 ligral 中标记为输出的信号（例如 Scope、Print 和 PhaseDiagram）都会被保存到本地的 CSV 文件中（具体参见模块文档），除此之外一些模块可能也会有输出文件，该设置项设置了输出文件存放的文件夹。

**默认：** 输出文件放在与 lig 文件同名的文件夹下。运行`ligral folder/test.lig`，若 lig 文件中没有规定输出文件夹，则输出文件被放置在文件夹 folder/test/* 下。

**设置方法：**

- 可以在运行时添加`-o`参数进行设置，例如：`ligral test.lig -o out`，将输出文件夹设置为 out。
- 可以在 lig 文件中写下`conf output_folder = 'out';`，也可达到同样效果。

### IP 地址

**定义：** Ligral 程序运行时通过发送、接收 UDP 包的方式与其他程序共享数据，改设置项设置了网关的 IP 地址。

**默认：** 127.0.0.1。

**设置方法：**

- 可以在 lig 文件中写下`conf ip_address = '127.0.0.1';`，将 IP 地址设置为 127.0.0.1。

### 端口

**定义：** Ligral 程序运行时通过发送、接收 UDP 包的方式与其他程序共享数据，改设置项设置了网关的接收端口，发送端口为接收端口加一。

**默认：** 接收端口8783，发送端口8784。

**设置方法：**

- 可以在 lig 文件中写下`conf port = 8783;`，将接收端口设置为 8783，发送端口设置为8784。也可以通过`listening_port`和`sending_port`分别设置接收端口和发送端口。

### 求解器

**定义：** 该项设置了仿真采用的求解器。目前支持的求解器包括：`ode1`(`euler`)、`ode2`、`ode2m`、`ode4`、`ode45`等，其中`ode2m`在 control 包中实现。

**默认：** ode4

**设置方法：**

- 可以在 lig 文件中写下`conf solver = 'ode2';`，设置求解器。

### 实时仿真

**定义：** 该项设置了是否启用实时仿真。如果启用实时仿真，求解器必须数定步长求解器。如果启用了内部绘图模块，会自动调用实时绘图模块，如果使用外部绘图，则会实时发送数据至绘图工具，从而实现实时绘图。
**默认：** 非实时仿真。

**设置方法：**

- 可以在 lig 文件中写下`conf realtime = true;`，启用实时仿真。

### Python 解释器

**定义：** 该项设置了 Python 可执行程序的路径（含文件名），如果已经在系统目录，也可以只指定文件名。在 Windows 系统下，默认的 python 即可，在 Linux 下一般需要改成 python3。未来可能考虑识别系统信息，修改默认值。

**默认：** python

**设置方法：**

- 可以在 lig 文件中写下`conf python = 'python';`，设置解释器可执行文件路径。

### 版权信息

**定义：** 包含一组设置项，包括`author`、`date`、`license`、`email`、`homepage`（或`home_page`），这些设置项不影响程序运行。

**默认：** 无

**设置方法：**

- 可以在 lig 文件中写下`conf author = 'JRY Zheng';`。

### 内部绘图工具

**定义：** 该项是一个设置集合，用来设置内部绘图工具的一些属性。

**设置项：** ：

- `enable`：设置是否启用内部绘图模块
  - **默认：** 不启用
  - **设置方法：** `conf inner_plotter.enable = true;`
- `output_script`：设置是否将绘制图像的脚本保存，仅在非实时仿真有效
  - **默认：** 不输出脚本
  - **设置方法：** `conf inner_plotter.output_script = true;`
- `save_figure`：设置是否将绘制的图像保存
  - **默认：** 不保存图像
  - **设置方法：** `conf inner_plotter.save_figure = true;`

### 日志系统

**定义：** 该项是一个设置集合，用来设置日志系统的一些属性。

**设置项：** ：

- `print_out`：设置是否将日志输出到屏幕
  - **默认：** 是
  - **设置方法：** `conf logger.print_out = true;`
- `min_print_out_level`：设置将日志输出到屏幕的最低等级（从低到高依次为`debug`、`info`、`warning`、`prompt`、`error`、`fatal`）
  - **默认：** `warning`
  - **设置方法：** `conf logger.min_print_out_level = 'warning';`
- `min_log_file_level`：设置将日志输出到文件的最低等级
  - **默认：** `info`
  - **设置方法：** `conf logger.min_log_file_level = 'info';`
- `print_out_plain_text`：设置是否将日志元信息（如时间戳、发送方等）输出到屏幕
  - **默认：** 否
  - **设置方法：** `conf logger.print_out_plain_text = false;`
- `log_file`：设置日志输出文件，如果为空则不输出
  - **默认：** 是
  - **设置方法：** `conf logger.log_file = 'ligral.log';`

### 线性化工具

**定义：** 该项是一个设置集合，用来设置线性化工具的一些属性。参见[线性化工具使用说明](linearizer.md)

### 配平工具

**定义：** 该项是一个设置集合，用来设置配平工具的一些属性。参见[线性化工具使用说明](trimmer.md)

### 变步长求解器

**定义：** 该项是一个设置集合，用来设置变步长求解器的一些属性。参见[线性化工具使用说明](variable_step_solver.md)