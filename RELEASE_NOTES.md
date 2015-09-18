### 1.5.0.0 - July 30 2015
* Major code refactoring (less code, more awesome)
* Fixed critical security permission issue (unmanaged code could be invoked in sanbox)
* Fixed issue with No code optmization in Release mode
* Fixed issue with Unit tests. See Delegate.Sandbox.Tests project
* Added support for nested sandboxes. Ex: sandbox{ return sandbox{ return 42 } }
* Thanks to nested sandboxes, it's now possible to ensure that a libary is 100%
  IO Effect safe thanks to FSharp.Compiler.Services and a Post-Build F# script

### 1.0.0.0 - July 30 2015
* Initial release
