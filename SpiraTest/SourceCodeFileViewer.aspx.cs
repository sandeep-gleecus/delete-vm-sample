using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using System.Threading;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform writes out the source code file binary stream with the appropriate MIME encoding
	/// </summary>
    public partial class SourceCodeFileViewer : PageBase
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeFileViewer::";

		/// <summary>
		/// This method generates the streamable image from the passed-in XML
		/// </summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to get the project id and source code file key from the querystring
                if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY]) && !String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]))
                {
                    //Retrieve the specified source code file record
                    string fileKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY].Trim();
                    int projectId = -1;
                    if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID], out projectId))
                    {
                        Response.Write("The project id specified is invalid");
                        Response.End();
                        return;
                    }
                    //See if we have a revision specified
                    string revisionKey = "";
                    if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY]))
                    {
                        revisionKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY].Trim();
                    }

                    //See if we have a branch specified
                    string branchKey = "";
                    if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY]))
                    {
                        branchKey = Request.QueryString[GlobalFunctions.PARAMETER_SOURCE_CODE_BRANCH_KEY].Trim();
                    }

                    SourceCodeManager sourceCode = new SourceCodeManager(projectId);
                    try
                    {
                        SourceCodeFile sourceCodeFile = sourceCode.RetrieveFileByKey(fileKey, branchKey);
                        if (sourceCodeFile == null)
                        {
                            Response.Write("Unable to load the provided source code file.");
                            Response.End();
                            return;
                        }
                        //Get the filename and get the corresponding MIME type
                        string filename = sourceCodeFile.Name;

                        //Set the MIME type and 'filename' for the attachment
                        Response.ContentType = GlobalFunctions.GetFileMimeType(filename);
						Response.AddHeader("Content-Disposition", "inline; filename=\"" + filename + "\"");

                        //Open up the file for binary reading
                        SourceCodeFileStream sourceCodeFileStream = sourceCode.OpenFile(fileKey, revisionKey, branchKey);
                        Stream stream = sourceCodeFileStream.DataStream;

                        //Read the file in.
						// *FX* Let's do it bit-by-bit.
						int numRead = 4096;
						while ((numRead == 4096) && (stream.Position != stream.Length))
						{
							int streamLeft = (int)(stream.Length - stream.Position);
							if (streamLeft > 4096) { streamLeft = 4096; }
							byte[] byteArray = new byte[streamLeft];
							numRead = stream.Read(byteArray, 0, streamLeft);
							Response.BinaryWrite(byteArray);
							Response.Flush();
						}
                        //stream.Read(byteArray, 0, (int)stream.Length+1);
                        //Actually write out the attachment binary
                        //Response.BinaryWrite(byteArray);

                        //Close the file stream and the response stream
                        sourceCode.CloseFile(sourceCodeFileStream);
                    }
                    catch (ThreadAbortException)
                    {
                        //End without displaying another message
                        return;
                    }
                    catch (SourceCodeProviderGeneralException exception)
                    {
                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                        Response.Write("Error accessing the source code version control provider. Please check the server event log for details.");
                        Response.End();
                        return;
                    }
                }

				Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
            catch (ThreadAbortException)
            {
                //End without displaying another message
                return;
            }
            catch (Exception exception)
			{
				Response.Write ("Unable To Display Attachment... (Error: " + exception.Message + ")");

				Logger.LogErrorEvent (CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}
	}
}
