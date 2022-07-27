using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = false, Duration = 4, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.BirimToplamsalRapor)]
    public class RprBirimToplamsalController : Controller
    {
        // GET: RprBirimToplamsal
        public ActionResult Index()
        {
            var VsData = Management.CmbVASurecler(false);
            var VASurecID = (VsData.Any(a => a.Value.HasValue) ? VsData.First().Value.Value : 0);
            ViewBag.VASurecID = new SelectList(VsData, "Value", "Caption", null);
            ViewBag.AyID = new SelectList(Management.CmbVASurecleriAylar(VASurecID), "Value", "Caption", DateTime.Now.Month);
            ViewBag.Birimler = Management.CmbKullaniciRaporAnaBirimlerTree(false);

            return View();
        }

        public ActionResult getAyBilgisi(int VASurecID)
        {
            var AyData = Management.CmbVASurecleriAylar(VASurecID); 
            return AyData.ToJsonResult();
        }
    }
}