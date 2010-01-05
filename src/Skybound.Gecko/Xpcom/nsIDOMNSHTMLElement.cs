using System;
using System.Runtime.InteropServices;

namespace Skybound.Gecko
{
    interface ScrollAndClient_nsIDOMNSElement
    {
        int GetScrollTop();
        void SetScrollTop(int aScrollTop);
        int GetScrollLeft();
        void SetScrollLeft(int aScrollLeft);
        int GetScrollHeight();
        int GetScrollWidth();
        int GetClientHeight();
        int GetClientWidth();
    }

    [xpcVersion(0x1080000, 0x1090000, 0x1090100)]
    interface nsIDOMNSHTMLElement : ScrollAndClient_nsIDOMNSElement
    {
        int GetOffsetTop();
        int GetOffsetLeft();
        int GetOffsetWidth();
        int GetOffsetHeight();
        nsIDOMElement GetOffsetParent();
        void GetInnerHTML(nsAString aInnerHTML);
        void SetInnerHTML(nsAString aInnerHTML);
        int GetScrollTop();
        void SetScrollTop(int aScrollTop);
        int GetScrollLeft();
        void SetScrollLeft(int aScrollLeft);
        int GetScrollHeight();
        int GetScrollWidth();
        int GetClientHeight();
        int GetClientWidth();
        int GetTabIndex();
        void SetTabIndex(int aTabIndex);
        void Blur();
        void Focus();
        void ScrollIntoView(int top);
    }

    [Guid("da83b2ec-8264-4410-8496-ada3acd2ae42"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface nsIDOMNSHTMLElement_01080000
    {
        int GetOffsetTop();
        int GetOffsetLeft();
        int GetOffsetWidth();
        int GetOffsetHeight();
        nsIDOMElement GetOffsetParent();
        void GetInnerHTML(nsAString aInnerHTML);
        void SetInnerHTML(nsAString aInnerHTML);
        int GetScrollTop();
        void SetScrollTop(int aScrollTop);
        int GetScrollLeft();
        void SetScrollLeft(int aScrollLeft);
        int GetScrollHeight();
        int GetScrollWidth();
        int GetClientHeight();
        int GetClientWidth();
        int GetTabIndex();
        void SetTabIndex(int aTabIndex);
        void Blur();
        void Focus();
        void ScrollIntoView(int top);
    }

    [Guid("eac0a4ee-2e4f-403c-9b77-5cf32cfb42f7"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface nsIDOMNSHTMLElement_01090000
    {
        int GetOffsetTop();
        int GetOffsetLeft();
        int GetOffsetWidth();
        int GetOffsetHeight();
        nsIDOMElement GetOffsetParent();
        void GetInnerHTML(nsAString aInnerHTML);
        void SetInnerHTML(nsAString aInnerHTML);
        int GetScrollTop();
        void SetScrollTop(int aScrollTop);
        int GetScrollLeft();
        void SetScrollLeft(int aScrollLeft);
        int GetScrollHeight();
        int GetScrollWidth();
        int GetClientTop();
        int GetClientLeft();
        int GetClientHeight();
        int GetClientWidth();
        int GetTabIndex();
        void SetTabIndex(int aTabIndex);
        void GetContentEditable(nsAString aContentEditable);
        void SetContentEditable(nsAString aContentEditable);
        void Blur();
        void Focus();
        void ScrollIntoView(int top);
        bool GetSpellcheck();
        void SetSpellcheck(bool aSpellcheck);
    }

    [Guid("7F142F9A-FBA7-4949-93D6-CF08A974AC51"), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface nsIDOMNSHTMLElement_01090100
    {
        int GetOffsetTop();
        int GetOffsetLeft();
        int GetOffsetWidth();
        int GetOffsetHeight();
        nsIDOMElement GetOffsetParent();
        void GetInnerHTML(nsAString aInnerHTML);
        void SetInnerHTML(nsAString aInnerHTML);
        int GetTabIndex();
        void SetTabIndex(int aTabIndex);
        void GetContentEditable(nsAString aContentEditable);
        void SetContentEditable(nsAString aContentEditable);
        bool GetDraggable();
        void SetDraggable(bool aDraggable);
        void Blur();
        void Focus();
        void ScrollIntoView(int top);
        bool GetSpellcheck();
        void SetSpellcheck(bool aSpellcheck);
    }
}
