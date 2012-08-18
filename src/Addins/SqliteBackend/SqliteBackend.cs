// SqliteBackend.cs created with MonoDevelop
// User: boyd at 7:10 AM 2/11/2008
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using Mono.Unix;

namespace Tasque.Backends.Sqlite
{
	public class SqliteBackend : Backend
	{
		public SqliteBackend () : base (Catalog.GetString ("Local File"))
		{
			Configured = true;
		}
		
		public Database Database { get; private set; }
		
		protected override Task CreateTaskCore (string taskName, IEnumerable<Category> categories)
		{
			var index = 0;
			foreach (var cat in Categories) {
				if (cat == categories.ElementAt (0))
					break;
				index++;
			}
			return new SqliteTask (this, taskName, index);
		}
		
		protected override void OnDeleteTask (Task task)
		{
			task.Delete ();
			base.OnDeleteTask (task);
		}
		
		public override void Initialize ()
		{
			if (Database == null)
				Database = new Database ();
				
			Database.Open ();
			
			RefreshCategories ();
			RefreshTasks ();
		
			Initialized = true;		
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (Database != null)
					Database.Close ();
				Database = null;
				Initialized = false;
			}
			base.Dispose (disposing);
		}
		
		public override IBackendPreferences Preferences { get { return new SqlitePreferences (); } }

		/// <summary>
		/// Given some text to be input into the database, do whatever
		/// processing is required to make sure special characters are
		/// escaped, etc.
		/// </summary>
		internal string SanitizeText (string text)
		{
			return text.Replace ("'", "''");
		}
		
		public void RefreshCategories ()
		{
			SqliteCategory defaultCategory = null;
			SqliteCategory newCategory;
			var hasValues = false;
			
			var command = "SELECT Name FROM Categories";
			var cmd = Database.Connection.CreateCommand ();
			cmd.CommandText = command;
			var dataReader = cmd.ExecuteReader ();
			var isFirstEntry = true;
			while (dataReader.Read()) {
				var name = dataReader.GetString (0);
				hasValues = true;
				
				newCategory = new SqliteCategory (name);
				if (isFirstEntry) {
					defaultCategory = newCategory;
					isFirstEntry = false;
				}
				Categories.Add (newCategory);				
			}
			
			dataReader.Close ();
			cmd.Dispose ();

			if (!hasValues) {
				defaultCategory = new SqliteCategory (this, Catalog.GetString ("Work"));
				Categories.Add (defaultCategory);

				newCategory = new SqliteCategory (this, Catalog.GetString ("Personal"));
				Categories.Add (newCategory);
				
				newCategory = new SqliteCategory (this, Catalog.GetString ("Family"));
				Categories.Add (newCategory);	

				newCategory = new SqliteCategory (this, Catalog.GetString ("Project"));
				Categories.Add (newCategory);		
			}
			
			// remove fake default category
			Categories.Remove (DefaultCategory);
			DefaultCategory = defaultCategory;
		}

		public void RefreshTasks ()
		{
			SqliteTask newTask;
			var hasValues = false;

			var command = "SELECT id,Category,Name,DueDate,CompletionDate,Priority, State FROM Tasks";
			var cmd = Database.Connection.CreateCommand ();
			cmd.CommandText = command;
			var dataReader = cmd.ExecuteReader ();
			while (dataReader.Read()) {
				var id = dataReader.GetInt32 (0);
				var category = dataReader.GetInt32 (1);
				var name = dataReader.GetString (2);
				var dueDate = dataReader.GetInt64 (3);
				var completionDate = dataReader.GetInt64 (4);
				var priority = dataReader.GetInt32 (5);
				var state = dataReader.GetInt32 (6);

				hasValues = true;
				
				newTask = new SqliteTask (this, id, category, name, dueDate,
				                          completionDate, priority, state);
				Categories.ElementAt (category).Add (newTask);
			}

			dataReader.Close ();
			cmd.Dispose ();

			if (!hasValues)
				CreateTask (Catalog.GetString ("Create some tasks"), DefaultCategory);
		}
	}
}
