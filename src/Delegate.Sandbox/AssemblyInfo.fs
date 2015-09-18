namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Delegate.Sandbox")>]
[<assembly: AssemblyProductAttribute("Delegate.Sandbox")>]
[<assembly: AssemblyDescriptionAttribute("Delegate.Sandbox is library that provides I/O side-effects safe code by using a sandbox computation expression.")>]
[<assembly: AssemblyVersionAttribute("1.5.0.0")>]
[<assembly: AssemblyFileVersionAttribute("1.5.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.5.0.0"
