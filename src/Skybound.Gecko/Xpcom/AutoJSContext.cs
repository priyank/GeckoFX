#region ***** BEGIN LICENSE BLOCK *****
/* Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Skybound Software code.
 *
 * The Initial Developer of the Original Code is Skybound Software.
 * Portions created by the Initial Developer are Copyright (C) 2008-2009
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 */
#endregion END LICENSE BLOCK

using System;
using System.Runtime.InteropServices;

namespace Skybound.Gecko
{
	/// <summary>
	/// Creates a scoped, fake "system principal" security context.  This class is used primarly to work around bugs in gecko
	/// which prevent methods on nsIDOMCSSStyleSheet from working outside of javascript.
	/// </summary>
	public class AutoJSContext : IDisposable
	{
		#region Unmanaged Interfaces
		
		[Guid("c67d8270-3189-11d3-9885-006008962422"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface nsIJSContextStack
		{
			int GetCount();
			IntPtr Peek();
			IntPtr Pop();
			void Push(IntPtr cx);
		}

        [xpcVersion(0x1080000, 0x1090000, 0x1090100, 0x1090100)]
        interface nsIScriptSecurityManager
        {
            // nsIXPCSecurityManager:
            void CanCreateWrapper(out IntPtr aJSContext, ref Guid aIID, nsISupports aObj, IntPtr aClassInfo, IntPtr aPolicy); // aClassInfo=nsIClassInfo
            void CanCreateInstance(out IntPtr aJSContext, ref Guid aCID);
            void CanGetService(out IntPtr aJSContext, ref Guid aCID);
            void CanAccess(uint aAction, IntPtr aCallContext, out IntPtr aJSContext, out IntPtr aJSObject, nsISupports aObj, IntPtr aClassInfo, IntPtr aName, IntPtr aPolicy); // aCallContext=nsIXPCNativeCallContext

            void CheckLoadURIFromScript(out IntPtr cx, nsIURI uri);
            void CheckLoadURIWithPrincipal(MarshalByRefObject aPrincipal, nsIURI uri, uint flags);
            void CheckLoadURI(nsIURI from, nsIURI uri, uint flags);
            void CheckLoadURIStr(nsACString from, nsACString uri, uint flags);
            void CheckFunctionAccess(out IntPtr cx, out IntPtr funObj, IntPtr targetObj);
            bool CanExecuteScripts(out IntPtr cx, MarshalByRefObject principal);
            MarshalByRefObject GetSubjectPrincipal();
            MarshalByRefObject GetSystemPrincipal();
            MarshalByRefObject GetCertificatePrincipal(nsACString aCertFingerprint, nsACString aSubjectName, nsACString aPrettyName, nsISupports aCert, nsIURI aURI);
            MarshalByRefObject GetCodebasePrincipal(nsIURI aURI);
            void EnableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
            void RevertCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
            void DisableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
            bool SubjectPrincipalIsSystem();
            void CheckSameOrigin(out IntPtr aJSContext, nsIURI aTargetURI);
        }

		[Guid("f4d74511-2b2d-4a14-a3e4-a392ac5ac3ff"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface nsIScriptSecurityManager_01080000
        {
            // nsIXPCSecurityManager:
            void CanCreateWrapper(out IntPtr aJSContext, ref Guid aIID, nsISupports aObj, IntPtr aClassInfo, IntPtr aPolicy); // aClassInfo=nsIClassInfo
            void CanCreateInstance(out IntPtr aJSContext, ref Guid aCID);
            void CanGetService(out IntPtr aJSContext, ref Guid aCID);
            void CanAccess(uint aAction, IntPtr aCallContext, out IntPtr aJSContext, out IntPtr aJSObject, nsISupports aObj, IntPtr aClassInfo, IntPtr aName, IntPtr aPolicy); // aCallContext=nsIXPCNativeCallContext

			// nsIScriptSecurityManager:
			void CheckPropertyAccess(out IntPtr aJSContext, out IntPtr aJSObject, [MarshalAs(UnmanagedType.LPStr)] out string aClassName, IntPtr aProperty, uint aAction);
			void CheckConnect(out IntPtr aJSContext, nsIURI aTargetURI, [MarshalAs(UnmanagedType.LPStr)] out string aClassName, [MarshalAs(UnmanagedType.LPStr)] string aProperty);
			void CheckLoadURIFromScript(out IntPtr cx, nsIURI uri);
            void CheckLoadURIWithPrincipal(MarshalByRefObject aPrincipal, nsIURI uri, uint flags);
			void CheckLoadURI(nsIURI from, nsIURI uri, uint flags);
			void CheckLoadURIStr(nsACString from, nsACString uri, uint flags);
			void CheckFunctionAccess(out IntPtr cx, out IntPtr funObj, IntPtr targetObj);
            bool CanExecuteScripts(out IntPtr cx, MarshalByRefObject principal);
            MarshalByRefObject GetSubjectPrincipal();
            MarshalByRefObject GetSystemPrincipal();
            MarshalByRefObject GetCertificatePrincipal(nsACString aCertFingerprint, nsACString aSubjectName, nsACString aPrettyName, nsISupports aCert, nsIURI aURI);
            MarshalByRefObject GetCodebasePrincipal(nsIURI aURI);
			short RequestCapability(MarshalByRefObject principal, [MarshalAs(UnmanagedType.LPStr)] out string capability);
			bool IsCapabilityEnabled([MarshalAs(UnmanagedType.LPStr)] out string capability);
			void EnableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void RevertCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void DisableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void SetCanEnableCapability(nsACString certificateFingerprint, [MarshalAs(UnmanagedType.LPStr)] out string capability, short canEnable);
			MarshalByRefObject GetObjectPrincipal(out IntPtr aJSContext, out IntPtr aJSObject);
			bool SubjectPrincipalIsSystem();
			void CheckSameOrigin(out IntPtr aJSContext, nsIURI aTargetURI);
			void CheckSameOriginURI(nsIURI aSourceURI, nsIURI aTargetURI);
			void CheckSameOriginPrincipal(MarshalByRefObject aSourcePrincipal, MarshalByRefObject aTargetPrincipal);
			MarshalByRefObject GetPrincipalFromContext(out IntPtr aJSContext);
			bool SecurityCompareURIs(nsIURI aSubjectURI, nsIURI aObjectURI);
        }
		
		[Guid("3fffd8e8-3fea-442e-a0ed-2ba81ae197d5"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface nsIScriptSecurityManager_01090000
		{
			// nsIXPCSecurityManager:
			void CanCreateWrapper(out IntPtr aJSContext, ref Guid aIID, nsISupports aObj, IntPtr aClassInfo, IntPtr aPolicy); // aClassInfo=nsIClassInfo
			void CanCreateInstance(out IntPtr aJSContext, ref Guid aCID);
			void CanGetService(out IntPtr aJSContext, ref Guid aCID);
			void CanAccess(uint aAction, IntPtr aCallContext, out IntPtr aJSContext, out IntPtr aJSObject, nsISupports aObj, IntPtr aClassInfo, IntPtr aName, IntPtr aPolicy); // aCallContext=nsIXPCNativeCallContext
			
			// nsIScriptSecurityManager:
			void CheckPropertyAccess(out IntPtr aJSContext, out IntPtr aJSObject, [MarshalAs(UnmanagedType.LPStr)] string aClassName, IntPtr aProperty, uint aAction); // aProperty=jsval
			void CheckConnect(out IntPtr aJSContext, nsIURI aTargetURI, [MarshalAs(UnmanagedType.LPStr)] string aClassName, [MarshalAs(UnmanagedType.LPStr)] string aProperty);
			void CheckLoadURIFromScript(out IntPtr cx, nsIURI uri);
			void CheckLoadURIWithPrincipal(MarshalByRefObject aPrincipal, nsIURI uri, uint flags);
			void CheckLoadURI(nsIURI from, nsIURI uri, uint flags);
			void CheckLoadURIStrWithPrincipal(MarshalByRefObject aPrincipal, nsACString uri, uint flags);
			void CheckLoadURIStr(nsACString from, nsACString uri, uint flags);
			void CheckFunctionAccess(out IntPtr cx, out IntPtr funObj, IntPtr targetObj);
			bool CanExecuteScripts(out IntPtr cx, MarshalByRefObject principal);
			MarshalByRefObject GetSubjectPrincipal();
			MarshalByRefObject GetSystemPrincipal();
			MarshalByRefObject GetCertificatePrincipal(nsACString aCertFingerprint, nsACString aSubjectName, nsACString aPrettyName, nsISupports aCert, nsIURI aURI);
			MarshalByRefObject GetCodebasePrincipal(nsIURI aURI);
			short RequestCapability(MarshalByRefObject principal, [MarshalAs(UnmanagedType.LPStr)] string capability);
			bool IsCapabilityEnabled([MarshalAs(UnmanagedType.LPStr)] string capability);
			void EnableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void RevertCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void DisableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void SetCanEnableCapability(nsACString certificateFingerprint, [MarshalAs(UnmanagedType.LPStr)] string capability, short canEnable);
			MarshalByRefObject GetObjectPrincipal(out IntPtr cx, out IntPtr aJSObject);
			bool SubjectPrincipalIsSystem();
			void CheckSameOrigin(out IntPtr aJSContext, nsIURI aTargetURI);
			void CheckSameOriginURI(nsIURI aSourceURI, nsIURI aTargetURI, bool reportError);
			MarshalByRefObject GetPrincipalFromContext(out IntPtr cx);
			MarshalByRefObject GetChannelPrincipal(IntPtr aChannel); // nsIChannel
			bool IsSystemPrincipal(MarshalByRefObject aPrincipal);
			MarshalByRefObject GetCxSubjectPrincipal(IntPtr cx); // JSContext
		}

		[Guid("f8e350b9-9f31-451a-8c8f-d10fea26b780"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface nsIScriptSecurityManager_01090100
		{
			// nsIXPCSecurityManager:
			void CanCreateWrapper(out IntPtr aJSContext, ref Guid aIID, nsISupports aObj, IntPtr aClassInfo, IntPtr aPolicy); // aClassInfo=nsIClassInfo
			void CanCreateInstance(out IntPtr aJSContext, ref Guid aCID);
			void CanGetService(out IntPtr aJSContext, ref Guid aCID);
			void CanAccess(uint aAction, IntPtr aCallContext, out IntPtr aJSContext, out IntPtr aJSObject, nsISupports aObj, IntPtr aClassInfo, IntPtr aName, IntPtr aPolicy); // aCallContext=nsIXPCNativeCallContext
			
			// nsIScriptSecurityManager:
			void CheckPropertyAccess(out IntPtr aJSContext, out IntPtr aJSObject, [MarshalAs(UnmanagedType.LPStr)] string aClassName, IntPtr aProperty, uint aAction); // aProperty=jsval
			void CheckConnect(out IntPtr aJSContext, nsIURI aTargetURI, [MarshalAs(UnmanagedType.LPStr)] string aClassName, [MarshalAs(UnmanagedType.LPStr)] string aProperty);
			void CheckLoadURIFromScript(out IntPtr cx, nsIURI uri);
			void CheckLoadURIWithPrincipal(MarshalByRefObject aPrincipal, nsIURI uri, uint flags);
			void CheckLoadURI(nsIURI from, nsIURI uri, uint flags);
			void CheckLoadURIStrWithPrincipal(MarshalByRefObject aPrincipal, nsACString uri, uint flags);
			void CheckLoadURIStr(nsACString from, nsACString uri, uint flags);
			void CheckFunctionAccess(out IntPtr cx, out IntPtr funObj, IntPtr targetObj);
			bool CanExecuteScripts(out IntPtr cx, MarshalByRefObject principal);
			MarshalByRefObject GetSubjectPrincipal();
			MarshalByRefObject GetSystemPrincipal();
			MarshalByRefObject GetCertificatePrincipal(nsACString aCertFingerprint, nsACString aSubjectName, nsACString aPrettyName, nsISupports aCert, nsIURI aURI);
			MarshalByRefObject GetCodebasePrincipal(nsIURI aURI);
			short RequestCapability(MarshalByRefObject principal, [MarshalAs(UnmanagedType.LPStr)] string capability);
			bool IsCapabilityEnabled([MarshalAs(UnmanagedType.LPStr)] string capability);
			void EnableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void RevertCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void DisableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
			void SetCanEnableCapability(nsACString certificateFingerprint, [MarshalAs(UnmanagedType.LPStr)] string capability, short canEnable);
			MarshalByRefObject GetObjectPrincipal(out IntPtr cx, out IntPtr aJSObject);
			bool SubjectPrincipalIsSystem();
			void CheckSameOrigin(out IntPtr aJSContext, nsIURI aTargetURI);
			void CheckSameOriginURI(nsIURI aSourceURI, nsIURI aTargetURI, bool reportError);
			MarshalByRefObject GetPrincipalFromContext(out IntPtr cx);
			MarshalByRefObject GetChannelPrincipal(IntPtr aChannel); // nsIChannel
			bool IsSystemPrincipal(MarshalByRefObject aPrincipal);
			MarshalByRefObject GetCxSubjectPrincipal(IntPtr cx); // JSContext
			MarshalByRefObject getCxSubjectPrincipalAndFrame(IntPtr cx, out IntPtr fp);
		}

        [Guid("8229DD23-47C5-4601-A80B-0166D595A21E"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface nsIScriptSecurityManager_01090200
        {
            // nsIXPCSecurityManager:
            void CanCreateWrapper(out IntPtr aJSContext, ref Guid aIID, nsISupports aObj, IntPtr aClassInfo, IntPtr aPolicy); // aClassInfo=nsIClassInfo
            void CanCreateInstance(out IntPtr aJSContext, ref Guid aCID);
            void CanGetService(out IntPtr aJSContext, ref Guid aCID);
            void CanAccess(uint aAction, IntPtr aCallContext, out IntPtr aJSContext, out IntPtr aJSObject, nsISupports aObj, IntPtr aClassInfo, IntPtr aName, IntPtr aPolicy); // aCallContext=nsIXPCNativeCallContext

            // nsIScriptSecurityManager:
            void CheckPropertyAccess(out IntPtr aJSContext, out IntPtr aJSObject, [MarshalAs(UnmanagedType.LPStr)] string aClassName, IntPtr aProperty, uint aAction); // aProperty=jsval
            void CheckConnect(out IntPtr aJSContext, nsIURI aTargetURI, [MarshalAs(UnmanagedType.LPStr)] string aClassName, [MarshalAs(UnmanagedType.LPStr)] string aProperty);
            void CheckLoadURIFromScript(out IntPtr cx, nsIURI uri);
            void CheckLoadURIWithPrincipal(MarshalByRefObject aPrincipal, nsIURI uri, uint flags);
            void CheckLoadURI(nsIURI from, nsIURI uri, uint flags);
            void CheckLoadURIStrWithPrincipal(MarshalByRefObject aPrincipal, nsACString uri, uint flags);
            void CheckLoadURIStr(nsACString from, nsACString uri, uint flags);
            void CheckFunctionAccess(out IntPtr cx, out IntPtr funObj, IntPtr targetObj);
            bool CanExecuteScripts(out IntPtr cx, MarshalByRefObject principal);
            MarshalByRefObject GetSubjectPrincipal();
            MarshalByRefObject GetSystemPrincipal();
            MarshalByRefObject GetCertificatePrincipal(nsACString aCertFingerprint, nsACString aSubjectName, nsACString aPrettyName, nsISupports aCert, nsIURI aURI);
            MarshalByRefObject GetCodebasePrincipal(nsIURI aURI);
            short RequestCapability(MarshalByRefObject principal, [MarshalAs(UnmanagedType.LPStr)] string capability);
            bool IsCapabilityEnabled([MarshalAs(UnmanagedType.LPStr)] string capability);
            void EnableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
            void RevertCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
            void DisableCapability([MarshalAs(UnmanagedType.LPStr)] string capability);
            void SetCanEnableCapability(nsACString certificateFingerprint, [MarshalAs(UnmanagedType.LPStr)] string capability, short canEnable);
            MarshalByRefObject GetObjectPrincipal(out IntPtr cx, out IntPtr aJSObject);
            bool SubjectPrincipalIsSystem();
            void CheckSameOrigin(out IntPtr aJSContext, nsIURI aTargetURI);
            void CheckSameOriginURI(nsIURI aSourceURI, nsIURI aTargetURI, bool reportError);
            MarshalByRefObject GetPrincipalFromContext(out IntPtr cx);
            MarshalByRefObject GetChannelPrincipal(IntPtr aChannel); // nsIChannel
            bool IsSystemPrincipal(MarshalByRefObject aPrincipal);
            MarshalByRefObject GetCxSubjectPrincipal(IntPtr cx); // JSContext
            MarshalByRefObject getCxSubjectPrincipalAndFrame(IntPtr cx, out IntPtr fp);
            void pushContextPrincipal(IntPtr cx, IntPtr fp, IntPtr principal);
            void popContextPrincipal(IntPtr cx);
        }

        [xpcVersion(0x1080000, 0x1090000)]
        interface nsIPrincipal
        {
            // nsISerializable:
            void Read(IntPtr aInputStream); // nsIObjectInputStream
            void Write(IntPtr aOutputStream); // nsIObjectOutputStream

            // nsIPrincipal:
            void GetPreferences(out string prefBranch, out string id, out string subjectName, out string grantedList, out string deniedList);
            bool Equals(nsIPrincipal other);
            uint GetHashValue();
            IntPtr GetJSPrincipals(IntPtr aJSContext); // returns: JSPrincipals
            IntPtr GetSecurityPolicy();
            IntPtr SetSecurityPolicy();
            short CanEnableCapability(out string capability);
            void SetCanEnableCapability(out string capability, short canEnable);
            bool IsCapabilityEnabled(out string capability, out IntPtr annotation);
            void EnableCapability(out string capability, IntPtr annotation);
            void RevertCapability(out string capability, IntPtr annotation);
            void DisableCapability(out string capability, IntPtr annotation);
            nsIURI GetURI();
            nsIURI GetDomain();
            void SetDomain(nsIURI aDomain);
            [return: MarshalAs(UnmanagedType.LPStr)]
            string GetOrigin();
            bool GetHasCertificate();
            void GetFingerprint(nsACString aFingerprint);
            void GetPrettyName(nsACString aPrettyName);
            bool Subsumes(MarshalByRefObject other);
            void GetSubjectName(nsACString aSubjectName);
            nsISupports GetCertificate();
        }
		
		[Guid("fb9ddeb9-26f9-46b8-85d5-3978aaee05aa"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface nsIPrincipal_01080000
		{
			// nsISerializable:
			void Read(IntPtr aInputStream); // nsIObjectInputStream
			void Write(IntPtr aOutputStream); // nsIObjectOutputStream
			
			// nsIPrincipal:
			void GetPreferences(out string prefBranch, out string id, out string subjectName, out string grantedList, out string deniedList);
			bool Equals(MarshalByRefObject other);
			uint GetHashValue();
			IntPtr GetJSPrincipals(IntPtr aJSContext); // returns: JSPrincipals
			IntPtr GetSecurityPolicy();
			IntPtr SetSecurityPolicy();
			short CanEnableCapability(out string capability);
			void SetCanEnableCapability(out string capability, short canEnable);
			bool IsCapabilityEnabled(out string capability, out IntPtr annotation);
			void EnableCapability(out string capability, IntPtr annotation);
			void RevertCapability(out string capability, IntPtr annotation);
			void DisableCapability(out string capability, IntPtr annotation);
			nsIURI GetURI();
			nsIURI GetDomain();
			void SetDomain(nsIURI aDomain);
			[return: MarshalAs(UnmanagedType.LPStr)] string GetOrigin();
			bool GetHasCertificate();
			void GetFingerprint(nsACString aFingerprint);
			void GetPrettyName(nsACString aPrettyName);
			bool Subsumes(MarshalByRefObject other);
			void GetSubjectName(nsACString aSubjectName);
			nsISupports GetCertificate();
		}

        [Guid("b8268b9a-2403-44ed-81e3-614075c92034"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface nsIPrincipal_01090000
        {
            // nsISerializable:
            void Read(IntPtr aInputStream); // nsIObjectInputStream
            void Write(IntPtr aOutputStream); // nsIObjectOutputStream

            // nsIPrincipal:
            void GetPreferences(out string prefBranch, out string id, out string subjectName, out string grantedList, out string deniedList);
            bool Equals(MarshalByRefObject other);
            uint GetHashValue();
            IntPtr GetJSPrincipals(IntPtr aJSContext); // returns: JSPrincipals
            IntPtr GetSecurityPolicy();
            IntPtr SetSecurityPolicy();
            short CanEnableCapability(out string capability);
            void SetCanEnableCapability(out string capability, short canEnable);
            bool IsCapabilityEnabled(out string capability, out IntPtr annotation);
            void EnableCapability(out string capability, IntPtr annotation);
            void RevertCapability(out string capability, IntPtr annotation);
            void DisableCapability(out string capability, IntPtr annotation);
            nsIURI GetURI();
            nsIURI GetDomain();
            void SetDomain(nsIURI aDomain);
            [return: MarshalAs(UnmanagedType.LPStr)]
            string GetOrigin();
            bool GetHasCertificate();
            void GetFingerprint(nsACString aFingerprint);
            void GetPrettyName(nsACString aPrettyName);
            bool Subsumes(MarshalByRefObject other);
            void CheckMayLoad(nsIURI uri, bool report);
            void GetSubjectName(nsACString aSubjectName);
            nsISupports GetCertificate();
        }
		
		[Guid("e7d09265-4c23-4028-b1b0-c99e02aa78f8"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface nsIJSRuntimeService
		{
			IntPtr GetRuntime();
			IntPtr GetBackstagePass(); // nsIXPCScriptable
        }

        #region JSStackFrame : Not used anywhere ?
        [xpcVersion(0x1080000, 0x1090000, 0x1090100, 0x1090200)]
        struct JSStackFrame
        {
            IntPtr callobj;       /* lazily created Call object */
            IntPtr argsobj;       /* lazily created arguments object */
            IntPtr varobj;        /* variables object, where vars go */
            public IntPtr script;        /* script being interpreted */
            IntPtr fun;           /* function being called or null */
            IntPtr thisp;         /* "this" pointer if in method */
            IntPtr argc;           /* actual argument count */
            IntPtr argv;          /* base of argument stack slots */
            int rval;           /* function return value */
            uint nvars;          /* local variable count */
            IntPtr vars;          /* base of variable stack slots */
            public IntPtr down;          /* previous frame */
            IntPtr annotation;    /* used by Java security */
            IntPtr scopeChain;    /* scope chain */
            uint sharpDepth;     /* array/object initializer depth */
            IntPtr sharpArray;    /* scope for #n= initializer vars */
            uint flags;          /* frame flags -- see below */
            IntPtr dormantNext;   /* next dormant frame chain */
            IntPtr xmlNamespace;  /* null or default xml namespace in E4X */
            IntPtr blockChain;    /* active compile-time block scopes */
        };

        [StructLayout(LayoutKind.Sequential)]
        struct JSStackFrame_01080000
        {
            IntPtr callobj;       /* lazily created Call object */
            IntPtr argsobj;       /* lazily created arguments object */
            IntPtr varobj;        /* variables object, where vars go */
            public IntPtr script;        /* script being interpreted */
            IntPtr fun;           /* function being called or null */
            IntPtr thisp;         /* "this" pointer if in method */
            IntPtr argc;           /* actual argument count */
            IntPtr argv;          /* base of argument stack slots */
            int rval;           /* function return value */
            uint nvars;          /* local variable count */
            IntPtr vars;          /* base of variable stack slots */
            public IntPtr down;          /* previous frame */
            IntPtr annotation;    /* used by Java security */
            IntPtr scopeChain;    /* scope chain */
            IntPtr pc;            /* program counter */
            IntPtr sp;            /* stack pointer */
            IntPtr spbase;        /* operand stack base */
            uint sharpDepth;     /* array/object initializer depth */
            IntPtr sharpArray;    /* scope for #n= initializer vars */
            uint flags;          /* frame flags -- see below */
            IntPtr dormantNext;   /* next dormant frame chain */
            IntPtr xmlNamespace;  /* null or default xml namespace in E4X */
            IntPtr blockChain;    /* active compile-time block scopes */
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JSStackFrame_01090000
        {
            IntPtr regs;
            IntPtr spbase;        /* operand stack base */
            IntPtr callobj;       /* lazily created Call object */
            IntPtr argsobj;       /* lazily created arguments object */
            IntPtr varobj;        /* variables object, where vars go */
            IntPtr callee;        /* function or script object */
            public IntPtr script;        /* script being interpreted */
            IntPtr fun;           /* function being called or null */
            IntPtr thisp;         /* "this" pointer if in method */
            IntPtr argc;           /* actual argument count */
            IntPtr argv;          /* base of argument stack slots */
            int rval;           /* function return value */
            uint nvars;          /* local variable count */
            IntPtr vars;          /* base of variable stack slots */
            public IntPtr down;          /* previous frame */
            IntPtr annotation;    /* used by Java security */
            IntPtr scopeChain;    /* scope chain */
            uint sharpDepth;     /* array/object initializer depth */
            IntPtr sharpArray;    /* scope for #n= initializer vars */
            uint flags;          /* frame flags -- see below */
            IntPtr dormantNext;   /* next dormant frame chain */
            IntPtr xmlNamespace;  /* null or default xml namespace in E4X */
            IntPtr blockChain;    /* active compile-time block scopes */

            IntPtr pcDisabledSave; // reserved for debug use
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JSStackFrame_01090100
        {
            IntPtr regs;
            IntPtr imacpc;        /* null or interpreter macro call pc */
            IntPtr slots;         /* variables, locals and operand stack */
            IntPtr callobj;       /* lazily created Call object */
            IntPtr argsobj;       /* lazily created arguments object */
            IntPtr varobj;        /* variables object, where vars go */
            IntPtr callee;        /* function or script object */
            public IntPtr script;        /* script being interpreted */
            IntPtr fun;           /* function being called or null */
            IntPtr thisp;         /* "this" pointer if in method */
            IntPtr argc;           /* actual argument count */
            IntPtr argv;          /* base of argument stack slots */
            int rval;           /* function return value */
            public IntPtr down;          /* previous frame */
            IntPtr annotation;    /* used by Java security */
            IntPtr scopeChain;    /* scope chain */
            IntPtr blockChain;    /* scope chain */
            uint sharpDepth;     /* array/object initializer depth */
            IntPtr sharpArray;    /* scope for #n= initializer vars */
            uint flags;          /* frame flags -- see below */
            IntPtr dormantNext;   /* next dormant frame chain */
            IntPtr xmlNamespace;  /* null or default xml namespace in E4X */
            IntPtr displaySave;    /* previous value of display entry for
                                       script->staticLevel */
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JSStackFrame_01090200
        {
            IntPtr regs;
            IntPtr imacpc;        /* null or interpreter macro call pc */
            IntPtr slots;         /* variables, locals and operand stack */
            IntPtr callobj;       /* lazily created Call object */
            IntPtr argsobj;       /* lazily created arguments object, must be
                                       JSVAL_OBJECT */
            IntPtr varobj;        /* variables object, where vars go */
            public IntPtr script;        /* script being interpreted */
            IntPtr fun;           /* function being called or null */
            IntPtr thisp;         /* "this" pointer if in method */
            IntPtr argc;           /* actual argument count */
            IntPtr argv;          /* base of argument stack slots */
            int rval;           /* function return value */
            public IntPtr down;          /* previous frame */
            IntPtr annotation;    /* used by Java security */
            IntPtr scopeChain;    /* scope chain */
            IntPtr blockChain;    /* scope chain */
            uint sharpDepth;     /* array/object initializer depth */
            IntPtr sharpArray;    /* scope for #n= initializer vars */
            uint flags;          /* frame flags -- see below */
            IntPtr dormantNext;   /* next dormant frame chain */
            IntPtr displaySave;    /* previous value of display entry for
                                       script->staticLevel */
        }
        #endregion

		#endregion
		
		#region Native Members
		
		[DllImport("js3250", CharSet=CharSet.Ansi)]
		static extern IntPtr JS_CompileScriptForPrincipals(IntPtr aJSContext, IntPtr aJSObject, IntPtr aJSPrincipals, string bytes, int length, string filename, int lineNumber);
		
		[DllImport("js3250")]
		static extern IntPtr JS_GetGlobalObject(IntPtr aJSContext);
		
		[DllImport("js3250")]
		static extern IntPtr JS_NewContext(IntPtr aJSRuntime, int stackchunksize);
		
		[DllImport("js3250")]
		static extern void JS_DestroyContextNoGC(IntPtr cx);
		
		[DllImport("js3250")]
		static extern IntPtr JS_BeginRequest(IntPtr cx);
		
		[DllImport("js3250")]
		static extern IntPtr JS_EndRequest(IntPtr cx);
		#endregion
		
		public AutoJSContext()
		{
			// obtain the JS runtime used by gecko
			nsIJSRuntimeService runtimeService = (nsIJSRuntimeService)Xpcom.GetService("@mozilla.org/js/xpc/RuntimeService;1");
			IntPtr jsRuntime = runtimeService.GetRuntime();
			
			// create a new JSContext
			cx = JS_NewContext(jsRuntime, 8192);
			
			// begin a new request
			JS_BeginRequest(cx);
			
			// push the context onto the context stack
			nsIJSContextStack contextStack = Xpcom.GetService<nsIJSContextStack>("@mozilla.org/js/xpc/ContextStack;1");
			contextStack.Push(cx);
			
			// obtain the system principal (no security checks) which we will use when compiling the empty script below
			nsIPrincipal system = multiversion<nsIPrincipal>.Cast(Xpcom.GetService<nsIScriptSecurityManager>("@mozilla.org/scriptsecuritymanager;1").GetSystemPrincipal());
			IntPtr jsPrincipals = system.GetJSPrincipals(cx);
			
			// create a fake stack frame
			JSStackFrame frame = multiversion<JSStackFrame>.Create();
			frame.script = JS_CompileScriptForPrincipals(cx, JS_GetGlobalObject(cx), jsPrincipals, "", 0, "", 1);
			
			// put a pointer to the fake stack frame on the JSContext
			
			// frame.down = cx->fp
			IntPtr old = Marshal.ReadIntPtr(cx, OfsetOfContextFP);
			frame.down = old;

			IntPtr framePtr = multiversion<JSStackFrame>.StructureToPtr(frame);
			
			// cx->fp = framePtr;
            Marshal.WriteIntPtr(cx, OfsetOfContextFP, framePtr);
		}
		
		//NOTE: these hard-coded field offsets are based on the unmanaged layout of JSContext objects.  this will
		// probably not work for versions other than 1.8, 1.9 and 1.9.1
        public static int OfsetOfContextFP
        {
            get
            {
                versioned<int> v = new versioned<int>(0x34,
                                                      new verb<int>(0x01080000, 0x34), 
                                                      new verb<int>(0x01090000, 0x54), 
                                                      new verb<int>(0x01090100, 0x98),
                                                      new verb<int>(0x01090200, 0x94)
                    );
                return v.Value;
            }
        }

		IntPtr cx;
		
		public void Dispose()
		{
			nsIJSContextStack contextStack = Xpcom.GetService<nsIJSContextStack>("@mozilla.org/js/xpc/ContextStack;1");
			contextStack.Pop();
			
			// free the memory allocated for the fake stack frame
			Marshal.FreeHGlobal(Marshal.ReadIntPtr(cx, OfsetOfContextFP));
			
			// end the request, destroy the context
			JS_EndRequest(cx);
			JS_DestroyContextNoGC(cx);
		}
	}
}
