# 解释器

Ligral 是一种仿真语言，也是该仿真语言的解释器。本篇文章主要介绍了解释器的实现，内容是基于读者没有编写解释器的经验和理论知识这一假设，因此无需编译原理基础也可以看得懂。

Ligral 解释器是一种典型的两趟式解释器，其中词法分析和语法解析在一趟内完成，而解释在独立的一趟完成。所谓的“一趟”指的是把源文件所包含的信息从头到尾处理一遍，这里的信息可能是字符流、token 流、语法树等。

### 词法分析

词法分析简单来说就是将源文件进行分词，并判断是否合法。如果合法，分词的结果称之为 token，进而判断 token 所属类别。例如语句 `let a=1;`，就会被词法分析分解成`let`、`a`、`=`、`1`、`;`五个 token，分别属于关键字、标识符、等号、数字、分号五个类型，这些 token 都是合法的。

*注意：词法分析的合法并不意味着语法合法，也不意味着语义合法。`let a = ;`从词法上来说也是合法的，但是语法不合法；`let a = b;`词法、语法都合法，但是如果`b`是没有定义的，或者不是数字或矩阵，语义就不合法。*

如果出现了不合法的词，如 Ligral 目前不支持`$`符号，如果源文件中出现这个符号，将会被判定为不合法，则程序退出。当然，如果这些“不合法”的词出现在注释中或者字符串中，就不会被判定为不合法。

词法分析有手工编写和使用状态机自动解析两种实现方式，Ligral 采用手工编写词法分析器。Token 的类型和定义写在`/src/Syntax/Token.cs`里，词法分析器写在`/src/Syntax/Lexer.cs`中。词法分析器的输入是字符流，维护了一个当前字符（尚未被消费但即将被消费的字符）以及一些必要的前瞻字符用以分析。前瞻的作用在于将诸如`->`的符号解析成一个连接符号而不是一个负号加一个大于号。

*一个负号加一个大于号始终是无意义的，因此`->`是一个“好的”符号。反之`<-`不是一个“好的”符号，因为小于号加负号可能是有意义的，例如`a<-1`可能是`a`小于`-1`的含义。因此，我们在 v0.2.0起取消了`<-`这个符号。*

词法分析器为每一个 token 维护了其所在的文件、行号和列号，以便于在报错时能够准确定位错误。为了避免文件路径无意义的复制，采用了文件号来标记文件路径，否则每个 token 储存一份文件路径的副本也是比较大的开销。

另外，由于语法解析器设计上的失误，导致部分语法可能需要回溯，在词法分析器中也设计了备份和恢复的函数，用来在语法分析出错时回溯到备份点。如何避免回溯将在语法分析一节提到。