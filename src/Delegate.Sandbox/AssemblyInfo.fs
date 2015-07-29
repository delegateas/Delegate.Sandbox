namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Delegate.Sandbox")>]
[<assembly: AssemblyProductAttribute("Delegate.Sandbox")>]
[<assembly: AssemblyDescriptionAttribute("A library that provides a Computation Expression named Sandbox Builder, sandbox { return 42 }, which ensures that values returned from the computation are I/O side-effects safe and if not, they are marked as unsafe returning an exception.")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
