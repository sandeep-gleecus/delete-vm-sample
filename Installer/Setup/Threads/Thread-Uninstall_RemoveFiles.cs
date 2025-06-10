using Inflectra.SpiraTest.Installer.UI;
using System;
using System.IO;
using System.Threading;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		private bool Uninstall_RemoveFiles(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Uninstall_RemoveFiles - Starting");

			int TaskDisplayLine = 12;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });

			// Set up initial values.
			ItemProgress.ProcessStatusEnum finalStatus = ItemProgress.ProcessStatusEnum.Processing;

			// TODO: Stop the Application Pool.

			// Call the function to remove the directory and all associated files.
			bool keepTrying = true;
			int numTries = 0;
			while (keepTrying)
			{
				try
				{
					ForceDeleteDirectory(App._installationOptions.InstallationFolder, streamWriter);
					streamWriter.WriteLine("Delete Folder Exist");
					// If we got here, we do not need to try again.
					keepTrying = false;
					finalStatus = ItemProgress.ProcessStatusEnum.Success;
					streamWriter.WriteLine("Delete Folder Exist Status--" + finalStatus);
				}
				catch (Exception ex)
				{
					// An error happened. Try again, if we haven't tried enough.
					numTries++;
					streamWriter.WriteLine("Delete Folder Error--" + numTries + ": " + ex.Message);
					if (numTries > 54)
					{
						keepTrying = false;
						finalStatus = ItemProgress.ProcessStatusEnum.Failure; // You can set a different failure status if necessary
						streamWriter.WriteLine("Delete Folder Error Status--" + finalStatus);
					}
					else
						Thread.Sleep(1000); // Wait for a second before retrying
				}
			}

			streamWriter.WriteLine("Uninstall_RemoveFiles - Finished");
			// Return Success for now
			ProgressUpdate(
				this,
				new ProgressArgs()
				{
					ErrorText = "",
					Progress = -1,
					Status = finalStatus,
					TaskNum = TaskDisplayLine
				}
			);
			return true;
		}

		private void ForceDeleteDirectory(string directoryPath, StreamWriter streamWriter)
		{
			if (Directory.Exists(directoryPath))
			{
				// Delete all files and subdirectories first
				foreach (string file in Directory.GetFiles(directoryPath))
				{
					try
					{
						// Remove read-only attributes before deleting the file
						File.SetAttributes(file, FileAttributes.Normal);
						File.Delete(file);
					}
					catch (Exception ex)
					{
						streamWriter.WriteLine($"Error deleting file: {file}. Exception: {ex.Message}");
					}
				}

				// Recursively delete subdirectories
				foreach (string subDirectory in Directory.GetDirectories(directoryPath))
				{
					ForceDeleteDirectory(subDirectory, streamWriter); // Recursive call to delete subdirectories
				}

				// Now delete the directory itself
				Directory.Delete(directoryPath, true); // true to delete contents recursively
			}
		}

	}
}
