using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Version = StardewModdingAPI.Version;

namespace Pathoschild.Stardew.ChestsAnywhere.Common
{
    /// <summary>Provides utility methods for mod updates.</summary>
    internal class UpdateHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get a semantic version number from the given version data.</summary>
        /// <param name="version">The version data.</param>
        public static string GetSemanticVersion(Version version)
        {
            return version.PatchVersion != 0
                ? $"{version.MajorVersion}.{version.MinorVersion}.{version.PatchVersion}"
                : $"{version.MajorVersion}.{version.MinorVersion}";
        }

        /// <summary>Get the latest release in a GitHub repository.</summary>
        /// <param name="repository">The name of the repository from which to fetch releases (like "pathoschild/LookupAnything").</param>
        public static async Task<GitRelease> GetLatestReleaseAsync(string repository)
        {
            // build request
            // (avoid HttpClient for Mac compatibility)
            HttpWebRequest request = WebRequest.CreateHttp($"https://api.github.com/repos/{repository}/releases/latest");
            AssemblyName assembly = typeof(UpdateHelper).Assembly.GetName();
            request.UserAgent = $"{assembly.Name}/{assembly.Version}";
            request.Accept = "application/vnd.github.v3+json";

            // fetch data 
            using (WebResponse response = request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<GitRelease>(responseText);
            }
        }
    }
}
