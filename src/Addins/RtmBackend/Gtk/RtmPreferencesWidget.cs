// RtmPreferencesWidget.cs created with MonoDevelop
// User: boyd at 11:29 PM 2/18/2008
using System;
using System.Diagnostics;
using Mono.Unix;
using Gtk;

namespace Tasque.Backends.RtmBackend
{
	public class RtmPreferencesWidget : EventBox, IBackendPreferences
	{
		private LinkButton		authButton;
		private Label			statusLabel;
		private Image		image;
		private bool			authRequested;
		private bool			isAuthorized;
		private static Gdk.Pixbuf normalPixbuf;
		
		static RtmPreferencesWidget ()
		{
			normalPixbuf = Utilities.GetIcon ("rtmLogo", 122);
		}
		
		public RtmPreferencesWidget () : base ()
		{
			LoadPreferences ();
			
			BorderWidth = 0;
			
			// We're using an event box so we can paint the background white
			EventBox imageEb = new EventBox ();
			imageEb.BorderWidth = 0;
			imageEb.ModifyBg (StateType.Normal, new Gdk.Color (255, 255, 255));
			imageEb.ModifyBase (StateType.Normal, new Gdk.Color (255, 255, 255)); 
			imageEb.Show ();
			
			VBox mainVBox = new VBox (false, 0);
			mainVBox.BorderWidth = 10;
			mainVBox.Show ();
			Add (mainVBox);

			// Add the rtm logo
			image = new Image (normalPixbuf);
			image.Show ();
			//make the dialog box look pretty without hard coding total size and
			//therefore clipping displays with large fonts.
			Alignment spacer = new Alignment ((float)0.5, 0, 0, 0);
			spacer.SetPadding (0, 0, 125, 125);
			spacer.Add (image);
			spacer.Show ();
			imageEb.Add (spacer);
			mainVBox.PackStart (imageEb, true, true, 0);

			// Status message label
			statusLabel = new Label ();
			statusLabel.Justify = Justification.Center;
			statusLabel.Wrap = true;
			statusLabel.LineWrap = true;
			statusLabel.Show ();
			statusLabel.UseMarkup = true;
			statusLabel.UseUnderline = false;

			authButton = new LinkButton (Catalog.GetString ("Click Here to Connect"));
			
			authButton.Clicked += OnAuthButtonClicked;
			
			if (isAuthorized) {
				statusLabel.Text = "\n\n" +
					Catalog.GetString ("You are currently connected");
				var userName = Application.Instance.Preferences.Get (Preferences.UserNameKey);
				if (userName != null && userName.Trim () != string.Empty)
					statusLabel.Text = "\n\n" +
						Catalog.GetString ("You are currently connected as") +
						"\n" + userName.Trim ();
			} else {
				statusLabel.Text = "\n\n" +
					Catalog.GetString ("You are not connected");
				authButton.Show ();
			}
			mainVBox.PackStart (statusLabel, false, false, 0);
			mainVBox.PackStart (authButton, false, false, 0);

			Label blankLabel = new Label ("\n");
			blankLabel.Show ();
			mainVBox.PackStart (blankLabel, false, false, 0);
		}
		
		private void LoadPreferences ()
		{
			var authToken = Application.Instance.Preferences.Get (Preferences.AuthTokenKey);
			if (authToken == null || authToken.Trim () == "") {
				Debug.WriteLine ("Rtm: Not authorized");
				isAuthorized = false;
			} else {
				Debug.WriteLine ("Rtm: Authorized");
				isAuthorized = true;
			}
		}
		
		private void OnAuthButtonClicked (object sender, EventArgs args)
		{
			var rtmBackend = Application.Instance.Backend as RtmBackend;
			if (rtmBackend != null) {
				if (!isAuthorized && !authRequested) {
					string url = string.Empty;
					try {
						url = rtmBackend.GetAuthUrl ();
					} catch (Exception) {
						Debug.WriteLine ("Failed to get auth URL from Remember the Milk. Try again later.");
						authButton.Label = Catalog.GetString ("Remember the Milk not responding. Try again later.");
						return;
					}
					Debug.WriteLine ("Launching browser to authorize with Remember the Milk");
					try {
						Application.Instance.OpenUrlInBrowser (url);
						authRequested = true;
						authButton.Label = Catalog.GetString ("Click Here After Authorizing");
					} catch (Exception ex) {
						Trace.TraceError ("Exception opening URL: {0}", ex.Message);
						authButton.Label = Catalog.GetString ("Set the default browser and try again");						
					}			
				} else if (!isAuthorized && authRequested) {
					authButton.Label = Catalog.GetString ("Processing...");
					try {
						rtmBackend.FinishedAuth ();
						Debug.WriteLine ("Successfully authorized with Remember the Milk");
						isAuthorized = true;
						authRequested = false;
					} catch (RtmNet.RtmApiException) {
						Debug.WriteLine ("Failed to authorize with Remember the Milk");
						isAuthorized = false;
						authRequested = true;
						authButton.Label = Catalog.GetString ("Failed, Try Again");
					}
				}
				if (isAuthorized) {
					authButton.Label = Catalog.GetString ("Thank You");
					authButton.Sensitive = false;
					statusLabel.Text = "\n\n" +
						Catalog.GetString ("You are currently connected");
					string userName =
						Application.Instance.Preferences.Get (Preferences.UserNameKey);
					if (userName != null && userName.Trim () != string.Empty)
						statusLabel.Text = "\n\n" +
							Catalog.GetString ("You are currently connected as") +
							"\n" + userName.Trim ();
				}
			}
		}
	}
}
