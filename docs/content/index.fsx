(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"
#I "../../bin/Delegate.Sandbox"

(**
Delegate.Sandbox
================

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Delegate.Sandbox library can be
      <a href="https://nuget.org/packages/Delegate.Sandbox">installed from NuGet</a>:
      <pre>PM> Install-Package Delegate.Sandbox</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

What is it?
-----------

Delegate.Sandbox is library that provides a Computation Expression named 
`SandboxBuilder`, `sandbox{ return 42 }`, which ensures that values returned 
from the computation are I/O side-effects safe (`IOSafe`) and if not, they are 
marked as unsafe (`Unsafe`) and returning an exception.

The library allows to bind `>>=` several sandbox computations together in order 
to create side-effect free code and based on the final result, then proceed to 
perform the desired side-effects.

Examples
--------

The following example shows that even though there is a call to `printfn`, the 
output is not passed to the console and hereby, no side-effect is generated:

*)

#r "Delegate.Sandbox.dll"

open System
open System.IO
open Delegate.Sandbox

let inline (>>=) m f = IOEffect.bind f m

let addition x y = sandbox{ return x + y }
let power2 x = sandbox{ printfn "Injected side-effect"; return x * x }
let result = addition 21 21 >>= power2

printfn "Sum of x and y, then power2: %A" result

(**
Evaluates to the following output:
<pre>
Sum of x and y and then power2: IOSafe 1764
</pre>

> **Remark**: No output is written to the console *)

(**
In the next example, we add `System.Console.ReadLine()` in order to block the
function until somebody presses enter. Additionally, if side-effects were allowed,
the entered input would affect return value of the function:
*)

let fooBar = sandbox{ return Console.ReadLine() + "FooBar" }

printfn "Prints only 'IOSafe FooBar': %A" fooBar

(**
Evaluates to the following output:
<pre>
Prints only 'IOSafe FooBar': IOSafe FooBar
</pre>

> **Remark**: No blocking readline or input from console.*)

(**
The next example show how we try to get access to the current file directory,
count the amount of files and add it to the final result. This action will try 
to perform an `File IO` which is not allowed in the `sandbox`. Due to this, the
whole function is evaluated to an `Unsafe` value, whichs contain the `Exception` 
throwed at runtime: *)

let addition' x y = sandbox{ 
  return (Directory.EnumerateFiles(".") |> Seq.length) + x + y }

