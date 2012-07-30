// 
// UICommand.cs
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

namespace Tasque.UIModel
{
	//DOCS: sealed so that protected SetCanExecute and UnsetCanExecute
	// can't be invoked directly
	public sealed class UICommand : CommandBase
	{
		new public bool CanExecute {
			get { return base.CanExecute; }
			set {
				if (value && ExecuteAction == null)
					throw new InvalidOperationException ("Can't set CanExecute to true " +
					                                     "when ExecuteAction is null.");
				
				if (!value)
					UnsetCanExecute (string.Empty);
				else
					SetCanExecute ();
			}
		}
		
		public Action ExecuteAction { get { return executeAction; } }
		
		public override void Execute ()
		{
			if (CanExecute) {
				ExecuteAction ();
				if (Executed != null)
					Executed (this, EventArgs.Empty);
			} else
				Debug.WriteLine ("Cannot execute: " + (string.IsNullOrWhiteSpace(ErrorMessage)
				                                       ? "(none given)" : ErrorMessage));
		}
		
		//DOCS: sets canexecute to true
		public void SetExecuteAction (Action executeAction)
		{
			this.executeAction = executeAction;
			CanExecute = true;
		}
		
		public void SetExecuteAction (Action executeAction, bool canExecute)
		{
			SetExecuteAction (executeAction);
			CanExecute = canExecute;
		}
		
		public event EventHandler Executed;
		
		Action executeAction;
	}
}
