// RtmBackend.cs created with MonoDevelop
// User: boyd at 7:10 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using Mono.Unix;
using RtmNet;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Net;

namespace Tasque.Backends.RtmBackend
{
	public class RtmBackend : Backend
	{
		public RtmBackend () : base ("Remember the Milk")
		{
			taskLock = new Object ();
			catLock = new Object ();

			runRefreshEvent = new AutoResetEvent (false);
			runningRefreshThread = false;
			refreshThread = new Thread (RefreshThreadLoop);
		}
		
		public override Category DefaultCategory {
			get {
				if (!Initialized)
					throw new InvalidOperationException ("Backend not initialized");
				return Categories.ElementAt (0);
			}
			set { throw new NotSupportedException (); }
		}
		
		public string RtmUserName {
			get {
				if (rtmAuth != null && rtmAuth.User != null)
					return rtmAuth.User.Username;
				else
					return null;
			}
		}

		#region Public Methods
		public override Task CreateTask (string taskName)
		{
			throw new NotImplementedException ();
//			var category = categories.ElementAt (0);
//			var categoryID = (category as RtmCategory).ID;
//			RtmTask rtmTask = null;
//
//			if (rtm != null) {
//				try {
//					var list = rtm.TasksAdd (timeline, taskName, categoryID);
//					var ts = list.TaskSeriesCollection [0];
//					if (ts != null)
//						rtmTask = new RtmTask (ts, this, list.ID);
//				} catch (Exception e) {
//					Debug.WriteLine ("Unable to set create task: " + taskName);
//					Debug.WriteLine (e.ToString ());
//				}
//			} else
//				throw new Exception ("Unable to communicate with Remember The Milk");
//				
//			return rtmTask;
		}
		
//		protected override void OnDeleteTask (Task task)
//		{
//			var rtmTask = task as RtmTask;
//			if (rtm != null) {
//				try {
//					rtm.TasksDelete (timeline, rtmTask.ListID, rtmTask.SeriesTaskID, rtmTask.TaskTaskID);
//				} catch (Exception e) {
//					Debug.WriteLine ("Unable to delete task: " + task.Name);
//					Debug.WriteLine (e.ToString ());
//				}
//			} else
//				throw new Exception ("Unable to communicate with Remember The Milk");
//			base.OnDeleteTask (task);
//		}
		
		public override void Refresh ()
		{
			Debug.WriteLine ("Refreshing data...");

			if (!runningRefreshThread)
				StartThread ();

			runRefreshEvent.Set ();
			
			Debug.WriteLine ("Done refreshing data!");
		}
		
		public override void Initialize ()
		{
			// *************************************
			// AUTHENTICATION to Remember The Milk
			// *************************************
			var authToken = Application.Instance.Preferences.Get (Tasque.Preferences.AuthTokenKey);
			if (authToken != null) {
				Debug.WriteLine ("Found AuthToken, checking credentials...");
				try {
					rtm = new Rtm (ApiKey, SharedSecret, authToken);
					rtmAuth = rtm.AuthCheckToken (authToken);
					timeline = rtm.TimelineCreate ();
					Debug.WriteLine ("RTM Auth Token is valid!");
					Debug.WriteLine ("Setting configured status to true");
					Configured = true;
				} catch (RtmApiException e) {
					Application.Instance.Preferences.Set (Tasque.Preferences.AuthTokenKey, null);
					Application.Instance.Preferences.Set (Tasque.Preferences.UserIdKey, null);
					Application.Instance.Preferences.Set (Tasque.Preferences.UserNameKey, null);
					rtm = null;
					rtmAuth = null;
					Trace.TraceError ("Exception authenticating, reverting" + e.Message);
				} catch (RtmWebException e) {
					rtm = null;
					rtmAuth = null;
					Trace.TraceError ("Not connected to RTM, maybe proxy: #{0}", e.Message);
				} catch (WebException e) {
					rtm = null;
					rtmAuth = null;
					Trace.TraceError ("Problem connecting to internet: #{0}", e.Message);
				}
			}

			if (rtm == null)
				rtm = new Rtm (ApiKey, SharedSecret);
	
			StartThread ();	
		}

