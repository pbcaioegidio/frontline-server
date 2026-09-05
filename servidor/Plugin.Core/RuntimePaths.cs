using System;
using System.IO;

namespace Plugin.Core
{
    public static class RuntimePaths
    {
        private static string _contentRoot;

        public static string ContentRoot
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_contentRoot))
                    return _contentRoot;
                string configured = Environment.GetEnvironmentVariable("PB_CONTENT_ROOT");
                _contentRoot = string.IsNullOrWhiteSpace(configured)
                    ? Directory.GetCurrentDirectory()
                    : Path.GetFullPath(configured);
                return _contentRoot;
            }
        }

        public static void SetContentRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Content root cannot be empty.", nameof(path));
            _contentRoot = Path.GetFullPath(path);
            Directory.SetCurrentDirectory(_contentRoot);
        }
    }
}
