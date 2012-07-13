// 
// Task.cs
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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Tasque
{
	public abstract class Task : IComparable<Task>, INotifyPropertyChanged
	{
		protected Task (string name, TaskNoteSupport noteSupport)
		{
			Name = name;
			notes = new ObservableCollection<TaskNote> ();
			Notes = new ReadOnlyObservableCollection<TaskNote> (notes);

			NoteSupport = noteSupport;
		}
		
		#region Properties
		public string Name {
			get { return name; }
			set {
				if (value == null)
					throw new ArgumentNullException ("name");

				if (value != name) {
					name = value;
					OnNameChanged ();
					OnPropertyChanged ("Name");
				}
			}
		}
		
		public DateTime DueDate
		{
			get { return dueDate; }
			set {
				if (value != dueDate) {
					Logger.Debug ("Setting new task due date");
					dueDate = value;
					OnDueDateChanged ();
					OnPropertyChanged ("DueDate");
				}
			}
		}
		
		public DateTime CompletionDate
		{
			get { return completionDate; }
			protected set {
				if (value != completionDate) {
					Logger.Debug ("Setting new task completion date");
					completionDate = value;
					OnCompletionDateChanged ();
					OnPropertyChanged ("CompletionDate");
				}
			}
		}

		public bool HasNotes { get { return Notes.Count > 0; } }

		public bool IsComplete { get { return State == TaskState.Completed; } }

		public bool IsCompletionDateSet { get { return CompletionDate != DateTime.MinValue; } }

		public bool IsDueDateSet { get { return DueDate != DateTime.MinValue; } }

		public ReadOnlyObservableCollection<TaskNote> Notes { get; private set; }

		public TaskPriority Priority
		{
			get { return priority; }
			set {
				if (value != priority) {
					Logger.Debug ("Setting new task priority");
					priority = value;
					OnPriorityChanged ();
					OnPropertyChanged ("Priority");
				}
			}
		}

		public TaskState State {
			get { return state; }
			protected set {
				if (state != value) {
					state = value;
					OnStateChanged ();
					OnPropertyChanged ("State");
				}
			}
		}

		public TaskNoteSupport NoteSupport { get; private set; }
		#endregion
		
		#region Methods
		public abstract void Activate ();

		public void AddNote (TaskNote note)
		{
			if (note == null)
				throw new ArgumentNullException ("note");

			if (notes.Contains (note))
				return;

			OnAddNote (note);
			notes.Add (note);
		}

		public int CompareTo (Task task)
		{
			if (task == null)
				return 1;

			bool isSameDate = true;
			if (DueDate.Year != task.DueDate.Year || DueDate.DayOfYear != task.DueDate.DayOfYear)
				isSameDate = false;
			
			if (!isSameDate) {
				if (DueDate == DateTime.MinValue) {
					// No due date set on this task. Since we already tested to see
					// if the dates were the same above, we know that the passed-in
					// task has a due date set and it should be "higher" in a sort.
					return 1;
				} else if (task.DueDate == DateTime.MinValue) {
					// This task has a due date and should be "first" in sort order.
					return -1;
				}
				
				int result = DueDate.CompareTo (task.DueDate);
				
				if (result != 0)
					return result;
			}
			
			// The due dates match, so now sort based on priority and name
			return CompareToByPriorityAndName (task);
		}

		public abstract void Complete ();

		public abstract void Delete ();

		public bool RemoveNote (TaskNote note)
		{
			if (notes.Contains (note))
				OnRemoveNote (note);

			return notes.Remove (note);
		}

		protected virtual void OnAddNote (TaskNote note) {}

		protected virtual void OnCompletionDateChanged () {}

		protected virtual void OnDueDateChanged () {}

		protected virtual void OnNameChanged () {}

		protected virtual void OnPriorityChanged () {}

		protected void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}

		protected virtual void OnRemoveNote (TaskNote note) {}

		protected virtual void OnStateChanged () {}
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		internal int CompareToByPriorityAndName (Task task)
		{
			// The due dates match, so now sort based on priority
			if (Priority != task.Priority) {
				switch (Priority) {
				case TaskPriority.High:
					return -1;
				case TaskPriority.Medium:
					if (task.Priority == TaskPriority.High)
						return 1;
					else
						return -1;
				case TaskPriority.Low:
					if (task.Priority == TaskPriority.None)
						return -1;
					else
						return 1;
				case TaskPriority.None:
					return 1;
				}
			}
			
			// Due dates and priorities match, now sort by name
			return Name.CompareTo (task.Name);
		}

		DateTime completionDate;
		DateTime dueDate;
		string name;
		ObservableCollection<TaskNote> notes;
		TaskPriority priority;
		TaskState state;
	}
}
