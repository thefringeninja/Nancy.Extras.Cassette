Nancy.Extras.Cassette
=====================

This is a module based approach to using Cassette with Nancy. Big ups go to ChrisMH for his work on Cassette.Nancy.

Eventually this will be available as a nuget packjage. The idea being you just install it, You will need to do a couple of things to get this working.

1) Add a cassette.targets to your project file. Mine looks like:
```
<?xml version="1.0" encoding="utf-8" ?>
<!--
    The web application csproj file has been modified to import this file.
    So after a build, the Cassette bundles will be saved into a cache directory.
-->
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <UsingTask AssemblyFile="$(SolutionDir).cassette\Cassette.MSBuild.dll" TaskName="CreateBundles"/>
  <Target Name="Bundle" AfterTargets="Build">
    <CreateBundles Condition="'$(OutDir)' != '$(OutputPath)'"
      Output="$(OutDir)_PublishedWebsites\$(MSBuildProjectName)\cassette-cache" 
      Bin="$(OutDir)_PublishedWebsites\$(MSBuildProjectName)\bin" />
    <CreateBundles Condition="'$(OutDir)' == '$(OutputPath)'" 
      Output="$(OutputPath)\..\cassette-cache" 
      Bin="$(OutputPath)" />
  </Target>
</Project>
```  
sadly this means no on the fly editing of assets, yet. I plan on adding this later.

2) Implement `IConfiguration<BundleCollection>`

3) Subclass  `Nancy.Extras.Cassette.Startup.CassetteRegistrations`. This is so we don't have a hard dependency on System.Web.
```
public class CassetteRegistrations : Nancy.Extras.Cassette.Startup.CassetteRegistrations
{
    /// <summary>
    ///   Hate to use a hosting specific provider here but we know we are going to use asp.net 
    ///   for the forseeable future.
    /// </summary>
    public class HttpContextLifetimeProvider :
        TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        private readonly string _KeyName = String.Format("TinyIoC.HttpContext.{0}", Guid.NewGuid());

        #region ITinyIoCObjectLifetimeProvider Members

        public object GetObject()
        {
            return HttpContext.Current.Items[_KeyName];
        }

        public void SetObject(object value)
        {
            HttpContext.Current.Items[_KeyName] = value;
        }

        public void ReleaseObject()
        {
            var item = GetObject() as IDisposable;

            if (item != null)
                item.Dispose();

            SetObject(null);
        }

        #endregion
    }

    protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider GetLifetimeProvider()
    {
        return new HttpContextLifetimeProvider();
    }
}
```  
4) Implement a helper and subclass `Nancy.Extras.Cassette.Startup.CassetteStartup` to get your custom helper to your view engine. Your helper will need access to `IPlaceholderTracker` for replacing Cassette's tokens with the correct url. Again I did this so there is no hard dependency on any view engine.
```
public class BundlesHelper : IBundlesHelper
{
    private readonly IPlaceholderTracker placeholderTracker;
    private readonly IReferenceBuilder referenceBuilder;

    public BundlesHelper(IReferenceBuilder referenceBuilder, IPlaceholderTracker placeholderTracker)
    {
        this.referenceBuilder = referenceBuilder;
        this.placeholderTracker = placeholderTracker;
        
    }

    #region IBundlesHelper Members

    public void Reference(string assetPathOrBundlePathOrUrl, string pageLocation = null)
    {
        referenceBuilder.Reference(assetPathOrBundlePathOrUrl, pageLocation);
    }

    public IHtmlString RenderScripts(string pageLocation = null)
    {
        return Render<ScriptBundle>();
    }

    public IHtmlString RenderStylesheets(string pageLocation = null)
    {
        return Render<StylesheetBundle>();
    }
    public void AddInlineScript(Func<object, object> scriptContent, string pageLocation = null)
    {
        AddInlineScript(scriptContent(null).ToString(), pageLocation);
    }
    public void AddInlineScript(string scriptContent, string pageLocation = null)
    {
        var script = new InlineScriptBundle(scriptContent);
        referenceBuilder.Reference(script, pageLocation);
    }

    public string FileUrl(string bundlePath)
    {
        throw new NotImplementedException();
    }

    #endregion

    private IHtmlString Render<TBundle>(string pageLocation = null) where TBundle : Bundle
    {
        var html = placeholderTracker.ReplacePlaceholders(referenceBuilder.Render<TBundle>(pageLocation));
        return new NonEncodedHtmlString(html);
    }

    public const string BUNDLES_HELPER = "Cassette.BundlesHelper";
}

public class CassetteStartup : Nancy.Extras.Cassette.Startup.CassetteStartup
{
    private readonly IHostCassette host;

    public CassetteStartup(IHostCassette host, IRootPathProvider rootPathProvider, IUrlGenerator urlGenerator,
        IUrlModifier urlModifier) : base(host, rootPathProvider, urlGenerator, urlModifier)
    {
        this.host = host;
    }

    public override void Initialize(IPipelines pipelines)
    {
        base.Initialize(pipelines);
        pipelines.BeforeRequest.AddItemToStartOfPipeline(IncludeBundlesHelper);
    }

    private Response IncludeBundlesHelper(NancyContext context)
    {
        context.Items[BundlesHelper.BUNDLES_HELPER] = new BundlesHelper(
            host.ReferenceBuilder,
            host.PlaceholderTracker);
        return null;
    }
}
```
5) Finally, subclass Nancy.Extras.Cassette.Modules.CassetteModule:
```
public class CassetteModule : Nancy.Extras.Cassette.Modules.CassetteModule
{
    public CassetteModule(IProvideBundleCollections bundleProvider, IRootPathProvider rootPathProvider, IHostCassette host) : base(bundleProvider, rootPathProvider, host)
    {
    }
}
```
