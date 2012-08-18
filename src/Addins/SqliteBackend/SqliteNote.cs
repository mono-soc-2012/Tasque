// SqliteNote.cs created with MonoDevelop
// User: calvin at 10:56 AM 2/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;

namespace Tasque.Backends.Sqlite
{
	public class SqliteNote : TaskNote
	{
		public SqliteNote (int id, string text) : base (text)
		{
			Id = id;
		}

		public int Id { get; private set; }
		
		protected override void OnTextChanged ()
		{
			if (OnTextChangedAction != null)
				OnTextChangedAction ();
			base.OnTextChanged ();
		}
		
		internal Action OnTextChangedAction { get; set; }
	}
}