		public void StartThread ()
		{
			if (!Configured) {
				Debug.WriteLine ("Backend not configured, not starting thread");
				return;
			}
			runningRefreshThread = true;
			Debug.WriteLine ("ThreadState: " + refreshThread.ThreadState);
			if (refreshThread.ThreadState == System.Threading.ThreadState.Running)
				Debug.WriteLine ("RtmBackend refreshThread already running");
			else {
				if (!refreshThread.IsAlive)
					refreshThread = new Thread (RefreshThreadLoop);
				refreshThread.Start ();
			}
			runRefreshEvent.Set ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				runningRefreshThread = false;
				runRefreshEvent.Set ();
				refreshThread.Abort ();
			}
			base.Dispose (disposing);
		}

		public override IBackendPreferences Preferences { get { return new RtmPreferencesWidget (); } }

		public string GetAuthUrl ()
		{
			frob = rtm.AuthGetFrob ();
			return rtm.AuthCalcUrl (frob, AuthLevel.Delete);
		}

		public void FinishedAuth ()
		{
			rtmAuth = rtm.AuthGetToken (frob);
			if (rtmAuth != null) {
				var prefs = Application.Instance.Preferences;
				prefs.Set (Tasque.Preferences.AuthTokenKey, rtmAuth.Token);
				if (rtmAuth.User != null) {
					prefs.Set (Tasque.Preferences.UserNameKey, rtmAuth.User.Username);
					prefs.Set (Tasque.Preferences.UserIdKey, rtmAuth.User.UserId);
				}
			}
			
			var authToken = Application.Instance.Preferences.Get (Tasque.Preferences.AuthTokenKey);
			if (authToken != null) {
				Debug.WriteLine ("Found AuthToken, checking credentials...");
				try {
					rtm = new Rtm (ApiKey, SharedSecret, authToken);
					rtmAuth = rtm.AuthCheckToken (authToken);
					timeline = rtm.TimelineCreate ();
					Debug.WriteLine ("RTM Auth Token is valid!");
					Debug.WriteLine ("Setting configured status to true");
					Configured = true;
					Refresh ();
				} catch (Exception e) {
					rtm = null;
					rtmAuth = null;				
					Trace.TraceError ("Exception authenticating, reverting" + e.Message);
				}	
			}
		}
		
