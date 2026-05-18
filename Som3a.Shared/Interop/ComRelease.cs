using System;
using System.Runtime.InteropServices;

namespace Som3a.Shared.Interop
{
    public static class ComRelease
    {
        public static void SafeRelease(object? com)
        {
            try
            {
                if (com != null && Marshal.IsComObject(com))
                    Marshal.FinalReleaseComObject(com);
            }
            catch
            {
                // تجاهل
            }
        }
    }
}
