<!-- Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

     Distributed under MIT license.
     See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
-->

# 引用依赖

一个结构清晰的中大型仿真工程离不开引用依赖，不论这些依赖来自于内部或是引用自外部。在 ligral 中，您可以很方便地处理这些依赖关系并且引用到仿真工程中。本文涉及的所有例子均可在`/examples/import-logic/`下查看。

### 绝对路径引用

最简单的情况莫过于所有 lig 文件都在同一个文件夹，ligral 提供了两个子句以供使用。例如下面的文件结构：

    ┬ 根目录
    ├──── main.lig
    └──── library1.lig:
         ┌─────────────────────────────┐
         │let hello = 1;               │
         └─────────────────────────────┘

在`main.lig`中调用`hello`有两种方法：

    # 方法 1
    import library1;
    hello -> Print;

    # 方法 2
    using library1;
    library1.hello -> Print;

顾名思义，`import`代表导入，会将文件`library1.lig`中的所有成员（包括常量、节点和连接关系等等）量导入到当前的命名空间。`using`代表使用（或引用），即使用`library`这一命名空间来访问文件`library1.lig`中的成员。

注意，重复导入或者引用同一个 lig 文件，该文件只会被加载一次。

这个例子中的`main.lig`被称为入口文件，入口文件所在的目录是工程根目录，是绝对引用的参考目录。也就是说，在此目录下如果还有子文件夹，导入或引用子文件夹中的 lig 文件必须给出相对于工程根目录的路径。例如下面的文件结构：

    ┬ 根目录
    ├──── main.lig
    └───┬ subfolder:
        └──── sublib1.lig:
             ┌─────────────────────────────┐
             │let world = 1;               │
             └─────────────────────────────┘

同样，根据需要可以使用两种语法来导入或引用该文件，且通过`using`子句引用的变量需要完整路径引用：

    # 方法 1
    import subfolder.sublib1;
    world -> Print;

    # 方法 2
    using subfolder.sublib1;
    subfolder.sublib1.world -> Print;

### 相对路径引用

相对路径引用是针对不在工程根目录的文件而言的，因为此时文件所在目录和工程根目录不一致。请看以下文件结构：

    ┬ 根目录
    ├──── main.lig
    └───┬ subfolder:
        ├──── sublib1.lig
        └──── sublib2.lig:
             ┌─────────────────────────────┐
             │let world = 2;               │
             └─────────────────────────────┘

如果要在`sublib1.lig`中使用`world`，以`using`为例，有绝对引用和相对引用两种方法，绝对引用如下：

    using subfolder.sublib2;
    subfolder.sublib2.world -> Print;

这么写的前提是入口文件是`main.lig`。如果入口文件发生变动，这一语句就不能正常执行，因此 ligral 提供了相对应用的方式：

    using .sublib2;
    sublib2.world -> Print;

`sublib2`前的`.`号表明在文件`sublib1.lig`所在目录搜索`sublib2.lig`。如果这个目录下仍有文件夹`subsubfolder`，自然就可以写成`.subsubfolder.file`。

如果要访问上一级目录，就在`.`号之前继续加`.`，也就是如果有两个`.`，表示在上一级目录搜索，如果有三个`.`，则表示上两级目录。但是一般建议不要超过一级目录进行跨级访问。例如下面的文件结构：

    ┬ 根目录
    ├──── main.lig
    ├───┬ subfolder:
    │   └──── sublib1.lig
    └───┬ package:
        └──── module1.lig:
             ┌─────────────────────────────┐
             │let ligral = 1;              │
             └─────────────────────────────┘

如果`sublib1.lig`文件需要引用`world`，一种方法是通过绝对路径访问，这要求`main.lig`是入口文件：

    using package.module1;
    package.module1.ligral -> Print;

如果用相对引用的方式，则可以写：

    using ..package.module1;
    package.module1.ligral -> Print;

可以看到，标识符前面的两个点是被省略了的，不需要写出上一级文件夹的名字。如果使用`import`子句，就不会存在这个问题。

### 包引用

