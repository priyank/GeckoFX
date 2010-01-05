==================================================
GeckoFX Multi 1.9.1.0 v.1
based on GeckoFX 1.9.1.0 by
(C) 2008-2009 Skybound Software. All Rights Reserved.
http://www.geckofx.org
==================================================
GeckoFx Multi modifications by:
(c) brainsucker (Nik Medved), 2009-2010.
http://blog.brains.by
--------------------------------------------------

Please, read the Readme.txt first, for the basic information
about GeckoFx.

What is it?
-----------
GeckoFx Multi is modified GeckoFx library which:
 * Has built-in discovery of Gecko's based applications (like Firefox)
 * Can be used with any Gecko version from: 1.8.1, 1.9.0, 1.9.1, 1.9.2b5, 
   meaning it is compatible with the installations of:
	XULRunner 2.0.x-... (Gecko 1.8.1.x-1.9.2b5)
	Firefox 3.0 (or any other Gecko 1.9.0.x installation)
	Firefox 3.5 (or any other Gecko 1.9.1.x installation)
	Firefox 3.6b5 (or any other Gecko 1.9.2b5 installation)
 * Single library (single compilation) for any Gecko version.
GeckoFx Multi will probably retain compatibility with Gecko 1.9.2
(release of Firefox 3.6), but this can't be guaranteed at the moment.

Getting Started
---------------

GeckoFx is a very nice library developed by Skybound Software, 
which has one little drawback: it can be used only with specific 
version of XULRunner it was compiled for. It's not a big deal if 
you are able to redistribute the always up-to-date XulRunner with 
your software, but it becomes a problem if your installation has 
to be as small as possible and you want to use  Gecko installed at 
user's system (for example Firefox browser, which will probably 
be up-to-date itself).

Example of such program is HLR Ban Poster (http://garena.brains.by), 
with setup less than 1 mb in size, which autodetects and uses any 
Gecko available at user's system, and can rollback to standard
Microsoft WebBrowser control in the worst case.

GeckoFx Multi uses proxies and custom marshalers to retain 
compatibility with xpcom NS interfaces changed in different Gecko
versions.

Reliability and Compatibility
-----------------------------

GeckoFx Multi modification was developed especially for use in one 
particular project (HLR Ban Poster), and wasn't throughly examinated
in other conditions. All code inside original GeckoFx for supporting 
different Gecko versions was retained and converted, but wasn't 
throughly tested, so 100% compatibility can't be guaranteed. 

In some situations GeckoFx Multi may work worse than original library,
so if you need a stable Gecko-based control working with single dedicated 
version of Gecko engine I recommend using the latest library from
SkyBound website (http://www.geckofx.org) instead.


How to use
----------

The same way like you used usual GeckoFx. The same control, the same
initialization, GeckoFxMulti should work instead of original GeckoFx 
library without any code modifications.

If you want to initialize GeckoFx with the latest supported version of Gecko
present in the system (searches registry for applications distributed with 
Gecko (like Firefox), release versions are preferred over alpha/beta) use
the following initializer:

Skybound.Gecko.Xpcom.Initialize(true);

The other possibility is to get all geckos detected at the system and give
user the ability to select the desired one:

// just use auto detection
var gad = Skybound.Gecko.GeckoAppDiscovery();
// use auto detection and provide several custom gecko paths to check.
// you can also add custom gecko path to check using AddGeckoPath method
var gad = Skybound.Gecko.GeckoAppDiscovery("c:\\xul2.0", "c:\\firefoxportable3.6b5");
gad.AddGeckoPath("d:\\firefox");

After Gecko's discovery you can retrieve the list of all or valid-only geckos
using the gad.Geckos or gad.ValidGeckos properties.
Also you can initialize a single GeckoAppInfo class this way:

var gai = new Skybound.Gecko.GeckoAppInfo("d:\\firefox");

When your GeckoAppInfo structure is prepared (selected from ValidGeckos list or
filled in manually), you can pass it to the Initialize method:

Skybound.Gecko.Xpcom.Initialize(gai);

Please note: you can't initialize several Gecko's at once or one after another,
the only possible solution to let user pick second Gecko and initialize it is 
to restart your process (if your software is using a separate AppDomain for 
Gecko browser, it should be possible to reload just this AppDomain).

Changelog
=========

1.9.1.0 v.1 based on GeckoFx 1.9.1.0 (4 Jan 2010)
--------------------------------------------------
 * Merged SkyBound modifications for GeckoFx 1.9.1.0.
 * nsIDOMHTMLDocument.Get/SetTitle: changed back to nsAString,
   nsAUTF8String (introduced at GeckoFx 1.9.1) seems inappropriate here.
 * Support for PreAlpha/Alpha/Beta Gecko sub-versions.
 * Prepared for public release (additional constructor, etc).
 * Preliminary support for Gecko 1.9.2 (tested with Firefox 3.6b5).

1.9.1.0 v.0 based on GeckoFx 1.9.0.0 (August 2009)
--------------------------------------------------
 * Original private release, used only in HLR Ban Poster up to version 0.5.1.
 * Support for Firefox 3.5 by brainsucker.