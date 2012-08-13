// 
// GtkTray.cs
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
using Mono.Unix;
using Gtk;
using Tasque.UIModel.Legacy;

namespace Tasque
{
	public class GtkTray
	{
		public static GtkTray CreateTray (TrayModel viewModel)
		{
#if APPINDICATOR
			return new AppIndicatorTray (viewModel);
#else
			return new StatusIconTray (viewModel);
#endif
		}

		public GtkTray (TrayModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException ("viewModel");
			ViewModel = viewModel;
			
			var newTask = viewModel.NewTask;
			newTask.CanExecuteChanged += delegate {
				UIManager.GetAction ("/TrayIconMenu/NewTaskAction").Sensitive = newTask.CanExecute (null);
			};
			
			var refresh = viewModel.Refresh;
			refresh.CanExecuteChanged += delegate {
				UIManager.GetAction ("/TrayIconMenu/RefreshAction").Sensitive = refresh.CanExecute (null);
			};
		}
		
		protected Menu Menu { get { return (Menu)UIManager.GetWidget ("/TrayIconMenu"); } }
		
		protected Gtk.Action ToggleTaskWindowAction { get; private set; }
		
		protected UIManager UIManager {
			get {
				if (uiManager == null)
					RegisterUIManager ();
				return uiManager;
			}
		}
		
		protected TrayModel ViewModel { get; private set; }
		
		void RegisterUIManager ()
		{
			ActionGroup trayActionGroup = new ActionGroup ("Tray");
			trayActionGroup.Add (new ActionEntry [] {
				new ActionEntry ("NewTaskAction", Stock.New, Catalog.GetString ("New Task ..."),
				                 null, null, delegate { ViewModel.NewTask.Execute (null); }),
				new ActionEntry ("AboutAction", Stock.About, delegate { ViewModel.ShowAbout.Execute (null); }),
				new ActionEntry ("PreferencesAction", Stock.Preferences,
				                 delegate { ViewModel.ShowPreferences.Execute (null); }),
				new ActionEntry ("RefreshAction", Stock.Execute, Catalog.GetString ("Refresh Tasks ..."),
				                 null, null, delegate { ViewModel.Refresh.Execute (null); }),
				new ActionEntry ("QuitAction", Stock.Quit, delegate { ViewModel.Quit.Execute (null); })
			});
			
			ToggleTaskWindowAction = new Gtk.Action ("ToggleTaskWindowAction", null);
			ToggleTaskWindowAction.ActionGroup = trayActionGroup;
			ToggleTaskWindowAction.Activated += delegate { ViewModel.ToggleTaskWindow.Execute (null); };
			
			uiManager = new UIManager ();
			uiManager.AddUiFromString (MenuXml);
			uiManager.InsertActionGroup (trayActionGroup, 0);
		}
		
		
		void RefreshTrayIconTooltip ()
		{
//			var sb = new StringBuilder ();
//			if (overdue_tasks != null) {
//				int count =  overdue_tasks.Count;
//
//				if (count > 0) {
//					sb.Append (String.Format (Catalog.GetPluralString ("{0} task is Overdue\n", "{0} tasks are Overdue\n", count), count));
//				}
//			}
//			
//			if (today_tasks != null) {
//				int count =  today_tasks.Count;
//
//				if (count > 0) {
//					sb.Append (String.Format (Catalog.GetPluralString ("{0} task for Today\n", "{0} tasks for Today\n", count), count));
//				}
//			}
//
//			if (tomorrow_tasks != null) {
//				int count =  tomorrow_tasks.Count;
//
//				if (count > 0) {
//					sb.Append (String.Format (Catalog.GetPluralString ("{0} task for Tomorrow\n", "{0} tasks for Tomorrow\n", count), count));
//				}
//			}
//
//			if (sb.Length == 0) {
//				// Translators: This is the status icon's tooltip. When no tasks are overdue, due today, or due tomorrow, it displays this fun message
//				trayIcon.Tooltip = Catalog.GetString ("Tasque Rocks");
//				return;
//			}
//
//			trayIcon.Tooltip = sb.ToString ().TrimEnd ('\n');
		}
		
		UIManager uiManager;
		const string MenuXml = @"
<ui>
	<popup name=""TrayIconMenu"">
		<menuitem action=""NewTaskAction""/>
		<separator/>
		<menuitem action=""PreferencesAction""/>
		<menuitem action=""AboutAction""/>
		<separator/>
		<menuitem action=""RefreshAction""/>
		<separator/>
		<menuitem action=""QuitAction""/>
	</popup>
</ui>
";
	}
}
