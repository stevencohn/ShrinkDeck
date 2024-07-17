//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace ShrinkDeck
{
	using System.IO;
	using System.IO.Compression;


	internal sealed class ScratchpadAdapter : IDisposable
	{

		private bool disposed;


		public ScratchpadAdapter()
		{
			Pad = Path.Combine(
				Path.GetTempPath(),
				Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
				);
		}


		public string Pad { get; private set; }


		public void Dispose()
		{
			if (!disposed)
			{
				Cleanup();
				disposed = true;
			}
		}


		private void Cleanup()
		{
			if (Directory.Exists(Pad))
			{
				try
				{
					"cleaning up scratchpad".Report(ConsoleColor.DarkGray);

					// this will unset the ReadOnly flag for all files/dirs in and below dir
					var dir = new DirectoryInfo(Pad)
					{
						Attributes = FileAttributes.Normal
					};

					foreach (var info in dir.GetFileSystemInfos("*", SearchOption.AllDirectories))
					{
						info.Attributes = FileAttributes.Normal;
					}

					// recursively delete del
					dir.Delete(true);
				}
				catch (Exception exc)
				{
					$"cannot delete {Pad}".Report(ConsoleColor.Red);
					exc.Message.Report(ConsoleColor.DarkRed);
				}
			}
		}


		public void UnpackArtifacts(string path)
		{
			$"extracting to {Pad}".Report(ConsoleColor.DarkGray);

			try
			{
				if (Directory.Exists(Pad))
				{
					Cleanup();
				}

				ZipFile.ExtractToDirectory(path, Pad);
			}
			catch (Exception exc)
			{
				$"error extracting {path} to {Pad}".Report(ConsoleColor.Red);
				exc.Message.Report(ConsoleColor.DarkRed);
			}
		}


		public void RepackageArtifacts(string path)
		{
			$"repackaging to {path}".Report(ConsoleColor.DarkGray);

			try
			{
				if (File.Exists(path))
				{
					var info = new FileInfo(path)
					{
						Attributes = FileAttributes.Normal
					};

					info.Delete();
				}

				ZipFile.CreateFromDirectory(Pad, path);
			}
			catch (Exception exc)
			{
				$"error packing {path} to {Pad}".Report(ConsoleColor.Red);
				exc.Message.Report(ConsoleColor.DarkRed);
			}
		}
	}
}