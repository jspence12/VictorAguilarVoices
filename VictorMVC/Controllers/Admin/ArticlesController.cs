using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using VictorMVC.Models;
using HtmlAgilityPack;
using System.Reflection;

namespace VictorMVC.Controllers.Admin
{
    public class ArticlesController : AdminController
    {
        private const string RelativePath = "/Content/Articles/";
        private readonly string AbsolutePath = $"{HttpRuntime.AppDomainAppPath}\\Content\\Articles\\";

        //storage location
        private HtmlDocument UploadContent = new HtmlDocument();
        private List<HtmlNode> NewImgList = new List<HtmlNode>();   //List of Base64 images added to a new or updated article
        private List<string> UsedImgList = new List<string>();      //List of images currently in use by the article being processed


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Manage()
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            ViewBag.Title = "Manage Articles";
            using (ContentContext db = new ContentContext())
            {
                ViewBag.Articles = db.Articles.OrderBy(e => e.List_Order).ToArray();
            }
            return View();
        }

        /// <summary>
        /// Loads page which allows user to create a new Article record or
        /// update an existing record if ID is provided
        /// </summary>
        /// <param name="id">ID of existing Article to update. Assume create operation if null.</param>
        /// <returns>Article Edit Form. If ID is specified, fields will be pre-filled</returns>
        public ActionResult Modify(int? id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (id == null)
            {
                ViewBag.Title = "Add new Article";
                ViewBag.IsNew = true;
                return View(new Article());
            }
            else
            {
                using (ContentContext db = new ContentContext())
                {
                    Article row = db.Articles.SingleOrDefault(d => d.ID == id);
                    if (row is null)
                    {
                        throw new HttpException(404, "Article not found");
                    }
                    else
                    {
                        ViewBag.Title = $"Update {row.Title}";
                        ViewBag.IsNew = false;
                        return View(row);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new Article Record in the database.
        /// Adds new Image records to database where applicable.
        /// </summary>
        /// <param name="article">Article object to be added to the database</param>
        /// <param name="confirm">Whether the user has confirmed they wish to overwrite existing files. Front end defaults to false</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(Article article, bool confirm)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Invalid Article Input" });
            }
            try
            {
                //get collection of unique files so we only save identical files once
                UploadContent.LoadHtml(article.Content);
                RecurseImgNodes(UploadContent.DocumentNode);
            }
            catch (Exception ex) //sortImgNodes will throw an error if two files have the same name but different content
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, ex.Message });
            }
            if (!confirm)   // user must confirm that they want pre-existing files overwritten
            {
                List<string> overwriteConflicts = getOverwriteConflicts();
                if (overwriteConflicts.Count() > 0)
                {
                    Response.StatusCode = 300;
                    return Json(new { Response.StatusCode, Conflicts = overwriteConflicts });
                }
            }
            article.Content = UploadContent.DocumentNode.OuterHtml;
            ProcessImages();
            using (ContentContext db = new ContentContext())
            {
                db.CreateOrdered(db.Articles, article);
                db.SaveChanges();
                //save changes so we can access the Article ID in next steps
            }
            AddNewImages(article.ID);
            return RedirectToAction("Manage");
        }

        /// <summary>
        /// Updates an existing Article record in the database based on ID of submitted article.
        /// Maintains validity of Images table based on changes to the article
        /// </summary>
        /// <param name="article">Updated article object to replace existing record in database</param>
        /// <param name="confirm">Whether the user has confirmed they wish to overwrite existing files. Front end defaults to false</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(Article article, bool confirm)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Invalid Article Input" });
            }
            try //get collection of unique files so we only save identical files once
            {
                UploadContent.LoadHtml(article.Content);
                RecurseImgNodes(UploadContent.DocumentNode);
            }
            catch (Exception ex) //getCollections will throw an error if two new files have the same name but different content
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, ex.Message });
            }
            if (!confirm)   // user must confirm that they want pre-existing files overwritten
            {
                List<string> overwriteConflicts = getOverwriteConflicts();
                if (overwriteConflicts.Count() > 0)
                {
                    Response.StatusCode = 300;
                    return Json(new { Response.StatusCode, Conflicts = overwriteConflicts });
                }
            }
            article.Content = UploadContent.DocumentNode.OuterHtml;
            ProcessImages();
            using (ContentContext db = new ContentContext())
            {
                try
                {
                    db.UpdateOrdered(db.Articles, article);
                }
                catch (KeyNotFoundException)
                {
                    //log here
                    Response.StatusCode = 404;
                    return Json(new { Response.StatusCode, Message = "Unable to update Article. Article not found in Database." });
                }
                db.SaveChanges();
            }
            AddNewImages(article.ID);
            RemoveOrphanedImages(article.ID);
            return RedirectToAction("Manage");
        }


        /// <summary>
        /// Deletes existing Article Record from Database.
        /// Deletes images from file system & database which were only in use
        /// by this article.
        /// </summary>
        /// <param name="id">ID of article to delete</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                List<string> fileNames = new List<string>();
                try
                {
                    List<Image> deletionCandidates = db.Images.Where(i => i.Article_ID == id).ToList();
                    //Get list of file names which are only being used for the article we are deleting.
                    foreach (Image candidate in deletionCandidates)
                    {
                        if (db.Images.Where(i => i.FileName == candidate.FileName).Count() == 1)
                        {
                            fileNames.Add(candidate.FileName);
                        }
                    }
                    db.DeleteOrdered(db.Articles, id);
                    db.Images.RemoveRange(deletionCandidates);
                    foreach (string file in fileNames)
                    {
                        //There may be an issue here if an images is accessed during a request.
                        //May want to revisit edge case.
                        try
                        {
                            System.IO.File.Delete($"{AbsolutePath}{file}");
                        }
                        catch (DirectoryNotFoundException)
                        {
                            //log attempt to delete non-existent file
                            //let process continue to clean records which may have
                            //been out of sync with the file system
                            continue;
                        }
                    }
                    db.SaveChanges();
                }
                catch (KeyNotFoundException)
                {
                    Response.StatusCode = 404;
                    return Json(new { Response.StatusCode, Message = "Unable to Delete Article; Demo does not exist" });
                }
                catch
                {
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, Message = "Unable to Delete Article; Internal Server Error." });
                }
            }
            return Json(new { Response.StatusCode, Message = "Article Successfully Deleted"});
        }

        /// <summary>
        /// Saves all base-64-encoded image files to disk.
        /// </summary>
        private void ProcessImages()
        {
            foreach (HtmlNode node in NewImgList)
            {
                string fileName = node.GetAttributeValue("data-filename", null);
                if (fileName != null)
                {
                    byte[] base64img = Convert.FromBase64String(node.GetAttributeValue("src", null)
                        .Split(new[] { "base64," }, StringSplitOptions.None).Last());
                    using (FileStream fs = System.IO.File.Create($"{AbsolutePath}{fileName}"))
                    {
                        int bytesLeft = base64img.Count();
                        int byteOffset = 0;
                        int bytesToRead = 1000000;
                        while (bytesLeft > 0)
                        {
                            fs.Write(base64img, byteOffset, Math.Min(bytesToRead, bytesLeft));
                            byteOffset += bytesToRead;
                            bytesLeft -= bytesToRead;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines which newly added files may result in previously-existing files being overwritten.
        /// </summary>
        /// <returns>list of file names which would be overwritten</returns>
        private List<string> getOverwriteConflicts()
        {
            List<string> overwrites = new List<string>();
            foreach (HtmlNode node in NewImgList) {
                string fileName = node.GetAttributeValue("data-filename", null);
                if (System.IO.File.Exists($"{AbsolutePath}{fileName}"))
                {
                    overwrites.Add(fileName);
                }
            }
            return overwrites;
        }

        /// <summary>
        /// Adds new Image record to the database.
        /// </summary>
        /// <param name="articleID">Article ID to associate images with</param>
        private void AddNewImages(int articleID)
        {
            using (ContentContext db = new ContentContext())
            {
                foreach (HtmlNode img in NewImgList)
                {
                    string fileName = img.GetAttributeValue("data-filename", null);
                    //Check if duplicate. Add if not.
                    if (db.Images.SingleOrDefault(i => i.FileName == fileName && i.Article_ID == articleID) == null)
                    {
                        db.Images.Add(new Image()
                        {
                            Article_ID = articleID,
                            FileName = fileName
                        });
                    }
                }
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Sorts all image nodes into old and new images based on
        /// whether the src is a url or base64 string.
        /// </summary>
        /// <param name="collection">collection of img nodes to be sorted</param>
        private void CheckImgConflict(HtmlNode node)
        {
            bool stored = false;
            if (NewImgList.Count() > 0)
            {
                foreach (HtmlNode storedNode in NewImgList)
                {
                    if (storedNode.GetAttributeValue("data-filename","One") == node.GetAttributeValue("data-filename","Two"))
                    {
                        //Check that there are no identically-named nodes with different src contents.
                        if (storedNode.GetAttributeValue("src", null) != node.GetAttributeValue("src", null))
                        {
                            throw new Exception("Images with identical file names must have identical content.");
                        }
                        stored = true;
                        break;
                    }
                }
            }
            if (!stored)
            {
                NewImgList.Add(node.Clone());
            }
        }

        /// <summary>
        /// Deletes any orphaned db records resulting from article update.
        /// Deletes images from file system which only belong to a newly orphaned record.
        /// </summary>
        /// <param name="articleID">ID of article which was deleted</param>
        private void RemoveOrphanedImages(int articleID)
        {
            using (ContentContext db = new ContentContext())
            {
                IQueryable<Image> records = db.Images.Where(i => i.Article_ID == articleID);
                List<Image> recordsToDelete = new List<Image>();

                //get list of images no longer used by the updated article
                foreach (Image img in records)
                {
                    bool isInUpdatedContent = false;
                    foreach (string file in UsedImgList)
                    {
                        if (img.FileName == file.Split('/').Last())
                        {
                            isInUpdatedContent = true;
                            break;
                        }
                    }
                    if (!isInUpdatedContent)
                    {
                        recordsToDelete.Add(img);
                    }
                }
                foreach (Image img in recordsToDelete)
                {
                    db.Images.Remove(img);
                    // Delete file if no records exist outside the article.
                    if (db.Images.Where(i => i.FileName == img.FileName && i.Article_ID != articleID).Count() == 0)
                    {
                        string filePath = $"{AbsolutePath}{img.FileName}";
                        System.IO.File.SetAttributes(filePath, FileAttributes.Normal);
                        System.IO.File.Delete(filePath);
                    }
                    // Save after each iteration to keep file system and database in sync in event of error. 
                    db.SaveChanges();
                }
            }
        }

        private void RecurseImgNodes(HtmlNode node)
        {
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.HasChildNodes)
                {
                    RecurseImgNodes(child);
                }
                else
                {
                    if (child.Name == "img")
                    {
                        string fileName = child.GetAttributeValue("data-filename", null);
                        if (fileName != null)
                        {
                            CheckImgConflict(child);
                            child.SetAttributeValue("src", $"{RelativePath}{fileName}");
                            child.Attributes.Remove("data-filename");
                        }
                        UsedImgList.Add(child.GetAttributeValue("src", null));
                    }
                }
            }
        }
    }
}