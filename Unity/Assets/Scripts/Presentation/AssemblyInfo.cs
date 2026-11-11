using System.Runtime.CompilerServices;

// Lets tests call internal helpers (e.g. PiecePlacedHandler.WaveCells) without making them public API.
// The name must match the test asmdef name.
[assembly: InternalsVisibleTo("Squarely.Presentation.Tests")]
