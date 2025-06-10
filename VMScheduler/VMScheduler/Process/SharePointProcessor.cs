using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;


namespace VMScheduler.Process
{
    public class SharePointProcessor
    {
        private Logger logger;

        public SharePointProcessor(Logger logger)
        {
            this.logger = logger;
        }

        public void UploadFileSP365(string fileName, string filePath, string destinationFolder, string userFolder)
        {
            string siteUrl = ConfigurationManager.AppSettings.Get("SharePoint365SiteUrl");
            string userName = ConfigurationManager.AppSettings.Get("SharePoint365User");
            string password = ConfigurationManager.AppSettings.Get("SharePoint365Password");

            //filePath = @"C:\Temp\LargeFile.txt";
            //fileName = "LargeFile.txt";
            try
            {
                OfficeDevPnP.Core.AuthenticationManager authMgr = new OfficeDevPnP.Core.AuthenticationManager();

                using (var ctx = authMgr.GetSharePointOnlineAuthenticatedContextTenant(siteUrl, userName, password))
                {
                    string baseFolder = ConfigurationManager.AppSettings.Get("SharePoint365BaseFolder");
                    
                    Web web = ctx.Web;
                    ctx.Load(web);
                    ctx.Load(web.Lists);
                    ctx.ExecuteQueryRetry();
                    List list = web.Lists.GetByTitle(baseFolder);

                    ctx.Load(list);
                    ctx.ExecuteQueryRetry();

                    if (string.IsNullOrEmpty(userFolder))
                    {
                        Folder generalFolder = list.RootFolder.EnsureFolder(destinationFolder);
                        ctx.Load(generalFolder);
                        ctx.ExecuteQueryRetry();

                        Folder folderToUpload = web.GetFolderByServerRelativeUrl(generalFolder.ServerRelativeUrl);
                        folderToUpload.UploadFile(fileName, filePath, true); 
                        folderToUpload.Update();
                        ctx.Load(generalFolder);
                        ctx.ExecuteQueryRetry();
                    }
                    else
                    {
                        Folder generalFolder7 = list.RootFolder.EnsureFolder(userFolder);
                        ctx.Load(generalFolder7);
                        ctx.ExecuteQueryRetry();

                        Folder folderToUpload7 = web.GetFolderByServerRelativeUrl(generalFolder7.ServerRelativeUrl);
                        folderToUpload7.UploadFile(fileName, filePath, true); 
                        folderToUpload7.Update();
                        ctx.Load(generalFolder7);
                        ctx.ExecuteQueryRetry();
                    }
                }
            }
            catch (Exception ex)
            {
                var messages = new List<string>();
                do
                {
                    messages.Add(ex.Message);
                    ex = ex.InnerException;
                }
                while (ex != null);
                var message = string.Join(" - ", messages);
                logger.CreateErrorLogRecord(message, "SharePointProcessor - UploadFileSP365");
                System.Console.WriteLine($@"Exception occurred : {message}");
                System.Console.ReadLine();
            }
        }

        public void UploadFileSPonPrem(string fileName, string filePath, string destinationFolder, string userFolder)
        {
            try
            {
                string p = filePath + fileName;

                string sharePointSite = ConfigurationManager.AppSettings.Get("SharePointPremSite");
                //string sharePointSite = "https://my.psclistens.com/internalit/";
                //string sharePointSite = "http://validationmasteronline.com/vmaster/";

                ClientContext context = new ClientContext(sharePointSite);
                string pwd = ConfigurationManager.AppSettings.Get("SharePointPremPassword");
                string user = ConfigurationManager.AppSettings.Get("SharePointPremUser");
                context.Credentials = new NetworkCredential(user, pwd);
                Web site = context.Web;

                context.Load(site);

                context.ExecuteQuery();

                Console.WriteLine(site.Title);

                //Get the required RootFolder
                string baseFolder = ConfigurationManager.AppSettings.Get("SharePointPremBaseFolder");
                string barRootFolderRelativeUrl = string.IsNullOrEmpty(userFolder) ? $"{baseFolder}/{destinationFolder}" : $"{baseFolder}/{destinationFolder}/{userFolder}";

                //Add file to new Folder
                Folder currentRunFolder = site.GetFolderByServerRelativeUrl(barRootFolderRelativeUrl);
                FileCreationInformation newFile = new FileCreationInformation { Content = System.IO.File.ReadAllBytes(p), Url = Path.GetFileName(p), Overwrite = true };
                currentRunFolder.Files.Add(newFile);
                currentRunFolder.Update();

                context.ExecuteQuery();
            }

            catch (Exception ex)
            {
                var messages = new List<string>();
                do
                {
                    messages.Add(ex.Message);
                    ex = ex.InnerException;
                }
                while (ex != null);
                var message = string.Join(" - ", messages);
                logger.CreateErrorLogRecord(message, "SharePointProcessor - UploadFileSPonPrem");
                System.Console.WriteLine($@"Exception occurred : {message}");
                System.Console.ReadLine();
            }
        }
    }
}
