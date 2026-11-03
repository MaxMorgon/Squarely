using System.Runtime.CompilerServices;

// Lets tests use internal mutators (e.g. Board.SetCell) to set up scenarios.
// The name must match the test assembly name in both Sim.Tests.csproj and the test asmdef.
[assembly: InternalsVisibleTo("Squarely.Simulation.Tests")]
