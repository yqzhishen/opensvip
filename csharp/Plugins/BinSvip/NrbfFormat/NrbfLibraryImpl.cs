using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace XSAppModel.NrbfFormat
{
    internal static unsafe class NrbfLibraryImpl
    {
        /* Native methods */
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        public static extern int FormatMessage(int flag, ref IntPtr source, int msgid, int langid, ref string buf,
            int size, ref IntPtr args);


        /* Library attributes */
        public static readonly string DllName = "QNrbfFormat.dll";

        public static IntPtr DllPtr = IntPtr.Zero;

        public static string DllPath = "";


        /* Load or free */
        public static void Init()
        {
            // Find library
            if (DllPath == "")
            {
                DllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            string dll = Path.Combine(DllPath, DllName);
            if (!File.Exists(dll))
            {
                throw new DllNotFoundException($"Required library \"{DllName}\" not found.");
            }

            // Load library
            DllPtr = LoadLibrary(dll);
            if (DllPtr == IntPtr.Zero)
            {
                int errCode = Marshal.GetLastWin32Error();
                IntPtr ptr = IntPtr.Zero;
                string msg = $"Required library \"{DllName}\" not valid.";
                FormatMessage(0x1300, ref ptr, errCode, 0, ref msg, 255, ref ptr);
                msg = msg.Trim().Replace("\r\n", "").Replace("%1", DllName);
                throw new DllNotFoundException(msg);
            }

            // Get function addresses
            qnrbf_dll_init_ptr = GetFunctionEntry<plain_delegate>("qnrbf_dll_init");
            qnrbf_dll_exit_ptr = GetFunctionEntry<plain_delegate>("qnrbf_dll_exit");
            
            qnrbf_malloc_ptr = GetFunctionEntry<qnrbf_malloc_delegate>("qnrbf_malloc");
            qnrbf_free_ptr = GetFunctionEntry<qnrbf_free_delegate>("qnrbf_free");
            qnrbf_memcpy_ptr = GetFunctionEntry<qnrbf_memcpy_delegate>("qnrbf_memcpy");
            qnrbf_memset_ptr = GetFunctionEntry<qnrbf_memset_delegate>("qnrbf_memset");

            qnrbf_xstudio_alloc_context_ptr =
                GetFunctionEntry<qnrbf_xstudio_alloc_context_delegate>("qnrbf_xstudio_alloc_context");
            qnrbf_xstudio_free_context_ptr =
                GetFunctionEntry<qnrbf_xstudio_free_context_delegate>("qnrbf_xstudio_free_context");
            qnrbf_xstudio_read_ptr = GetFunctionEntry<qnrbf_xstudio_read_delegate>("qnrbf_xstudio_read");
            qnrbf_xstudio_write_ptr = GetFunctionEntry<qnrbf_xstudio_write_delegate>("qnrbf_xstudio_write");

            Console.WriteLine($"Successfully load library \"{DllName}\".");
        }

        public static void Deinit()
        {
            if (DllPtr == IntPtr.Zero)
            {
                return;
            }

            // Unset function pointers
            qnrbf_dll_init_ptr = null;
            qnrbf_dll_exit_ptr = null;
            
            qnrbf_malloc_ptr = null;
            qnrbf_free_ptr = null;
            qnrbf_memcpy_ptr = null;
            qnrbf_memset_ptr = null;

            qnrbf_xstudio_alloc_context_ptr = null;
            qnrbf_xstudio_free_context_ptr = null;
            qnrbf_xstudio_read_ptr = null;
            qnrbf_xstudio_write_ptr = null;

            // Free library
            FreeLibrary(DllPtr);
            DllPtr = IntPtr.Zero;

            Console.WriteLine($"Successfully unload library \"{DllName}\".");
        }

        private static T GetFunctionEntry<T>(string name) where T : Delegate
        {
            var address = GetProcAddress(DllPtr, name);
            if (address == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException($"Entry of function \"{name}\" not found.");
            }

            return (T)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
        }


        /* Allocator */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void plain_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* qnrbf_malloc_delegate(int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void qnrbf_free_delegate(void* data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void qnrbf_memcpy_delegate(void* dst, void* src, int count);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void qnrbf_memset_delegate(void* dst, int value, int count);

        /* Context */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NrbfLibrary.qnrbf_xstudio_context* qnrbf_xstudio_alloc_context_delegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void qnrbf_xstudio_free_context_delegate(NrbfLibrary.qnrbf_xstudio_context* ctx);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void qnrbf_xstudio_read_delegate(NrbfLibrary.qnrbf_xstudio_context* @params);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void qnrbf_xstudio_write_delegate(NrbfLibrary.qnrbf_xstudio_context* ctx);
        
        
        /* You must call the following functions when loading or freeing this library,
        * to make it thread-safe for your application.
        */
        public static plain_delegate qnrbf_dll_init_ptr = null;

        public static plain_delegate qnrbf_dll_exit_ptr = null;
        
        /* Allocator */
        public static qnrbf_malloc_delegate qnrbf_malloc_ptr = null;

        public static qnrbf_free_delegate qnrbf_free_ptr = null;

        public static qnrbf_memcpy_delegate qnrbf_memcpy_ptr = null;

        public static qnrbf_memset_delegate qnrbf_memset_ptr = null;

        /* Context */
        public static qnrbf_xstudio_alloc_context_delegate qnrbf_xstudio_alloc_context_ptr = null;

        public static qnrbf_xstudio_free_context_delegate qnrbf_xstudio_free_context_ptr = null;

        public static qnrbf_xstudio_read_delegate qnrbf_xstudio_read_ptr = null;

        public static qnrbf_xstudio_write_delegate qnrbf_xstudio_write_ptr = null;
    }
}