printfn "Sum of x and y, then power2 (with error msg): %A" (addition' 21 21 >>= power2)

(**
Evaluates to the following output:
<pre>
Sum of x and y (with error msg): Unsafe System.Security.SecurityException:
Request for the permission of type 'System.Security.Permissions.FileIOPermission,
mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' failed.
   at System.Security.CodeAccessSecurityEngine.Check(Object demand, StackCrawlMark& 
   stackMark, Boolean isPermSet)
   at System.Security.CodeAccessPermission.Demand()
   at System.IO.FileSystemEnumerableIterator`1..ctor(String path, String originalUserPath, 
   String searchPattern, SearchOption searchOption, SearchResultHandler`1 resultHandler, 
   Boolean checkHost)
   at System.IO.Directory.EnumerateFiles(String path)
   at Program.addition'@12-1.Invoke(Unit unitVar)
   at Delegate.Sandbox.GlobalValues.SandboxBuilder.Delay[a](FSharpFunc`2 f)
The action that failed was:
Demand
The type of the first permission that failed was:
System.Security.Permissions.FileIOPermission
The first permission that failed was:
``
<IPermission 
  class="System.Security.Permissions.FileIOPermission, mscorlib, Version=4.0.0.0, 
    Culture=neutral, PublicKeyToken=b77a5c561934e089"
  version="1"
  PathDiscovery="D:\...\."/>
``
The demand was for:
``
<IPermission 
  class="System.Security.Permissions.FileIOPermission, mscorlib, Version=4.0.0.0, 
    Culture=neutral, PublicKeyToken=b77a5c561934e089"
  version="1"
  PathDiscovery="D:\...\."/>
``
The granted set of the failing assembly was:
``
<PermissionSet 
  class="System.Security.PermissionSet"
  version="1">
<IPermission 
  class="System.Security.Permissions.SecurityPermission, mscorlib, Version=4.0.0.0, 
    Culture=neutral, PublicKeyToken=b77a5c561934e089"
  version="1"
  Flags="UnmanagedCode, Execution"/>
</PermissionSet>
``
The assembly or AppDomain that failed was:
Sandbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
The method that caused the failure was:
IOEffect`1 Invoke(Microsoft.FSharp.Core.Unit)
The Zone of the assembly that failed was:
MyComputer
The Url of the assembly that failed was:
file:///D:/.../bin/Release/Sandbox.EXE
</pre>

> **Remark**: The computation and bindings works like the `Either` Monad where 
> you either have a value of the type `IOSafe` or you have an `Exception` of the
> type `Unsafe`. The main point here is that the I/O side-effect **is NOT performed**
> and the computation catches the attempt by tainting the whole expression and 
> providing the thrown `Exception` which can be re-thrown or logged in order to 
> revise and fix the code. *)

(**
How it works and limitations
----------------------------

 * A few words on the `SandboxBuilder` works:
    - The library is build on top of the [AppDomain Class][appdomain] which 
      allows to [Run Partially Trusted Code in a Sandbox][sandbox]. The 
      `SandboxBuilder` is only allowed to execute code
      `(SecurityPermissionFlag.Execution)`, which is the minimum permission that
      can be granted.
    - `sandbox` is implemented as a computation expression that only implements  
      **return** (`Return : v:'b -> 'b IOEffect`), which ensures that values 
      returning from the computation are of the desired value type, and **delay** 
      (`Delay : f:(unit -> 'a IOEffect) -> 'a IOEffect`), which tries to evaluate
      the function at the newly created domain (`AppDomain`) with the minimum
      granted permision instead of the executing `AppDomain.CurrentDomain`.
      If the function evaluation is succesful then an `IOSafe 'a` value is returned, 
      otherwise an `Unsafe` `Exception` is returned.
    - In order to ensure that `IOEffect` types are only instantiated from inside
      the computation expression, a few examples: `IOSafe "42"` or 
      `IOSafe (fun _ -> Directory.EnumerateFiles(".") |> Seq.length)`, we use type
      encapsulation and we afterwards expose them with the help of **active patterns**.
      For more info on this matter, please see this Gist from [Scott Wlaschin][wlaschin].
    - To remove `System.Console` I/O side-effects, we need to execute some 
      `SecurityPermissionFlag.UnmanagedCode` before we instantiate the `SandboxBuilder`.
      This is handled by `RemoveConsoleInOutEffects`. When the type is instantiated, 
      the `System.Console.SetIn`, `System.Console.SetOut` and `System.Console.SetError`
      are set to `Stream.Null`. Once this task is performed, the 
      `SecurityPermissionFlag.UnmanagedCode` flag is removed in order for the 
      new `AppDomain` runs with the minimal permision possible.
    - For more information, please look into the code (about +80 lines) at [GitHub][gh]

 * We describe a few **limitations** we found while we were making the library:
    - **No code optmization**: When a project that refers to the library is built in
      `Release` mode, default is set to `Optimize code`, then it will not work as
      some of the code is transformed to use `Reflection` which is not supported
      in the `AppDomain`.
    - **Unit tests**: As stated before, `Reflection` is not supported and because NUnit 
      uses this approach to execute the test, then it will not work either. This makes
      it really difficult to test code, specially because `Unsafe` types are 
      runtime and not compile time.
    - **F# Interactive (fsiAnyCpu.exe)**: As the computation expression is built on 
      top of the `AppDomain`, it will not be possible to use this library in
      interactive mode (scripts, ...).

*)
 
(**
Contributing and copyleft
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests.

The library is available under an Open Source MIT license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [appdomain]: https://msdn.microsoft.com/en-us/library/system.appdomain(v=vs.110).aspx
  [sandbox]: https://msdn.microsoft.com/en-us/library/bb763046(v=vs.110).aspx
  [wlaschin]: https://gist.github.com/swlaschin/54cfff886669ccab895a
  [gh]: https://github.com/Delegate.Sandbox
  [issues]: https://github.com/Delegate.Sandbox/issues
  [license]: https://github.com/Delegate.Sandbox/blob/master/LICENSE.md
*)
