using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using otr_project.Models;
using otr_project.ViewModels;

namespace otr_project.Utils
{
    public class ItemImages
    {
        private const string PATH = "~/Uploads";

        public int Upload(HttpPostedFileBase file, string itemID, HttpContextBase context)
        {
            try
            {
                if (file.ContentLength > 0 || file.ContentType.Contains("image/"))
                {
                    string dirPath = Path.Combine(context.Server.MapPath(PATH), itemID);
                    string filePath;

                    if (Directory.Exists(dirPath) == false)
                    {
                        Directory.CreateDirectory(dirPath);
                        filePath = Path.Combine(dirPath, Path.GetFileName(file.FileName));
                        file.SaveAs(filePath);
                        return Directory.EnumerateFiles(dirPath).Count();
                    }

                    if (Directory.EnumerateFiles(dirPath).Count() >= 5)
                    {
                        //Reached the maximum number of images per item
                        return -1;
                    }

                    filePath = Path.Combine(dirPath, Path.GetFileName(file.FileName));
                    file.SaveAs(filePath);
                    return Directory.EnumerateFiles(dirPath).Count();
                }
                //File is empty or not an image
                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }

    public class RentalAgreements
    {
        private const string PATH = "~/Uploads/Agreements";

        public Agreement Upload(HttpPostedFileBase file, string userID, HttpContextBase context)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string dirPath = Path.Combine(context.Server.MapPath(PATH), userID);
                    string filePath;

                    if (Directory.Exists(dirPath) == false)
                        Directory.CreateDirectory(dirPath);

                    filePath = Path.Combine(dirPath, Path.GetFileName(file.FileName));
                    file.SaveAs(filePath);
                    
                    var result = new Agreement()
                    {
                        Id = System.Guid.NewGuid().ToString(),
                        FileName = Path.GetFileName(file.FileName),
                        UserModelEmail = userID,
                        ContentLength = file.ContentLength,
                        ContentType = file.ContentType
                    };
                    
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public FileStream Download(string fileName, string userID, HttpContextBase context)
        {
            string filePath = Path.Combine(context.Server.MapPath(PATH), userID, fileName);
            FileStream fs = new FileStream(filePath, FileMode.Open);
            return fs;
        }

        public void DeleteAll(string userID, HttpContextBase context)
        {
            string filePath = Path.Combine(context.Server.MapPath(PATH), userID);
            foreach (string s in Directory.EnumerateFiles(filePath))
            {
                File.Delete(s);
            }
        }

        public void Delete(string fileName, string userID, HttpContextBase context)
        {
            string filePath = Path.Combine(context.Server.MapPath(PATH), userID, fileName);
            File.Delete(filePath);
        }
    }
}