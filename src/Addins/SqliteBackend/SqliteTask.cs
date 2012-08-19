// SqliteTask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008
using System;
using System.Diagnostics;

namespace Tasque.Backends.Sqlite
{
	public class SqliteTask : Task
	{
		public SqliteTask (SqliteBackend backend, string name)
			: base (backend.SanitizeText (name), TaskNoteSupport.Multiple)
		{
			if (backend == null)
				throw new ArgumentNullException ("backend");
			
			name = backend.SanitizeText (name);
			this.backend = backend;
			Debug.WriteLine ("Creating New Task Object : {0} (id={1})", name, id);
			var dueDate = Database.FromDateTime (DueDate);
			var completionDate = Database.FromDateTime (CompletionDate);
			var priority = (int)Priority;
			var state = (int)State;
			var command = string.Format ("INSERT INTO Tasks (Name, DueDate, CompletionDate, Priority, State, Category, ExternalID) values ('{0}','{1}', '{2}','{3}', '{4}', '{5}', '{6}'); SELECT last_insert_rowid();", 
								name, dueDate, completionDate, priority, state, 0, string.Empty);
			id = Convert.ToInt32 (backend.Database.ExecuteScalar (command));
		}

		public SqliteTask (SqliteBackend backend, int id, int category, string name, 
		                   long dueDate, long completionDate, int priority, int state)
			: base (name, TaskNoteSupport.Multiple)
		{
			this.backend = backend;
			this.id = id;
			DueDate = Database.ToDateTime (dueDate);
			CompletionDate = Database.ToDateTime (completionDate);
			Priority = (TaskPriority)priority;
			State = (TaskState)state;
		}
		
		protected override void OnNameChanged ()
		{
			if (backend == null)
				return;
			
			var name = backend.SanitizeText (Name);
			if (name != Name) {
				Name = name;
				return;
			}
			
			var command = string.Format ("UPDATE Tasks set Name='{0}' where ID='{1}'", name, id);
			backend.Database.ExecuteScalar (command);
			base.OnNameChanged ();
		}
		
		protected override void OnDueDateChanged ()
		{
			var dueDate = Database.FromDateTime (DueDate);
			var command = string.Format ("UPDATE Tasks set DueDate='{0}' where ID='{1}'", dueDate, id);
			backend.Database.ExecuteScalar (command);
			base.OnDueDateChanged ();
		}
		
		protected override void OnCompletionDateChanged ()
		{
			var completionDate = Database.FromDateTime (CompletionDate);
			var command = string.Format ("UPDATE Tasks set CompletionDate='{0}' where ID='{1}'",
			                             completionDate, id);
			backend.Database.ExecuteScalar (command);
			base.OnCompletionDateChanged ();
		}
		
		protected override void OnPriorityChanged ()
		{
			var priority = (int)Priority;
			var command = string.Format ("UPDATE Tasks set Priority='{0}' where ID='{1}'", priority, id);
			backend.Database.ExecuteScalar (command);
			base.OnPriorityChanged ();
		}
		
		protected override void OnStateChanged ()
		{
			var state = (int)State;
			var command = string.Format ("UPDATE Tasks set State='{0}' where ID='{1}'", state, id);
			backend.Database.ExecuteScalar (command);
			base.OnStateChanged ();
		}
			
		public override TaskNote CreateNote (string text)
		{
			Debug.WriteLine ("Creating New Note Object : {0} (id={1})", text, id);
			text = backend.SanitizeText (text);
			var command = String.Format (
				"INSERT INTO Notes (Task, Text) VALUES ('{0}','{1}'); SELECT last_insert_rowid();",
				id, text);
			var taskId = Convert.ToInt32 (backend.Database.ExecuteScalar (command));

			var note = new SqliteNote (taskId, text);
			note.OnTextChangedAction = delegate { SaveNote (note); };
			return note;
		}
		
		protected override void OnRemoveNote (TaskNote note)
		{
			var sqNote = note as SqliteNote;
			var command = string.Format ("DELETE FROM Notes WHERE ID='{0}'", sqNote.Id);
			backend.Database.ExecuteScalar (command);
			base.OnRemoveNote (note);
		}
				
		void SaveNote (TaskNote note)
		{
			var sqNote = note as SqliteNote;
			var text = backend.SanitizeText (sqNote.Text);
			var command = string.Format ("UPDATE Notes SET Text='{0}' WHERE ID='{1}'", text, sqNote.Id);
			backend.Database.ExecuteScalar (command);
		}
		
		SqliteBackend backend;
		int id;
	}
}
