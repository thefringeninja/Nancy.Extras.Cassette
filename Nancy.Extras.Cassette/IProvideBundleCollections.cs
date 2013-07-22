using Cassette;

namespace Nancy.Extras.Cassette
{
    public interface IProvideBundleCollections
    {
        BundleCollection Provide();
    }
}