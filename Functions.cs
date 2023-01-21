namespace AsyncHandles
{
    public static class Functions
    {
        // Create a static HttpClient instance, and a function to create a new HttpClient instance if needed.
        // This is a bit of a hack, but it works for this example.
        // In a real application, you would probably want to use a dependency injection framework to manage the lifetime of the HttpClient.
        private static HttpClient s_client = new HttpClient();
        private static HttpClient GetHttpClient()
        {
            if (s_client == null)
                s_client = new HttpClient();
            return s_client;
        }

        public static object WebPageHandle(string url)
        {
            return GlobalCache.CreateHandle(nameof(WebPageHandle), new object[] { url }, 
                (objectType, parameters) => WebPageGetStringAsync(url).Result);
        }

        public static object WebPageHandleAsync(string url)
        {
            return GlobalCache.CreateHandleAsync(nameof(WebPageHandleAsync), new object[] { url }, 
                (objectType, parameters) => WebPageGetStringAsync(url));
        }

        static async Task<object> WebPageGetStringAsync(string url)
        {
            var client = GetHttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public static string WebPageSubstring(string handle, int start, int length)
        {
            var  results = GlobalCache.TryReadObject<string, string>(handle, (s) => s.Substring(start, length));
            if (results.Item1)
                return results.Item2;
            else
                return "Error";
        }

    }
}