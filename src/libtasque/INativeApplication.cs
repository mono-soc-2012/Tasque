using System;

namespace Tasque
{
	public interface IApplication
	{
		string ConfDir { get; }

		void Exit (int exitcode);

		void Initialize (string [] args);

		void InitializeIdle ();

		void OpenUrlInBrowser (string url);

		void QuitMainLoop ();

		void StartMainLoop ();

		event EventHandler Exiting;
	}
}
