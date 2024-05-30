using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BiskaUtil;
using WebApp.Models;
using Database;
using Quartz;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.SistemAyarlari)]
    public class SistemAyarlariController : Controller
    {
        private readonly IScheduler scheduler = MvcApplication._quartzScheduler;
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index(string unToggleKategoris)
        {
            var unToggleKategoriList = (unToggleKategoris ?? "").Split(',').ToList();
            var data = db.Ayarlars.OrderBy(o => o.Kategori).ThenBy(t => t.SiraNo).ToList();
            var cats = data.Select(s => new { s.Kategori, Toggle = !unToggleKategoriList.Contains(s.Kategori) }).Distinct().ToList();
            var panelToggled = new Dictionary<string, bool>();
            foreach (var item in cats)
            {
                panelToggled.Add(item.Kategori, item.Toggle);
            }
            ViewBag.PanelToggled = panelToggled;
            return View(data);
        }
        [HttpPost]
        public ActionResult Index(List<string> ayarAdi, List<string> ayarDegeri, List<string> panelToggled)
        {
            var qSistemAyarAdi = ayarAdi.Select((s, index) => new { inx = index, s }).ToList();
            var qSistemAyarDegeri = ayarDegeri.Select((s, index) => new { inx = index, s }).ToList();

            var qModel = (from sa in qSistemAyarAdi
                          join sad in qSistemAyarDegeri on sa.inx equals sad.inx
                          select new
                          {
                              RowID = sa.inx,
                              AyarAdi = sa.s,
                              AyarDegeri = sad.s,
                          }).ToList();
            bool startJob = false;
            foreach (var item in qModel)
            {
                var ayar = db.Ayarlars.FirstOrDefault(p => p.AyarAdi == item.AyarAdi);
                if (ayar != null)
                {

                    ayar.AyarDegeri = item.AyarDegeri;
                    if (ayar.AyarAdi == SistemAyar.GeciciDosyalarOtomatikOlarakKaldirilsin)
                    {
                        if (ayar.AyarDegeri.ToBoolean(false)) startJob = true;
                    }

                }
            }
            db.SaveChanges();

            MessageBox.Show("Sistem Ayarları Güncellendi", MessageBox.MessageType.Success);

            var panelToggledDct = new Dictionary<string, bool>();
            foreach (var item in panelToggled)
            {
                var ptg = item.Replace("__", "◘").Split('◘');
                panelToggledDct.Add(ptg[0], ptg[1].ToBoolean().Value);
            }
            ViewBag.PanelToggled = panelToggledDct;
            if (startJob)
            {
                var unToggleKategoriList = string.Join(",", panelToggledDct.Where(p => p.Value == false).Select(s => s.Key).ToList());
                return RedirectToAction("StartJob", "SistemAyarlari", new { UnToggleKategoris = unToggleKategoriList });
            }
            else scheduler.PauseAll();
            var data = db.Ayarlars.OrderBy(o => o.Kategori).ThenBy(t => t.SiraNo).ToList();

            return View(data);
        }
        public async Task<ActionResult> StartJob(string unToggleKategoris)
        {
            await scheduler.Clear(); 
            await scheduler.Start();
            await scheduler.ResumeAll();
            IJobDetail job = Jobs.JobDetailGeciciDosyaTemizleme();
            ITrigger trigger = Jobs.TriggerGeciciDosyaTemizleme();
            await scheduler.ScheduleJob(job, trigger);
            return RedirectToAction("Index", "SistemAyarlari", new { UnToggleKategoris = unToggleKategoris });
        }
    }

}
