using BenchmarkDotNet.Running;
using Companion.Signaling.Benchmark;

/* 
 * The benchmarks provided do not state anything about the performance of this library. Whether
 * it is an enhancement or not is fully dependent on the use case. When working with a lot of 
 * components (e.g. buttons, inputs, etc.) or very complex ones, the overhead of the signaling
 * system is far less than the performance gains from avoiding unnecessary renders.
 * 
 * The overhead of accessing and rendering a simple string value is increased by roughly 150%
 * in comparison to a default Blazor component.
 * 
 * The provided benchmarks are only meant as base implementations to build custom ones that reflect
 * the specific use-cases.
 */
BenchmarkRunner.Run<SignalingBenchmark>();
