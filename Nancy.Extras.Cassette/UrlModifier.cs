using System.Configuration;
using System.Text.RegularExpressions;
using Cassette;
using Nancy.Extras.Cassette.Startup;

namespace Nancy.Extras.Cassette
{
    public class UrlModifier : IUrlModifier
    {
        private static readonly Regex Axd = new Regex("^(.*cassette.axd)", RegexOptions.Compiled);

        #region IUrlModifier Members

        public string Modify(string url)
        {
            return Axd.Replace(url, CassetteConfiguration.ModulePath);
        }

        #endregion
    }
}