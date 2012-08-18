// SqliteTask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Data.Sqlite;

namespace Tasque.Backends.Sqlite
{
	public class SqliteTask : Task
	{
		public SqliteTask (SqliteBackend backend, string name)
			: base (backend.SanitizeText (name), TaskNoteSupport.Multiple)
		{
			name = backend.SanitizeText (name);
			this.backend = backend;
			Debug.WriteLine ("Creating New Task Object : {0} (id={1})", name, id);
			var dueDate = Database.FromDateTime (DueDate);
			var completionDate = Database.FromDateTime (CompletionDate);
			var category = 0;
			var priority = (int)Priority;
			var state = (int)State;
			var command = string.Format ("INSERT INTO Tasks (Name, DueDate, CompletionDate, Priority, State, Category, ExternalID) values ('{0}','{1}', '{2}','{3}', '{4}', '{5}', '{6}'); SELECT last_insert_rowid();", 
								name, dueDate, completionDate, priority, state, category, string.Empty);
			id = Convert.ToInt32 (backend.Database.ExecuteScalar (command));
		}

		public SqliteTask (SqliteBackend backend, int id, int category, string name, 
		                   long dueDate, long completionDate, int priority, int state)
		{
			this.backend = backend;
			this.id = id;
			Name = name;
			DueDate = dueDate;
			CompletionDate = completionDate;
			Priority = priority;
			State = state;
		}		
		
		#region Public Properties
		
		public override string Id {
			get { return id.ToString (); }
		}

		internal int SqliteId {
			get { return id; } 
		}
		
		public override string Name {
			get { return this.name; }
			set {
				string name = backend.SanitizeText (value);
				this.name = name;
				string command = String.Format (
					"UPDATE Tasks set Name='{0}' where ID='{1}'",
					name,
					id
				);
				backend.Database.ExecuteScalar (command);
				backend.UpdateTask (this);
			}
		}
		
		public override DateTime DueDate {
			get { return Database.ToDateTime (this.dueDate); }
			set {
				this.dueDate = Database.FromDateTime (value);
				string command = String.Format (
					"UPDATE Tasks set DueDate='{0}' where ID='{1}'",
					this.dueDate,
					id
				);
				backend.Database.ExecuteScalar (command);
				backend.UpdateTask (this);				
			}
		}
		
		public override DateTime CompletionDate {
			get { return Database.ToDateTime (this.completionDate); }
			set {
				this.completionDate = Database.FromDateTime (value);
				string command = String.Format (
					"UPDATE Tasks set CompletionDate='{0}' where ID='{1}'",
					this.completionDate,
					id
				);
				backend.Database.ExecuteScalar (command);
				backend.UpdateTask (this);
			}
		}
		
		public override bool IsComplete {
			get {
				if (CompletionDate == DateTime.MinValue)
					return false;
				
				return true;
			}
		}
		
		public override TaskPriority Priority {
			get { return (TaskPriority)this.priority; }
			set {
				this.priority = (int)value;
				string command = String.Format (
					"UPDATE Tasks set Priority='{0}' where ID='{1}'",
					this.priority,
					id
				);
				backend.Database.ExecuteScalar (command);
				backend.UpdateTask (this);
			}
		}

		public override bool HasNotes {
			get {
				string command = String.Format (
					"SELECT COUNT(*) FROM Notes WHERE Task='{0}'",
					id
				);
				return backend.Database.GetSingleInt (command) > 0;
			}
		}
		
		public override bool SupportsMultipleNotes {
			get { return true; }
		}
		
		public override TaskState State {
			get { return LocalState; }
		}
		
		public TaskState LocalState {
			get { return (TaskState)this.state; }
			set {
				this.state = (int)value;
				string command = String.Format (
					"UPDATE Tasks set State='{0}' where ID='{1}'",
					this.state,
					id
				);
				backend.Database.ExecuteScalar (command);
				backend.UpdateTask (this);
			}
		}

		public override Category Category {
			get { return new SqliteCategory (backend, this.category); }
			set {
				this.category = (int)(value as SqliteCategory).ID;
				string command = String.Format (
					"UPDATE Tasks set Category='{0}' where ID='{1}'",
					category,
					id
				);
				backend.Database.ExecuteScalar (command);
				backend.UpdateTask (this);
			}
		}
		
		public override List<TaskNote> Notes {
			get {
				List<TaskNote> notes = new List<TaskNote> ();

				string command = String.Format (
					"SELECT ID, Text FROM Notes WHERE Task='{0}'",
					id
				);
				SqliteCommand cmd = backend.Database.Connection.CreateCommand ();
				cmd.CommandText = command;
				SqliteDataReader dataReader = cmd.ExecuteReader ();
				while (dataReader.Read()) {
					int taskId = dataReader.GetInt32 (0);
					string text = dataReader.GetString (1);
					notes.Add (new SqliteNote (taskId, text));
				}

				return notes;
			}
		}		
		
		#endregion // Public Properties
		
		#region Public Methods
		public override void Activate ()
		{
			// Debug.WriteLine ("SqliteTask.Activate ()");
			CompletionDate = DateTime.MinValue;
			LocalState = TaskState.Active;
			backend.UpdateTask (this);
		}
		
		public override void Inactivate ()
		{
			// Debug.WriteLine ("SqliteTask.Inactivate ()");
			CompletionDate = DateTime.Now;
			LocalState = TaskState.Inactive;
			backend.UpdateTask (this);
		}
		
		public override void Complete ()
		{
			//Debug.WriteLine ("SqliteTask.Complete ()");
			CompletionDate = DateTime.Now;
			LocalState = TaskState.Completed;
			backend.UpdateTask (this);
		}
		
		public override void Delete ()
		{
			//Debug.WriteLine ("SqliteTask.Delete ()");
			LocalState = TaskState.Deleted;
			backend.UpdateTask (this);
		}
		
		public override TaskNote CreateNote (string text)
		{
			Debug.WriteLine ("Creating New Note Object : {0} (id={1})", text, id);
			text = backend.SanitizeText (text);
			string command = String.Format (
				"INSERT INTO Notes (Task, Text) VALUES ('{0}','{1}'); SELECT last_insert_rowid();",
				id,
				text
			);
			int taskId = Convert.ToInt32 (backend.Database.ExecuteScalar (command));

			return new SqliteNote (taskId, text);
		}
		
		public override void DeleteNote (TaskNote note)
		{
			SqliteNote sqNote = (note as SqliteNote);

			string command = String.Format (
				"DELETE FROM Notes WHERE ID='{0}'",
				sqNote.ID
			);
			backend.Database.ExecuteScalar (command);
		}

		public override void SaveNote (TaskNote note)
		{
			SqliteNote sqNote = (note as SqliteNote);

			string text = backend.SanitizeText (sqNote.Text);
			string command = String.Format (
				"UPDATE Notes SET Text='{0}' WHERE ID='{1}'",
				text,
				sqNote.ID
			);
			backend.Database.ExecuteScalar (command);
		}

		#endregion // Public Methods
		
		SqliteBackend backend;
		int id;
	}
}
