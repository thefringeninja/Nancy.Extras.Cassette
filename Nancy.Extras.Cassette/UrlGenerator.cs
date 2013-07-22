using System;
using System.Reflection;
using Cassette;

namespace Nancy.Extras.Cassette
{
    public class UrlGenerator : IUrlGenerator
    {
        private static readonly PropertyInfo urlProperty
            = typeof (Bundle).GetProperty("Url", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly string cassetteHandlerPrefix;
        private readonly IUrlModifier urlModifier;

        public UrlGenerator(IUrlModifier urlModifier, IHostCassette casseteHost)
            : this(urlModifier, casseteHost.BasePath + "/")
        {
        }

        protected UrlGenerator(IUrlModifier urlModifier, string cassetteHandlerPrefix)
        {
            this.urlModifier = urlModifier;
            this.cassetteHandlerPrefix = cassetteHandlerPrefix;
        }

        #region IUrlGenerator Members

        public string CreateBundleUrl(Bundle bundle)
        {
            return urlModifier.Modify(cassetteHandlerPrefix + urlProperty.GetValue(bundle, null));
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return urlModifier.Modify(
                cassetteHandlerPrefix + "asset" + asset.Path.Substring(1) + "?"
                + asset.Hash.ToUrlSafeBase64String());
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (!filename.StartsWith("~"))
                throw new ArgumentException("Image filename must be application relative (starting with '~').");

            var str = ConvertToForwardSlashes(filename).Substring(1);

            var startIndex = str.LastIndexOf('.');

            return urlModifier.Modify(
                cassetteHandlerPrefix + "file" + (startIndex < 0
                    ? str + "-" + hash
                    : str.Insert(startIndex, "-" + hash)));
        }

        public string CreateRawFileUrl(string filename)
        {
            if (!filename.StartsWith("~"))
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            var str = ConvertToForwardSlashes(filename).Substring(1);
            return urlModifier.Modify(cassetteHandlerPrefix + "file" + str);
        }

        public string CreateAbsolutePathUrl(string applicationRelativePath)
        {
            return urlModifier.Modify(applicationRelativePath.TrimStart('~', '/'));
        }

        public string CreateCachedFileUrl(string filename)
        {
            if (!filename.StartsWith("~"))
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            var str = ConvertToForwardSlashes(filename).Substring(1);
            return urlModifier.Modify(cassetteHandlerPrefix + "file" + str);
        }

        #endregion

        private string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}