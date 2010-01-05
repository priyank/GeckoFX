using System;
using System.Runtime.InteropServices;

namespace Skybound.Gecko
{
    [Guid("a6cf906b-15b3-11d2-932e-00805f8add32"), ComImport, System.Runtime.InteropServices.InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface nsIDOMWindow
    {
        nsIDOMDocument GetDocument();
        nsIDOMWindow GetParent();
        nsIDOMWindow GetTop();
        IntPtr GetScrollbars(); // nsIDOMBarProp
        IntPtr GetFrames(); // nsIDOMWindowCollection
        void GetName(nsAString aName);
        void SetName(nsAString aName);
        float GetTextZoom();
        void SetTextZoom(float aTextZoom);
        int GetScrollX();
        int GetScrollY();
        void ScrollTo(int xScroll, int yScroll);
        void ScrollBy(int xScrollDif, int yScrollDif);
        nsISelection GetSelection(); // nsISelection
        void ScrollByLines(int numLines);
        void ScrollByPages(int numPages);
        void SizeToContent();
    }

    [xpcVersion(0x1080000, 0x1090000)]
    interface nsIDOMWindow2 : nsIDOMWindow
    {
        // nsIDOMWindow:
        new nsIDOMDocument GetDocument();
        new nsIDOMWindow GetParent();
        new nsIDOMWindow GetTop();
        new IntPtr GetScrollbars(); // nsIDOMBarProp
        new nsIDOMWindowCollection GetFrames();
        new void GetName(nsAString aName);
        new void SetName(nsAString aName);
        new float GetTextZoom();
        new void SetTextZoom(float aTextZoom);
        new int GetScrollX();
        new int GetScrollY();
        new void ScrollTo(int xScroll, int yScroll);
        new void ScrollBy(int xScrollDif, int yScrollDif);
        new nsISelection GetSelection(); // nsISelection
        new void ScrollByLines(int numLines);
        new void ScrollByPages(int numPages);
        new void SizeToContent();

        // nsIDOMWindow2:
        nsIDOMEventTarget GetWindowRoot();
    }

	[Guid("65455132-b96a-40ec-adea-52fa22b1028c"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface nsIDOMWindow2_01080000 : nsIDOMWindow
    {
        // nsIDOMWindow:
        new nsIDOMDocument GetDocument();
        new nsIDOMWindow GetParent();
        new nsIDOMWindow GetTop();
        new IntPtr GetScrollbars(); // nsIDOMBarProp
        new nsIDOMWindowCollection GetFrames();
        new void GetName(nsAString aName);
        new void SetName(nsAString aName);
        new float GetTextZoom();
        new void SetTextZoom(float aTextZoom);
        new int GetScrollX();
        new int GetScrollY();
        new void ScrollTo(int xScroll, int yScroll);
        new void ScrollBy(int xScrollDif, int yScrollDif);
        new nsISelection GetSelection(); // nsISelection
        new void ScrollByLines(int numLines);
        new void ScrollByPages(int numPages);
        new void SizeToContent();

        // nsIDOMWindow2:
        nsIDOMEventTarget GetWindowRoot();
    }

    [Guid("73c5fa35-3add-4c87-a303-a850ccf4d65a"), System.Runtime.InteropServices.ComImport, System.Runtime.InteropServices.InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface nsIDOMWindow2_01090000 : nsIDOMWindow
    {
        // nsIDOMWindow:
        new nsIDOMDocument GetDocument();
        new nsIDOMWindow GetParent();
        new nsIDOMWindow GetTop();
        new IntPtr GetScrollbars(); // nsIDOMBarProp
        new nsIDOMWindowCollection GetFrames();
        new void GetName(nsAString aName);
        new void SetName(nsAString aName);
        new float GetTextZoom();
        new void SetTextZoom(float aTextZoom);
        new int GetScrollX();
        new int GetScrollY();
        new void ScrollTo(int xScroll, int yScroll);
        new void ScrollBy(int xScrollDif, int yScrollDif);
        new nsISelection GetSelection(); // nsISelection
        new void ScrollByLines(int numLines);
        new void ScrollByPages(int numPages);
        new void SizeToContent();

        // nsIDOMWindow2:
        nsIDOMEventTarget GetWindowRoot();
        void GetApplicationCache(IntPtr aApplicationCache); // nsIDOMOfflineResourceList
    }
}
