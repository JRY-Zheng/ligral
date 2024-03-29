<!-- Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# 用户文档

“纸上得来终觉浅，绝知此事要躬行。”如果您刚刚开始了解这个项目，还没有看过[快速开始](../quick-start)的话，建议您先按照快速开始安装 ligral 并试着运行。Ligral 的语法和常见语言（C 和从 C 衍生出来语言，包括 Java、python 等等，函数式语言，SQL 语言，modelica 语言等等）的语法均不同，需要实操才能理解和熟悉。

正式开始熟悉 ligral 的用法之前，先了解 ligral 的目标似乎很有必要。您可能需要知道 ligral 是用来解决什么问题的，它具体做的是什么事。接着，如果您先前已经在使用另一款工具做着相同的事，您可能还感兴趣 ligral 提出了什么新思路，解决了什么痛点，具备哪些优势，而又可能带来哪些劣势。

### Ligral 的目标

Ligral 这个概念同时指代 ligral 语言及其解释器和编译器。

对于 ligral 语言，它的目标是简洁高效地描述仿真模型。如果您用 C 语言来描述模型，您可能关注的是算法；如果您用 modelica 语言来描述模型，您可能关注的是公式；而如果您用 ligral 语言来描述模型，您可能关注的是结构，正如您使用 Simulink 来描述仿真模型时一样。

我们不得不承认，在描述仿真模型时，对比 C 语言甚至 modelica 语言，Simulink 是更加高效的。一方面，Simulink 之间描述了模块之间的连接关系，而不是背后的逻辑关系。另一方面，图形化界面也让这种关系更加直观。而 ligral 语言描述的也正是模块之间的连接关系，同时，它也通过文本的形式尽可能地还原图形界面的直观。

所以 ligral 语言所做的事就是：

- 定义、声明、描述模块；
- 描述模块之间的连接关系；从而
- 描述整个仿真模型。

最终，ligral 语言所能提供的就是两个函数：

```math
\dot{x}=f(x,u,t)\\
y=g(x,u,t)
```

有了这两个函数，求解器自己维护一个状态列表，就可以根据输入计算输出，从而完成仿真。

而 ligral 解释器和编译器所实现的，除了解析 ligral 语言之外，更重要的就是提供了一系列的求解器，能求解 ligral 语言所描述的仿真问题。与此同时，ligral 还提供了一系列工具箱，用以处理输入输出的数据传输。

### Ligral 的优势

Ligral 区别于市面上常见的仿真工具，最大的不同就在于它本身是一个文本语言，但是又具备图形语言的直观性。表示模块之间的连接时，ligral 并不像 modelica 那样使用一个`connect`函数，而是用`->`符号模拟图形语言中的箭头，从而让用户很直观地看出模块之间的关系。

Ligral 在可读性和简洁性的设计上进行了一番革新。除了上面说的箭头符号之外，ligral 还引入了“声明 - 引用”系统，作为一个内建的功能，而非像 Simulink 中一样用模块来实现。这让模块的声明和引用变得十分简单高效。允许模块先声明后引用，也极大程度地保证了连接图部分的简洁与清晰。此外，Ligral 传参的语法参考了 JSON，这让传参变得简单。

文本语言经过长年的发展，语法高亮、智能语法提示（尚待开发）等技术以及大大降低文本语言的门槛，在编码速度和效率上以及能比肩甚至超越图形语言。没有 GUI 这个负担，ligral 语言甚至可以在直接下位机开发、编写，在本地编译、部署，免去了从上位机下载的麻烦。

Ligral 的另一项重大革新则是引入了面向对象的思想，包括类和对象、接口和继承等等。当您定义了一些模块及其相互连接关系，您实际上定义的是一个部件或者一个系统，这些部件或系统有可能在更大的工程里充当一个节点。这些功能可能是需要复用的，而且可能随着一些参数的改变而略有不同。因此，ligral 提供了“路由”这一概念，这对等于 C++ 等面向对象的语言中的类。类似地，您可以从这些“类”（也就是路由）声明他们的实例，并传入参数。Simulink 中虽然也有 Reference Model 等可以复用逻辑的功能，但是最常用的 Subsystem 并没有类的概念，您改动一个（复制粘贴得到的）副本之后其他副本并不会改动。

而接口的思想更是 ligral 所独有的。在仿真中，您可能发现有一些部件经常需要更换，比如您设计了一系列控制器，想要应用到同样的模型上，或者有很多被控对象，需要检验控制器的鲁棒性，被控对象和控制器之间的接口可能是固定的。如对控制器而言，系统状态作为输入，控制信号作为输出，被控对象则相反。那么，您可以针对这些输入输出设计接口，让控制器和被控对象分别继承各自的接口，这样它们之间的相互替换就是安全的。Ligral 提供了“签名”这一概念，这对等于 C#、Java 等面向对象的语言中的接口，也和函数的签名有共通之处。之所以区别于“接口”，是因为在 C#、Java 中，一个类能继承多个接口;而在 ligral 中，一个路由只能有一个签名，签名声明了该路由的所有接口。

在设计更大的仿真对象时，接口的概念尤为重要。比如您在设计汽车整车模型时，您可能会为发动机、悬架、离合器等等大的部件设计接口，通过这些接口把整车级和系统级的逻辑分离，从而降低代码的耦合性，让仿真模型变得清晰明了，避免了混乱和隐蔽的错误。后期进行技术升级时，就可以通过接口用新的部件替换旧的部件。

综上所述，ligral 作为仿真语言中的新生力量，具备了许多传统语言不可比拟的优势。当然，ligral 目前肯定还存在不少劣势，比如开发不完全，生态不足等等。但我们坚信 ligral 会有一个光明的未来。为何不从现在开始了解它呢？