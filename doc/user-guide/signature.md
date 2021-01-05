<!-- Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# 接口签名

在一个规模宏大的仿真工程中，实现各个子系统之间的解耦的一种重要手段就是接口签名。接口签名实现了一种先定义后实现、定义与实现分离的建模方式。签名定义了一个节点在与其他节点交互时的全部接口信息，隐藏了内部实现逻辑。

### 语法

定义一个接口签名的语法如下所示：

    signature SignatureName(inputs; outputs);

与路由的属性语法十分类似，所不同之处有三点：

- 关键字为`signature`而非`route`
- 不可以声明签名的签名
- 不需要写明参数列表

此外由于没有主体部分，接口签名声明语句直接以`;`结尾。之所以不可以声明签名的签名，是因为签名已经完全规定了一个结点的所有接口，声明了该签名的节点没有变化的自由，如果一个签名声明了另一个签名，那么它们就是同一个签名，重复定义没有意义。签名并没有规定节点应该采用什么样的参数列表，因为参数不属于接口的范畴。因此，实例化必须在路由层面进行，不能实例化一个接口签名。

### 用法

在[上一节](route)中，签名的用法已经介绍了，这里再总结一下。签名除了再定义语句外还能出现在两个地方，一处是路由的定义语句：

    route Route:NodeSignature (...) ...

这表示路由`Route`声明了一个签名`NodeSignature`。另一处是路由的参数列表：

    route Route2 (param:NodeSignature, ...; ...) ...

这表示路由`Route2`中的参数`param`是一个节点，且该节点声明了`NodeSignature`签名。因此，在实例化`Route2`的时候，您就可以写：

    Route2{param:Route, ...};

请注意，此处的`:`并不是表明`param`声明了`Route`，首先，`Route`是一个节点实例而非签名，不能被声明；其次，此处的`:`作用和 JSON 中的冒号类似，或者您也可以参考 C# 中可省参数的用法，代表将`Route`赋值给标识符`param`。

### 示例

在[早先的文档](route)中给出了一个控制器的例子`PIDController`，在[快速开始](../quick-start)中有一个弹簧阻尼质量块系统`MassSpringDamper`。假设需要构建一个闭环模型，只需要把这两个模块连接起来即可。

    MassSpringDamper[sys]{m:0.1, k:10, d:0.3, x0:1, v0: 0};
    PIDController[controller]{Kp:1, Ki:0.1, Kd:4);
    sys:x -> controller -> sys;

这样一个闭环的弹簧阻尼质量块模型，很可能是需要复用的，因此将其定义成一个路由会是很好的选择。

    route ClosedLoopMSD(m, k, d, x0, v0, Kp, Ki=0, Kd=0, tau=0.01; F; x, v)
        MassSpringDamper[sys]{m:m, k:k, d:d, x0:x0, v0:v0};
        PIDController[controller]{Kp:Kp, Ki:Ki, Kd:Kd, tau:tau);
        (sys:x -> controller) + F -> sys -> (x, v);
    end

但是这么写弊端有很多。首先，所有节点的参数如果支持修改的话，都要定义在路由的参数列表里，并且所有缺省参数都必须要列明，并且显示赋值，这样极大地降低了语法的简洁性，并且可能造成参数标识符冲突等问题。其次，子节点的逻辑是固定的，如果您想把弹簧阻尼质量块替换成一个非线性版本，或者把 PID 控制器替换成滑模控制器，就必须要重新编写一个路由。

利用 ligral 的签名语法，可以消除以上问题，保持简洁的语法。定义两个签名：

    signature MSDSignature(F; x, v);
    signature CTRSignature(x; u);

分别规定了弹簧阻尼质量块的接口和控制器的接口。然后在闭环模型的定义里：

    route ClosedLoopMSD(sys:MSDSignature, controller:CTRSignature; F; x, v)
        (sys:x -> controller) + F -> sys -> (x, v);
    end

最后在实例化`ClosedLoopMSD`之前定义好`sys`和`controller`实例，并传入：

    MassSpringDamper[sys]{m:0.1, k:10, d:0.3, x0:1, v0: 0};
    PIDController[controller]{Kp:1, Ki:0.1, Kd:4);
    ClosedLoopMSD{sys:sys, controller:controller};
    F -> ClosedLoopMSD:x -> Scope;

如果您设计好另一个控制器（比如滑模控制器）：

    route SMC:CTRSignature(...; x; u)
        ...
    end

您可以很轻松地替换：

    SMC[smc]{...};
    ClosedLoopMSD{sys:sys, controller:smc};