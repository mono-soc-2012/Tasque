// DummyTask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008
// 
// DummyTask.cs
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
using Tasque;
using System.Diagnostics;

namespace Tasque.Backends.Dummy
{
	public class DummyTask : Task
	{
		public DummyTask(string name) : base (name, TaskNoteSupport.Multiple) {}
		
		#region Public Methods
		public override void Activate ()
		{
			Debug.WriteLine ("DummyTask.Activate ()");
			CompletionDate = DateTime.MinValue;
			State = TaskState.Active;
		}
		
		public override void Complete ()
		{
			Debug.WriteLine ("DummyTask.Complete ()");
			CompletionDate = DateTime.Now;
			State = TaskState.Completed;
		}
		
		public override void Delete ()
		{
			Debug.WriteLine ("DummyTask.Delete ()");
			State = TaskState.Deleted;
		}
		#endregion
	}
}
