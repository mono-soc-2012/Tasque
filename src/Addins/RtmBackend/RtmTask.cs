// Task.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008
using System;
using System.Collections.Generic;
using System.Diagnostics;
using RtmNet;

namespace Tasque.Backends.RtmBackend
{
	public class RtmTask : Task
	{
		RtmBackend rtmBackend;
		TaskSeries taskSeries;
		
		/// <summary>
		/// Constructor that is created from an RTM Task Series
		/// </summary>
		/// <param name="taskSeries">
		/// A <see cref="TaskSeries"/>
		/// </param>
		public RtmTask (TaskSeries taskSeries, RtmBackend be, string listId)
			: base (taskSeries.Name, TaskNoteSupport.Multiple)
		{
			ListID = listId;
			this.taskSeries = taskSeries;
			rtmBackend = be;
			DueDate = taskSeries.Task.Due;
			CompletionDate = taskSeries.Task.Completed;
			
			var priority = TaskPriority.None;
			switch (taskSeries.Task.Priority) {
			case "N":
				priority = TaskPriority.None;
				break;
			case "1":
				priority = TaskPriority.High;
				break;
			case "2":
				priority = TaskPriority.Medium;
				break;
			case "3":
				priority = TaskPriority.Low;
				break;
			}
			Priority = priority;
			
			if (taskSeries.Task.Completed == DateTime.MinValue)
				State = TaskState.Active;
			else
				State = TaskState.Completed;
			
			if (taskSeries.Notes.NoteCollection != null) {
				foreach (var note in taskSeries.Notes.NoteCollection) {
					var rtmNote = new RtmNote (note);
					AddNote (rtmNote);
				}
			}
		}
		
		public string ListID { get; private set; }
		
		protected override void OnNameChanged ()
		{
			var name = Name.Trim ();
			if (name != Name) {
				Name = name;
				return;
			}
			
			if (taskSeries != null) {
				taskSeries.Name = name;
				rtmBackend.UpdateTaskName (this);
			}
			base.OnNameChanged ();
		}
		
		protected override void OnDueDateChanged ()
		{
			taskSeries.Task.Due = DueDate;
			rtmBackend.UpdateTaskDueDate (this);		
			base.OnDueDateChanged ();
		}

		/// <value>
		/// Due Date for the task
		/// </value>
		public string DueDateString {
			get {
				// Return the due date in UTC format
				string format = "yyyy-MM-ddTHH:mm:ssZ";
				string dateString = taskSeries.Task.Due.ToUniversalTime ().ToString (format);
				return dateString;
			}
		}
		
		protected override void OnPriorityChanged ()
		{
			switch (Priority) {
			case TaskPriority.None:
				taskSeries.Task.Priority = "N";
				break;
			case TaskPriority.High:
				taskSeries.Task.Priority = "1";
				break;
			case TaskPriority.Medium:
				taskSeries.Task.Priority = "2";
				break;
			case TaskPriority.Low:
				taskSeries.Task.Priority = "3";
				break;
			}
			rtmBackend.UpdateTaskPriority (this);	
			base.OnPriorityChanged ();
		}
		
		public string PriorityString { get { return taskSeries.Task.Priority; } }
		
		/// <value>
		/// Holds the current RtmBackend for this task
		/// </value>
		public RtmBackend RtmBackend {
			get { return this.rtmBackend; }
		}
		
		public string ID {
			get { return taskSeries.TaskID; }
		}
		
		public string SeriesTaskID {
			get { return taskSeries.TaskID; }
		}
		
		public string TaskTaskID {
			get { return taskSeries.Task.TaskID; }
		}
		
		protected override void OnActivate ()
		{
			rtmBackend.UpdateTaskActive (this);
			base.OnActivate ();
		}
		
		protected override void OnComplete ()
		{
			rtmBackend.UpdateTaskCompleted (this);
			base.OnComplete ();
		}
		
		protected override void OnDelete ()
		{
			rtmBackend.UpdateTaskDeleted (this);
			base.OnDelete ();
		}
		
		public override TaskNote CreateNote (string text)
		{
			return rtmBackend.CreateNote (this, text);
		}
		
		protected override void OnRemoveNote (TaskNote note)
		{
			rtmBackend.DeleteNote (this, note as RtmNote);
			base.OnRemoveNote (note);
		}
	}
}
