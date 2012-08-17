// 
// DeleteCompiledTranslations.cs
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
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tasque.Build
{
	public class DeleteCompiledTranslations : Task
	{
		[Required]
		public ITaskItem GettextCatalogName { get; set; }
		
		[Required]
		public ITaskItem OutputPath { get; set; }
		
		[Required]
		public ITaskItem [] TranslationSource { get; set; }
		
		public override bool Execute ()
		{
			var catalogName = GettextCatalogName.ItemSpec;
			if (string.IsNullOrWhiteSpace (catalogName)) {
				Log.LogError ("GettextCatalogName is invalid.");
				return false;
			}
			
			var outputPath = OutputPath.ItemSpec ?? string.Empty;
			
			try {
				foreach (var item in TranslationSource) {
					var itemName = Path.GetFileNameWithoutExtension (item.ItemSpec);
					var dirPath = Path.Combine (outputPath, itemName, "LC_MESSAGES");
					var path = Path.Combine (dirPath, catalogName + ".mo");
					
					if (File.Exists (path))
					    File.Delete (path);
				}
			} catch (Exception ex) {
				Log.LogErrorFromException (ex, true);
				return false;
			}
			return true;
		}
	}
}
