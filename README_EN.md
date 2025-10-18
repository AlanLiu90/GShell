# GShell

[![NuGet](https://img.shields.io/nuget/v/GShell.svg)](https://www.nuget.org/packages/GShell)
[![Releases](https://img.shields.io/github/release/AlanLiu90/GShell.svg)](https://github.com/AlanLiu90/GShell/releases)

English | [简体中文](./README.md)

A REPL tool for Unity:

1. Supports Mono (both in the Editor and in the Player)  
2. Supports IL2CPP (requires integration with [HybridCLR](https://github.com/focus-creative-games/hybridclr))  
3. Supports direct access to non-public classes, methods, properties, fields, etc., without using reflection  
4. Supports Unity 2019 and later

For implementation details, see this [blog post](https://alanliu90.hatenablog.com/entry/2025/03/08/Unity%E4%B8%ADREPL%E5%8A%9F%E8%83%BD%E7%9A%84%E5%AE%9E%E7%8E%B0) (In Chinese)

## Table of Contents

- [Feature Examples](#feature-examples)  
- [How to Run the Demo](#how-to-run-the-demo)  
  - [Run in the Editor](#run-in-the-editor)  
  - [Run in an IL2CPP Build](#run-in-an-il2cpp-build)  
  - [Use the Web Version of GShell](#use-the-web-version-of-gshell)  
- [Integration](#integration)  
- [Configuration](#configuration)  
- [Limitations](#limitations)

### Feature Examples

```
> 1 + 2                                   // Evaluate expression
3
> var x = 2 + 3;                          // Define a variable
> x                                       // Output previously defined variable
5
> int Add(int a, int b)                   // Define a function, supports multi-line input
* {
*     return a + b;
* }
> Add(3, 1)                               // Call the previously defined function
4
> Entry.Run_AOTGeneric();                 // Call a method in HotUpdate.dll, but the dll isn’t referenced yet
(1,1): error CS0103: The name 'Entry' does not exist in the current context
> #r "HotUpdate.dll"                      // Reference HotUpdate.dll (you can pre-configure common dlls to avoid doing this every time)
> Entry.Run_AOTGeneric();                 // Run_AOTGeneric is a private method and can be called directly
> new Entry.MyVec3()                      // MyVec3 is a private class and can be accessed directly
[Entry+MyVec3] {
  x: 0,
  y: 0,
  z: 0
}
> using UnityEngine.SceneManagement;      // Import a namespace
> SceneManager.GetActiveScene().name      // Access a type from the imported namespace
"main"
> var transform = GameObject.Find("LoadDll").transform;
> transform.localPosition = Vector3.zero; // Modify a property
> var list = new List<int>() { 1, 2, 3 }; // Create a List
> list                                    // Output variable value
List<int>(3) {
  1,
  2,
  3
}
> var s = "";
> foreach (var item in list)              // Loop, supports multi-line input
*     s += item + ",";
> s
"1,2,3,"
> #load "test.cs"                         // Load and execute the test.cs file in the current directory (demo\Client)
> #reset                                  // Reset the state
> s                                       // Previously declared variables no longer exist
(1,1): error CS0103: The name 's' does not exist in the current context
```

## How to Run the Demo

### Run in the Editor

1. Open the `demo\Client` project with Unity.  
2. Start the HTTP Server: `Demo -> Start HTTP Server`.  
3. Open the scene: `Scenes\main.unity`.  
4. Enter Play Mode.  
5. Open **Shell Launcher**: `MODX -> Shell Launcher` and select `EditorShellSettings`.  
6. Click **Launch**.

### Run in an IL2CPP Build

1. Open the `demo\Client` project with Unity.  
2. Start the HTTP Server: `Demo -> Start HTTP Server`.  
3. Install HybridCLR: `HybridCLR -> Installer`.  
4. Build the Player: `Build -> Win64`.  
5. Run the Player: `demo\Client\Release-Win64\HybridCLRTrial.exe`.  
6. Open **Shell Launcher**: `MODX -> Shell Launcher` and select `PlayerShellSettings`.  
7. Click **Compile Scripts**.  
8. Click **Launch**.

### Use the Web Version of GShell

> Using the Editor as an example

1. Open the `demo\Client` project with Unity.  
2. Start the HTTP Server: `Demo -> Start HTTP Server`.  
3. Open the scene: `Scenes\main.unity`.  
4. Enter Play Mode.  
5. Modify the config file `demo\GShell.Web\shellsettings.json`:  
   - **TargetFramework**: Set to `netstandard2.0` or `netstandard2.1` depending on your Unity version.  
   - **SearchPaths**: Adjust the path according to your local Unity installation.  
6. Run GShell.Web (choose one of the following):  
   - Open `demo\GShell.Web\GShell.Web.sln` in Visual Studio and press F5 (browser opens automatically).  
   - Run `dotnet run GShell.Web.csproj` in the `demo\GShell.Web` directory, then open [http://localhost:5052/](http://localhost:5052/) in your browser.  
7. Enter `100` in the **PlayerId** field (100 is the default for the demo client).  
8. Click **Start**.

## Integration

The tool communicates externally via HTTP(S). Your server can receive data from GShell, forward it to the target client for execution, and then send the result back to GShell.

Steps:

1. Add the package: com.modx.shell, e.g., https://github.com/AlanLiu90/GShell.git?path=/src/GShell.UnityClient/Packages/com.modx.shell#v1.3.0

2. Install the tool:  
   ```
   dotnet tool install --global GShell
   ```

3. Support communication with GShell:  
   1. The server receives data from GShell (GShell sends JSON via POST to the configured `Execute URL`):  
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
   2. The server uses data in `ExtraData` (from `Extra Data Items` in the configuration) to determine the target client and forwards the `ShellPostData`.  
   3. The client executes the code and sends the result back:  
   ```c#
   public class ShellResponse
   {
       public string Result { get; set; }
       public bool Success { get; set; }
   }

   // Initialization
   ShellExecutor mExecutor = new ShellExecutor(maximumOutputLength: 2048);

   // Execute code
   var shellData = JsonConvert.DeserializeObject<ShellPostData>(text);

   mExecutor.EnsureObjectFormatterCreated(shellData.ExtraEncodedAssemblies);

   var (result, success) = await mExecutor.Execute(shellData.SessionId, shellData.SubmissionId, shellData.EncodedAssembly, shellData.ScriptClassName);

   var shellResponse = new ShellResponse() { Result = (string)result, Success = success };

   // Send shellResponse back to the server
   ```
   4. The server replies to GShell with `shellResponse`.  
   5. See the demo implementation in `demo\Client\Assets\HotUpdate\TestShell.cs` and `demo\HttpServer\HttpServer.cs`.

4. Copy `EditorShellSettings.asset` and `PlayerShellSettings.asset` from `demo\Client\Assets\Editor` into your project and modify:  
   - **Command** → `gshell`  
   - **Execute URL** → your actual endpoint  
   - **Extra Data Items** → adjust according to your project

## Configuration

1. **Assembly Compilation Settings**  
   - Used to generate a platform-specific DLL for GShell to reference during compilation.  
   - When used in the Editor, set the Build Target to **No Target**.

2. **Dynamic Code Compilation Settings**  
   - **Search Paths**: List of directories to search for referenced DLLs. Earlier entries have higher priority.  
     - When used in the Editor, add `Library\ScriptAssemblies`.  
     - The tool automatically prepends the output directory for compiled assemblies.  
     - It automatically appends the directory containing `UnityEngine.CoreModule.dll`.  
   - **References**: DLLs to reference during compilation. Common DLLs:  
     - `UnityEngine.CoreModule.dll`  
     *(mscorlib.dll, System.Core.dll, and other framework DLLs are included automatically)*  
   - **Usings**: Namespaces automatically imported during compilation. Common namespaces:  
     - `System`  
     - `System.Collections.Generic`  
     - `System.Linq`  
     - `UnityEngine`  
   - **Script Class Name**: When compiling dynamic code, the type name is generated automatically based on it and usually doesn’t need to be modified.

3. **Runtime**
   - Use **Mono** in the Editor
   - Use **Mono** or **IL2CPP** in the Player depending on the scripting backend.
4. **Command**: The command to run GShell.  
5. **Execute URL**: The URL to send compiled code to for execution.  
6. **Extra Assemblies**: Additional assemblies sent by GShell.  
   - `GShell.ObjectFormatter.dll`: Enables using Roslyn’s `CSharpObjectFormatter` for object formatting.  
7. **Extra Datas**: Additional data sent to the `Execute URL`, e.g., player ID for routing requests.  
8. **Authentication Settings**
   - **Type**:
     - `None`: No authentication  
     - `Basic`: Basic auth (requires username/password)  
     - `JWT`: JSON Web Token (requires token)  
   - See `demo\HttpServer` for authentication and token generation examples.  
   - When using authentication, HTTPS is recommended.

## Limitations

Each input in GShell (e.g., an expression, statement, or function definition) is compiled into a separate DLL. HybridCLR supports loading a maximum of 338 DLLs ([documentation](https://hybridclr.doc.code-philosophy.com/docs/help/faq)). This limitation applies only to IL2CPP.
