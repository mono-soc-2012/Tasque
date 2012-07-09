// ITaskBackend.cs created with MonoDevelop
// User: boyd at 7:02 AM 2/11/2008
// 
// IBackend.cs
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
using System.Collections;
using System.Collections.ObjectModel;

namespace Tasque.Backends
{
	public delegate void BackendInitializedHandler ();
	public delegate void BackendSyncStartedHandler ();
	public delegate void BackendSyncFinishedHandler ();
	
	/// <summary>
	/// This is the main integration interface for different backends that
	/// Tasque can use.
	/// </summary>
	public interface IBackend
	{
		event BackendInitializedHandler BackendInitialized;
		event BackendSyncStartedHandler BackendSyncStarted;
		event BackendSyncFinishedHandler BackendSyncFinished;

		#region Properties
		/// <value>
		/// A human-readable name for the backend that will be displayed in the
		/// preferences dialog to allow the user to select which backend Tasque
		/// should use.
		/// </value>
		string Name {
			get;
		}
		
		/// <value>
		/// All the tasks provided by the backend.
		/// </value>
		ObservableCollection<ITask> Tasks {	get; }

		IEnumerable SortedTasks { get; }
		
		/// <value>
		/// This returns all the ICategory items from the backend.
		/// </value>
		ObservableCollection<ICategory> Categories { get; }
		
		IEnumerable SortedCategories { get;	}
		
		/// <value>
		/// Indication that the backend has enough information
		/// (credentials/etc.) to run.  If false, the properties dialog will
		/// be shown so the user can configure the backend.
		/// </value>
		bool Configured {
			get;
		}
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		bool Initialized {
			get;
		}
		
		/// <summary>
		/// An object that provides a means of managing backend specific preferences.
		/// </summary>
		/// <returns>
		/// A <see cref="Tasque.Backends.BackendPreferences"/>
		/// </returns>
		IBackendPreferences Preferences { get; }
		#endregion // Properties
		
		#region Methods
		/// <summary>
		/// Create a new task.
		/// </summary>
		ITask CreateTask (string taskName, ICategory category);

		/// <summary>
		/// Deletes the specified task.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		void DeleteTask (ITask task);
		
		/// <summary>
		/// Refreshes the backend.
		/// </summary>
		void Refresh ();
		
		/// <summary>
		/// Initializes the backend
		/// </summary>
		void Initialize ();

		/// <summary>
		/// Cleanup the backend before quitting
		/// </summary>
		void Cleanup ();
		#endregion // Methods
	}
}
