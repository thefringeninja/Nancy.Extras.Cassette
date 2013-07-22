using Cassette;
using Nancy.Bootstrapper;

namespace Nancy.Extras.Cassette.Startup
{
    public abstract class CassetteStartup : IApplicationStartup
    {
        private readonly IHostCassette host;
        private readonly IRootPathProvider rootPathProvider;
        private readonly IUrlGenerator urlGenerator;
        private readonly IUrlModifier urlModifier;

        protected CassetteStartup(IHostCassette host, IRootPathProvider rootPathProvider, IUrlGenerator urlGenerator, IUrlModifier urlModifier)
        {
            this.host = host;
            this.rootPathProvider = rootPathProvider;
            this.urlGenerator = urlGenerator;
            this.urlModifier = urlModifier;
        }

        public virtual void Initialize(IPipelines pipelines)
        {
            host.Initialize(rootPathProvider, urlGenerator, urlModifier);
        }
    }
}