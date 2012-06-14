// 
// GtkAboutDialog.cs
//  
// Author:
//       Antonius Riha <antoniusriha@gmail.com>
// 
// Copyright (c) 2012 Antonius Riha
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Text;
using Mono.Unix;
using Tasque.UI.Desktop;

namespace Tasque
{
	public class GtkAboutDialog : AboutDialog
	{
		public override void Show ()
		{
			/* string [] documenters = new string [] {
			   "Calvin Gaisford <calvinrg@gmail.com>"
			   };
			*/
			
			var translators = new StringBuilder ();
			foreach (var item in Translators) {
				translators.AppendLine (item);
			}
				
			Gtk.AboutDialog about = new Gtk.AboutDialog ();
			about.ProgramName = "Tasque";
			about.Version = Version;
			about.Logo = Utilities.GetIcon("tasque-48", 48);
			about.Copyright = CopyrightInfo;
			about.Comments = Description;
			about.Website = WebsiteUrl;
			about.WebsiteLabel = Catalog.GetString ("Tasque Project Homepage");
			about.Authors = Authors;
			//about.Documenters = documenters;
			about.TranslatorCredits = translators.ToString ();
			about.IconName = "tasque";
			about.SetSizeRequest(300, 300);
			about.Run ();
			about.Destroy ();
		}
	}
}
