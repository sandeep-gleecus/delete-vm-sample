using Microsoft.SharePoint.Client;
using System;
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
            string siteUrl = ConfigurationManager.AppSettings.Get("SharePoint365SiteUrl");// "https://onshore.sharepoint.com/vmaster//";
            string userName = ConfigurationManager.AppSettings.Get("SharePoint365User");//"ggreen@onshoretech.com";
            string password = ConfigurationManager.AppSettings.Get("SharePoint365Password");//"Daj61203";

            filePath = @"C:\Temp\LargeFile.txt";
            fileName = "LargeFile.txt";
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
                    //Folder folder = list.RootFolder;

                    if (string.IsNullOrEmpty(userFolder))
                    {
                        Folder generalFolder = list.RootFolder.EnsureFolder(destinationFolder);
                        ctx.Load(generalFolder);
                        ctx.ExecuteQueryRetry();

                        Folder folderToUpload = web.GetFolderByServerRelativeUrl(generalFolder.ServerRelativeUrl);
                        folderToUpload.UploadFile(fileName, filePath, true); //"LargeFile.txt", "C:\\Temp\\LargeFile.txt", true);
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
                        folderToUpload7.UploadFile(fileName, filePath, true); //"LargeFile.txt", "C:\\Temp\\LargeFile.txt", true);
                        folderToUpload7.Update();
                        ctx.Load(generalFolder7);
                        ctx.ExecuteQueryRetry();


                        ////clientContext.Credentials = new SharePointOnlineCredentials(userName, securePassword);
                        ////Web web = clientContext.Web;
                        ////clientContext.Load(web, a => a.ServerRelativeUrl);
                        ////clientContext.ExecuteQuery();
                        //List documentsList = ctx.Web.Lists.GetByTitle("Contact");

                        //var fileCreationInformation = new FileCreationInformation();
                        ////Assign to content byte[] i.e. documentStream

                        //fileCreationInformation.Content = System.IO.File.ReadAllBytes(@"D:\document.pdf");
                        ////Allow owerwrite of document

                        //fileCreationInformation.Overwrite = true;
                        ////Upload URL

                        //fileCreationInformation.Url = "https://testlz.sharepoint.com/sites/jerrydev/" + "Contact/demo" + "/document.pdf";

                        //Microsoft.SharePoint.Client.File uploadFile = documentsList.RootFolder.Files.Add(fileCreationInformation);

                        ////Update the metadata for a field having name "DocType"
                        //uploadFile.ListItemAllFields["Title"] = "UploadedviaCSOM";

                        //uploadFile.ListItemAllFields.Update();
                        //clientContext.ExecuteQuery();


                        ////using (SP.ClientContext cnx = new SP.ClientContext("https://test.sharepoint.com/sites/test01"))
                        //{
                        //    //var secret = new SecureString();
                        //    //foreach (char c in password)
                        //    //{
                        //    //    secret.AppendChar(c);
                        //    //}
                        //    //cnx.Credentials = new SP.SharePointOnlineCredentials(userName, secret);

                        //    //SP.Web web = cnx.Web;

                        //    SP.FileCreationInformation newFile = new SP.FileCreationInformation();
                        //    newFile.Content = System.IO.File.ReadAllBytes("document.pdf");

                        //    //file url is name
                        //    newFile.Url = @"document.pdf";
                        //    SP.List docs = web.Lists.GetByTitle("Contact");

                        //    //get folder and add to that
                        //    SP.Folder folder = docs.RootFolder.Folders.GetByUrl("demo");
                        //    SP.File uploadFile = folder.Files.Add(newFile);

                        //    ctx.Load(docs);
                        //    ctx.Load(uploadFile);
                        //    ctx.ExecuteQuery();
                        //    Console.WriteLine("done");
                        //};









                        //var uploadFolderUrl = $"{destinationFolder}/{userFolder}";
                        //var uploadFilePath = filePath;
                        //var fileCreationInfo = new FileCreationInformation
                        //{
                        //    Content = System.IO.File.ReadAllBytes(uploadFilePath),
                        //    Overwrite = true,
                        //    Url = fileName //Path.GetFileName(uploadFilePath)
                        //};
                        //var targetFolder = ctx.Web.GetFolderByServerRelativeUrl(uploadFolderUrl);
                        //var uploadFile = targetFolder.Files.Add(fileCreationInfo);
                        //ctx.Load(uploadFile);
                        //ctx.ExecuteQuery();


                        //List list2 = web.Lists.GetByTitle(destinationFolder);
                        //Folder generalFolder = list.RootFolder.EnsureFolder($"{destinationFolder}");
                        //Folder generalFolders = list.RootFolder.ResolveSubFolder(userFolder);

                        //Folder generalFolder2 = list.RootFolder.EnsureFolder($"{userFolder}");
                        //ctx.Load(generalFolder2);
                        //ctx.ExecuteQueryRetry();

                        //Folder folderToUpload = web.GetFolderByServerRelativeUrl(generalFolder2.ServerRelativeUrl);
                        //folderToUpload.UploadFile(fileName, filePath, true); //"LargeFile.txt", "C:\\Temp\\LargeFile.txt", true);
                        //folderToUpload.Update();
                        //ctx.Load(generalFolder2);
                        //ctx.ExecuteQueryRetry();


                        //Folder generalFolder = list.RootFolder.EnsureFolder(destinationFolder);
                        //ctx.Load(generalFolder);
                        //ctx.ExecuteQueryRetry();

                        ////SharePoint User Folder
                        //Folder spUserfolder = list.RootFolder.EnsureFolder(userFolder);
                        //ctx.Load(spUserfolder);
                        //ctx.ExecuteQueryRetry();

                        //Folder folderToUpload = web.GetFolderByServerRelativeUrl(spUserfolder.ServerRelativeUrl);
                        //folderToUpload.UploadFile(fileName, filePath, true); //"LargeFile.txt", "C:\\Temp\\LargeFile.txt", true);
                        //folderToUpload.Update();
                        //ctx.Load(spUserfolder);
                        //ctx.ExecuteQueryRetry();
                    }

                }
            }
            catch (Exception ex)
            {
                logger.CreateErrorLogRecord(ex.Message, "SharePointProcessor - UploadFileSP365");
                System.Console.WriteLine("Exception occurred : " + ex.Message);
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
                logger.CreateErrorLogRecord(ex.Message, "SharePointProcessor - UploadFileSPonPrem");
                System.Console.WriteLine("Exception occurred : " + ex.Message);
                System.Console.ReadLine();
            }
        }
    }
}
