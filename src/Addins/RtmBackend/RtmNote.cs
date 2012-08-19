// RtmNote.cs created with MonoDevelop
// User: calvin at 11:05 AM 2/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using RtmNet;
using Tasque;

namespace Tasque.Backends.RtmBackend
{
	public class RtmNote : TaskNote
	{
		static string GetText (Note note)
		{
			if (note.Title != null && note.Title.Length > 0)
				note.Text = note.Title + note.Text;
			note.Title = string.Empty;
			return note.Text;
		}
		
		public RtmNote (Note note) : base (note.Text)
		{
			this.note = note;
		}
		
		public string ID { get { return note.ID; } }
		
		protected override void OnTextChanged ()
		{
			if (OnTextChangedAction != null)
				OnTextChangedAction ();
			base.OnTextChanged ();
		}
		
		internal Action OnTextChangedAction { get; set; }
		
		Note note;
	}
}
