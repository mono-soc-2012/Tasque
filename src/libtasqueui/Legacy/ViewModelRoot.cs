// 
// ViewModelRoot.cs
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
	class ViewModelRoot : ViewModel
	{
		internal ViewModelRoot () : base (null)
		{
			
		}
		
		
		
		public MainWindowModel MainWindowModel { get; private set; }
		
		public RelayCommand ShowMainWindow {
			get {
				return showMainWindow ?? (showMainWindow = new RelayCommand () {
					ExecuteAction = delegate {
						if (MainWindowModel == null)
							MainWindowModel = new MainWindowModel (null, this);
						OnPropertyChanged ("MainWindowModel");
					}					
				});
			}
		}
		
		public PreferencesDialogModel PreferencesDialogModel { get; private set; }
		
		public RelayCommand ShowPreferencesDialog {
			get {
				return showPreferencesDialog ?? (showPreferencesDialog = new RelayCommand () {
					ExecuteAction = delegate {
						if (PreferencesDialogModel == null)
						PreferencesDialogModel = new PreferencesDialogModel (this);
						OnPropertyChanged ("PreferencesDialogModel");
					}
				});
			}
		}
		
		public AboutDialogModel AboutDialogModel { get; private set; }
		
		public ICommand ShowAboutDialog {
			get {
				return showAboutDialog ?? (showAboutDialog = new RelayCommand () {
					ExecuteAction = delegate {
						if (AboutDialogModel == null)
							AboutDialogModel = new AboutDialogModel ("tasque-24", this);
						OnPropertyChanged ("AboutDialogModel");
					}					
				});
			}
		}
		
		internal bool IsMainWindowVisible {
			get { return MainWindowModel != null || MainWindowModel.IsVisible; }
		}
		
		RelayCommand showAboutDialog, showMainWindow, showPreferencesDialog;
	}
}
