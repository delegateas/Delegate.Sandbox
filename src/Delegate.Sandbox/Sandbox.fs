namespace Delegate.Sandbox

[<AutoOpen>]
module GlobalValues = 
  open System
  open System.IO
  open System.Reflection
  open System.Security
  open System.Security.Permissions
  open System.Security.Policy

  type 'a IOEffect = private | IOSafe of 'a | Unsafe of exn
    with
      override x.ToString() = x |> function
        | IOSafe s -> sprintf "IOSafe %s" (s.ToString())
        | Unsafe e -> sprintf "Unsafe %A" e

  let (|IOSafe|Unsafe|) = function | IOSafe s -> IOSafe s | Unsafe e -> Unsafe e

  module IOEffect = 
    let bind f = function | IOSafe s -> f s | Unsafe u -> Unsafe u
    let defaultValue x = function | IOSafe s -> s | Unsafe _ -> x

  let inline (>>=) m f = IOEffect.bind f m

  [<Sealed>]
  type private RemoveConsoleIO() = 
    inherit MarshalByRefObject()
    do 
      Console.SetIn(new StreamReader(Stream.Null))
      Console.SetOut(new StreamWriter(Stream.Null))
      Console.SetError(new StreamWriter(Stream.Null))
  
  [<Sealed>]
  type SandboxBuilder() = 
    inherit MarshalByRefObject()
    member x.Return v = IOSafe v
    [<SecurityPermissionAttribute(SecurityAction.PermitOnly, Execution = true)>]
    member x.Delay f = try f() with ex -> Unsafe ex
  
  let private sandboxDomain,sandboxType =
    match AppDomain.CurrentDomain.GetData("domain"),
          AppDomain.CurrentDomain.GetData("typeof") with
    | null,_ | _,null ->
      let sandboxType' = typeof<SandboxBuilder>
      let consoleType' = typeof<RemoveConsoleIO>

      let permissionSet = PermissionSet(PermissionState.None)
      let securityPermission =
        SecurityPermission(
            SecurityPermissionFlag.UnmanagedCode ||| 
            SecurityPermissionFlag.Execution)
      do permissionSet.AddPermission(securityPermission) |> ignore

      let sandboxDomain' = 
        AppDomain.CreateDomain(
          "Sandbox_" + Guid.NewGuid().ToString(),
          AppDomain.CurrentDomain.Evidence,
          AppDomain.CurrentDomain.SetupInformation,
          permissionSet)
      // Most likely not theadsafe but it's always the same value so ...
      do sandboxDomain'.SetData("domain", sandboxDomain' :> obj)
      do sandboxDomain'.SetData("typeof", sandboxType' :> obj)

      sandboxDomain'.CreateInstanceAndUnwrap(
        consoleType'.Assembly.FullName, consoleType'.FullName)
          :?> RemoveConsoleIO |> ignore

      sandboxDomain',sandboxType'
    | domain,``typeof`` -> domain :?> AppDomain, ``typeof`` :?> Type

  let sandbox =
    sandboxDomain.CreateInstanceAndUnwrap(
      sandboxType.Assembly.FullName, sandboxType.FullName) :?> SandboxBuilder
