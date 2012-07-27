// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 
//  
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// 
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com) 
// Copyright (c) 2012 Antonius Riha
// 
// Authors: 
//      Sandy Armstrong <sanfordarmstrong@gmail.com>
//      Antonius Riha <antoniusriha@gmail.com>
// 

using System;
using System.IO;

namespace Tasque
{
	public class GtkApplication : NativeApplication
	{
		private string confDir;

		public GtkApplication ()
		{
			confDir = Path.Combine (
				Environment.GetFolderPath (
				Environment.SpecialFolder.ApplicationData),
				"tasque");
			if (!Directory.Exists (confDir))
				Directory.CreateDirectory (confDir);
		}

		public override void Initialize (string[] args)
		{
			Gtk.Application.Init ();
		}
		
		public override void StartMainLoop ()
		{
			Gtk.Application.Run ();
		}
		
		public override void QuitMainLoop ()
		{
			Gtk.Application.Quit ();
		}

		public override string ConfDir
		{
			get
			{
				return confDir;
			}
		}

		public override void OpenUrlInBrowser (string url)
		{
			try {
				System.Diagnostics.Process.Start (url);
			} catch (Exception e) {
				Logger.Error ("Error opening url [{0}]:\n{1}", url, e.ToString ());
			}
		}
	}
}
