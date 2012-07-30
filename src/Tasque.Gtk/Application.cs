/***************************************************************************
 *  Application.cs
 *
 *  Copyright (C) 2008 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.Sockets;

using Gtk;
using Gdk;
using Mono.Unix;
using Mono.Unix.Native;
using Tasque.UIModel.Legacy;
using System.Diagnostics;


#if ENABLE_NOTIFY_SHARP
using Notifications;
#endif

namespace Tasque
{
	class Application
	{
		private static Tasque.Application application = null;
		private static System.Object locker = new System.Object();
		private NativeApplication nativeApp;

		private Gtk.StatusIcon trayIcon;	
		private Preferences preferences;
		private Backend backend;
		private TaskGroupModel overdue_tasks, today_tasks, tomorrow_tasks;
		private PreferencesDialog preferencesDialog;
		private bool quietStart = false;
		
		private DateTime currentDay = DateTime.Today;
		
		/// <value>
		/// Keep track of the available backends.  The key is the Type name of
		/// the backend.
		/// </value>
		private Dictionary<string, Backend> availableBackends;
		
		private Backend customBackend;



		public static Backend Backend
		{ 
			get { return Application.Instance.backend; }
			set { Application.Instance.SetBackend (value); }
		}
		
		public static List<Backend> AvailableBackends
		{
			get {
				return new List<Backend> (Application.Instance.availableBackends.Values);
			}
//			get { return Application.Instance.availableBackends; }
		}
		
		public static Application Instance
		{
			get {
				lock(locker) {
					if(application == null) {
						lock(locker) {
							application = new Application();
						}
					}
					return application;
				}
			}
		}

		public NativeApplication NativeApplication
		{
			get
			{
				return nativeApp;
			}
		}

		public static Preferences Preferences
		{
			get { return Application.Instance.preferences; }
		}

		private Application () : this (null) {}

		private Application (string[] args)
		{
			Init(args);
		}

		private void Init(string[] args)
		{
#if WIN32 || GTKLINUX
			nativeApp = new GtkApplication ();
#else
			nativeApp = new GnomeApplication ();
#endif
			nativeApp.Initialize (args);
			
			RegisterUIManager ();

			preferences = new Preferences (nativeApp.ConfDir);
			

			
			string potentialBackendClassName = null;
			
			for (int i = 0; i < args.Length; i++) {
				switch (args [i]) {
					
				case "--quiet":
					quietStart = true;
					Debug.WriteLine ("Starting quietly");
					break;
					
				case "--backend":
					if ( (i + 1 < args.Length) &&
					    !string.IsNullOrEmpty (args [i + 1]) &&
					    args [i + 1] [0] != '-') {
						potentialBackendClassName = args [++i];
					} // TODO: Else, print usage
					break;
					
				default:
					// Support old argument behavior
					if (!string.IsNullOrEmpty (args [i]))
						potentialBackendClassName = args [i];
					break;
				}
			}
			
			// See if a specific backend is specified
			if (potentialBackendClassName != null) {
				Debug.WriteLine ("Backend specified: " +
				              potentialBackendClassName);
				
				customBackend = null;
				Assembly asm = Assembly.GetCallingAssembly ();
				try {
					customBackend = (Backend)
						asm.CreateInstance (potentialBackendClassName);
				} catch (Exception e) {
					Trace.TraceWarning ("Backend specified on args not found: {0}\n\t{1}",
						potentialBackendClassName, e.Message);
				}
			}
			
			// Discover all available backends
			LoadAvailableBackends ();

			GLib.Idle.Add(InitializeIdle);
			GLib.Timeout.Add (60000, CheckForDaySwitch);
		}
		
		/// <summary>
		/// Load all the available backends that Tasque can find.  First look in
		/// Tasque.exe and then for other DLLs in the same directory Tasque.ex
		/// resides.
		/// </summary>
		private void LoadAvailableBackends ()
		{
			availableBackends = new Dictionary<string,Backend> ();
			
			List<Backend> backends = new List<Backend> ();
			
			Assembly tasqueAssembly = Assembly.GetCallingAssembly ();
			
			// Look for other backends in Tasque.exe
			backends.AddRange (GetBackendsFromAssembly (tasqueAssembly));
			
			// Look through the assemblies located in the same directory as
			// Tasque.exe.
			Debug.WriteLine ("Tasque.exe location:  {0}", tasqueAssembly.Location);
			
			DirectoryInfo loadPathInfo =
				Directory.GetParent (tasqueAssembly.Location);
			Trace.TraceInformation ("Searching for Backend DLLs in: {0}", loadPathInfo.FullName);
			
			foreach (FileInfo fileInfo in loadPathInfo.GetFiles ("*.dll")) {
				Trace.TraceInformation ("\tReading {0}", fileInfo.FullName);
				Assembly asm = null;
				try {
					asm = Assembly.LoadFile (fileInfo.FullName);
				} catch (Exception e) {
					Debug.WriteLine ("Exception loading {0}: {1}",
								  fileInfo.FullName,
								  e.Message);
					continue;
				}
				
				backends.AddRange (GetBackendsFromAssembly (asm));
			}
			
			foreach (Backend backend in backends) {
				string typeId = backend.GetType ().ToString ();
				if (availableBackends.ContainsKey (typeId))
					continue;
				
				Debug.WriteLine ("Storing '{0}' = '{1}'", typeId, backend.Name);
				availableBackends [typeId] = backend;
			}
		}
		
		private List<Backend> GetBackendsFromAssembly (Assembly asm)
		{
			List<Backend> backends = new List<Backend> ();
			
			Type[] types = null;
			
			try {
				types = asm.GetTypes ();
			} catch (Exception e) {
				Trace.TraceWarning ("Exception reading types from assembly '{0}': {1}",
					asm.ToString (), e.Message);
				return backends;
			}
			foreach (Type type in types) {
				if (!type.IsClass) {
					continue; // Skip non-class types
				}
				if (type.GetInterface ("Tasque.Backends.IBackend") == null) {
					continue;
				}
				Debug.WriteLine ("Found Available Backend: {0}", type.ToString ());
				
				Backend availableBackend = null;
				try {
					availableBackend = (Backend)
						asm.CreateInstance (type.ToString ());
				} catch (Exception e) {
					Trace.TraceWarning ("Could not instantiate {0}: {1}",
								 type.ToString (),
								 e.Message);
					continue;
				}
				
				if (availableBackend != null) {
					backends.Add (availableBackend);
				}
			}
			
			return backends;
		}

		private void SetBackend (Backend value)
		{
			bool changingBackend = false;
			if (this.backend != null) {
				UnhookFromTooltipTaskGroupModels ();
				changingBackend = true;
				// Cleanup the old backend
				try {
					Debug.WriteLine ("Cleaning up backend: {0}",
					              this.backend.Name);
					this.backend.Cleanup ();
				} catch (Exception e) {
					Trace.TraceWarning ("Exception cleaning up '{0}': {1}",
					             this.backend.Name,
					             e);
				}
			}
				
			// Initialize the new backend
			this.backend = value;
			if (this.backend == null) {
				RefreshTrayIconTooltip ();
				return;
			}
				
			Trace.TraceInformation ("Using backend: {0} ({1})",
			             this.backend.Name,
			             this.backend.GetType ().ToString ());
			this.backend.Initialize();
			
			if (!changingBackend) {
				TaskWindow.Reinitialize (!this.quietStart);
			} else {
				TaskWindow.Reinitialize (true);
			}

			RebuildTooltipTaskGroupModels ();
			RefreshTrayIconTooltip ();
			
			Debug.WriteLine("Configuration status: {0}",
			             this.backend.Configured.ToString());
		}

		private bool InitializeIdle()
		{
			if (customBackend != null) {
				Application.Backend = customBackend;
			} else {
				// Check to see if the user has a preference of which backend
				// to use.  If so, use it, otherwise, pop open the preferences
				// dialog so they can choose one.
				string backendTypeString = Preferences.Get (Preferences.CurrentBackend);
				Debug.WriteLine ("CurrentBackend specified in Preferences: {0}", backendTypeString);
				if (backendTypeString != null
						&& availableBackends.ContainsKey (backendTypeString)) {
					Application.Backend = availableBackends [backendTypeString];
				}
			}
			
			SetupTrayIcon ();
			
			if (backend == null) {
				// Pop open the preferences dialog so the user can choose a
				// backend service to use.
				Application.ShowPreferences ();
			} else if (!quietStart) {
				TaskWindow.ShowWindow ();
			}
			if (backend == null || !backend.Configured){
				GLib.Timeout.Add(1000, new GLib.TimeoutHandler(RetryBackend));
			}

			nativeApp.InitializeIdle ();
			
			return false;
		}
		private bool RetryBackend(){
			try {
				if (backend != null && !backend.Configured) {
					backend.Cleanup();
					backend.Initialize();
				}
			} catch (Exception e) {
				Trace.TraceError("{0}", e.Message);
			}
			if (backend == null || !backend.Configured) {
				return true;
			} else {
				return false;
			}
		}
		
		private bool CheckForDaySwitch ()
		{
			if (DateTime.Today != currentDay) {
				Debug.WriteLine ("Day has changed, reloading tasks");
				currentDay = DateTime.Today;
				// Reinitialize window according to new date
				if (TaskWindow.IsOpen)
					TaskWindow.Reinitialize (true);
				
				UnhookFromTooltipTaskGroupModels ();
				RebuildTooltipTaskGroupModels ();
				RefreshTrayIconTooltip ();
			}
			
			return true;
		}

		private void SetupTrayIcon ()
		{
			GtkTrayBase tray;
#if APPINDICATOR
			tray = new AppIndicatorTray (null);
#else
			tray = new StatusIconTray (null);
#endif
			trayIcon = new Gtk.StatusIcon();
			trayIcon.Pixbuf = Utilities.GetIcon ("tasque-24", 24);
			
			// hooking event
			trayIcon.Activate += OnTrayIconClick;
			trayIcon.PopupMenu += OnTrayIconPopupMenu;

			// showing the trayicon
			trayIcon.Visible = true;

			RefreshTrayIconTooltip ();
		}

		void UnhookFromTooltipTaskGroupModels ()
		{
			foreach (TaskGroupModel model in new TaskGroupModel[] { overdue_tasks, today_tasks, tomorrow_tasks })
			{
				if (model == null)
					continue;
				
				model.CollectionChanged -= OnTooltipModelChanged;
			}
		}

		void OnTooltipModelChanged (object sender, EventArgs args)
		{
			RefreshTrayIconTooltip ();
		}

		void RebuildTooltipTaskGroupModels ()
		{
			if (backend == null || backend.Tasks2 == null) {
				overdue_tasks = null;
				today_tasks = null;
				tomorrow_tasks = null;
				
				return;
			}

			overdue_tasks = TaskGroupModelFactory.CreateOverdueModel (backend.Tasks);
			today_tasks = TaskGroupModelFactory.CreateTodayModel (backend.Tasks);
			tomorrow_tasks = TaskGroupModelFactory.CreateTomorrowModel (backend.Tasks);

			foreach (TaskGroupModel model in new TaskGroupModel[] { overdue_tasks, today_tasks, tomorrow_tasks })
			{
				if (model == null)
					continue;
				
				model.CollectionChanged += OnTooltipModelChanged;
			}
		}
		
		private void RefreshTrayIconTooltip ()
		{
			if (trayIcon == null) {
				return;
			}

			StringBuilder sb = new StringBuilder ();
			if (overdue_tasks != null) {
				int count =  overdue_tasks.Count;

				if (count > 0) {
					sb.Append (String.Format (Catalog.GetPluralString ("{0} task is Overdue\n", "{0} tasks are Overdue\n", count), count));
				}
			}
			
			if (today_tasks != null) {
				int count =  today_tasks.Count;

				if (count > 0) {
					sb.Append (String.Format (Catalog.GetPluralString ("{0} task for Today\n", "{0} tasks for Today\n", count), count));
				}
			}

			if (tomorrow_tasks != null) {
				int count =  tomorrow_tasks.Count;

				if (count > 0) {
					sb.Append (String.Format (Catalog.GetPluralString ("{0} task for Tomorrow\n", "{0} tasks for Tomorrow\n", count), count));
				}
			}

			if (sb.Length == 0) {
				// Translators: This is the status icon's tooltip. When no tasks are overdue, due today, or due tomorrow, it displays this fun message
				trayIcon.Tooltip = Catalog.GetString ("Tasque Rocks");
				return;
			}

			trayIcon.Tooltip = sb.ToString ().TrimEnd ('\n');
		}


		private void OnPreferences (object sender, EventArgs args)
		{
			Trace.TraceInformation ("OnPreferences called");
			if (preferencesDialog == null) {
				preferencesDialog = new PreferencesDialog ();
				preferencesDialog.Hidden += OnPreferencesDialogHidden;
			}
			
			preferencesDialog.Present ();
		}
		
		private void OnPreferencesDialogHidden (object sender, EventArgs args)
		{
			preferencesDialog.Destroy ();
			preferencesDialog = null;
		}
		
		public static void ShowPreferences()
		{
			application.OnPreferences(null, EventArgs.Empty);
		}

		private void OnAbout (object sender, EventArgs args)
		{
			string [] authors = new string [] {
				"Boyd Timothy <btimothy@gmail.com>",
				"Calvin Gaisford <calvinrg@gmail.com>",
				"Sandy Armstrong <sanfordarmstrong@gmail.com>",
				"Brian G. Merrell <bgmerrell@novell.com>"
			};

			/* string [] documenters = new string [] {
			   "Calvin Gaisford <calvinrg@gmail.com>"
			   };
			   */

			string translators = Catalog.GetString ("translator-credits");
			if (translators == "translator-credits")
				translators = null;
			
			Gtk.AboutDialog about = new Gtk.AboutDialog ();
			about.ProgramName = "Tasque";
			about.Version = GlobalDefines.Version;
			about.Logo = Utilities.GetIcon("tasque-48", 48);
			about.Copyright =
				Catalog.GetString ("Copyright \xa9 2008 Novell, Inc.");
			about.Comments = Catalog.GetString ("A Useful Task List");
			about.Website = "http://live.gnome.org/Tasque";
			about.WebsiteLabel = Catalog.GetString("Tasque Project Homepage");
			about.Authors = authors;
			//about.Documenters = documenters;
			about.TranslatorCredits = translators;
			about.IconName = "tasque";
			about.SetSizeRequest(300, 300);
			about.Run ();
			about.Destroy ();

		}


		private void OnShowTaskWindow (object sender, EventArgs args)
		{
			TaskWindow.ShowWindow();
		}
		
		private void OnNewTask (object sender, EventArgs args)
		{
			// Show the TaskWindow and then cause a new task to be created
			TaskWindow.ShowWindow ();
			TaskWindow.GrabNewTaskEntryFocus ();
		}

		private void OnQuit (object sender, EventArgs args)
		{
			Trace.TraceInformation ("OnQuit called - terminating application");
			if (backend != null) {
				UnhookFromTooltipTaskGroupModels ();
				backend.Cleanup();
			}
			TaskWindow.SavePosition();

			nativeApp.QuitMainLoop ();
		}
		
		private void OnRefreshAction (object sender, EventArgs args)
		{
			Application.Backend.Refresh();
		}

		private void OnTrayIconClick (object sender, EventArgs args) // handler for mouse click
		{
			TaskWindow.ToggleWindowVisible ();
		}

		

		public static void Main(string[] args)
		{
			try 
			{
				application = GetApplicationWithArgs(args);
				application.StartMainLoop ();
			} 
			catch (Exception e)
			{
				Debug.WriteLine("Exception is: {0}", e);
				Instance.NativeApplication.Exit (-1);
			}
		}

		public static Application GetApplicationWithArgs(string[] args)
		{
			lock(locker)
			{
				if(application == null)
				{
					lock(locker)
					{
						application = new Application(args);
					}
				}
				return application;
			}
		}

#if ENABLE_NOTIFY_SHARP
		public static void ShowAppNotification(Notification notification)
		{
			// TODO: Use this API for newer versions of notify-sharp
			//notification.AttachToStatusIcon(
			//		Tasque.Application.Instance.trayIcon);
			notification.Show();
		}
#endif

		public void StartMainLoop ()
		{
			nativeApp.StartMainLoop ();
		}

		public void Quit ()
		{
			OnQuit (null, null);
		}
	}
}
