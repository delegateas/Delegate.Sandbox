namespace Delegate.Sandbox

/// Documentation for my library
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
[<AutoOpen>]
module Sandbox = 
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

  [<Sealed>]
  type RemoveConsoleInOutEffects() = 
    inherit MarshalByRefObject()
    do 
      Console.SetIn(new StreamReader(Stream.Null))
      Console.SetOut(new StreamWriter(Stream.Null))
      Console.SetError(new StreamWriter(Stream.Null))
  
  [<Sealed>]
  type SandboxBuilder() = 
    inherit MarshalByRefObject()
    member x.Return v = IOSafe v
    member x.Delay f = try f() with ex -> Unsafe ex
  
  let private appBase () = 
    AppDomain.CurrentDomain.SetupInformation.ApplicationBase

  let private strongName (assembly : Assembly) = 
    let an = assembly.GetName()
    let pb = StrongNamePublicKeyBlob(an.GetPublicKey())
    StrongName(pb, an.Name, an.Version)

  let private dirName x = Path.GetDirectoryName(x)

  let private fileName x = Path.GetFileName(x)

  let private assemblyLocation t = Assembly.GetAssembly(t).Location

  /// Returns 42
  ///
  /// ## Parameters
  ///  - `num` - whatever
  let sandbox =
    let ads = AppDomainSetup()
    do ads.ApplicationBase <- dirName (appBase ())

    let ps = PermissionSet(PermissionState.None)
    let fps =
        SecurityPermission(
            SecurityPermissionFlag.Execution ||| 
            SecurityPermissionFlag.UnmanagedCode)
    do ps.AddPermission(fps) |> ignore

    let nd = 
        AppDomain.CreateDomain(
            "Sandbox_" + Guid.NewGuid().ToString(), 
            AppDomain.CurrentDomain.Evidence,
            ads, ps, [| strongName (typedefof<SandboxBuilder>.Assembly); |])

    // Execute UnmanagedCode in order to remove Console I/O side-effects
    Activator
        .CreateInstanceFrom(
            nd,
            fileName(assemblyLocation typedefof<RemoveConsoleInOutEffects>),
            typedefof<RemoveConsoleInOutEffects>.FullName)
        .Unwrap() :?> RemoveConsoleInOutEffects |> ignore

    // Remove UnmanagedCode permission before instantiating the sandbox builder
    nd.PermissionSet.RemovePermission(typedefof<SecurityPermission>) |> ignore
    let lps = SecurityPermission(SecurityPermissionFlag.Execution)
    do ps.AddPermission(lps) |> ignore

    Activator
        .CreateInstanceFrom(
            nd,
            fileName(assemblyLocation typedefof<SandboxBuilder>),
            typedefof<SandboxBuilder>.FullName)
        .Unwrap() :?> SandboxBuilder