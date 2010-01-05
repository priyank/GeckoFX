using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Skybound.Gecko
{
    #region detection over xpcom XUL not working - nsIXULAppInfo not implemented (need to call XRE_main)
    [Guid("a61ede2a-ef09-11d9-a5ce-001124787b2e"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface nsIXULAppInfo
    {
        /**
   * @see nsXREAppData.vendor
   * @returns an empty string if nsXREAppData.vendor is not set.
   */
        void GetVendor(nsAString aVendor);

        /**
   * @see nsXREAppData.name
   */
        void GetName(nsAString aName);

        /**
   * @see nsXREAppData.ID
   * @returns an empty string if nsXREAppData.ID is not set.
   */
        void GetID(nsAString aID);

        /**
   * The version of the XUL application. It is different than the
   * version of the XULRunner platform. Be careful about which one you want.
   *
   * @see nsXREAppData.version
   * @returns an empty string if nsXREAppData.version is not set.
   */
        void GetVersion(nsAString aVersion);

        /**
   * The build ID/date of the application. For xulrunner applications,
   * this will be different than the build ID of the platform. Be careful
   * about which one you want.
   */
        void GetAppBuildID(nsAString aAppBuildID);

        /**
   * The version of the XULRunner platform.
   */
        void GetPlatformVersion(nsAString aPlatformVersion);

        /**
   * The build ID/date of gecko and the XULRunner platform.
   */
        void GetPlatformBuildID(nsAString aPlatformBuildID);
    } ;

    public class XULAppInfo
    {
        private static nsIXULAppInfo _xulInfo;

        internal static void Initialize()
        {
        }

        static nsIXULAppInfo xulInfo
        {
            get
            {
                if (_xulInfo != null)
                    return _xulInfo;

                _xulInfo = (nsIXULAppInfo)Xpcom.GetService("@mozilla.org/xre/app-info;1"); ;
                return _xulInfo;
            }
        }

        internal static void Shutdown()
        {
            if (_xulInfo != null)
            {
                _xulInfo = null;
            }
        }

        public static string Vendor
        {
            get { return nsString.Get(xulInfo.GetVendor); }
        }

        public static string Version
        {
            get { return nsString.Get(xulInfo.GetVersion); }
        }

        public static string PlatformVersion
        {
            get { return nsString.Get(xulInfo.GetPlatformVersion); }
        }

        public static string PlatformBuildID
        {
            get { return nsString.Get(xulInfo.GetPlatformBuildID); }
        }

        public static string Name
        {
            get { return nsString.Get(xulInfo.GetName); }
        }

        public static string ID
        {
            get { return nsString.Get(xulInfo.GetID); }
        }

        public static string AppBuildID
        {
            get { return nsString.Get(xulInfo.GetAppBuildID); }
        }

        public static string FullDetails()
        {
            return String.Format(@"
Vendor: {0}
Name: {1}
ID: {2}
Version: {3}
AppBuildID: {4}
PlatformVersion: {5}
PlatformBuildID: {6}
            ", Vendor, Name, ID, Version, AppBuildID, PlatformVersion, PlatformBuildID);
        }
    }
    #endregion

    #region GeckoAppInfo and Discovery

    [Flags]
    public enum eGeckoReleaseStatus : long
    {
        Release = 0xFF0000,
        ReleaseCandidate = 0x80000,
        Beta = 0x40000,
        Alpha = 0x20000,
        NotPreRelease = 0x10000, // not set for pre-releases (like pre alpha 1)
    }

    // stupid detection over .ini files/dlls
    public class GeckoAppInfo
    {
        private long geckoVersion = 0, geckoReleaseStatus = (long)eGeckoReleaseStatus.Release;
        private string vendor = String.Empty,
                       version = String.Empty,
                       platformVersion = String.Empty,
                       platformBuildID = String.Empty,
                       name = String.Empty,
                       id = String.Empty,
                       appBuildId = String.Empty, 
                       geckoPath = String.Empty;

        // auto detect gecko version, location in application startup folder
        public GeckoAppInfo() : this(null)
        {
        }

        // auto detect gecko version
        public GeckoAppInfo(string path) : this (path, null)
        {
        }

        // will use gecko interfaces as of supplied version, strings auto detected
        public GeckoAppInfo(string path, long geckoVer)
            : this(path, GeckoVersionToString(geckoVer))
        {
            // override auto detected version
            geckoVersion = geckoVer;
        }

        // will try to detect exact gecko version, otherwise will use supplied
        public GeckoAppInfo(string path, string version) : this (path, version, null)
        {
        }

        // will try to detect exact gecko version, otherwise will use supplied
        public GeckoAppInfo(string path, string version, string appName)
        {
            geckoPath = (path ?? Application.StartupPath).Trim();
            name = appName ?? ((Path.GetFileName(geckoPath).Trim().Length > 0)?(Path.GetFileName(geckoPath)):(Path.GetFileName(Path.GetDirectoryName(geckoPath))));
            DetectGeckoVersion(version);
        }

        public static string GeckoVersionToString(long ver)
        {
            if (ver < Xpcom.DefaultGeckoVersion || ver >= 0x100000000) return null;

            return String.Format("{0}.{1}.{2}.{3}", ver >> 24, (ver >> 16) & 0xFF, (ver >> 8) & 0xFF, (ver & 0xFF));
        }

        public static Regex rxGeckoVer = new Regex(@"(?: (?<v>\d+) \.? ){3,4} (?: (?<r>[ab]) (?<rv>\d+)? )? (?<pre> pre)? ", 
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public void ParseGeckoVersion(string ver)
        {
            geckoVersion = Xpcom.DefaultGeckoVersion;
            geckoReleaseStatus = (long)eGeckoReleaseStatus.Release;

            if (ver == null || ver.Trim().Length == 0) return;

            Match m = rxGeckoVer.Match(ver);

            if (m.Groups["r"].Success)
            {
                switch (m.Groups["r"].Value.ToUpperInvariant())
                {
                    case "A":
                        geckoReleaseStatus = (long) (eGeckoReleaseStatus.Alpha | eGeckoReleaseStatus.NotPreRelease);
                        break;
                    case "B":
                        geckoReleaseStatus = (long) (eGeckoReleaseStatus.Beta | eGeckoReleaseStatus.NotPreRelease);
                        break;
                    default:
                        geckoReleaseStatus = (long) eGeckoReleaseStatus.Release;
                        break;
                }
                if (m.Groups["rv"].Success)
                    geckoReleaseStatus |= long.Parse(m.Groups["rv"].Value);
            }
            if (m.Groups["pre"].Success)
                geckoReleaseStatus &= ~((long) eGeckoReleaseStatus.NotPreRelease);

            var vc = m.Groups["v"].Captures;
            if (vc.Count < 3) return;
            long vl = 0;
            for (int i = 0; i < 4; i++)
            {
                vl <<= 8;
                if (i >= vc.Count || vc[i].Value.Trim().Length == 0) continue;
                int c = 0;
                if (!int.TryParse(vc[i].Value, out c)) continue;
                if (c < 0 || c > 0xFF) c = 0;
                vl += c;
            }
            if (vl < Xpcom.DefaultGeckoVersion || vl >= 0x100000000) vl = Xpcom.DefaultGeckoVersion;
            geckoVersion = vl;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);

        static string GetIniString(string iniFile, string section, string value, string defValue)
        {
            StringBuilder sb = new StringBuilder(512);
            if (GetPrivateProfileString(section, value, defValue, sb, (uint) sb.Capacity, iniFile) <= 0)
                return null;
            return sb.ToString();
        }

        private void DetectGeckoVersion(string versionHint)
        {
            ParseGeckoVersion(versionHint);
            platformVersion = versionHint ?? GeckoVersionToString(geckoVersion);

            // xpcom and xul should be present anyway
            if (!IsGeckoValid) return;

            string xpcom = Path.Combine(geckoPath, "xpcom.dll");
            string xul = Path.Combine(geckoPath, "xul.dll");
            string pini = Path.Combine(geckoPath, "platform.ini");
            string aini = Path.Combine(geckoPath, "application.ini");

            // try to detect by platform.ini file
            if (File.Exists(pini))
            {
                platformBuildID = GetIniString(pini, "Build", "BuildID", null) ?? PlatformBuildId;
                string ver = GetIniString(pini, "Build", "Milestone", null);
                if (ver != null) ParseGeckoVersion(platformVersion = ver);
            }
            // platform ini not found, or not valid, try to detect by dll versions
            if (geckoVersion == Xpcom.DefaultGeckoVersion)
            {
                try
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(xul); ;
                    if (!IsValidGeckoFVI(fvi)) fvi = FileVersionInfo.GetVersionInfo(xpcom);
                    if (IsValidGeckoFVI(fvi))
                    {
                        int vb = (fvi.FileBuildPart >= 0 && fvi.FileBuildPart < 0xFF) ? fvi.FileBuildPart : 0;
                        int vp = (fvi.FilePrivatePart >= 0 && fvi.FilePrivatePart < 0xFF) ? fvi.FilePrivatePart : 0;
                        geckoVersion = (fvi.FileMajorPart * 0x1000000) + (fvi.FileMinorPart * 0x10000) + (vb * 0x100) + vp;
                        if (versionHint == null || versionHint.Trim().Length == 0)
                            platformVersion = GeckoVersionToString(geckoVersion);
                    }
                }
                catch { }
            }

            // try to detect application.ini strings
            if (File.Exists(aini))
            {
                vendor = GetIniString(aini, "App", "Vendor", null) ?? vendor;
                name = GetIniString(aini, "App", "Name", null) ?? name;
                version = GetIniString(aini, "App", "Version", null) ?? version;
                appBuildId = GetIniString(aini, "App", "BuildID", null) ?? appBuildId;
                id = GetIniString(aini, "App", "ID", null) ?? id;
            }
        }

        public long GeckoVersion
        {
            get { return geckoVersion; }
        }

        public long GeckoReleaseStatus
        {
            get { return geckoReleaseStatus; }
        }

        public string Vendor
        {
            get { return vendor; }
        }

        public string Version
        {
            get { return version; }
        }

        public string PlatformVersion
        {
            get { return platformVersion; }
        }

        public string PlatformBuildId
        {
            get { return platformBuildID; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Id
        {
            get { return id; }
        }

        public string AppBuildId
        {
            get { return appBuildId; }
        }

        public string GeckoPath
        {
            get { return geckoPath; }
        }

        public bool IsAutoDiscovered { get; internal set; }
        
        public bool IsGeckoValid
        {
            get
            {
                if (geckoPath == null || geckoPath.Trim().Length == 0) return false;
                string xpcom = Path.Combine(geckoPath, "xpcom.dll");
                string xul = Path.Combine(geckoPath, "xul.dll");
                return File.Exists(xpcom) && File.Exists(xul);
            }
        }

        public bool IsVersionSupported
        {
            get
            {
                return geckoVersion >= Xpcom.CompatibleGeckoVersionMin &&
                       geckoVersion <= Xpcom.CompatibleGeckoVersionMax;
            }
        }

        public static bool IsValidGeckoFVI(FileVersionInfo fvi)
        {
            return fvi != null && ((fvi.FileMajorPart == 1 && fvi.FileMinorPart >= 8) || (fvi.FileMajorPart > 1)) &&
                   fvi.FileMajorPart < 0xFF && fvi.FileMinorPart < 0xFF;
        }


        public string FullDetails()
        {
            return String.Format(@"{0} {1} {3} (built {4})
Gecko {5} (built {6})

ID: {2}
Located: {7}", Vendor, Name, Id, Version, AppBuildId, PlatformVersion, PlatformBuildId, GeckoPath);
        }

        public string ToBrowserTitle()
        {
            return String.Format("Gecko {0} / {1}{2}{3}{4}", platformVersion, Vendor, (Vendor.Length > 0 ? " " : ""), Name, (version.Length > 0 ? (String.Format(" ({0})", (version))) : ""));
        }

        public override string ToString()
        {
            return ToBrowserTitle();
        }
    }

    public class RegistryKeyChain : IDisposable
    {
        private RegistryKey rk;
        private RegistryKeyChain parent;

        public RegistryKeyChain(RegistryKey rk)
        {
            this.rk = rk;
        }

        private RegistryKeyChain(RegistryKey key, RegistryKeyChain chain)
        {
            rk = key;
            parent = chain;
        }

        public RegistryKeyChain Parent
        {
            get { return parent; }
        }

        public bool Is(string name)
        {
            return String.Compare(name, Path.GetFileName(rk.Name), true) == 0;
        }

        public string Value(string name)
        {
            return rk.GetValue(name, null) as string;
        }

        public string[] Subkeys
        {
            get
            {
                return rk.GetSubKeyNames();
            }
        }

        public string Name
        {
            get
            {
                return Path.GetFileName(rk.Name);
            }
        }

        public string RootName
        {
            get
            {
                RegistryKeyChain rkc = this;
                while (rkc.Parent != null && rkc.Parent.Parent != null) rkc = rkc.Parent;
                return rkc.Name;
            }
        }

        public RegistryKeyChain Subkey(string name)
        {
            RegistryKey rsk = rk.OpenSubKey(name, false);
            if (rsk != null)
                return new RegistryKeyChain(rsk, this);
            return null;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (rk != null)
                rk.Close();
            rk = null;
        }

        #endregion
    }

    public class GeckoAppDiscovery
    {
        private List<GeckoAppInfo> geckos = new List<GeckoAppInfo>();

        public GeckoAppDiscovery(params string [] additionalPaths)
        {
            DiscoverGeckosInRegistry();
            Array.ForEach(additionalPaths, s => AddGeckoPathImpl(s));
            SortGeckos();
        }

        public GeckoAppInfo AddGeckoPath(string path)
        {
            GeckoAppInfo gai = AddGeckoPathImpl(path);
            SortGeckos();
            return gai;
        }

        GeckoAppInfo AddGeckoPathImpl(string path)
        {
            path = Path.GetFullPath(path.Trim());
            foreach (var cgai in geckos)
                if (String.Compare(cgai.GeckoPath, path, true) == 0)
                    return cgai;
            GeckoAppInfo gai = new GeckoAppInfo(path);
            geckos.Add(gai);
            return gai;
        }

        static int GeckosComparer(GeckoAppInfo gai1, GeckoAppInfo gai2)
        {
            int d = -gai1.IsGeckoValid.CompareTo(gai2.IsGeckoValid);
            if (d != 0) return d;
            d = -gai1.IsVersionSupported.CompareTo(gai2.IsVersionSupported);
            if (d != 0) return d;
            d = -gai1.GeckoReleaseStatus.CompareTo(gai2.GeckoReleaseStatus);
            if (d != 0) return d;
            d = -gai1.GeckoVersion.CompareTo(gai2.GeckoVersion);
            return d;
        }

        void SortGeckos()
        {
            // sort geckos
            geckos.Sort(GeckosComparer);
        }

        private void DiscoverGeckosInRegistry()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"Software\Mozilla", false);
                if (rk != null)
                {
                    using (var rkc = new RegistryKeyChain(rk))
                        WalkRegistryKey(rkc);
                    rk.Close();
                }
            } catch {}

            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Mozilla", false);
                if (rk != null)
                {
                    using (var rkc = new RegistryKeyChain(rk))
                        WalkRegistryKey(rkc);
                    rk.Close();
                }
            }
            catch { }
        }

        bool IsGeckoPathKnown(string path)
        {
            path = Path.GetFullPath(path.Trim());
            // have we got same path already?
            foreach (var gai in geckos)
                if (String.Compare(gai.GeckoPath, path, true) == 0)
                    return true;
            return false;
        }

        private void WalkRegistryKey(RegistryKeyChain key)
        {
            try
            {
                string path = null, version = null, appname = null;
                path = key.Value("Install Directory");
                if (path == null)
                {
                    string exe = key.Value("PathToExe");
                    if (exe != null)
                        path = Path.GetDirectoryName(exe.Trim());
                }
                if (path != null) path = Path.GetFullPath(path.Trim());
                if (path != null && IsGeckoPathKnown(path))
                    path = null;
                // try to detect gecko version from this or parent key
                if (path != null)
                {
                    version = key.Value("GeckoVer");
                    appname = key.RootName;

                    if (version == null && key.Is("bin") && key.Parent != null)
                    {
                        version = key.Parent.Value("GeckoVer");
                        appname = key.Parent.Name;
                    }
                    if (version == null && key.Is("Main") && key.Parent != null && key.Parent.Parent != null)
                    {
                        version = key.Parent.Parent.Value(null);
                        appname = String.Format("{0} {1}", key.Parent.Parent.Name, key.Parent.Name);
                    }
                }
                if (path != null)
                {
                    GeckoAppInfo gai = new GeckoAppInfo(path, version, appname) { IsAutoDiscovered = true };
                    geckos.Add(gai);
                }
            } catch {}

            string[] skeys = key.Subkeys;
            Array.ForEach(skeys, sk =>
            {
                try
                {
                    using (RegistryKeyChain rsk = key.Subkey(sk))
                    {
                        if (rsk != null)
                            WalkRegistryKey(rsk);
                    }
                }
                catch
                {
                }
            });
        }

        public List<GeckoAppInfo> Geckos
        {
            get { return geckos; }
        }

        public IEnumerable<GeckoAppInfo> ValidGeckos
        {
            get
            {
                foreach (var gai in Geckos)
                    if (gai.IsGeckoValid)
                        yield return gai;
            }
        }

        public GeckoAppInfo GetValidGeckoAt(string path)
        {
            if (path == null) return null;
            foreach (GeckoAppInfo gai in geckos)
                if (String.Compare(gai.GeckoPath, path, true) == 0 && gai.IsGeckoValid)
                    return gai;
            return null;
        }
    }

    #endregion

    #region multimarshaler and multiversion

    abstract class geckomultiproxy : RealProxy
    {
        protected geckomultiproxy(Type type) : base(type)
        {
        }

        protected geckomultiproxy()
        {
        }

        public object OriginalObject
        {
            get
            {
                return GetUnwrappedServer();
            }
        }
    }

    class multimarshaler : geckomultiproxy, ICustomMarshaler
    {
        #region Implementation of ICustomMarshaler

        private Type it, at;
        private MarshalByRefObject obj;
        private static Dictionary<string, multimarshaler> marshallers = new Dictionary<string, multimarshaler>();

        private multimarshaler(Type it)
        {
            this.it = it;
            at = GetActualType(it);
        }
        
        private multimarshaler(Type it, Type at, MarshalByRefObject obj) : base(it)
        {
            this.it = it;
            this.at = at;
            this.obj = obj;
        }

        public static ICustomMarshaler GetInstance(string cookie)
        {
            multimarshaler mm;
            if (!marshallers.TryGetValue(cookie, out mm))
                marshallers.Add(cookie, mm = new multimarshaler(Type.GetType(cookie)));
            return mm;
        }

        object ICustomMarshaler.MarshalNativeToManaged(IntPtr pNativeData)
        {
            object o = Marshal.GetTypedObjectForIUnknown(pNativeData, at);
            return (new multimarshaler(it, at, o as MarshalByRefObject)).GetTransparentProxy();
        }

        IntPtr ICustomMarshaler.MarshalManagedToNative(object ManagedObj)
        {
            // try to get real proxy (if the object is proxified) by multi marshaller or multiversion
            try
            {
                RealProxy rp = RemotingServices.GetRealProxy(ManagedObj);
                geckomultiproxy gmp = rp as geckomultiproxy;
                if (gmp != null)
                {
                    object o = gmp.OriginalObject;
                    if (o != null)
                        return Marshal.GetIUnknownForObject(o);
                }
            } catch (Exception)
            {
            }

            // it seems object is not marshalled/version - must be original .NET object we want to marshal to COM            
            object comSimulatedObject = (new multimarshaler(at, it, ManagedObj as MarshalByRefObject)).GetTransparentProxy();
            IntPtr iunk = Marshal.GetIUnknownForObject(comSimulatedObject);
            return iunk;
        }

        void ICustomMarshaler.CleanUpNativeData(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return;

            Marshal.Release(pNativeData);
        }

        void ICustomMarshaler.CleanUpManagedData(object ManagedObj)
        {
            object o = ((multimarshaler)RemotingServices.GetRealProxy(ManagedObj)).obj;
            Marshal.ReleaseComObject(o);
        }

        int ICustomMarshaler.GetNativeDataSize()
        {
            return -1;
        }

        #endregion

        private static Dictionary<Type, Type> actualTypes = new Dictionary<Type, Type>();
        public static Type GetActualType(Type outerT)
        {
            Type actualType = null;
            if (!actualTypes.TryGetValue(outerT, out actualType))
            {
                Type realT = outerT;
                long ver = Xpcom.GeckoVersion;
                xpcVersionAttribute[] vas =
                    outerT.GetCustomAttributes(typeof (xpcVersionAttribute), false) as xpcVersionAttribute[];
                xpcVersionAttribute va = (vas != null && vas.Length > 0) ? vas[0] : null;
                if (va != null && va.VersionsAvailable.Length > 0)
                {
                    // try to find nearest available version
                    long nearest = 0;
                    Array.ForEach(va.VersionsAvailable, v =>
                    {
                        if (v > nearest && v <= ver)
                            nearest = v;
                    });
                    if (nearest >= Xpcom.DefaultGeckoVersion)
                        realT = Type.GetType(String.Format("{0}_{1:X8}", outerT.FullName, nearest));
                }
                actualTypes[outerT] = actualType = realT;
            }
            return actualType;
        }

        #region Overrides of RealProxy

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage imcm = msg as IMethodCallMessage;

            IMethodReturnMessage imrm = RemotingServices.ExecuteMessage(obj, new OwnMethodCallMessage(imcm, at));
            return imrm;
        }

        #endregion
    }

    class multiversion<t> : geckomultiproxy
    {
        private MarshalByRefObject obj;
        private Type ownT;
        //private static Dictionary<Type, Type> actualTypes = new Dictionary<Type, Type>();

        private multiversion(object obj, Type ownT)
            : base(typeof(t))
        {
            this.obj = obj as MarshalByRefObject;
            this.ownT = ownT;
        }

        public multiversion(object obj)
            : this(obj, ActualType)
        {
        }

        public static implicit operator t(multiversion<t> mvo)
        {
            return (t)mvo.GetTransparentProxy();
        }

        public static implicit operator multiversion<t>(t o)
        {
            return RemotingServices.GetRealProxy(o) as multiversion<t>;
        }

        public static implicit operator multiversion<t>(MarshalByRefObject o)
        {
            return RemotingServices.GetRealProxy(o) as multiversion<t>;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage imcm = msg as IMethodCallMessage;

            IMethodReturnMessage imrm = RemotingServices.ExecuteMessage(obj, new OwnMethodCallMessage(imcm, ownT));
            return imrm;
        }

        public static t Cast(object o)
        {
            Type at = ActualType;
            if (at == typeof(t))
                return (t)o;
            return (t)new multiversion<t>(o, at).GetTransparentProxy();
        }

        public static object Unwrap(t o)
        {
            multiversion<t> mvp = RemotingServices.GetRealProxy(o) as multiversion<t>;
            if (mvp == null) return null;
            return mvp.GetUnwrappedServer();
        }

        public static t Create(params object[] args)
        {
            Type rt = typeof(t);
            object o = rt.GetConstructor(Type.GetTypeArray(args ?? new object[] { })).Invoke(args);
            return Cast(o);
        }

        public static int MarshalledSize
        {
            get
            {
                return Marshal.SizeOf(ActualType);
            }
        }

        public static IntPtr StructureToPtr(t o)
        {
            object obj = Unwrap(o);
            IntPtr framePtr = Marshal.AllocHGlobal(MarshalledSize);
            Marshal.StructureToPtr(obj, framePtr, true);
            return framePtr;
        }

        public static Type ActualType
        {
            get
            {
                return multimarshaler.GetActualType(typeof(t));
            }
        }
    }

    public class versioned<t> : Dictionary<long, t>
    {
        t def;
        public versioned(t def, params verb<t> [] values)
        {
            this.def = def;
            Array.ForEach(values, vb => Add(vb.Version, vb.Value));
        }

        public t Value
        {
            get
            {
                // try to find nearest available version
                long nearest = 0;
                long ver = Xpcom.GeckoVersion;
                foreach (long v in Keys)
                    if (v > nearest && v <= ver)
                        nearest = v;
                if (nearest >= Xpcom.DefaultGeckoVersion)
                    return this[nearest];
                return def;
            }
        }
    }

    public class verb<t>
    {
        private long version;
        private t value;

        public verb(long v, t d)
        {
            version = v;
            value = d;
        }

        public long Version
        {
            get { return version; }
        }

        public t Value
        {
            get { return value; }
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class xpcVersionAttribute : Attribute
    {
        private long[] versionsAvailable;

        public xpcVersionAttribute(params long[] versionsAvailable)
        {
            this.versionsAvailable = versionsAvailable;
        }

        public long[] VersionsAvailable
        {
            get { return versionsAvailable; }
        }
    }

    public class OwnMethodCallMessage : IMethodCallMessage
    {
        private IMethodCallMessage orig;
        private Type otherType;
        private Hashtable props;
            
        public OwnMethodCallMessage(IMethodCallMessage orig, Type t)
        {
            this.orig = orig;
            otherType = t;
            props = new Hashtable(orig.Properties);
            foreach (DictionaryEntry de in orig.Properties)
                if (de.Value != null && de.Value.ToString() == orig.TypeName)
                    props[de.Key] = TypeName;
        }

        public IDictionary Properties
        {
            get
            {
                return props;
            }
        }

        public string GetArgName(int index)
        {
            return orig.GetArgName(index);
        }

        public object GetArg(int argNum)
        {
            return orig.GetArg(argNum);
        }

        public string Uri
        {
            get { return orig.Uri; }
        }

        public string MethodName
        {
            get { return orig.MethodName; }
        }

        public string TypeName
        {
            get { return otherType.FullName; }
        }

        public object MethodSignature
        {
            get { return orig.MethodSignature; }
        }

        public int ArgCount
        {
            get { return orig.ArgCount; }
        }

        public object[] Args
        {
            get { return orig.Args; }
        }

        public bool HasVarArgs
        {
            get { return orig.HasVarArgs; }
        }

        public LogicalCallContext LogicalCallContext
        {
            get { return orig.LogicalCallContext; }
        }

        public MethodBase MethodBase
        {
            get { return otherType.GetMethod(MethodName, MethodSignature as Type[]); }
        }

        public string GetInArgName(int index)
        {
            return orig.GetInArgName(index);
        }

        public object GetInArg(int argNum)
        {
            return orig.GetInArg(argNum);
        }

        public int InArgCount
        {
            get { return orig.InArgCount; }
        }

        public object[] InArgs
        {
            get { return orig.InArgs; }
        }
    }
    #endregion
}
