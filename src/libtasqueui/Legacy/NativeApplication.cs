// 
// NativeApplication.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mono.Options;
using CrossCommand;
using System.ComponentModel;

namespace Tasque.UIModel.Legacy
{
	public abstract class NativeApplication : INotifyPropertyChanged, IDisposable
	{
		protected NativeApplication () : this (Path.Combine (Environment.GetFolderPath (
			Environment.SpecialFolder.ApplicationData), "tasque")) {}
		
		protected NativeApplication (string confDir)
		{
			if (confDir == null)
				throw new ArgumentNullException ("confDir");
			
			currentDay = DateTime.Today;
			
			ConfDir = confDir;
			if (!Directory.Exists (confDir))
				Directory.CreateDirectory (confDir);
			
			viewModelRoot = new ViewModelRoot ();
		}
		
		public Backend CurrentBackend { get; private set; }
		
		public ReadOnlyCollection<Backend> AvailableBackends { get; private set; }
		
		protected string ConfDir { get; private set; }
		
		public void Exit (int exitcode)
		{
			OnExit (exitcode);

			if (Exiting != null)
				Exiting (this, EventArgs.Empty);

			Environment.Exit (exitcode);
		}

		public void Initialize (string[] args)
		{
			if (IsRemoteInstanceRunning ()) {
				Trace.TraceInformation ("Another instance of Tasque is already running.");
				Exit (0);
			}
			
//			RemoteInstanceKnocked += delegate {
//				if (MainWindowModel != null && MainWindowModel.Show.CanExecute)
//					MainWindowModel.Show.Execute ();
//			};
			
			preferences = new Preferences (ConfDir);
			
			ParseArgs (args);
			
			SetCustomBackend ();
			
			// Discover all available backends
			LoadAvailableBackends ();
			
			OnInitialize ();
		}
		
		protected virtual void OnInitialize () {}

		public virtual void InitializeIdle ()
		{
			if (customBackend != null)
				CurrentBackend = customBackend;
			else {
				// Check to see if the user has a preference of which backend
				// to use.  If so, use it, otherwise, pop open the preferences
				// dialog so they can choose one.
				string backendTypeString = preferences.Get (Preferences.CurrentBackend);
				Debug.WriteLine ("CurrentBackend specified in Preferences: {0}", backendTypeString);
				if (backendTypeString != null && availableBackends.ContainsKey (backendTypeString))
					CurrentBackend = availableBackends [backendTypeString];
			}
			
//			SetupTray (new TrayModel ());
//			
//			if (CurrentBackend == null) {
//				// Pop open the preferences dialog so the user can choose a
//				// backend service to use.
//				Application.ShowPreferences ();
//			} else if (!quietStart) {
//				TaskWindow.ShowWindow ();
//			}
//			if (backend == null || !backend.Configured){
//				GLib.Timeout.Add(1000, new GLib.TimeoutHandler(RetryBackend));
//			}
//
//			nativeApp.InitializeIdle ();
			
//			return false;
		}
		
		protected abstract void SetupTray (TrayModel trayModel);
		
		protected virtual void OnExit (int exitCode) {}

		public virtual void OpenUrlInBrowser (string url)
		{
			Process.Start (url);
		}

		public abstract void QuitMainLoop ();

		public abstract void StartMainLoop ();

		public event EventHandler Exiting;

		//DOCS: Tasque is a single instance app. If Tasque is already started in the current user's
		// domain, don't start it again. Returns null if no instance found
		protected abstract bool IsRemoteInstanceRunning ();
		
		protected abstract event EventHandler RemoteInstanceKnocked;
		
		#region IDisposable implementation
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		protected virtual void Dispose (bool disposing) {}
		
		~NativeApplication ()
		{
			Dispose (false);
		}
		#endregion

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}
		#endregion
		
		/// <summary>
		/// Load all the available backends that Tasque can find.  First look in
		/// Tasque.exe and then for other DLLs in the same directory Tasque.ex
		/// resides.
		/// </summary>
		void LoadAvailableBackends ()
		{
			availableBackends = new Dictionary<string, Backend> ();
			var backends = new List<Backend> ();
			var tasqueAssembly = Assembly.GetCallingAssembly ();
			
			// Look for other backends in Tasque.exe
			backends.AddRange (GetBackendsFromAssembly (tasqueAssembly));
			
			// Look through the assemblies located in the same directory as Tasque.exe.
			Debug.WriteLine ("Tasque.exe location: {0}", tasqueAssembly.Location);
			
			var loadPathInfo = Directory.GetParent (tasqueAssembly.Location);
			Trace.TraceInformation ("Searching for Backend DLLs in: {0}", loadPathInfo.FullName);
			
			foreach (var fileInfo in loadPathInfo.GetFiles ("*.dll")) {
				Trace.TraceInformation ("\tReading {0}", fileInfo.FullName);
				Assembly asm = null;
				try {
					asm = Assembly.LoadFile (fileInfo.FullName);
				} catch (Exception e) {
					Debug.WriteLine ("Exception loading {0}: {1}", fileInfo.FullName, e.Message);
					continue;
				}
				
				backends.AddRange (GetBackendsFromAssembly (asm));
			}
			
			foreach (var backend in backends) {
				string typeId = backend.GetType ().ToString ();
				if (availableBackends.ContainsKey (typeId))
					continue;
				
				Debug.WriteLine ("Storing '{0}' = '{1}'", typeId, backend.Name);
				availableBackends [typeId] = backend;
			}
		}
		
