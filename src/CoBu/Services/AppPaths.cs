using System;
using System.IO;

namespace CoBu.Services
{
    /// <summary>
    /// Where CoBu keeps its data. By default next to the exe (in the folder the user picked in the installer);
    /// if that folder is not writable (e.g. a portable copy unpacked into Program Files) it falls back to %LocalAppData%\CoBu.
    /// </summary>
    public static class AppPaths
    {
        private static string? _storageRoot;

        public static string StorageRoot => _storageRoot ??= ResolveStorageRoot();

        // Inside the storage folder: it is the one place that is always writable (the exe folder may be Program Files)
        public static string CrashLogPath => Path.Combine(StorageRoot, "crash.log");

        private static string ResolveStorageRoot()
        {
            string beside = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CoBu_Storage");
            if (IsWritable(beside))
                return beside;

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CoBu", "CoBu_Storage");
        }

        private static bool IsWritable(string directory)
        {
            try
            {
                Directory.CreateDirectory(directory);
                string probe = Path.Combine(directory, ".write_test_" + Guid.NewGuid().ToString("N"));
                File.WriteAllText(probe, "x");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
