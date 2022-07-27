using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BiskaUtil;
using WebApp.Models;
using Database;
using Quartz;
using WebApp.Jobs;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.SistemAyarlari)]
    public class SistemAyarlariController : Controller
    {
        private IScheduler _scheduler;
        public SistemAyarlariController()
        {
            _scheduler = MvcApplication._quartzScheduler;
        }
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index(string UnToggleKategoris)
        {
            var _UnToggleKategoris = (UnToggleKategoris ?? "").Split(',').ToList();
            var data = db.Ayarlars.OrderBy(o => o.Kategori).ThenBy(t => t.SiraNo).ToList();
            var cats = data.Select(s => new { s.Kategori, Toggle = !_UnToggleKategoris.Contains(s.Kategori) }).Distinct().ToList();
            var PanelToggled = new Dictionary<string, bool>();
            foreach (var item in cats)
            {
                PanelToggled.Add(item.Kategori, item.Toggle);
            }
            ViewBag.PanelToggled = PanelToggled;
            return View(data);
        }
        [HttpPost]
        public ActionResult Index(List<string> AyarAdi, List<string> AyarDegeri, List<string> PanelToggled)
        {
            var qSistemAyarAdi = AyarAdi.Select((s, Index) => new { inx = Index, s }).ToList();
            var qSistemAyarDegeri = AyarDegeri.Select((s, Index) => new { inx = Index, s }).ToList();

            var qModel = (from sa in qSistemAyarAdi
                          join sad in qSistemAyarDegeri on sa.inx equals sad.inx
                          select new
                          {
                              RowID = sa.inx,
                              AyarAdi = sa.s,
                              AyarDegeri = sad.s,
                          }).ToList();
            bool StartJob = false;
            foreach (var item in qModel)
            {
                var ayar = db.Ayarlars.Where(p => p.AyarAdi == item.AyarAdi).FirstOrDefault();
                if (ayar != null)
                {

                    ayar.AyarDegeri = item.AyarDegeri;
                    if (ayar.AyarAdi == SistemAyar.GeciciDosyalarOtomatikOlarakKaldirilsin)
                    {
                        if (ayar.AyarDegeri.ToBoolean(false)) StartJob = true;
                    }

                }
            }
            db.SaveChanges();

            MessageBox.Show("Sistem Ayarları Güncellendi", MessageBox.MessageType.Success);

            var _PanelToggled = new Dictionary<string, bool>();
            foreach (var item in PanelToggled)
            {
                var _ptg = item.Replace("__", "◘").Split('◘');
                _PanelToggled.Add(_ptg[0], _ptg[1].ToBoolean().Value);
            }
            ViewBag.PanelToggled = _PanelToggled;
            if (StartJob)
            {
                var _UnToggleKategoris = string.Join(",", _PanelToggled.Where(p => p.Value == false).Select(s => s.Key).ToList());
                return RedirectToAction("StartJob", "SistemAyarlari", new { UnToggleKategoris = _UnToggleKategoris });
            }
            else _scheduler.PauseAll();
            var data = db.Ayarlars.OrderBy(o => o.Kategori).ThenBy(t => t.SiraNo).ToList();

            return View(data);
        }
        public async Task<ActionResult> StartJob(string UnToggleKategoris)
        {
            await _scheduler.Clear(); 
            await _scheduler.Start();
            await _scheduler.ResumeAll();
            IJobDetail job = Jobs.Jobs.JobDetailGeciciDosyaTemizleme();
            ITrigger trigger = Jobs.Jobs.TriggerGeciciDosyaTemizleme();
            await _scheduler.ScheduleJob(job, trigger);
            return RedirectToAction("Index", "SistemAyarlari", new { UnToggleKategoris = UnToggleKategoris });
        }
    }

}
