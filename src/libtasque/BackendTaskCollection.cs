// 
// BackendTaskCollection.cs
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
using System.Collections.Specialized;
using System.Linq;

namespace Tasque
{
	class BackendTaskCollection : ReadOnlySortedNotifyCollection<Task>
	{
		internal BackendTaskCollection (SortedNotifyCollection<Category> categories)
		{
			if (categories == null)
				throw new ArgumentNullException ("source");
			categories.CollectionChanged += HandleSourceCollectionChanged;
			this.categories = categories;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (!disposed && disposing) {
				foreach (var item in categories)
					item.CollectionChanged -= HandleCollectionChanged;
				disposed = true;
			}
		}
		
		internal protected override int CountProtected {
			get { return categories.SelectMany (c => c).Distinct ().Count (); }
		}
		
		internal protected override bool ContainsProtected (Task item)
		{
			return categories.Any (c => c.Contains (item));
		}
		
		internal protected override void CopyToProtected (Task[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException ("arrayIndex is less than 0.");
			if (CountProtected > array.Length - arrayIndex)
				throw new ArgumentException ("There is not enough space for the elements of the" +
					"source collection in the provided array starting from arrayIndex."
				);
			
			foreach (var item in this)
				array [arrayIndex++] = item;
		}
		
		internal protected override IEnumerator<Task> GetEnumeratorProtected ()
		{
			Task [] tasks = new Task [CountProtected];
			int i = 0;
			foreach (var collection in categories) {
				foreach (var item in collection) {
					if (tasks.Contains (item))
						continue;
					tasks [i++] = item;
					yield return item;
				}
			}
		}
		
		void AddCollection (Category category)
		{
			categories.Add (category);
			if (categories.Contains (category))
			    category.CollectionChanged += HandleCollectionChanged;
		}
		
		bool RemoveCollection (Category category)
		{
			if (categories.Contains (category))
				category.CollectionChanged -= HandleCollectionChanged;
			return categories.Remove (category);
		}
		
		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			OnCollectionChanged (e);
		}
		
		void HandleSourceCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddCollection (e.NewItems [0] as Category);
				break;
			case NotifyCollectionChangedAction.Remove:
				RemoveCollection (e.OldItems [0] as Category);
				break;
			}
		}
		
		SortedNotifyCollection<Category> categories;
		bool disposed;
	}
}
