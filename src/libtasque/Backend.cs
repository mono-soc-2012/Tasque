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
using CollectionTransforms;
using Tasque;
using System.ComponentModel;
using System;
using System.Collections.Generic;

namespace Tasque
{
	public delegate void BackendInitializedHandler ();
	public delegate void BackendSyncStartedHandler ();
	public delegate void BackendSyncFinishedHandler ();
	
	/// <summary>
	/// This is the main integration interface for different backends that
	/// Tasque can use.
	/// </summary>
	public abstract class Backend
	{
		protected Backend ()
		{
			tasks = new ObservableCollection<Task> ();
			Tasks = new ReadOnlyObservableCollection<Task> (tasks);
			var cvTasks = new CollectionView<Task> (Tasks);
			/*
			 * this invokes the default comparer, which in turn
			 * will use the IComparable implmentation of Task
			 */
			cvTasks.SortDescriptions.Add (new SortDescription ());
			SortedTasks = cvTasks;
			
			Categories = new ObservableCollection<ICategory> ();
			var cvCategories = new CollectionView<ICategory> (Categories);
			cvCategories.SortDescriptions.Add (new SortDescription ());
			SortedCategories = cvCategories;
		}
		
		public abstract event BackendInitializedHandler BackendInitialized;
		public abstract event BackendSyncStartedHandler BackendSyncStarted;
		public abstract event BackendSyncFinishedHandler BackendSyncFinished;

		#region Properties
		/// <value>
		/// A human-readable name for the backend that will be displayed in the
		/// preferences dialog to allow the user to select which backend Tasque
		/// should use.
		/// </value>
		public abstract string Name {
			get;
		}
		
		/// <value>
		/// All the tasks provided by the backend.
		/// </value>
		public ReadOnlyObservableCollection<Task> Tasks {	get; }

		public abstract IEnumerable SortedTasks { get; }
		
		/// <value>
		/// This returns all the ICategory items from the backend.
		/// </value>
		public abstract ObservableCollection<ICategory> Categories { get; }
		
		public abstract IEnumerable SortedCategories { get;	}
		
		public ICategory DefaultCategory {
			get { return defaultCategory; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				defaultCategory = value;
			}
		}
		
		/// <value>
		/// Indication that the backend has enough information
		/// (credentials/etc.) to run.  If false, the properties dialog will
		/// be shown so the user can configure the backend.
		/// </value>
		public abstract bool Configured {
			get;
		}
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public abstract bool Initialized {
			get;
		}
		
		/// <summary>
		/// An object that provides a means of managing backend specific preferences.
		/// </summary>
		/// <returns>
		/// A <see cref="Tasque.Backends.BackendPreferences"/>
		/// </returns>
		public abstract IBackendPreferences Preferences { get; }
		#endregion // Properties
		
		#region Methods
		public Task CreateTask (string taskName)
		{
			CreateTask (taskName, null);
		}
		
		/// <summary>
		/// Create a new task.
		/// </summary>
		public Task CreateTask (string taskName, ICategory category)
		{
			return CreateTask (taskName, new ICategory [] { category });
		}
		
		public Task CreateTask (string taskName, IEnumerable<ICategory> categories)
		{

		}
		
		/// <summary>
		/// Deletes the specified task.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		public abstract void DeleteTask (Task task);
		
		/// <summary>
		/// Refreshes the backend.
		/// </summary>
		public abstract void Refresh ();
		
		/// <summary>
		/// Initializes the backend
		/// </summary>
		public abstract void Initialize ();

		/// <summary>
		/// Cleanup the backend before quitting
		/// </summary>
		public abstract void Cleanup ();
		#endregion // Methods
		
		protected void AddTask (Task task, IEnumerable<ICategory> categories)
		{
			if (Categories.Count == 0)
				throw new InvalidOperationException ("Cannot add a task to a backend " +
					"which doesn't have any categories defined.");
			if (categories == null)
				throw new ArgumentNullException ("categories");
			if (task == null)
				throw new ArgumentNullException ("task");
			
			foreach (var cat in categories) {
				if (cat == null)
					cat = DefaultCategory;
				
				if (!Categories.Contains (cat))
					throw new ArgumentException (string.Format (
						"The provided category {0} has not been added to this backend.", cat.Name));
				
				
			}
		}
		
		ICategory defaultCategory;
		ObservableCollection<Task> tasks;
	}
}
