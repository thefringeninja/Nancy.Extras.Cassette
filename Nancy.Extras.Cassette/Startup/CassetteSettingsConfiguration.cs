using Nancy.Bootstrapper;

namespace Nancy.Extras.Cassette.Startup
{
    public static class CassetteConfiguration
    {
        public static bool OptimizeOutput { get; set; }

        public static string ModulePath { get; set; }

        static CassetteConfiguration()
        {
            ModulePath = "/_cassette";
            AppDomainAssemblyTypeScanner.LoadAssemblies("Cassette.CoffeeScript.dll");
            AppDomainAssemblyTypeScanner.LoadAssemblies("Cassette.Hogan.dll");
            AppDomainAssemblyTypeScanner.LoadAssemblies("Cassette.JQueryTmpl.dll");
            AppDomainAssemblyTypeScanner.LoadAssemblies("Cassette.KnockoutJQueryTmpl.dll");
            AppDomainAssemblyTypeScanner.LoadAssemblies("Cassette.Less.dll");
            AppDomainAssemblyTypeScanner.LoadAssemblies("Cassette.Sass.dll");
        }
    }
}