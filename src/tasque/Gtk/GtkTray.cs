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
using Tasque.UI.Desktop;
using Gtk;
using Mono.Unix;
using Tasque;

namespace Tasque
{
	public class GtkTray : Tray
	{
		protected override void OnInitialize ()
		{
			RegisterUIManager ();
			SetupTrayIcon ();
		}

		void OnTrayIconPopupMenu (object sender, EventArgs args)
		{
			Menu popupMenu = (Menu) uiManager.GetWidget ("/TrayIconMenu");

//			bool backendItemsSensitive = (backend != null && backend.Initialized);
			
//			uiManager.GetAction ("/TrayIconMenu/NewTaskAction").Sensitive = backendItemsSensitive;
//			uiManager.GetAction ("/TrayIconMenu/RefreshAction").Sensitive = backendItemsSensitive;

			popupMenu.ShowAll(); // shows everything
			popupMenu.Popup();
		}	

//		private void RefreshTrayIconTooltip ()
//		{
//			if (trayIcon == null) {
//				return;
//			}
//
//			StringBuilder sb = new StringBuilder ();
//			if (overdue_tasks != null) {
//				int count =  overdue_tasks.IterNChildren ();
//
//				if (count > 0) {
//					sb.Append (String.Format (Catalog.GetPluralString ("{0} task is Overdue\n", "{0} tasks are Overdue\n", count), count));
//				}
//			}
//			
//			if (today_tasks != null) {
//				int count =  today_tasks.IterNChildren ();
//
//				if (count > 0) {
//					sb.Append (String.Format (Catalog.GetPluralString ("{0} task for Today\n", "{0} tasks for Today\n", count), count));
//				}
//			}
//
//			if (tomorrow_tasks != null) {
//				int count =  tomorrow_tasks.IterNChildren ();
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
//		}

		void RegisterUIManager ()
		{
			ActionGroup trayActionGroup = new ActionGroup ("Tray");
			trayActionGroup.Add (new ActionEntry [] {
				new ActionEntry ("NewTaskAction",
				                 Stock.New,
				                 Catalog.GetString ("New Task ..."),
				                 null,
				                 null,
				                 delegate {}),
//				                 OnNewTask),

				new ActionEntry ("ShowTasksAction",
				                 null,
				                 Catalog.GetString ("Show Tasks ..."),
				                 null,
				                 null,
				                 delegate {}),
//				                 OnShowTaskWindow),

				new ActionEntry ("AboutAction",
				                 Stock.About,
				                 delegate { ShowAboutDialog ();}),
//				                 OnAbout),
				
				new ActionEntry ("PreferencesAction",
				                 Stock.Preferences,
				                 delegate {}),
//				                 OnPreferences),
				
				new ActionEntry ("RefreshAction",
				                 Stock.Execute,
				                 Catalog.GetString ("Refresh Tasks ..."),
				                 null,
				                 null,
				                 delegate {}),
//				                 OnRefreshAction),
				
				new ActionEntry ("QuitAction",
				                 Stock.Quit,
				                 delegate {

//					if (backend != null) {
//						UnhookFromTooltipTaskGroupModels ();
//						backend.Cleanup();
//					}
					TaskWindow.SavePosition();
					QuitApplication(); })
			});
			
			uiManager = new UIManager ();
			uiManager.AddUiFromString (MenuXml);
			uiManager.InsertActionGroup (trayActionGroup, 0);
		}

		void SetupTrayIcon ()
		{
			trayIcon = new StatusIcon();
			trayIcon.Pixbuf = Utilities.GetIcon ("tasque-24", 24);

			// hooking event
			trayIcon.Activate += (sender, e) => TaskWindow.ToggleWindowVisible ();
			trayIcon.PopupMenu += OnTrayIconPopupMenu;

			// showing the trayicon
			trayIcon.Visible = true;

//			RefreshTrayIconTooltip ();
		}

		StatusIcon trayIcon;
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

