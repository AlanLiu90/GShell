# GShell

## 介绍
用于Unity的REPL工具：
1. 支持在编辑器内使用
2. 支持在IL2CPP打包版本中使用（需要集成HybridCLR）
3. 支持直接访问非公有的类、方法、属性、字段，不需要使用反射

## 运行

### 在编辑器中运行
1. 执行src\GShell\publish_win64.bat
2. 用Unity打开demo工程
3. 打开场景：Scenes\main.unity
4. 进入Play Mode
5. 在Unity中打开Shell Launcher：MODX -> Shell Launcher，配置选择EditorShellSettings
6. 点击“启动”

### 在IL2CPP打包版本中运行
1. 执行src\GShell\publish_win64.bat
2. 用Unity打开demo工程
3. 安装HybridCLR：HybridCLR -> Installer
4. 构建Player：Build -> Win64
5. 运行Player：demo\Release-Win64\HybridCLRTrial.exe
6. 在Unity中打开Shell Launcher：MODX -> Shell Launcher，配置选择PlayerShellSettings
7. 点击“启动”

### 示例
```
> 1+2                                     // 执行表达式，输出值
3
> var x = 2 + 3;                          // 定义变量
> x                                       // 输出之前定义的变量的值
5
> int Add(int a, int b)                   // 定义函数，支持多行输入
* {
*     return a + b;
* }
> Add(3, 1)                               // 调用前面定义的函数
4
> Entry.Run_AOTGeneric();                 // 调用HotUpdate.dll中的方法，但dll没有引用
(1,1): error CS0103: The name 'Entry' does not exist in the current context
> #r "HotUpdate.dll"                      // 引用HotUpdate.dll（推荐在配置中填上常用的dll，避免每次启动都需要手动引用）
> Entry.Run_AOTGeneric();                 // Run_AOTGeneric是私有方法，可以直接调用
> using UnityEngine.SceneManagement;      // 导入命名空间
> SceneManager.GetActiveScene().name      // 访问先前导入的命名空间中的类型
main
> var list = new List<int>() { 1, 2, 3 }; // 创建List
> var s = "";
> foreach (var item in list)              // 循环语句，支持多行输入
*     s += item + ",";
> s
1,2,3,
> #load "test.cs"                         // 加载当前目录（demo）下的test.cs文件，并执行其中的代码
> #reset                                  // 重置状态
> s                                       // 之前声明的变量不存在了
(1,1): error CS0103: The name 's' does not exist in the current context
```

## 配置
demo工程的配置在`Assets\Editor\*ShellSettings.asset`
1. 编译程序集的配置
   * 用于生成一份指定平台的dll，供GShell编译动态代码时引用
   * 在编辑器中使用时，需要将Build Target设置为`No Target`
2. 编译动态代码的配置
   * Search Paths：搜索引用的dll的路径列表，越前面的目录优先级越高
      * 在编辑器中使用时，需要添加Library\ScriptAssemblies
      * 工具会自动在最前面添加编译程序集的输出目录
      * 工具会自动在最后面添加mscorlib.dll、UnityEngine.CoreModule.dll所在的目录
   * References：编译动态代码时引用的dll，下面是一些常用的dll：
      * mscorlib.dll
      * System.Core.dll
      * UnityEngine.CoreModule.dll
   * Usings：编译动态代码时自动导入的命名空间，下面是一些常用的命名空间：
      * System
      * System.Collections.Generic
      * System.Linq
      * UnityEngine
   * Script Class Name：编译动态代码时，自动创建的类型名，一般不需要修改
3. Runtime：编辑器中使用选择Mono，IL2CPP打包版本中使用选择IL2CPP
3. Tool Path：GShell的可执行的文件的路径
4. Execute URL：GShell编译代码后，将发送给这个URL执行
5. Extra Datas：GShell发送给Execute URL的额外数据。比如可以添加玩家ID，让游戏服务器通过它将GShell请求转发给指定的客户端执行

## 集成
工具使用HTTP协议和外部通信。项目可以在服务端接收GShell发送的数据，将其转发给指定的客户端执行。客户端执行之后，通过游戏服务器转发回GShell

demo工程中提供了集成的示例，代码在 demo\Assets\HotUpdate\TestShell.cs

## 限制
GShell中的每个输入（比如一个表达式、一条语句或者一个函数定义）都会编译为一个单独的dll，而HybridCLR支持加载最多338个dll（[文档](https://hybridclr.doc.code-philosophy.com/docs/help/faq)）

## 常见问题
1. 使用经过裁剪的mscorlib.dll（比如在使用HybridCLR DHE的项目中，用快照dll的目录作为搜索路径）时，可能因为代码裁剪，导致动态代码无法编译：
```
> 1+2
(1,1): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1.AwaitOnCompleted'
```
这种情况需要在项目的Assets目录（或者其子目录）添加link.xml，重新打包，并将新生成的dll的所在目录设置为搜索路径：
```xml
<?xml version="1.0" encoding="utf-8"?>
<linker>
  <assembly fullname="mscorlib">
    <type fullname="System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1" preserve="all" />
  </assembly>
</linker>
```
2. 在Unity 2022中使用默认的mscorlib.dll（在Unity的安装目录内）时，有一些动态代码无法编译：
```
> using UnityEngine.SceneManagement;
> SceneManager.GetActiveScene().name
(1,14): error CS0012: The type 'Object' is defined in an assembly that is not referenced. You must add a reference to assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'.
(1,1): error CS0012: The type 'ValueType' is defined in an assembly that is not referenced. You must add a reference to assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'.
```
这种情况下需要添加引用netstandard.dll