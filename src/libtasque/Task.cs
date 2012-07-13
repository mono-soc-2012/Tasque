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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Tasque
{
	public abstract class Task : IComparable<Task>, INotifyPropertyChanged
	{
		protected Task (string name)
		{
			Name = name;
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
		
		public abstract DateTime DueDate { get; set; }
		
		public abstract DateTime CompletionDate { get; set; }

		public bool HasNotes { get { return Notes.Count > 0; } }

		public bool IsComplete { get { return State == TaskState.Completed; } }

		public bool IsCompletionDateSet { get { return CompletionDate != DateTime.MinValue; } }

		public bool IsDueDateSet { get { return DueDate != DateTime.MinValue; } }

		public abstract ReadOnlyObservableCollection<TaskNote> Notes { get; }

		public abstract TaskPriority Priority { get; set; }

		public abstract TaskState State { get; }

		public abstract TaskNoteSupport TaskNoteSupport { get; }
		#endregion
		
		#region Methods
		public abstract void Activate ();

		public void AddNote (TaskNote note)
		{
			if (note == null)
				throw new ArgumentNullException ("note");

			if (Notes.Contains (note))
				return;

			OnAddNote ();
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

		public abstract TaskNote CreateNote(string text);

		public abstract void Delete ();

		public abstract bool DeleteNote(TaskNote note);

		protected abstract void OnAddNote ();

		protected virtual void OnNameChanged () {}

		protected void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}
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

		string name;
	}
}
