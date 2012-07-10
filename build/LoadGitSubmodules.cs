// 
// LoadGitSubmodules.cs
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tasque.Build
{
	public class LoadGitSubmodules : Task
	{
		[Required]
		public ITaskItem SolutionDirectory {
			get { return solutionDirectory; }
			set {
				solutionDirectory = value;
				slnDir = value.ItemSpec;
			}
		}
		
		public override bool Execute ()
		{
			var startInfo = new ProcessStartInfo ("git", "submodule update --init");
			startInfo.WorkingDirectory = slnDir;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.CreateNoWindow = true;
			
			try {
				var p = new Process ();
				p.StartInfo = startInfo;
				p.OutputDataReceived += (sender, e) => { if (e.Data != null) Log.LogMessage (e.Data); };
				p.ErrorDataReceived += (sender, e) => { if (e.Data != null) Log.LogError (e.Data); };
				p.Start ();
				p.BeginOutputReadLine ();
				p.BeginErrorReadLine ();
				
				p.WaitForExit (20000);
				if (p.ExitCode != 0)
					return false;
				
			} catch (Exception e) {
				Log.LogErrorFromException (e, true);
				return false;
			}
			
			return true;
		}
		
		string slnDir;
		ITaskItem solutionDirectory;
	}
}

