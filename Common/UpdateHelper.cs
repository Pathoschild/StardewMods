using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
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
        /// <param name="url">The URL from which to fetch the release versions.</param>
        public static async Task<IDictionary<string, string>> GetLatestReleasesAsync(string url = CommonConstants.UpdateUrl)
        {
            // build request
            // (avoid HttpClient for Mac compatibility)
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);
            AssemblyName assembly = typeof(UpdateHelper).Assembly.GetName();
            request.UserAgent = $"{assembly.Name}/{assembly.Version}";

            // fetch data
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
            }
        }

        /// <summary>Log a message to the console indicating the version check result.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="current">The current version.</param>
        /// <param name="key">The mod key in the update document.</param>
        /// <param name="url">The URL from which to fetch the release versions.</param>
        public static async Task<ISemanticVersion> LogVersionCheck(IMonitor monitor, ISemanticVersion current, string key, string url = CommonConstants.UpdateUrl)
        {
            try
            {
                // get version
                ISemanticVersion latest = null;
                {
                    IDictionary<string, string> versions = await UpdateHelper.GetLatestReleasesAsync(url).ConfigureAwait(false);
                    if (versions == null || !versions.ContainsKey("schema"))
                        monitor.Log("There might be a new version of this mod. The update check failed because it couldn't parse the server response.", LogLevel.Warn);
                    else if (new SemanticVersion(versions["schema"]).IsNewerThan(new SemanticVersion("1.0")))
                        monitor.Log($"There might be a new version of this mod. The version document uses schema {versions["schema"]} instead of 1.0.", LogLevel.Warn);
                    else if (!versions.ContainsKey(key))
                        monitor.Log($"There might be a new version of this mod. The version document doesn't contain a record for '{key}'.", LogLevel.Warn);
                    else
                        latest = new SemanticVersion(versions[key]);
                }
                if (latest == null)
                    return null;

                // log check
                if (latest.IsNewerThan(current))
                    monitor.Log($"Update to version {latest} available.", LogLevel.Alert);
                else
                    monitor.Log("Checking for update... none found.", LogLevel.Trace);

                return latest;
            }
            catch (Exception ex)
            {
                monitor.InterceptError(ex, "checking for a newer version");
                return null;
            }
        }
    }
}
