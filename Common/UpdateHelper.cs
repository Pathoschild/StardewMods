using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides utility methods for mod updates.</summary>
    internal class UpdateHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the latest release from the GitHub repository.</summary>
        /// <param name="key">The mod key in the update document.</param>
        /// <param name="url">The URL from which to fetch the release versions.</param>
        public static async Task<ISemanticVersion> GetLatestReleaseAsync(string key, string url = CommonConstants.UpdateUrl)
        {
            // build request
            // (avoid HttpClient for Mac compatibility)
            HttpWebRequest request = WebRequest.CreateHttp(url);
            AssemblyName assembly = typeof(UpdateHelper).Assembly.GetName();
            request.UserAgent = $"{assembly.Name}/{assembly.Version}";
            request.Accept = "application/vnd.github.v3+json";

            // fetch data
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseText = reader.ReadToEnd();
                IDictionary<string, string> versions = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                // validate
                if (versions == null || !versions.ContainsKey("schema") || versions["schema"] != "1.0")
                    throw new InvalidOperationException("The update check failed because the latest version info couldn't be parsed. The mod may need to be updated.");

                // get version
                return versions.ContainsKey(key)
                    ? new SemanticVersion(versions[key])
                    : null;
            }
        }
    }
}
