using System;
using System.Windows.Forms;

namespace Skybound.Gecko
{
	public partial class ChromeDialog : Form
	{
		public ChromeDialog()
		{
			InitializeComponent();
		}

		public GeckoWebBrowser WebBrowser
		{
			get { return webBrowser; }
		}
		
		private void webBrowser_DocumentTitleChanged(object sender, EventArgs e)
		{
			Text = webBrowser.DocumentTitle;
		}

		private void webBrowser_WindowSetBounds(object sender, GeckoWindowSetBoundsEventArgs e)
		{
			SetBounds(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height, e.BoundsSpecified);
		}

		private void webBrowser_WindowClosed(object sender, EventArgs e)
		{
			Close();
		}
	}
}