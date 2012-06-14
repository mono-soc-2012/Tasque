//
// GtkMainWindow.cs
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
using Tasque.UI.Desktop;

namespace Tasque
{
	public class GtkMainWindow : MainWindow
	{
		public GtkMainWindow ()
		{
			mainWindow = new Gtk.Window(Gtk.WindowType.Toplevel);
			mainWindow.SetSizeRequest (300, 300);
			mainWindow.ShowAll ();
		}

		public override bool IsVisible {
			get {
				throw new System.NotImplementedException ();
			}
			set {
				throw new System.NotImplementedException ();
			}
		}

		protected override Tray InitializeTray ()
		{
			throw new System.NotImplementedException ();
		}

		Gtk.Window mainWindow;

		protected override AboutDialog InitializeAboutDialog ()
		{
			return new GtkAboutDialog ();
		}

		protected override Tasque.UI.Desktop.PreferencesDialog InitializePreferencesDialog ()
		{
			return new GtkPreferencesDialog ();
		}
	}
}

