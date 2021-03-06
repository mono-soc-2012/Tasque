// NoteDialog.cs created with MonoDevelop
// User: boyd at 5:24 PM 2/13/2008
using System;
using System.Diagnostics;
using Mono.Unix;

namespace Tasque
{
	public class NoteDialog : Gtk.Dialog
	{
		private Task task;
		
		Gtk.VBox targetVBox;
		Gtk.Button addButton = new Gtk.Button(Gtk.Stock.Add);
		Gtk.ScrolledWindow sw = new Gtk.ScrolledWindow ();
		
		#region Constructors
		public NoteDialog (Gtk.Window parentWindow, Task task)
			: base ()
		{
			this.ParentWindow = parentWindow.GdkWindow;
			this.task = task;
			this.Title = String.Format(Catalog.GetString("Notes for: {0:s}"), task.Name);
			this.HasSeparator = false;
			this.SetSizeRequest(500,320);
			this.Icon = Utilities.GetIcon ("tasque-16", 16);
			//this.Flags = Gtk.DialogFlags.DestroyWithParent;
			
			
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.HscrollbarPolicy = Gtk.PolicyType.Never;

			sw.BorderWidth = 0;
			sw.CanFocus = true;
			sw.Show ();
			
			Gtk.EventBox innerEb = new Gtk.EventBox();
			innerEb.BorderWidth = 0;


			targetVBox = new Gtk.VBox();
			targetVBox.BorderWidth = 5;
			targetVBox.Show ();
			innerEb.Add(targetVBox);
			innerEb.Show ();
			
			if(task.Notes != null) {
				foreach (TaskNote note in task.Notes) {
					NoteWidget noteWidget = new NoteWidget (note);
					noteWidget.TextChanged += OnNoteTextChanged;
					noteWidget.DeleteButtonClicked += OnDeleteButtonClicked;
					noteWidget.EditCanceled += OnEditCanceled;
					noteWidget.Show ();
					targetVBox.PackStart (noteWidget, false, false, 0);
				}
			}
			
			sw.AddWithViewport(innerEb);
			sw.Show ();
			
			VBox.PackStart (sw, true, true, 0);

			if(task.NoteSupport == TaskNoteSupport.Multiple) {
				addButton = new Gtk.Button(Gtk.Stock.Add);
				addButton.Show();
				this.ActionArea.PackStart(addButton);
				addButton.Clicked += OnAddButtonClicked;
			}
			
			AddButton (Gtk.Stock.Close, Gtk.ResponseType.Close);
					
			Response += delegate (object sender, Gtk.ResponseArgs args) {
				// Hide the window.  The TaskWindow watches for when the
				// dialog is hidden and will take care of the rest.
				Hide ();
			};
		}
		#endregion // Constructors
		
		#region Properties
		public Task Task
		{
			get { return task; }
		}
		#endregion // Properties
		
		#region Public Methods
		public void CreateNewNote()
		{
			Debug.WriteLine("Creating a new note");
			NoteWidget noteWidget = new NoteWidget (null);
			noteWidget.TextChanged += OnNoteTextChanged;
			noteWidget.DeleteButtonClicked += OnDeleteButtonClicked;
			noteWidget.EditCanceled += OnEditCanceled;
			targetVBox.PackStart (noteWidget, false, false, 0);
			noteWidget.FocusTextArea ();
			noteWidget.SaveButton.Sensitive = false;
			noteWidget.Show ();
		}
		#endregion // Public Methods
		
		#region Private Method
		#endregion // PrivateMethods
		
		#region Event Handlers
		void OnAddButtonClicked (object sender, EventArgs args)
		{
			this.CreateNewNote();

			// scrolling to the bottom should work fine to make the
			// new note visible to the user
			sw.Vadjustment.Value = sw.Vadjustment.Upper;
		}
		
		void OnDeleteButtonClicked (object sender, EventArgs args)
		{
			NoteWidget nWidget = sender as NoteWidget;
			try {
				task.RemoveNote (nWidget.Note);
				targetVBox.Remove (nWidget);
			} catch(Exception e) {
				Debug.WriteLine("Unable to delete the note");
				Debug.WriteLine(e.ToString());
			}
		}

		void OnEditCanceled (object sender, EventArgs args)
		{
			NoteWidget nWidget = sender as NoteWidget;
			// remove the note widget if it's empty
			if (nWidget.Text == String.Empty) 
				targetVBox.Remove (nWidget);
		}

		void OnNoteTextChanged (object sender, EventArgs args)
		{
			NoteWidget nWidget = sender as NoteWidget;

			// if null, add a note
			if (nWidget.Note == null) {
				try {
					var note = task.CreateNote (nWidget.Text);
					task.AddNote (note);
					nWidget.Note = note;
				} catch(Exception e) {
					Debug.WriteLine("Unable to create a note");
					Debug.WriteLine(e.ToString());
				}
			}
		}
		#endregion // Event Handlers
	}
}
