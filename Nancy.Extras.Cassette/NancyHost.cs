using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using Cassette;
using Cassette.IO;
using Cassette.TinyIoC;
using Nancy.Bootstrapper;
using Nancy.Extras.Cassette.Startup;
using IsolatedStorageFile = System.IO.IsolatedStorage.IsolatedStorageFile;

namespace Nancy.Extras.Cassette
{
    public class NancyHost : HostBase, IProvideBundleCollections, IHostCassette
    {
        private readonly Func<TinyIoCContainer.ITinyIoCObjectLifetimeProvider> createLifetimeProvider;
        private IRootPathProvider rootPathProvider;
        private IUrlGenerator urlGenerator;
        private IUrlModifier urlModifier;

        static NancyHost()
        {
        }

        public NancyHost(Func<TinyIoCContainer.ITinyIoCObjectLifetimeProvider> createLifetimeProvider)
        {
            this.createLifetimeProvider = createLifetimeProvider;
        }

        protected TinyIoCContainer CasseteContainer
        {
            get { return Container; }
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return true; }
        }

        #region IHostCassette Members

        public void Initialize(IRootPathProvider rootPathProvider, IUrlGenerator urlGenerator, IUrlModifier urlModifier)
        {
            this.rootPathProvider = rootPathProvider;
            this.urlGenerator = urlGenerator;
            this.urlModifier = urlModifier;

            Initialize();
        }

        public IPlaceholderTracker PlaceholderTracker
        {
            get { return CasseteContainer.Resolve<IPlaceholderTracker>(); }
        }

        public IReferenceBuilder ReferenceBuilder
        {
            get { return CasseteContainer.Resolve<IReferenceBuilder>(); }
        }

        public string BasePath
        {
            get { return CassetteConfiguration.ModulePath; }
        }

        #endregion

        #region IProvideBundleCollections Members

        BundleCollection IProvideBundleCollections.Provide()
        {
            return CasseteContainer.Resolve<BundleCollection>();
        }

        #endregion

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new CassetteSettingsConfiguration(rootPathProvider);
        }

        protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider
            CreateRequestLifetimeProvider()
        {
            return createLifetimeProvider();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            CasseteContainer.Register(rootPathProvider);
            CasseteContainer.Register(urlModifier);
            CasseteContainer.Register(urlGenerator);
        }


        private class CassetteSettingsConfiguration : IConfiguration<CassetteSettings>
        {
            private readonly CassetteConfigurationSection configuration;
            private readonly IRootPathProvider rootPathProvider;

            public CassetteSettingsConfiguration(IRootPathProvider rootPathProvider)
            {
                this.rootPathProvider = rootPathProvider;

                configuration = (WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection)
                                ?? new CassetteConfigurationSection();
            }

            #region IConfiguration<CassetteSettings> Members

            public void Configure(CassetteSettings configurable)
            {
                configurable.IsDebuggingEnabled = !CassetteConfiguration.OptimizeOutput;
                configurable.IsHtmlRewritingEnabled = true;
                configurable.SourceDirectory = new FileSystemDirectory(rootPathProvider.GetRootPath());
                configurable.CacheDirectory = GetCacheDirectory(configuration);
            }

            #endregion

            private IDirectory GetCacheDirectory(CassetteConfigurationSection configurationSection)
            {
                var path = configurationSection.CacheDirectory;
                if (String.IsNullOrEmpty(path))
                {
                    return new IsolatedStorageDirectory(IsolatedStorageFile.GetMachineStoreForAssembly);
                }
                if (Path.IsPathRooted(path))
                {
                    return new FileSystemDirectory(path);
                }
                path = path.TrimStart('~', '/');
                return new FileSystemDirectory(Path.Combine(rootPathProvider.GetRootPath(), path));
            }
        }
    }
}