之前的情况都属于文件引用，如果引用了一个文件夹，该文件夹必须是一个包。成为包的充分必要条件就是该文件夹下存在一个`index.lig`文件，引用该文件夹等效于引用`index.lig`文件。例如下列文件结构：

    ┬ 根目录
    ├──── main.lig
    └───┬ package:
        ├──── index.lig:
        │    ┌─────────────────────────────┐
        │    │using .module1;              │
        │    └─────────────────────────────┘
        └──── module1.lig:
             ┌─────────────────────────────┐
             │let ligral = 1;              │
             └─────────────────────────────┘

如果您在`main.lig`中导入`package`这个包：

    import package;

您将会在当前命名空间看到`index.lig`中的所有内容。而`index.lig`中包含`module1`这一标识符，因此如果您需要使用`ligral`，应该写：

    module1.ligral -> Print;

需要说明的是，在`index.lig`文件中，您可以选择使用`using`，也可以使用`import`，根据实际需要。如果在上面的例子中，`index.lig`文件里是`import .module1;`，那么在`main.lig`中就可以直接调用`ligral`了。

### 选择性导入

有的时候一些包或者 lig 文件可能包含很多成员，但实际使用中可能只需要引用其中的部分成员，这时使用`using`子句可以避免命名污染。但是如果不想通过命名空间的方式来调用对象，可以使用`import`子句的选择性导入语法。

    ┬ 根目录
    ├──── main.lig
    └───┬ subfolder:
        └──── sublib4.lig:
             ┌─────────────────────────────┐
             │let world = 4;               │
             │let unused = 0;              │
             └─────────────────────────────┘

如果您只想使用`world`成员，您可以这么写：

    import subfolder.sublib4 : world;
    world -> Print;

此时仅有`world`被导入到当前命名空间，而`unused`没有，因此不能被访问。如果您需要导入多个成员，可以用逗号分隔。

### 别名引用

有的时候由于文件层级较多，使用`using`子句导入往往导致命名空间太长，此时可以使用`using`子句的别名语法。在上面的例子中，如果使用普通的`using`子句，需要这么写：

    using subfolder.sublib4;
    subfolder.sublib4.world -> Print;

如果采用别名语法，把命名空间起名为`lib`，则可以写：

    using subfolder.sublib4 : lib;
    lib.world -> Print;

这样可以避免冗长的代码，但是需要注意别名不能和现有的成员冲突。

**需要注意的是**，`import`子句和`using`子句虽然都使用了`:`来扩展语法，但是`:`的含义是不同的。`import`子句用`:`来引出需要导入的成员，`using`子句用`:`引出命名空间的别名。不过有一点是相同的：出现`:`的语句中，`:`之后的符号会被加载到当前命名空间中，之前的不会。

### 循环依赖

循环依赖指的是文件 A 依赖于文件 B，同时文件 B 也依赖于文件 A 的情况，这在一般情况下是需要避免的，不是一种良好的编程风格。但这并不意味着循环依赖一定会报错，某些场合很谨慎地使用也是可以解释通过的，通过这种方式您能更深入地了解 ligral 如何处理依赖关系。以下属于进阶内容，感兴趣的话可以深入研究。

    ┬ 根目录
    ├──── node1.lig:
    │    ┌─────────────────────────────┐
    │    │let b = 2;                   │
    │    │import .node2;               │
    │    │a -> Print;                  │
    │    └─────────────────────────────┘
    └──── node2.lig:
         ┌─────────────────────────────┐
         │import .node1;               │
         │b -> Print;                  │
         │let a = 1;                   │
         └─────────────────────────────┘

在上面这个例子中，如果程序从`node1.lig`启动，执行代码的顺序及当前的命名空间如下表所示：

| 代码 | node1 命名空间 | node2 命名空间 | 备注 |
|--|--|--|--|
|`let b = 2;`|`[b]`*|-||
|`import .node2`|`[b]`|`[]`*||
|`import .node1`|`[b]`|`[b]`*|不会重复加载 node1|
|`b -> Print;`|`[b]`|`[b]`*|成功访问到`b`|
|`let a = 1;`|`[b]`|`[b, a]`*||
|`a -> Print;`|`[b, a]`*|`[b, a]`|成功访问到`a`|

其中 * 号表示当前的命名空间。