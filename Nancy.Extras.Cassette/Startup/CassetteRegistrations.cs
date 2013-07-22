using System.Collections.Generic;
using System.Linq;
using Cassette;
using Cassette.TinyIoC;
using Nancy.Bootstrapper;

namespace Nancy.Extras.Cassette.Startup
{
    public abstract class CassetteRegistrations : IApplicationRegistrations
    {
        public IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                yield return new TypeRegistration(typeof(IUrlModifier), typeof(UrlModifier));
                yield return new TypeRegistration(typeof(IUrlGenerator), typeof(UrlGenerator));
            }
        }

        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get { yield break; }
        }

        private InstanceRegistration[] instanceRegistrations;
        public IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get
            {
                return instanceRegistrations ?? (instanceRegistrations = BuildInstanceRegistrations().ToArray());
            }
        }

        private IEnumerable<InstanceRegistration> BuildInstanceRegistrations()
        {
            var nancyHost = new NancyHost(GetLifetimeProvider);
            yield return new InstanceRegistration(typeof(IProvideBundleCollections), nancyHost);
            yield return new InstanceRegistration(typeof(IHostCassette), nancyHost);

        }

        protected abstract TinyIoCContainer.ITinyIoCObjectLifetimeProvider GetLifetimeProvider();
    }
}