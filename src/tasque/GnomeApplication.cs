// 
// GnomeApplication.cs
//  
// Authors:
//       Sandy Armstrong <sanfordarmstrong@gmail.com>
//       Antonius Riha <antoniusriha@gmail.com>
// 
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com) 
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
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Xml;

using Mono.Unix;
using Mono.Unix.Native;
using Tasque.UI;

namespace Tasque
{
	public class GnomeApplication : Program
	{
		private Gnome.Program program;
		private string confDir;

		public GnomeApplication ()
		{
			confDir = Path.Combine (
				Environment.GetFolderPath (
				Environment.SpecialFolder.ApplicationData),
				"tasque");
			if (!Directory.Exists (confDir))
				Directory.CreateDirectory (confDir);
		}

//		protected override NativeWindow InitializeMainWindow ()
//		{
//			throw new NotImplementedException ();
//		}

		protected override Tray InitializeTray ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnInitialize (string[] args)
		{
			Mono.Unix.Catalog.Init ("tasque", GlobalDefines.LocaleDir);
			try {
				SetProcessName ("Tasque");
			} catch {} // Ignore exception if fail (not needed to run)

			Gtk.Application.Init ();
			program = new Gnome.Program ("Tasque",
			                             GlobalDefines.Version,
			                             Gnome.Modules.UI,
			                             args);
		}

		public override void StartMainLoop ()
		{
			program.Run ();
		}

		public override void QuitMainLoop ()
		{
			Gtk.Main.Quit ();
		}

		[DllImport("libc")]
		private static extern int prctl (int option,
			                                 byte [] arg2,
			                                 IntPtr arg3,
			                                 IntPtr arg4,
			                                 IntPtr arg5);

		// From Banshee: Banshee.Base/Utilities.cs
		private void SetProcessName (string name)
		{
			if (prctl (15 /* PR_SET_NAME */,
			                Encoding.ASCII.GetBytes (name + "\0"),
			                IntPtr.Zero,
			                IntPtr.Zero,
			                IntPtr.Zero) != 0)
				throw new ApplicationException (
				        "Error setting process name: " +
				        Mono.Unix.Native.Stdlib.GetLastError ());
		}
		
		public override void OpenUrlInBrowser (string url)
		{
			Gnome.Url.Show (url);
		}
		
		public override string ConfDir {
			get {
				return confDir;
			}
		}
	}
}
