using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ax.Server.Controllers
{
    public class ShopfloorController : Controller
    {
        //
        // GET: /Shopfloor/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Bill(string progId, int funType)
        {
            this.ViewBag.ProgId = LibSysUtils.ToString(progId);
            this.ViewBag.FunType = funType;
            this.ViewBag.VclClass = string.Format("{0}Vcl", this.ViewBag.ProgId.Replace(".", string.Empty));
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Workstation()
        {
            return View();
        }
        public ActionResult StorageLogin()
        {
            return View();
        }
        public ActionResult StorageWorkstation()
        {
            return View();
        }
        public ActionResult MainTenanceLogin()
        {
            return View();
        }
        public ActionResult MainTenanceStation()
        {
            return View();
        }
        //
        // GET: /Shopfloor/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Shopfloor/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Shopfloor/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Shopfloor/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Shopfloor/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Shopfloor/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Shopfloor/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        #region 圣诺盟
        public ActionResult SNMMainTenanceLogin()
        {
            return View();
        }
        public ActionResult SNMMainTenanceStation()
        {
            return View();
        }

        public ActionResult SNMYCLogin()
        {
            return View();
        }
        public ActionResult SNMYCWorkStation()
        {
            return View();
        }
        #endregion
    }
}