		List<Backend> GetBackendsFromAssembly (Assembly asm)
		{
			var backends = new List<Backend> ();
			Type[] types = null;
			
			try {
				types = asm.GetTypes ();
			} catch (Exception e) {
				Trace.TraceWarning ("Exception reading types from assembly '{0}': {1}",
				                    asm.ToString (), e.Message);
				return backends;
			}
			
			foreach (var type in types) {
				if (!type.IsClass)
					continue; // Skip non-class types
				if (Type.GetType ("Tasque.Backend") == null)
					continue;
				
				Debug.WriteLine ("Found Available Backend: {0}", type.ToString ());
				
				Backend availableBackend = null;
				try {
					availableBackend = (Backend)asm.CreateInstance (type.ToString ());
				} catch (Exception e) {
					Trace.TraceWarning ("Could not instantiate {0}: {1}", type.ToString (), e.Message);
					continue;
				}
				
				if (availableBackend != null)
					backends.Add (availableBackend);
			}
			
			return backends;
		}
		
		void ParseArgs (string[] args)
		{
			bool showHelp = false;
			var p = new OptionSet () {
				{ "q|quiet", "hide the Tasque window upon start.", v => quietStart = true },
				{ "b|backend=", "the name of the {BACKEND} to use.", v => potentialBackendClassName = v },
				{ "h|help",  "show this message and exit.", v => showHelp = v != null },
			};

			List<string> extra;
			try {
				extra = p.Parse (args);
			} catch (OptionException e) {
				Console.Write ("tasque: ");
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try `tasque --help' for more information.");
				Exit (-1);
			}

			if (showHelp) {
				Console.WriteLine ("Usage: tasque [[-q|--quiet] [[-b|--backend] BACKEND]]");
				Console.WriteLine ();
				Console.WriteLine ("Options:");
				p.WriteOptionDescriptions (Console.Out);
				Exit (-1);
			}
		}
		
		void SetBackend (Backend value)
		{
			bool changingBackend = false;
//			if (this.backend != null) {
//				UnhookFromTooltipTaskGroupModels ();
//				changingBackend = true;
//				// Cleanup the old backend
//				try {
//					Debug.WriteLine ("Cleaning up backend: {0}",
//					              this.backend.Name);
//					this.backend.Cleanup ();
//				} catch (Exception e) {
//					Trace.TraceWarning ("Exception cleaning up '{0}': {1}",
//					             this.backend.Name,
//					             e);
//				}
//			}
				
			// Initialize the new backend
//			this.backend = value;
//			if (this.backend == null) {
//				RefreshTrayIconTooltip ();
//				return;
//			}
//				
//			Trace.TraceInformation ("Using backend: {0} ({1})",
//			             this.backend.Name,
//			             this.backend.GetType ().ToString ());
//			this.backend.Initialize();
//			
//			if (!changingBackend) {
//				TaskWindow.Reinitialize (!this.quietStart);
//			} else {
//				TaskWindow.Reinitialize (true);
//			}
//
//			RebuildTooltipTaskGroupModels ();
//			RefreshTrayIconTooltip ();
//			
//			Debug.WriteLine("Configuration status: {0}",
//			             this.backend.Configured.ToString());
		}

		void SetCustomBackend ()
		{
			// See if a specific backend is specified
			if (potentialBackendClassName != null) {
				Debug.WriteLine ("Backend specified: " + potentialBackendClassName);
				
				Assembly asm = Assembly.GetCallingAssembly ();
				try {
					customBackend = (Backend)asm.CreateInstance (potentialBackendClassName);
				} catch (Exception e) {
					Trace.TraceWarning ("Backend specified on args not found: {0}\n\t{1}",
						potentialBackendClassName, e.Message);
				}
			}
		}
		
		protected void CheckForDaySwitch ()
		{
			if (DateTime.Today != currentDay) {
				Debug.WriteLine ("Day has changed, reloading tasks");
				currentDay = DateTime.Today;
				
				// Reinitialize window according to new date
//				if (TaskWindow.IsOpen)
//					TaskWindow.Reinitialize (true);
				
//				UnhookFromTooltipTaskGroupModels ();
//				RebuildTooltipTaskGroupModels ();
//				RefreshTrayIconTooltip ();
			}
		}
		
		Preferences preferences;
		
		Backend customBackend;
		string potentialBackendClassName;
		bool quietStart;
		
		Dictionary<string, Backend> availableBackends;
		
		TrayModel tray;
		
		ViewModelRoot viewModelRoot;
		
		DateTime currentDay;
	}
}
