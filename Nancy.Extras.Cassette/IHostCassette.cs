using Cassette;

namespace Nancy.Extras.Cassette
{
    public interface IHostCassette
    {
        void Initialize(IRootPathProvider rootPathProvider, IUrlGenerator urlGenerator, IUrlModifier urlModifier);
        IPlaceholderTracker PlaceholderTracker { get; }
        IReferenceBuilder ReferenceBuilder { get; }
        string BasePath { get; }
    }
}