		public void UpdateTaskName (RtmTask task)
		{
			if (rtm != null) {
				try {
					rtm.TasksSetName (timeline, task.ListID, task.SeriesTaskID,
					                  task.TaskTaskID, task.Name);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set name on task: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}
		}
		
		public void UpdateTaskDueDate (RtmTask task)
		{
			if (rtm != null) {
				try {
					if (task.DueDate == DateTime.MinValue)
						rtm.TasksSetDueDate (timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
					else	
						rtm.TasksSetDueDate (timeline, task.ListID, task.SeriesTaskID,
						                     task.TaskTaskID, task.DueDateString);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set due date on task: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}
		}
		
		public void UpdateTaskPriority (RtmTask task)
		{
			if (rtm != null) {
				try {
					rtm.TasksSetPriority (timeline, task.ListID, task.SeriesTaskID,
					                      task.TaskTaskID, task.PriorityString);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set priority on task: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}
		}
		
		public void UpdateTaskActive (RtmTask task)
		{
			if (rtm != null) {
				try {
					rtm.TasksUncomplete (timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set Task as completed: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}
		}

		public void UpdateTaskCompleted (RtmTask task)
		{
			if (rtm != null) {
				try {
					rtm.TasksComplete (timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set Task as completed: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}
		}

		public void UpdateTaskDeleted (RtmTask task)
		{
			if (rtm != null) {
				try {
					rtm.TasksDelete (timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set Task as deleted: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}
		}

		public void MoveTaskCategory (RtmTask task, string id)
		{
			if (rtm != null) {
				try {
					rtm.TasksMoveTo (timeline, task.ListID, id, task.SeriesTaskID, task.TaskTaskID);
				} catch (Exception e) {
					Debug.WriteLine ("Unable to set Task as completed: " + task.Name);
					Debug.WriteLine (e.ToString ());
				}
			}					
		}
		
		public RtmNote CreateNote (RtmTask rtmTask, string text)
		{
			Note note = null;
			RtmNote rtmNote = null;
			
			if (rtm != null) {
				try {
					note = rtm.NotesAdd (timeline, rtmTask.ListID, rtmTask.SeriesTaskID,
					                     rtmTask.TaskTaskID, string.Empty, text);
					rtmNote = new RtmNote (note);
					rtmNote.OnTextChangedAction = delegate { SaveNote (rtmTask, rtmNote); };
				} catch (Exception e) {
					Debug.WriteLine ("RtmBackend.CreateNote: Unable to create a new note");
					Debug.WriteLine (e.ToString ());
				}
			} else
				throw new Exception ("Unable to communicate with Remember The Milk");
				
			return rtmNote;
		}

		public void DeleteNote (RtmTask rtmTask, RtmNote note)
		{
			if (rtm != null) {
				try {
					rtm.NotesDelete (timeline, note.ID);
				} catch (Exception e) {
					Debug.WriteLine ("RtmBackend.DeleteNote: Unable to delete note");
					Debug.WriteLine (e.ToString ());
				}
			} else
				throw new Exception ("Unable to communicate with Remember The Milk");
		}

		void SaveNote (RtmTask rtmTask, RtmNote note)
		{
			if (rtm != null) {
				try {
					rtm.NotesEdit (timeline, note.ID, string.Empty, note.Text);
				} catch (Exception e) {
					Debug.WriteLine ("RtmBackend.SaveNote: Unable to save note");
					Debug.WriteLine (e.ToString ());
				}
			} else
				throw new Exception ("Unable to communicate with Remember The Milk");
		}
		#endregion // Public Methods
		
		#region Private Methods
		/// <summary>
		/// Update the model to match what is in RTM
		/// FIXME: This is a lame implementation and needs to be optimized
		/// </summary>		
		void UpdateCategories (Lists lists)
		{
			Debug.WriteLine ("RtmBackend.UpdateCategories was called");
			try {
				foreach (var list in lists.listCollection) {
					lock (catLock) {
						Application.Instance.Dispatcher.Invoke (delegate {
							RtmCategory rtmCategory;
							if (!Categories.Any (c => ((RtmCategory)c).ID == list.ID)) {
								rtmCategory = new RtmCategory (list);
								Categories.Add (rtmCategory);
							}
						});
					}
				}
			} catch (Exception e) {
				Debug.WriteLine ("Exception in fetch " + e.Message);
			}
			Debug.WriteLine ("RtmBackend.UpdateCategories is done");
		}

		/// <summary>
		/// Update the model to match what is in RTM
		/// FIXME: This is a lame implementation and needs to be optimized
		/// </summary>		
		void UpdateTasks (Lists lists)
		{
			Debug.WriteLine ("RtmBackend.UpdateTasks was called");
			try {
				foreach (var list in lists.listCollection) {
					Tasks tasks = null;
					try {
						tasks = rtm.TasksGetList (list.ID);
					} catch (Exception tglex) {
						Debug.WriteLine ("Exception calling TasksGetList(list.ListID) " + tglex.Message);
					}

					if (tasks != null) {
						foreach (var tList in tasks.ListCollection) {
							if (tList.TaskSeriesCollection == null)
								continue;
							
							foreach (var ts in tList.TaskSeriesCollection) {
								lock (taskLock) {
									Application.Instance.Dispatcher.Invoke (delegate {
										if (!Tasks.Any (t => ((RtmTask)t).ID == ts.TaskID)) {
											
											var rtmTask = new RtmTask (ts, this, list.ID);
											var cat = Categories.SingleOrDefault (
												c => ((RtmCategory)c).ID == list.ID);
											if (cat != null)
												cat.Add (rtmTask);
										}
									});
								}
							}
						}
					}
				}
			} catch (Exception e) {
				Debug.WriteLine ("Exception in fetch " + e.Message);
				Debug.WriteLine (e.ToString ());
			}
			Debug.WriteLine ("RtmBackend.UpdateTasks is done");			
		}
		
		void RefreshThreadLoop ()
		{
			while (runningRefreshThread) {
				runRefreshEvent.WaitOne ();

				if (!runningRefreshThread)
					return;
				
				// Fire the event on the main thread
				Application.Instance.Dispatcher.Invoke (delegate { OnBackendSyncStarted (); });

				if (rtmAuth != null) {
					var lists = rtm.ListsGetList ();
					UpdateCategories (lists);
					UpdateTasks (lists);
				}


				// Fire the event on the main thread
				Application.Instance.Dispatcher.Invoke (delegate { 
					if (!Initialized)
						Initialized = true;
				});
			}

			// Fire the event on the main thread
			Application.Instance.Dispatcher.Invoke (delegate { OnBackendSyncFinished (); });
		}
		#endregion // Private Methods
		
		const string ApiKey = "b29f7517b6584035d07df3170b80c430";
		const string SharedSecret = "93eb5f83628b2066";
		Thread refreshThread;
		bool runningRefreshThread;
		AutoResetEvent runRefreshEvent;
		Rtm rtm;
		string frob;
		Auth rtmAuth;
		string timeline;
		object taskLock, catLock;
	}
}

