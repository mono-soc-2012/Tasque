using System;

namespace Tasque
{
	public interface INativeApplication
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
