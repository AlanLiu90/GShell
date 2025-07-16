# GShell

## 介绍
用于Unity的REPL工具：
1. 支持在编辑器内使用
2. 支持在IL2CPP打包版本中使用（需要集成HybridCLR）
3. 支持直接访问非公有的类、方法、属性、字段，不需要使用反射
4. 支持Unity 2019+

如果想了解实现细节，可以看这篇[博客](https://alanliu90.hatenablog.com/entry/2025/03/08/Unity%E4%B8%ADREPL%E5%8A%9F%E8%83%BD%E7%9A%84%E5%AE%9E%E7%8E%B0)

## 运行demo

### 在编辑器中运行
1. 执行src\GShell\publish_win64.bat
2. 执行demo\HttpServer\start.bat
3. 用Unity打开demo\Client工程
4. 打开场景：Scenes\main.unity
5. 进入Play Mode
6. 在Unity中打开Shell Launcher：MODX -> Shell Launcher，配置选择EditorShellSettings
7. 点击“启动”

### 在IL2CPP打包版本中运行
1. 执行src\GShell\publish_win64.bat
2. 执行demo\HttpServer\start.bat
3. 用Unity打开demo\Client工程
4. 安装HybridCLR：HybridCLR -> Installer
5. 构建Player：Build -> Win64
6. 运行Player：demo\Client\Release-Win64\HybridCLRTrial.exe
7. 在Unity中打开Shell Launcher：MODX -> Shell Launcher，配置选择PlayerShellSettings
8. 点击“启动”

### 功能示例
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
> var transform = GameObject.Find("LoadDll").transform;
> transform.localPosition = Vector3.zero; // 修改属性
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
1. 编译程序集的配置
   * 用于生成一份指定平台的dll，供GShell编译动态代码时引用
   * 在编辑器中使用时，需要将Build Target设置为`No Target`
2. 编译动态代码的配置
   * Search Paths：搜索引用的dll的路径列表，越前面的目录优先级越高
      * 在编辑器中使用时，需要添加Library\ScriptAssemblies
      * 工具会自动在最前面添加编译程序集的输出目录
      * 工具会自动在最后面添加UnityEngine.CoreModule.dll所在的目录
   * References：编译动态代码时引用的dll，下面是一些常用的dll (工具会自动引用mscorlib.dll、System.Core.dll等目标框架内置的dll，这里不需要添加)：
      * UnityEngine.CoreModule.dll
   * Usings：编译动态代码时自动导入的命名空间，下面是一些常用的命名空间：
      * System
      * System.Collections.Generic
      * System.Linq
      * UnityEngine
   * Script Class Name：编译动态代码时，自动创建的类型名，一般不需要修改
3. Runtime：编辑器中使用选择Mono，IL2CPP打包版本中使用选择IL2CPP
3. Tool Path：GShell的可执行文件的路径
4. Execute URL：GShell编译代码后，将发送给这个URL执行
5. Extra Datas：GShell发送给`Execute URL`的额外数据。比如可以添加玩家ID，让游戏服务器通过它将GShell请求转发给指定的客户端执行
6. 认证的配置
    * Type:
      * None: 不使用认证
      * Basic: 使用Basic认证方式，需要填写账号、密码
      * JWT: 使用JSON Web Token认证方式，需要填写令牌
    * 具体的认证逻辑和令牌生成逻辑，可参考demo\HttpServer
    * 使用认证时，建议使用HTTPS和服务器通信

## 集成
工具使用HTTP(S)协议和外部通信。项目可以在服务端接收GShell发送的数据，将其转发给指定的客户端执行。客户端执行之后，通过游戏服务器转发回GShell

demo工程中提供了集成的示例，代码在 demo\Client\Assets\HotUpdate\TestShell.cs 和 demo\HttpServer\HttpServer.cs

## 限制
GShell中的每个输入（比如一个表达式、一条语句或者一个函数定义）都会编译为一个单独的dll，而HybridCLR支持加载最多338个dll（[文档](https://hybridclr.doc.code-philosophy.com/docs/help/faq)）
