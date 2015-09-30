### 1.5.0.1 - Sep 30 2015
* Added AssemblyInfo to the library (no file version)

### 1.5.0.0 - July 30 2015
* Major code refactoring (less code, more awesomeness)
* Fixed critical security permission issue (unmanaged code could be invoked in sandbox)
* Fixed issue with No code optimization in Release mode
* Fixed issue with Unit tests. See Delegate.Sandbox.Tests project
* Added support for nested sandboxes. Ex: `sandbox{ return sandbox{ return 42 } }`
* Thanks to nested sandboxes, it's now possible to ensure that a library is 100%
  I/O side-effects safe if `FSharp.Compiler.Services` are used in combination with 
  Post-Build F# script (will be made available in v.2.0.0.0)

### 1.0.0.0 - July 30 2015
* Initial release
