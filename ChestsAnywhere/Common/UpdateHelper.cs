using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI;
using Version = StardewModdingAPI.Version;

namespace ChestsAnywhere.Common
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
            using (HttpClient client = new HttpClient())
            {
                // get assembly info for user agent
                AssemblyName assembly = typeof(UpdateHelper).Assembly.GetName();
                try
                {
                    // fetch latest release
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{repository}/releases/latest"))
                    {
                        request.Headers.UserAgent.Add(new ProductInfoHeaderValue(assembly.Name, assembly.Version.ToString()));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json")); // API version
                        using (HttpResponseMessage response = await client.SendAsync(request))
                        {
                            string responseText = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<GitRelease>(responseText);
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    Log.Error("Could not fetch response from: [https://api.github.com/];");
                    return new GitRelease { Errored = true };
                }
            }
        }
    }
}
