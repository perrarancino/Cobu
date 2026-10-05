using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace CoBu.Services
{
    /// <summary>
    /// Gives memory back to Windows while the window is hidden: full compacting GC (including the
    /// Large Object Heap, which .NET never compacts on its own) and a working-set trim.
    /// </summary>
    public static class MemoryTrimmer
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("psapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        public static void Trim()
        {
            try
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                EmptyWorkingSet(GetCurrentProcess());
            }
            catch
            {
                // best effort only
            }
        }
    }
}
