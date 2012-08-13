// 
// StatusIconTray.cs
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
using Gtk;
using Tasque.UIModel.Legacy;

namespace Tasque
{
	public class StatusIconTray : GtkTray
	{
		public StatusIconTray (TrayModel viewModel) : base (viewModel)
		{
			tray = new StatusIcon (Utilities.GetIcon (ViewModel.IconName, 16));
			tray.Visible = true;
			tray.Activate += HandleActivate;
			tray.PopupMenu += HandlePopupMenu;
		}

		void HandleActivate (object sender, System.EventArgs e)
		{
			
		}

		void HandlePopupMenu (object sender, PopupMenuArgs e)
		{
			var popupMenu = Menu;
			
			//TODO: incorp this logic into viewmodel
//			bool backendItemsSensitive = (backend != null && backend.Initialized);
//			uiManager.GetAction ("/TrayIconMenu/NewTaskAction").Sensitive = backendItemsSensitive;
//			uiManager.GetAction ("/TrayIconMenu/RefreshAction").Sensitive = backendItemsSensitive;
			
			popupMenu.ShowAll(); // shows everything
			tray.PresentMenu (popupMenu, (uint)e.Args [0], (uint)e.Args [1]);			
		}
		
		StatusIcon tray;
	}
}
