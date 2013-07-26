using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Cassette;
using Nancy.Extensions;
using Nancy.Responses;

namespace Nancy.Extras.Cassette.Modules
{
    public abstract class CassetteModule : NancyModule
    {
        private readonly HashAlgorithm hashAlgorithm = MD5.Create();

        protected CassetteModule(IProvideBundleCollections bundleProvider, IRootPathProvider rootPathProvider,
            IHostCassette cassetteHost)
            : base(cassetteHost.BasePath)
        {
            Get["/"] = _ => 501;

            Get["/(?<bundleType>(script|stylesheet|htmltemplate))/{id}/{path*}"]
                = p =>
                {
                    string path = "~/" + p.path;
                    string id = p.id;
                    var bundles = bundleProvider.Provide();

                    using (bundles.GetReadLock())
                    {
                        var bundle = bundles.FindBundlesContainingPath(path).FirstOrDefault();
                        
                        byte[] incomingHash;
                    
                        return (bundle == null
                                || false == id.TryParseHex(out incomingHash)
                                || false == bundle.Hash.SequenceEqual(incomingHash))
                            ? HttpStatusCode.NotFound
                            : HandleResourceRequest(bundle.OpenStream, bundle.ContentType, bundle.Hash);
                    }
                };

            Get["/asset/{path*}"]
                = p =>
                {
                    string path = "~/" + p.path;

                    var bundles = bundleProvider.Provide();

                    using (bundles.GetReadLock())
                    {
                        IAsset asset;
                        Bundle bundle;
                        
                        if (false == bundles.TryGetAssetByPath(path, out asset, out bundle))
                            return HttpStatusCode.NotFound;

                        return HandleResourceRequest(asset.OpenStream, bundle.ContentType, asset.Hash);
                    }
                };

            Get["/file/{path*}"]
                = p =>
                {
                    string path = p.path;

                    var filePath = GetFilePath(rootPathProvider, path);

                    if (false == File.Exists(filePath))
                    {
                        return HttpStatusCode.NotFound;
                    }
                    return HandleResourceRequest(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
                };
        }

        private string GetFilePath(IRootPathProvider rootPathProvider, string path)
        {
            var match = Regex.Match(
                path,
                "^(?<filename>.*)-[a-z0-9]+\\.(?<extension>[a-z]+)$",
                RegexOptions.IgnoreCase);
            
            return false == match.Success
                ? Path.Combine(rootPathProvider.GetRootPath(),  path.Replace('/', Path.DirectorySeparatorChar))
                : Regex.Replace(rootPathProvider.GetRootPath() + "\\"
                                + match.Groups["filename"].Value.Replace('/', '\\') + "."
                                + match.Groups["extension"].Value, "\\\\{2,}", "\\");
        }

        private Response HandleResourceRequest(Func<Stream> resource, string contentType, IEnumerable<byte> hash = null)
        {
            var etag = GetETag(resource, hash);
            if (Request.Headers.IfNoneMatch.Contains(etag))
            {
                return HttpStatusCode.NotModified;
            }
            return new StreamResponse(resource, contentType)
                .WithHeader("ETag", etag)
                .WithHeader("Expires", DateTime.UtcNow.Add(TimeSpan.FromDays(365)).ToString("r"))
                .WithHeader("Cache-Control", "public, max-age=31536000");
        }

        private string GetETag(Func<Stream> resource, IEnumerable<byte> hash = null)
        {
            if (hash != null) return "\"" + hash.ToHexString() + "\"";
            using (var stream = resource())
            {
                hash = hashAlgorithm.ComputeHash(stream);
            }
            return "\"" + hash.ToHexString() + "\"";
        }
    }
}