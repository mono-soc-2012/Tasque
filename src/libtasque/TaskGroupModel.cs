// 
// TaskGroupModel.cs
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
using CollectionTransforms;
using System.Collections.Generic;

namespace Tasque
{
	public class TaskGroupModel : ListCollectionView<Task>
	{
		public bool ShowCompletedTasks {
			get { return showCompletedTasks; }
			set {
				if (showCompletedTasks == value)
					return;
				
				showCompletedTasks = value;
				Refresh ();
			}
		}
		
		public DateTime TimeRangeStart { get; private set; }
		
		public DateTime TimeRangeEnd { get; private set; }
		
		public TaskGroupModel (DateTime rangeStart, DateTime rangeEnd, IEnumerable<Task> tasks)
			: base (tasks)
		{
			TimeRangeStart = rangeStart;
			TimeRangeEnd = rangeEnd;

			Filter = FilterTasks;
			IsObserving = true;
		}
		
		public void SetRange (DateTime rangeStart, DateTime rangeEnd)
		{
			if (rangeStart == TimeRangeStart && rangeEnd == TimeRangeEnd)
				return;
			
			TimeRangeStart = rangeStart;
			TimeRangeEnd = rangeEnd;
			Refresh ();
		}
		
		/// <summary>
		/// Filter out tasks that don't fit within the group's date range
		/// </summary>
		protected virtual bool FilterTasks (Task task)
		{
			if (task == null || task.State == TaskState.Deleted)
				return false;
			
			// Do something special when task.DueDate == DateTime.MinValue since
			// these tasks should always be in the very last category.
			if (task.DueDate == DateTime.MinValue) {
				if (TimeRangeEnd == DateTime.MaxValue) {
					if (!ShowCompletedTask (task))
						return false;
					
					return true;
				} else
					return false;
			}
			
			if (task.DueDate < TimeRangeStart || task.DueDate > TimeRangeEnd)
				return false;
			
			if (!ShowCompletedTask (task))
				return false;

			return true;
		}
		
		bool IsToday (DateTime date)
		{
			DateTime today = DateTime.Now;
			return today.Year == date.Year && today.DayOfYear == date.DayOfYear;
		}
		
		bool ShowCompletedTask (Task task)
		{
			if (task.State == TaskState.Completed) {
				if (!showCompletedTasks)
					return false;
				
				// Only show completed tasks that are from "Today".  Once it's
				// tomorrow, don't show completed tasks in this group and
				// instead, show them in the Completed Tasks Group.
				if (task.CompletionDate == DateTime.MinValue)
					return false; // Just in case
				
				if (!IsToday (task.CompletionDate))
					return false;
			}
			
			return true;
		}
		
		bool showCompletedTasks;
	}
}
