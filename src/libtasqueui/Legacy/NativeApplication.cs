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

namespace Tasque.UIModel.Legacy
{
	public abstract class NativeApplication : IDisposable
	{
		protected NativeApplication ()
			: this (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "tasque")) {}
		
		protected NativeApplication (string confDir)
		{
			if (confDir == null)
				throw new ArgumentNullException ("confDir");
			
			ConfDir = confDir;
			if (!Directory.Exists (confDir))
				Directory.CreateDirectory (confDir);
		}
		
		public Backend CurrentBackend { get; }
		
		public ReadOnlyCollection<Backend> AvailableBackends { get; }
		
		public MainWindowModel MainWindowModel { get; }
		
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
			
			RemoteInstanceKnocked += delegate {
				if (MainWindowModel != null && MainWindowModel.Show.CanExecute)
					MainWindowModel.Show.Execute ();
			};
			
			preferences = new Preferences (ConfDir);
			
			ParseArgs (args);
			
			SetCustomBackend ();
			
			// Discover all available backends
			LoadAvailableBackends ();
			
			OnInitialize ();
		}
		
		protected virtual void OnInitialize () {}

		public virtual void InitializeIdle () {}

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
				Trace.TraceWarning ("Exception reading types from assembly '{0}': {1}", asm.ToString (), e.Message);
				return backends;
			}
			
			foreach (var type in types) {
				if (!type.IsClass)
					continue; // Skip non-class types
				if (type.GetInterface ("Tasque.Backends.IBackend") == null)
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
			bool showHelp;
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
		
		Preferences preferences;
		
		Backend customBackend;
		string potentialBackendClassName;
		bool quietStart;
		
		Dictionary<string, Backend> availableBackends;
	}
}
