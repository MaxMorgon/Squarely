namespace Squarely.Simulation
{
    public enum ReplayDecodeFailure
    {
        None,
        WrongPrefix,
        UnknownVersion,
        CorruptData,
        UnknownCommandTag,
    }
}
