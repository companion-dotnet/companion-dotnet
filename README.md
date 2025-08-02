# Companion .NET
*Companion .NET* provides an API for change detection, therefore allowing recalculations only when it is actually necessary. The concept is pretty much the same as the signals that are available in [Angular](https://angular.dev/guide/signals).

- **How does this work?** *Companion .NET* provides a wrapper for values it should be used on
- **What are the restrictions?** The usage is currently restricted to synchronous tasks
- **What are the disadvantages?** There are two main disadvantages: Accessing a variable takes more computation time (as they are performing the change detection) and it results in slightly less maintainable code by default

## About the project
Currently, the project is only a proof of concept. Mainly the Blazor integration is not fully implemented yet, there are some heavy optimizations to be made. If there are any ideas/feedback, it is always welcome.

## Getting started
*Companion .NET* does not need any special setup. Just add the project as a reference and start using it. Details on how to do so can be found in the following files:

 - [Basic Concepts of *Companion .NET*](docs/BasicConcepts.md)
 - [Pitfalls of Memory Leaks](docs/MemoryLeaks.md)
 - [Usage with Blazor Components](docs/UsageWithBlazor.md)
