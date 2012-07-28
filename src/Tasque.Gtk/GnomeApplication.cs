using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Xml;

using Mono.Unix;

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
