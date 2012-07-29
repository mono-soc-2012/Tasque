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
using System.Diagnostics;
using Mono.Unix;
using Gtk;
using Tasque.UIModel.Legacy;

namespace Tasque
{
	public abstract class GtkApplicationBase : NativeApplication
	{
		#region just copied
		public UIManager UIManager
		{
			get { return uiManager; }
		}
		
		#region implemented abstract members of Tasque.UIModel.Legacy.NativeApplication
		public override Backend CurrentBackend {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public override System.Collections.ObjectModel.ReadOnlyCollection<Backend> AvailableBackends {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public override Tasque.UIModel.Legacy.MainWindowModel MainWindowModel {
			get {
				throw new System.NotImplementedException ();
			}
		}
		#endregion
		#endregion
		
		public GtkApplicationBase ()
		{
			confDir = Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "tasque");
			if (!Directory.Exists (confDir))
				Directory.CreateDirectory (confDir);
		}
		
		public override string ConfDir { get { return confDir; } }

		public override void Initialize (string[] args)
		{
			base.Initialize (args);
//			Catalog.Init ("tasque", GlobalDefines.LocaleDir);
			Gtk.Application.Init ();
			
			RegisterUIManager ();
		}
		
		public override void StartMainLoop ()
		{
			Gtk.Application.Run ();
		}
		
		public override void QuitMainLoop ()
		{
			Gtk.Application.Quit ();
		}
		
		public override void OpenUrlInBrowser (string url)
		{
			try {
				Process.Start (url);
			} catch (Exception e) {
				Trace.TraceError ("Error opening url [{0}]:\n{1}", url, e.ToString ());
			}
		}
		
		protected override event EventHandler RemoteInstanceKnocked;
		
		protected void OnRemoteInstanceKnocked ()
		{
			if (RemoteInstanceKnocked != null)
				RemoteInstanceKnocked (this, EventArgs.Empty);
		}
		
		void RegisterUIManager ()
		{
			ActionGroup trayActionGroup = new ActionGroup ("Tray");
//			trayActionGroup.Add (new ActionEntry [] {
//				new ActionEntry ("NewTaskAction",
//				                 Stock.New,
//				                 Catalog.GetString ("New Task ..."),
//				                 null,
//				                 null,
//				                 OnNewTask),
//				
//				new ActionEntry ("ShowTasksAction",
//				                 null,
//				                 Catalog.GetString ("Show Tasks ..."),
//				                 null,
//				                 null,
//				                 OnShowTaskWindow),
//
//				new ActionEntry ("AboutAction",
//				                 Stock.About,
//				                 OnAbout),
//				
//				new ActionEntry ("PreferencesAction",
//				                 Stock.Preferences,
//				                 OnPreferences),
//				
//				new ActionEntry ("RefreshAction",
//				                 Stock.Execute,
//				                 Catalog.GetString ("Refresh Tasks ..."),
//				                 null,
//				                 null,
//				                 OnRefreshAction),
//				
//				new ActionEntry ("QuitAction",
//				                 Stock.Quit,
//				                 OnQuit)
//			});
			
			uiManager = new UIManager ();
			uiManager.AddUiFromString (MenuXml);
			uiManager.InsertActionGroup (trayActionGroup, 0);
		}
		
		UIManager uiManager;
		const string MenuXml = @"
<ui>
	<popup name=""TrayIconMenu"">
		<menuitem action=""NewTaskAction""/>
		<separator/>
		<menuitem action=""PreferencesAction""/>
		<menuitem action=""AboutAction""/>
		<separator/>
		<menuitem action=""RefreshAction""/>
		<separator/>
		<menuitem action=""QuitAction""/>
	</popup>
</ui>
";
		
		string confDir;
	}
}
