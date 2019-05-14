using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SteamID
{
    public class SteamBridge : IDisposable
    {
        private readonly IntPtr _handle;

        private readonly IntPtr _steamClientVirtualTable;

        private bool _isDisposed;

        public SteamBridge()
        {
            var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "") as string;

            if (string.IsNullOrEmpty(steamPath))
                throw new InvalidOperationException("Unable to locate the steam folder");

            SetDllDirectory(steamPath);

            _handle = LoadLibraryEx(Environment.Is64BitProcess ? "steamclient64.dll" : "steamclient.dll", IntPtr.Zero,
                8);

            SetDllDirectory(null);

            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("Unable to load steamclient.dll");

            _steamClientVirtualTable = GetSteamClientVirtualTableAddress();

            if (_steamClientVirtualTable == IntPtr.Zero)
                throw new InvalidOperationException("Unable to get the address of ISteamClient012");
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_handle != IntPtr.Zero)
                FreeLibrary(_handle);

            _isDisposed = true;
        }

        ~SteamBridge()
        {
            Dispose();
        }


        private IntPtr CreateInterface(string version)
        {
            var address = GetProcAddress(_handle, "CreateInterface");

            if (address == IntPtr.Zero)
                throw new InvalidOperationException("CreateInterface not found in steamclient.dll");

            var createInterface = (CreateInterfaceFn) Marshal.GetDelegateForFunctionPointer(
                address, typeof(CreateInterfaceFn));

            return createInterface(version, IntPtr.Zero);
        }

        private IntPtr GetSteamClientVirtualTableAddress()
        {
            var address = CreateInterface("SteamClient012");

            return Marshal.ReadIntPtr(address);
        }

        private int CreateSteamPipe()
        {
            var createSteamPipe = (CreateSteamPipeFn) Marshal.GetDelegateForFunctionPointer(
                Marshal.ReadIntPtr(_steamClientVirtualTable, 0 * IntPtr.Size), typeof(CreateSteamPipeFn));

            return createSteamPipe(_steamClientVirtualTable);
        }

        private bool ReleaseSteamPipe(int hSteamPipe)
        {
            var releaseSteamPipe = (ReleaseSteamPipeFn) Marshal.GetDelegateForFunctionPointer(
                Marshal.ReadIntPtr(_steamClientVirtualTable, 1 * IntPtr.Size), typeof(ReleaseSteamPipeFn));

            return releaseSteamPipe(_steamClientVirtualTable, hSteamPipe);
        }

        private int ConnectToGlobalUser(int hSteamPipe)
        {
            var connectToGlobalUser = (ConnectToGlobalUserFn) Marshal.GetDelegateForFunctionPointer(
                Marshal.ReadIntPtr(_steamClientVirtualTable, 2 * IntPtr.Size), typeof(ConnectToGlobalUserFn));

            return connectToGlobalUser(_steamClientVirtualTable, hSteamPipe);
        }

        private void ReleaseUser(int hSteamPipe, int hSteamUser)
        {
            var releaseUser = (ReleaseUserFn) Marshal.GetDelegateForFunctionPointer(
                Marshal.ReadIntPtr(_steamClientVirtualTable, 4 * IntPtr.Size), typeof(ReleaseUserFn));

            releaseUser(_steamClientVirtualTable, hSteamPipe, hSteamUser);
        }

        public ulong GetSteamId()
        {
            var hSteamPipe = CreateSteamPipe();
            var hSteamUser = ConnectToGlobalUser(hSteamPipe);

            var getSteamUser = (GetSteamUserFn) Marshal.GetDelegateForFunctionPointer(
                Marshal.ReadIntPtr(_steamClientVirtualTable, 5 * IntPtr.Size), typeof(GetSteamUserFn));

            var steamUserAddress = getSteamUser(_steamClientVirtualTable, hSteamUser, hSteamPipe, "SteamUser012");

            var steamUserVirtualTableAddress = Marshal.ReadIntPtr(steamUserAddress);

            var getSteamId = (GetSteamIdFn) Marshal.GetDelegateForFunctionPointer(
                Marshal.ReadIntPtr(steamUserVirtualTableAddress, 2 * IntPtr.Size), typeof(GetSteamIdFn));

            ulong id = 0;

            getSteamId(steamUserAddress, ref id);

            ReleaseUser(hSteamPipe, hSteamUser);
            ReleaseSteamPipe(hSteamPipe);

            return id;
        }

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate IntPtr CreateInterfaceFn(string version, IntPtr returnCode);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal delegate int CreateSteamPipeFn(IntPtr thisA);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal delegate bool ReleaseSteamPipeFn(IntPtr thisA, int hSteamPipe);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal delegate int ConnectToGlobalUserFn(IntPtr thisA, int hSteamPipe);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal delegate void ReleaseUserFn(IntPtr thisA, int hSteamPipe, int hUser);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal delegate IntPtr GetSteamUserFn(IntPtr thisA, int hUser, int hSteamPipe, string pchVersion);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal delegate void GetSteamIdFn(IntPtr thisA, ref ulong steamId);

        #endregion

        #region Native

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        #endregion
    }
}