# GShell

[![NuGet](https://img.shields.io/nuget/v/GShell.svg)](https://www.nuget.org/packages/GShell)
[![Releases](https://img.shields.io/github/release/AlanLiu90/GShell.svg)](https://github.com/AlanLiu90/GShell/releases)

[English](./README_EN.md) | 简体中文

用于Unity的REPL工具：
1. 支持mono，包括编辑器和打包版本
2. 支持IL2CPP（需要集成[HybridCLR](https://github.com/focus-creative-games/hybridclr)）
3. 支持直接访问非公有的类、方法、属性、字段等，不需要使用反射
4. 支持Unity 2019+

如果想了解实现细节，可以看这篇[博客](https://alanliu90.hatenablog.com/entry/2025/03/08/Unity%E4%B8%ADREPL%E5%8A%9F%E8%83%BD%E7%9A%84%E5%AE%9E%E7%8E%B0)

## 目录

- [功能示例](#功能示例)
- [如何运行demo](#如何运行demo)
    - [在编辑器中运行](#在编辑器中运行)
    - [在IL2CPP打包版本中运行](#在IL2CPP打包版本中运行)
    - [使用网页版GShell](#使用网页版GShell)
- [集成](#集成)
    - [使用GShell](#使用GShell)
    - [使用GShell.Core](#使用GShellCore)
- [配置说明](#配置说明)
- [限制](#限制)	

## 功能示例
```
> 1 + 2                                   // 执行表达式
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
> new Entry.MyVec3()                      // MyVec3是私有类，可以直接访问
[Entry+MyVec3] {
  x: 0,
  y: 0,
  z: 0
}
> using UnityEngine.SceneManagement;      // 导入命名空间
> SceneManager.GetActiveScene().name      // 访问先前导入的命名空间中的类型
"main"
> var transform = GameObject.Find("LoadDll").transform;
> transform.localPosition = Vector3.zero; // 修改属性
> var list = new List<int>() { 1, 2, 3 }; // 创建List
> list                                    // 输出变量的值
List<int>(3) {
  1,
  2,
  3
}
> var s = "";
> foreach (var item in list)              // 循环语句，支持多行输入
*     s += item + ",";
> s
"1,2,3,"
> #load "test.cs"                         // 加载当前目录（demo\Client）下的test.cs文件，并执行其中的代码
> #reset                                  // 重置状态
> s                                       // 之前声明的变量不存在了
(1,1): error CS0103: The name 's' does not exist in the current context
```

## 如何运行demo

### 在编辑器中运行
1. 用Unity打开demo\Client工程
2. 启动HTTP服务器：Demo -> Start HTTP Server
3. 打开场景：Scenes\main.unity
4. 进入Play Mode
5. 打开Shell Launcher：MODX -> Shell Launcher，配置选择EditorShellSettings
6. 点击“Launch”

### 在IL2CPP打包版本中运行
1. 用Unity打开demo\Client工程
2. 启动HTTP服务器：Demo -> Start HTTP Server
3. 安装HybridCLR：HybridCLR -> Installer
4. 构建Player：Build -> Win64
5. 运行Player：demo\Client\Release-Win64\HybridCLRTrial.exe
6. 打开Shell Launcher：MODX -> Shell Launcher，配置选择PlayerShellSettings
7. 点击“Compile Scripts”
8. 点击“Launch”

### 使用网页版GShell
> 以在编辑器中运行为例

1. 用Unity打开demo\Client工程
2. 启动HTTP服务器：Demo -> Start HTTP Server
3. 打开场景：Scenes\main.unity
4. 进入Play Mode
5. 修改GShell.Web的配置：demo\GShell.Web\shellsettings.json
    1. TargetFramework：根据使用的Unity版本，填写 netstandard2.0 或 netstandard2.1
    2. SearchPaths：根据本地的Unity的安装目录，修改路径
6. 运行GShell.Web（任选以下一种方式）
    * 在Visual Studio中打开 demo\GShell.Web\GShell.Web.sln，按F5运行（会自动打开浏览器）
    * 在 demo\GShell.Web 目录执行`dotnet run GShell.Web.csproj`，并在浏览器中打开 http://localhost:5052/
7. 在PlayerId的输入框中输入100（100为demo的客户端配置的默认值）
8. 点击“Start”

## 集成

### 使用GShell
GShell使用HTTP(S)协议和外部通信。项目可以在服务端接收GShell发送的数据，将其转发给指定的客户端执行。客户端执行之后，通过服务端将结果转发回GShell

步骤：
1. 引用包：com.modx.gshell，参考格式：https://github.com/AlanLiu90/GShell.git?path=/src/GShell.UnityClient/Packages/com.modx.gshell#v1.3.1
2. 安装GShell：
   ```
   dotnet tool install --global GShell
   ```
3. 支持与GShell通信：
   1. 服务端接收GShell发送的数据（GShell使用JSON格式将以下数据用POST方式发送给服务端，地址为配置中的`Execute URL`）
   ```c#
   class ShellPostData
   {
	   public string SessionId { get; set; }
	   public int SubmissionId { get; set; }
	   public string EncodedAssembly { get; set; }
	   public string ScriptClassName { get; set; }
	   public Dictionary<string, string> ExtraEncodedAssemblies { get; set; }
	   public Dictionary<string, string> ExtraData { get; set; }
   }
   ```
   2. 服务端使用`ExtraData`（配置中的`Extra Data Items`）中的数据，确定目标客户端，并将`ShellPostData`转发过去
   3. 客户端收到数据后执行代码，并将结果发送回服务端
   ``` C#
   public class ShellResponse
   {
	   public string Result { get; set; }
	   public bool Success { get; set; }
   }
   
   // 初始化
   ShellExecutor mExecutor = new ShellExecutor(maximumOutputLength: 2048);
   
   // 执行代码
   var shellData = JsonConvert.DeserializeObject<ShellPostData>(text);

   mExecutor.EnsureObjectFormatterCreated(shellData.ExtraEncodedAssemblies);

   var (result, success) = await mExecutor.Execute(shellData.SessionId, shellData.SubmissionId, shellData.EncodedAssembly, shellData.ScriptClassName);

   var shellResponse = new ShellResponse() { Result = (string)result, Success = success };
   
   // 将shellResponse发送回服务端
   ```
   4. 服务端将`shellResponse`回复给GShell
   5. 具体实现可参考demo工程，代码在 demo\Client\Assets\HotUpdate\TestShell.cs 和 demo\HttpServer\HttpServer.cs
4. 将 demo\Client\Assets\Editor 中的 EditorShellSettings.asset 和 PlayerShellSettings.asset 拷贝到项目工程内，并做修改：
   1. Command：改为`gshell`
   2. Execute URL：改为项目实际使用的地址
   3. Extra Data Items：根据项目的实际情况修改

<details>
<summary>使用本地工具方式安装GShell</summary>

使用本地工具方式安装，可以将工具的版本信息添加到版本控制中，便于管理

步骤（以Windows为例）：
1. 在Unity工程目录（Assets所在目录），执行：
```bat
dotnet new tool-manifest
dotnet tool install GShell
```
2. 在同目录内，创建文件start_gshell.bat，并写入以下内容：
```bat
dotnet tool restore
dotnet tool run gshell %*
```
3. 将 .config\dotnet-tools.json 添加到版本控制中
4. 将配置文件中的Command改为`start_gshell.bat`

</details>

### 使用GShell.Core
对于期望在自己的工具中使用GShell功能的项目，可以集成GShell.Core

可参考GShell.Web工程，它相当于是在浏览器中运行的GShell。主要代码：
   * demo\GShell.Web\Shell\Shell.cs：实现了GShell.Core中的`ShellBase`的子类
   * demo\GShell.Web\Components\Pages\Terminal.razor：实现了网页终端的输入、输出

## 配置说明
1. Assembly Compilation Settings
   * 用于生成一份指定平台的dll，供GShell编译动态代码时引用
   * 在编辑器中使用时，需要将Build Target设置为`No Target`
2. Dynamic Code Compilation Settings
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
   * Script Class Name：编译动态代码时，根据它自动创建类型名，一般不需要修改
3. Runtime
   * 编辑器中使用选择Mono
   * 打包版本中使用，根据脚本后端选择Mono或IL2CPP
3. Command：运行GShell的命令
4. Execute URL：GShell编译代码后，将发送给这个URL执行
5. Extra Assemblies: GShell发送的额外的Assembly
    * GShell.ObjectFormatter.dll: 支持使用roslyn的`CSharpObjectFormatter`格式化输出对象
6. Extra Datas：GShell发送给`Execute URL`的额外数据。比如可以添加玩家ID，让游戏服务器依据它将GShell的请求转发给相应的客户端执行
7. Authentication Settings
    * Type:
      * None: 不使用认证
      * Basic: 使用Basic认证方式，需要填写账号、密码
      * JWT: 使用JSON Web Token认证方式，需要填写令牌
    * 具体的认证逻辑和令牌生成逻辑，可参考demo\HttpServer
    * 使用认证时，建议使用HTTPS和服务器通信

## 限制
GShell中的每个输入（比如一个表达式、一条语句或者一个函数定义）都会编译为一个单独的dll，而HybridCLR支持加载最多338个dll（[文档](https://hybridclr.doc.code-philosophy.com/docs/help/faq)）。这条限制仅适用于IL2CPP
