// Taken from TaskTreeView.cs created with MonoDevelop
// User: boyd on 2/9/2008
// 
// InactivateTimer.cs
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
using System.Diagnostics;
using Mono.Unix;

namespace Tasque
{
	/// <summary>
	/// Does the work of walking a task through the Inactive -> Complete
	/// states
	/// </summary>
	class InactivateTimer
	{
		/// <summary>
		/// Keep track of all the timers so that the pulseTimeoutId can
		/// be removed at the proper time.
		/// </summary>
		private static Dictionary<uint, InactivateTimer> timers;
			
		static InactivateTimer ()
		{
			timers = new Dictionary<uint,InactivateTimer> ();
		}
			
		private TaskTreeView tree;
		private Task task;
		private uint delay;
		private uint secondsLeft;
		protected uint pulseTimeoutId;
		private uint secondTimerId;
		private Gtk.TreeIter iter;
		private Gtk.TreePath path;
			
		public InactivateTimer (TaskTreeView treeView,
									Gtk.TreeIter taskIter,
									Task taskToComplete,
									uint delayInSeconds)
		{
			tree = treeView;
			iter = taskIter;
			path = treeView.Model.GetPath (iter);
			task = taskToComplete;
			secondsLeft = delayInSeconds;
			delay = delayInSeconds * 1000; // Convert to milliseconds
			pulseTimeoutId = 0;
		}
			
		public bool Paused {
			get;
			set;
		}

		public void StartTimer ()
		{
			pulseTimeoutId = GLib.Timeout.Add (500, PulseAnimation);
			StartSecondCountdown ();
			task.TimerID = GLib.Timeout.Add (delay, CompleteTask);
			timers [task.TimerID] = this;
		}

		public static void ToggleTimer (Task task)
		{
			InactivateTimer timer = null;
			if (timers.TryGetValue (task.TimerID, out timer))
				timer.Paused = !timer.Paused;
		}

		public static void CancelTimer (Task task)
		{
			Debug.WriteLine ("Timeout Canceled for task: " + task.Name);
			InactivateTimer timer = null;
			uint timerId = task.TimerID;
			if (timerId != 0) {
				if (timers.ContainsKey (timerId)) {
					timer = timers [timerId];
					timers.Remove (timerId);
				}
				GLib.Source.Remove (timerId);
				GLib.Source.Remove (timer.pulseTimeoutId);
				timer.pulseTimeoutId = 0;
				task.TimerID = 0;
			}
				
			if (timer != null) {
				GLib.Source.Remove (timer.pulseTimeoutId);
				timer.pulseTimeoutId = 0;
				GLib.Source.Remove (timer.secondTimerId);
				timer.secondTimerId = 0;
				timer.Paused = false;
			}
		}
			
		private bool CompleteTask ()
		{
			if (!Paused) {
				GLib.Source.Remove (pulseTimeoutId);
				if (timers.ContainsKey (task.TimerID))
					timers.Remove (task.TimerID);
					
				if (task.State != TaskState.Inactive)
					return false;
						
				task.Complete ();
				TaskTreeView.ShowCompletedTaskStatus ();
				tree.Refilter ();
				return false; // Don't automatically call this handler again
			}

			return true;
		}
			
		private bool PulseAnimation ()
		{
			if (tree.Model == null) {
				// Widget has been closed, no need to call this again
				return false;
			} else {
				if (!Paused) {
					// Emit this signal to cause the TreeView to update the row
					// where the task is located.  This will allow the
					// CellRendererPixbuf to update the icon.
					tree.Model.EmitRowChanged (path, iter);
						
					// Return true so that this method will be called after an
					// additional timeout duration has elapsed.
					return true;
				}
			}

			return true;
		}

		private void StartSecondCountdown ()
		{
			SecondCountdown ();
			secondTimerId = GLib.Timeout.Add (1000, SecondCountdown);
		}

		private bool SecondCountdown ()
		{
			if (tree.Model == null) {
				// Widget has been closed, no need to call this again
				return false;
			}
			if (!Paused) {
				if (secondsLeft > 0 && task.State == TaskState.Inactive) {
					TaskTreeView.status = String.Format (
						Catalog.GetString ("Completing Task In: {0}"),
						secondsLeft--
					);
					TaskWindow.ShowStatus (TaskTreeView.status);
					return true;
				} else {
					return false;
				}
			}
			return true;
		}
	
	}
}

