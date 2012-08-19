// ITaskBackend.cs created with MonoDevelop
// User: boyd at 7:02 AM 2/11/2008
// 
// Backend.cs
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

namespace Tasque
{
	/// <summary>
	/// This is the main integration interface for different backends that
	/// Tasque can use.
	/// </summary>
	public abstract class Backend : IDisposable
	{
		protected Backend (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Name = name;
			
			Categories = new SortedNotifyCollection<Category> ();
			Tasks = new BackendTaskCollection (Categories);
		}

		#region Properties
		/// <value>
		/// This returns all the ICategory items from the backend.
		/// </value>
		public SortedNotifyCollection<Category> Categories { get; private set; }

		/// <value>
		/// Indication that the backend has enough information
		/// (credentials/etc.) to run.  If false, the properties dialog will
		/// be shown so the user can configure the backend.
		/// </value>
		public bool Configured { get; protected set; }
		
		//DOCS: must not be null; must be entailed in Categories
		public abstract Category DefaultCategory { get; set; }

		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public bool Initialized {
			get { return initialized; }
			protected set {
				if (value != initialized) {
					initialized = value;
					OnBackendInitialized ();
				}
			}
		}

		/// <value>
		/// A human-readable name for the backend that will be displayed in the
		/// preferences dialog to allow the user to select which backend Tasque
		/// should use.
		/// </value>
		public string Name { get; private set; }
		
		/// <summary>
		/// An object that provides a means of managing backend specific preferences.
		/// </summary>
		/// <returns>
		/// A <see cref="Tasque.Backends.BackendPreferences"/>
		/// </returns>
		public abstract IBackendPreferences Preferences { get; }

		/// <value>
		/// All the tasks provided by the backend.
		/// </value>
		public ReadOnlySortedNotifyCollection<Task> Tasks { get; private set; }
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (!disposed && disposing) {
				Tasks.Dispose ();
				disposed = true;
			}
		}
		
		~Backend ()
		{
			Dispose (false);
		}
		
		bool disposed;
		#endregion
		
		#region Methods
		/// <summary>
		/// Create a new task.
		/// </summary>
		public abstract Task CreateTask (string taskName);
		
		public void DeleteTaskFromAllCategories (Task task)
		{
			if (task == null)
				throw new ArgumentNullException ("task");
			
			foreach (var cat in Categories)
				cat.Remove (task);
		}
		
		/// <summary>
		/// Initializes the backend
		/// </summary>
		public abstract void Initialize ();

		/// <summary>
		/// Refreshes the backend.
		/// </summary>
		public virtual void Refresh () {}
		
		protected void OnBackendInitialized () {
			if (BackendInitialized != null)
				BackendInitialized (this, EventArgs.Empty);
		}

		protected void OnBackendSyncFinished () {
			if (BackendSyncFinished != null)
				BackendSyncFinished (this, EventArgs.Empty);
		}

		protected void OnBackendSyncStarted () {
			if (BackendSyncStarted != null)
				BackendSyncStarted (this, EventArgs.Empty);
		}
		#endregion

		public event EventHandler BackendInitialized;
		public event EventHandler BackendSyncFinished;
		public event EventHandler BackendSyncStarted;
		
		bool initialized;
	}
}
