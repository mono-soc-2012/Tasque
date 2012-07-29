// 
// GtkWinApplication.cs
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
using System.Diagnostics;
using System.Threading;

namespace Tasque
{
	public class GtkWinApplication : GtkApplicationBase
	{
		protected override void Dispose (bool disposing)
		{
			if (disposing && waitHandle != null) {
				waitHandle.Dispose ();
				waitHandle = null;
			}
			
			base.Dispose (disposing);
		}
		
		protected override bool IsRemoteInstanceRunning ()
		{
			try {
				waitHandle = EventWaitHandle.OpenExisting(waitHandleName);
				waitHandle.Set();
				return true;
			} catch (WaitHandleCannotBeOpenedException) {
				waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, waitHandleName);
				Debug.WriteLine("EventWaitHandle created.");
				
				var porter = new Thread(new ThreadStart(WaitForAnotherInstance));
				porter.Start();
			}
			return false;
		}
		
		void WaitForAnotherInstance ()
		{
			while (!exiting) {
				waitHandle.WaitOne ();
				if (!exiting) {
					Debug.WriteLine ("Another app instance has just knocked on the door.");
					OnRemoteInstanceKnocked ();
				}
			}
		}
		
		bool exiting;
		EventWaitHandle waitHandle;
		readonly string waitHandleName = "Tasque." + Environment.UserName;
	}
}
