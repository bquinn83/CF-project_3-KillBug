﻿using KillBug.Classes;
using KillBug.Models;
using KillBug.ViewModels;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace KillBug.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserRolesHelper RolesHelper = new UserRolesHelper();
        private ApplicationDbContext db = new ApplicationDbContext();
        private NotificationHelper notifications = new NotificationHelper();

        //GET: TicketSettings
        public ActionResult TicketSettings()
        {
            var model = new TicketSettingsVM
            {
                Types = db.TicketTypes.ToList(),
                Status = db.TicketStatus.ToList(),
                Priorities = db.TicketPriorities.ToList()
            };

            return View(model);
        }
        //GET: ManageRoles
        public ActionResult ManageRoles()
        {
            var users = db.Users.ToList();
            var viewData = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                viewData.Add(new UserRoleViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = RolesHelper.ListUserRoles(user.Id).FirstOrDefault() ?? "None"
                });
            }
            ViewBag.UserIds = new MultiSelectList(db.Users, "Id", "Email");
            ViewBag.RoleName = new SelectList(db.Roles, "Name", "Name");

            return View(viewData);
        }

        //POST: ManageRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageRoles(List<string> userIds, string roleName)
        {
            if (userIds != null)
            {
                foreach (var userId in userIds)
                {
                    var userRole = RolesHelper.ListUserRoles(userId).FirstOrDefault();
                    if (userRole != null)
                    {
                        RolesHelper.RemoveUserFromRole(userId, userRole);
                    }
                    RolesHelper.AddUserToRole(userId, roleName);

                    //send notification
                    notifications.RoleChangeNotification(userId, roleName);
                }
            }
            return RedirectToAction("ManageRoles");
        }

        // GET: Admin
        public ActionResult AskAboutRoles()
        {
            if (RolesHelper.UsersInRole("Developer").Count == 0)
            {
                return View("NoDevs");
            }
            return View();
        }

        [HttpPost]
        public JsonResult AddTicketType(string type, string description)
        {
            if (type != null)
            {
                var ticket = new TicketType(type, description);
                db.TicketTypes.Add(ticket);
                db.SaveChanges();
                return Json(ticket);
            }
            else
            {
                return Json(false);
            }
        }
        [HttpPost]
        public JsonResult AddTicketStatus(string name)
        {
            if (name != null)
            {
                var status = new TicketStatus(name);
                db.TicketStatus.Add(status);
                db.SaveChanges();
                return Json(status);
            }
            else
            {
                return Json(false);
            }
        }
        [HttpPost]
        public JsonResult AddTicketPriority(string name)
        {
            if (name != null)
            {
                var priority = new TicketPriority(name);
                db.TicketPriorities.Add(priority);
                db.SaveChanges();
                return Json(priority);
            }
            else
            {
                return Json(false);
            }
        }
    }

    public class TicketSettingsVM
    {
        public List<TicketType> Types = new List<TicketType>();
        public List<TicketStatus> Status = new List<TicketStatus>();
        public List<TicketPriority> Priorities = new List<TicketPriority>();

    }
}