// DummyCategory.cs created with MonoDevelop
// User: boyd at 9:06 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
// 
// DummyCategory.cs
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
using Tasque;
using System.ComponentModel;

namespace Tasque.Backends.Dummy
{
	public class DummyCategory : ICategory
	{
		string name;
		
		public DummyCategory (string name)
		{
			this.name = name;
		}
		
		public string Name
		{
			get {
				return name;
			}
		}

		public bool ContainsTask(ITask task)
		{
			if(task.Category is DummyCategory)
				return (task.Category.Name.CompareTo(name) == 0);
			else
				return false;
		}
		
		public int CompareTo (ICategory other)
		{
			if (other == null)
				return -1;
			
			if (other is AllCategory)
				return 1;
			
			return name.CompareTo (other.Name);
		}

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
	}
}
