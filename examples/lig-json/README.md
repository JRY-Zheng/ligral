# Lig.Json 格式

Lig.Json 格式描述了一个 ligral 工程。

文件`test.lig.json`的内容相当于以下 ligral 代码：

~~~
# file group.lig
route TestGroup(;input;output)
    1-input -> output;
end

# file main.lig
import group;

4 -> TestGroup -> Print;
~~~