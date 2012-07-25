// 
// CompletedTaskGroupModel.cs
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
using System.Collections;

namespace Tasque
{
	public class CompletedTaskGroupModel : TaskGroupModel
	{
		public CompletedTaskGroupModel (DateTime rangeStart, DateTime rangeEnd, IEnumerable tasks)
			: base (rangeStart, rangeEnd, tasks)
		{
		}

		/// <summary>
		/// Override the default filter mechanism so that we show only
		/// completed tasks in this group.
		/// </summary>
		/// <param name="model">
		/// A <see cref="TreeModel"/>
		/// </param>
		/// <param name="iter">
		/// A <see cref="TreeIter"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		protected override bool FilterTasks (Task task)
		{
			// Don't show any task here if showCompletedTasks is false
			if (!ShowCompletedTasks)
				return false;
			
			if (task == null || task.State != TaskState.Completed)
				return false;
			
			// Make sure that the task fits into the specified range depending
			// on what the user has set the range slider to be.
			if (task.CompletionDate < TimeRangeStart)
				return false;
			
			if (task.CompletionDate == DateTime.MinValue)
				return true; // Just in case
			
			// Don't show tasks in the completed group that were completed
			// today.  Tasks completed today should still appear under their
			// original group until tomorrow.
			DateTime today = DateTime.Now;
			if (today.Year == task.CompletionDate.Year
				&& today.DayOfYear == task.CompletionDate.DayOfYear)
				return false;
			
			return true;
		}
	}
}
