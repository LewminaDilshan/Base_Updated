using ExcelDataReader;
using RedLineLanka_Enterprise.Areas.Base.Models;
using RedLineLanka_Enterprise.Common;
using RedLineLanka_Enterprise.Common.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace RedLineLanka_Enterprise.Areas.Base.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Home", "DashBoard");
        }

        public ActionResult GetTiles()
        {
            ViewBag.Tiles = GetTilesList();
            return View("Index");
        }

        private List<TileData> GetTilesList()
        {
            var lst = new List<TileData>();

                lst.Add(new TileData()
                {
                    Text = "Test",
                    LandingURL = Url.Action("ChangeActingDG", "Home", new { area = "Base" }),
                    DataURL = Url.Action("GetActingDGStatus", "Home", new { area = "Base" }),
                    ColorClass = "tile2",
                    IconURL = Url.Content("~/Content/Images/dropStudent.png")
                });


            return lst;
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SignIn()
        {
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated && Session[BaseController.sskCurUsrID] != null)
            { return RedirectToAction("Home", "Dashboard"); }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn([Bind(Include = "UserName,PassWord,RememberMe")] SignInVM signInVM, string ReturnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                { return View(signInVM); }

                var lst = db.Users.Where(x => x.UserName.ToLower() == signInVM.UserName.ToLower()).ToList();
                var obj = lst.Where(x => x.Password.Decrypt() == signInVM.PassWord).FirstOrDefault();

                if (obj == null)
                {
                    AddAlert(AlertStyles.danger, "The user name or password provided is incorrect.");
                    return View(signInVM);
                }
                //if (obj.Status == ActiveState.Inactive)
                //{
                //    AddAlert(AlertStyles.warning, "The user \"" + obj.UserName + "\" is inactive. Please contact IT Administrator.");
                //    return View(signInVM);
                //}

                var jser = new JavaScriptSerializer();
                var lstRoles = db.UserRoles.Include(x => x.Role).Where(x => x.UserID == obj.UserID).Select(x => x.Role.Code).ToList();

                var authTicket = new FormsAuthenticationTicket(
                    1,
                    obj.UserName,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(20),
                    signInVM.RememberMe,
                    jser.Serialize(new { userName = obj.UserName, roles = lstRoles }));

                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);
                Session[sskCurUsrID] = obj.UserID;

                if (Url.IsLocalUrl(ReturnUrl) && ReturnUrl.Length > 1 && ReturnUrl.StartsWith("/")
                    && !ReturnUrl.StartsWith("//") && !ReturnUrl.StartsWith("/\\"))
                { return Redirect(ReturnUrl); }
                else
                { return RedirectToAction("Home", "Dashboard"); }
            }
            catch (Exception)
            {
                AddAlert(AlertStyles.danger, "Error while communicating with NEON. Please contact SKYBLUE Software Solutions.");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("SignIn", "Home");
        }

        public ActionResult Error(int? id)
        {
            return View("~/Areas/Base/Views/Home/Error.cshtml", id);
        }

        public ActionResult ChangePassword()
        {
            var user = db.Users.Find(CurUserID);
            if (user == null)
            {
                return HttpNotFound();
            }

            return PartialView("_ChangePassword", new SignInVM(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "UserID,PassWord,NewPassword,ConfirmPassword")] SignInVM signInVM)
        {
            var objUser = db.Users.Find(signInVM.UserID);

            if (objUser.Password.Decrypt() != signInVM.PassWord)
            { ModelState.AddModelError("Password", "Incorrect password."); }
            else if (signInVM.PassWord == signInVM.NewPassword)
            { ModelState.AddModelError("NewPassword", "New password is same as the current password."); }
            else if (signInVM.ConfirmPassword != signInVM.NewPassword)
            { ModelState.AddModelError("ConfirmPassword", "Confirm password should be equal to new password."); }

            if (ModelState.IsValid)
            {
                objUser.Password = signInVM.NewPassword.Encrypt();
                objUser.ModifiedBy = this.GetCurrUser();
                objUser.ModifiedDate = DateTime.Now;

                db.SaveChanges();
                return Json(new { success = true });
            }

            return PartialView("_ChangePassword");
        }

        public class TileData
        {
            public string Text { get; set; }
            public string LandingURL { get; set; }
            public string DataURL { get; set; }
            public string ColorClass { get; set; }
            public string IconURL { get; set; }
            public bool OpenInNewTab { get; set; }
        }
    }
}