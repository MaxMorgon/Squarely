using System.Runtime.CompilerServices;

// Lets tests read internal members (e.g. MemoryKeyValueStore.SaveCount) without making them public API.
// The name must match the test asmdef name.
[assembly: InternalsVisibleTo("Squarely.App.Tests")]
