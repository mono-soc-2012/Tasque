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
using System.Collections.Generic;
using System.Collections.Specialized;

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

			tasks = new SortedNotifyCollection<Task> ();
			Tasks = new ReadOnlySortedNotifyCollection<Task> (tasks);

			categoriesChangedSources = new List<INotifyCollectionChanged> ();
			Categories = new SortedNotifyCollection<Category> ();
			
			// create default category here, because it is required for the model to be there. Overwrite
			// default category preferably in child class constructor with more appropriate value
			var defaultCategory = new Category ("Default");
			Categories.Add (defaultCategory);
			DefaultCategory = defaultCategory;
			
			Categories.CollectionChanged += HandleCategoriesChanged;
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
		
		public Category DefaultCategory {
			get { return defaultCategory; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (!Categories.Contains (value))
					throw new ArgumentException ("Value must be an element of Backend.Categories.", "value");
				defaultCategory = value;
			}
		}

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
		
		protected virtual void Dispose (bool disposing) {}
		
		~Backend ()
		{
			Dispose (false);
		}
		#endregion
		
		#region Methods
		/// <summary>
		/// Create a new task.
		/// </summary>
		public Task CreateTask (string taskName, Category category)
		{
			if (category == null)
				throw new ArgumentNullException ("category");

			return CreateTask (taskName, new Category [] { category });
		}
		
		public Task CreateTask (string taskName, IEnumerable<Category> categories)
		{
			if (taskName == null)
				throw new ArgumentNullException ("taskName");
			if (categories == null)
				throw new ArgumentNullException ("categories");

			var task = CreateTaskCore (taskName);

			bool isEmpty = true;
			foreach (var item in categories) {
				Category cat = item;
				if (item == null)
					cat = defaultCategory;

				cat.Add (task);
				isEmpty = false;
			}

			if (isEmpty)
				throw new ArgumentException ("This backend doesn't contain a category. Hence it's " +
					"impossible to add an item.", "categories");

			return task;
		}
		
		public void DeleteTask (Task task)
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

		protected abstract Task CreateTaskCore (string taskName);
		
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
		
		void HandleCategoriesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
				var cat = (Category)e.NewItems [0];
				RegisterCategoriesChanged (cat);
				foreach (var item in cat)
					tasks.Add (item);
				break;
			}
			case NotifyCollectionChangedAction.Remove: {
				var cat = (Category)e.OldItems [0];
				UnRegisterCategoriesChanged (cat);
				RemoveCategoryContent (cat);
				break;
			}
			case NotifyCollectionChangedAction.Reset:
				for (int i = 0; i < categoriesChangedSources.Count; i++) {
					UnRegisterCategoriesChanged (categoriesChangedSources [0]);
				}
				tasks.Clear ();
				break;
			}
		}

		void HandleTaskCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				tasks.Add ((Task)e.NewItems [0]);
				break;
			case NotifyCollectionChangedAction.Remove:
				var task = (Task)e.OldItems [0];
				if (IsSoleOccurenceInCategory ((Category)sender, task))
					tasks.Remove (task);
				break;
			case NotifyCollectionChangedAction.Reset:
				RemoveCategoryContent ((Category)sender);
				break;
			}
		}

		bool IsSoleOccurenceInCategory (Category cat, Task task)
		{
			foreach (var item in Categories) {
				if (cat != item && cat.Contains (task))
					return false;
			}
			return true;
		}

		void RemoveCategoryContent (Category cat)
		{
			foreach (var item in cat) {
				if (IsSoleOccurenceInCategory (cat, item))
					tasks.Remove (item);
			}
		}

		void RegisterCategoriesChanged (INotifyCollectionChanged source)
		{
			categoriesChangedSources.Add (source);
			source.CollectionChanged += HandleTaskCollectionChanged;
		}

		void UnRegisterCategoriesChanged (INotifyCollectionChanged source)
		{
			source.CollectionChanged -= HandleTaskCollectionChanged;
			categoriesChangedSources.Remove (source);
		}

		List<INotifyCollectionChanged> categoriesChangedSources;
		Category defaultCategory;
		SortedNotifyCollection<Task> tasks;
		bool initialized;
	}
}
