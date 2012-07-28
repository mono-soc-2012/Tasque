// 
// TaskGroupModelFactory.cs
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

namespace Tasque
{
	public static class TaskGroupModelFactory
	{
		public static TaskGroupModel CreateTodayModel (IEnumerable<Task> tasks)
		{
			DateTime rangeStart = DateTime.Now;
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			DateTime rangeEnd = DateTime.Now;
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			return new TaskGroupModel (rangeStart, rangeEnd, tasks);
		}

		public static TaskGroupModel CreateOverdueModel (IEnumerable<Task> tasks)
		{
			DateTime rangeStart = DateTime.MinValue;
			DateTime rangeEnd = DateTime.Now.AddDays (-1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month, rangeEnd.Day,
									 23, 59, 59);

			return new TaskGroupModel (rangeStart, rangeEnd, tasks);
		}

		public static TaskGroupModel CreateTomorrowModel (IEnumerable<Task> tasks)
		{
			DateTime rangeStart = DateTime.Now.AddDays (1);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			DateTime rangeEnd = DateTime.Now.AddDays (1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);

			return new TaskGroupModel (rangeStart, rangeEnd, tasks);
		}
	}
}
