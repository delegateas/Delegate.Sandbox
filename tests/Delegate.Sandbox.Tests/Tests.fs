module Delegate.Sandbox.Tests

open System
open Delegate.Sandbox
open NUnit.Framework

let inline (>>=) m f = IOEffect.bind f m
let inline (|=) m x = IOEffect.defaultValue x m

[<Test>]
let ``Reflection is not allowed with sandbox-mode`` () =
  // Some issues with NUnit and Reflection (not allowed in sandbox{ ... })
  let unsafe = (sandbox{ return 21 + 21 }).ToString()
  let fullname = typedefof<TypeInitializationException>.FullName
  let expected = "Unsafe " + fullname
  let result = unsafe.StartsWith(expected)
  Assert.AreEqual(true,result)