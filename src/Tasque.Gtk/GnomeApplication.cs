using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Xml;

using Mono.Unix;
using Mono.Unix.Native;

namespace Tasque
{
	public class GnomeApplication : NativeApplication
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

		public override void Initialize (string [] args)
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
