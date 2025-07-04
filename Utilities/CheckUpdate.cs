using System.Net.Http;
using System.Reflection;
namespace ReTranslator.Utilities
{
    public static class CheckUpdate
    {
        public static string GetCurrentVersion()
        {
            return System.Diagnostics.FileVersionInfo
                .GetVersionInfo(Assembly.GetExecutingAssembly().Location)
                .FileVersion ?? "未知版本";
        }

        public static async Task<string> GetLatestVersionAsync()
        {
            using var client = new HttpClient();
            // 替换为你自己的版本文件地址，比如 GitHub raw 文件
            string url = "https://gitee.com/retrocal/retranslator/raw/master/version";
            return await client.GetStringAsync(url);
        }

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            string current = GetCurrentVersion();
            string latest = await GetLatestVersionAsync();
            return current.Trim() != latest.Trim();
        }
    }
}