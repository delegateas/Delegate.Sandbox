module Delegate.Sandbox.Tests

open System
open Delegate.Sandbox
open NUnit.Framework

[<Test>]
let ``Very basic test`` () =
  let result = sandbox{ return 21 + 21 }
  Assert.AreEqual(true, result |== 0 = 42)

[<Test>]
let ``Basic nested test`` () =
  let result = sandbox{ return sandbox{ return 21 + 21 } }
  Assert.AreEqual(true, result |== sandbox{ return 0 } |== 0 = 42)