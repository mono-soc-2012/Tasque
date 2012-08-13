// 
// TrayModel.cs
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
using CrossCommand;

namespace Tasque.UIModel.Legacy
{
	public class TrayModel : ViewModel
	{
		public TrayModel (ViewModel parent) : base (parent)
		{
		}
		
		public string IconName { get { return "tasque"; } }
		
		public bool IsTaskWindowVisible { get { return false; } }
		
		public ICommand NewTask { get { throw new NotImplementedException (); } }
		
		public ICommand Quit { get { throw new NotImplementedException (); } }
		
		public ICommand Refresh { get { throw new NotImplementedException (); } }
		
		public ICommand ShowAbout { get { throw new NotImplementedException (); } }
		
		public ICommand ShowPreferences { get { throw new NotImplementedException (); } }
		
		public Command ToggleTaskWindow { get { throw new NotImplementedException (); } }
	}
}
