// SqliteCategory.cs created with MonoDevelop
// User: boyd at 9:06 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;

namespace Tasque.Backends.Sqlite
{
	public class SqliteCategory : Category
	{
		public SqliteCategory (SqliteBackend backend, string name) : base (name)
		{
			var command = string.Format (
				"INSERT INTO Categories (Name, ExternalID) values ('{0}', '{1}'); SELECT last_insert_rowid();",
				name, string.Empty);
			backend.Database.ExecuteScalar (command);
			//Debug.WriteLine("Inserted category named: {0} with id {1}", name, id);
		}
		
		public SqliteCategory (string name) : base (name) {}
	}
}
