using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette;
using Cassette.TinyIoC;
using Nancy.Bootstrapper;

namespace Nancy.Extras.Cassette.Startup
{
    public abstract class CassetteRegistrations : IApplicationRegistrations, IConfiguration<TinyIoCContainer>
    {
        private InstanceRegistration[] instanceRegistrations;

        #region IApplicationRegistrations Members

        public virtual IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                yield return new TypeRegistration(typeof (IUrlModifier), typeof (UrlModifier));
                yield return new TypeRegistration(typeof (IUrlGenerator), typeof (UrlGenerator));
            }
        }

        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get { yield break; }
        }

        public virtual IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get { return instanceRegistrations ?? (instanceRegistrations = BuildInstanceRegistrations().ToArray()); }
        }

        #endregion

        #region IConfiguration<TinyIoCContainer> Members

        public void Configure(TinyIoCContainer configurable)
        {
            foreach (var registration in TypeRegistrations)
            {
                configurable.Register(registration.RegistrationType, registration.ImplementationType);
            }

            foreach (var registration in InstanceRegistrations)
            {
                configurable.Register(registration.RegistrationType, registration.Implementation);
            }
            configurable.Register(typeof (IRootPathProvider), new MsbuildRootPathProvider());
        }

        #endregion

        private IEnumerable<InstanceRegistration> BuildInstanceRegistrations()
        {
            var nancyHost = new NancyHost(GetLifetimeProvider);
            yield return new InstanceRegistration(typeof (IProvideBundleCollections), nancyHost);
            yield return new InstanceRegistration(typeof (IHostCassette), nancyHost);
        }

        protected abstract TinyIoCContainer.ITinyIoCObjectLifetimeProvider GetLifetimeProvider();

        #region Nested type: MsbuildRootPathProvider
        /// <summary>
        /// this is just here so if you build with the msbuild task, cassette's container will take over and register this class
        /// </summary>
        private class MsbuildRootPathProvider : IRootPathProvider
        {
            #region IRootPathProvider Members

            public string GetRootPath()
            {
                var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

                return directory.Parent.FullName;
            }

            #endregion
        }

        #endregion
    }
}
