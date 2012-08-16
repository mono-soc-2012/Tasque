// TaskState.cs created with MonoDevelop
// User: boyd at 8:37 AM 2/12/2008
// 
// TaskState.cs
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

namespace Tasque
{
	public enum TaskState
	{
		/// <summary>
		/// A task that has not been completed.
		/// </summary>
		Active,
		
		/// <summary>
		/// A task that's in limbo...the user has clicked that it should be
		/// completed, but we're delaying so the user can get a visual of what's
		/// gonna happen.  This feature ROCKS!
		/// </summary>
		Inactive,
		
		/// <summary>
		/// A completed task.
		/// </summary>
		Completed,
		
		/// <summary>
		/// A tasks that's deleted.  This is used when tasks are cached locally.
		/// As soon as the task is actually deleted from the backend system, the
		/// task should really be deleted.
		/// </summary>
		Deleted
	}
}
