namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Delegate.Sandbox")>]
[<assembly: AssemblyProductAttribute("Delegate.Sandbox")>]
[<assembly: AssemblyDescriptionAttribute("Delegate.Sandbox is library that provides I/O side-effects safe code by using a sandbox computation expression.")>]
[<assembly: AssemblyCompanyAttribute("Delegate")>]
[<assembly: AssemblyCopyrightAttribute("Copyleft (ɔ) Delegate A/S 2015")>]
[<assembly: AssemblyVersionAttribute("1.5.0.1")>]
[<assembly: AssemblyFileVersionAttribute("1.5.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.5.0.1"
