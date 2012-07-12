// AbstractTask.cs created with MonoDevelop
// User: boyd at 6:52 AM 2/12/2008
// 
// AbstractTask.cs
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
using System.ComponentModel;

namespace Tasque
{
	public abstract class Task : IComparable<Task>, INotifyPropertyChanged
	{
		protected Task (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			
			Name = name;
		}
		
		uint timerID = 0;
		
		#region Properties
		public abstract string Id
		{
			get; 
		}

		public string Name { get; set; }
		
		public abstract DateTime DueDate {
			get;
			set;
		}
		
		public abstract DateTime CompletionDate
		{
			get;
			set;
		}
		
		public abstract bool IsComplete 
		{
			get;
		}
		
		public abstract TaskPriority Priority
		{
			get;
			set;
		}
		
		public abstract bool HasNotes
		{
			get;
		}
		
		public abstract bool SupportsMultipleNotes
		{
			get;
		}
		
		public abstract TaskState State
		{
			get;
		}
		
		public abstract List<INote> Notes
		{
			get;
		}

		/// <value>
		/// The ID of the timer used to complete a task after being marked
		/// inactive.
		/// </value>
		public uint TimerID
		{
			get { return timerID; }
			set { timerID = value; }
		}		
		#endregion // Properties
		
		#region Methods
		
		public abstract void Activate ();
		public abstract void Inactivate ();
		public abstract void Complete ();
		public abstract void Delete ();
		public abstract INote CreateNote(string text);
		public abstract void DeleteNote(INote note);
		public abstract void SaveNote(INote note);		
		
		public int CompareTo (Task task)
		{
			bool isSameDate = true;
			if (DueDate.Year != task.DueDate.Year
					|| DueDate.DayOfYear != task.DueDate.DayOfYear)
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
				
				if (result != 0) {
					return result;
				}
			}
			
			// The due dates match, so now sort based on priority and name
			return CompareByPriorityAndName (task);
		}
		
		public int CompareToByCompletionDate (Task task)
		{
			bool isSameDate = true;
			if (CompletionDate.Year != task.CompletionDate.Year
					|| CompletionDate.DayOfYear != task.CompletionDate.DayOfYear)
				isSameDate = false;
			
			if (!isSameDate) {
				if (CompletionDate == DateTime.MinValue) {
					// No completion date set for some reason.  Since we already
					// tested to see if the dates were the same above, we know
					// that the passed-in task has a CompletionDate set, so the
					// passed-in task should be "higher" in the sort.
					return 1;
				} else if (task.CompletionDate == DateTime.MinValue) {
					// "this" task has a completion date and should evaluate
					// higher than the passed-in task which doesn't have a
					// completion date.
					return -1;
				}
				
				return CompletionDate.CompareTo (task.CompletionDate);
			}
			
			// The completion dates are the same, so no sort based on other
			// things.
			return CompareByPriorityAndName (task);
		}
		#endregion // Methods

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}
		
		#region Private Methods
		
		int CompareByPriorityAndName (Task task)
		{
			// The due dates match, so now sort based on priority
			if (Priority != task.Priority) {
				switch (Priority) {
				case TaskPriority.High:
					return -1;
				case TaskPriority.Medium:
					if (task.Priority == TaskPriority.High) {
						return 1;
					} else {
						return -1;
					}
				case TaskPriority.Low:
					if (task.Priority == TaskPriority.None) {
						return -1;
					} else {
						return 1;
					}
				case TaskPriority.None:
					return 1;
				}
			}
			
			// Due dates and priorities match, now sort by name
			return Name.CompareTo (task.Name);
		}
		#endregion // Private Methods
		
		Backend backend;
	}
}
