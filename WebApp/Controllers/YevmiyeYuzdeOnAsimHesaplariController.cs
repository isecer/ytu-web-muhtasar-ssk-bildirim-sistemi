using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Models;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.YevmiyeYuzdeOnAsimHesaplari)]
    public class YevmiyeYuzdeOnAsimHesaplariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            return Index(new FmYevmiyeYuzdeOnAsimHesaplari { Yil = Yil }, export);
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeYuzdeOnAsimHesaplari model, bool export = false)
        {
            var data = GetMatchingAccounts(model.Yil.Value);

            model.Data = data;

            #region export
            if (export && model.Data.Any())
            {
                var gv = new GridView();
                gv.DataSource = model.Data.Where(p=>!p.IsGrupToplami).Select(s=> new
                {
                    HesapKodu=s.EslesenHesapKodu,
                    HesapAdi=s.EslesenHesapAdi,
                    Toplam=s.EslesenHesapToplam,
                    Yuzde10=s.EslesenHesapToplamYuzde10,
                    _830=s.Toplam,
                    s.Bakiye
                }).ToList();
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_YuzdeOnAsimHesaplariDokumu_" + model.Yil + "_" + model.AyID + ".xls");
            }
            #endregion
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            ViewBag.AyID = new SelectList(Management.CmbAylar(false), "Value", "Caption", model.AyID);
            return View(model);
        }
        public List<FrYevmiyeYuzdeOnAsimHesaplari> GetMatchingAccounts(int year)
        {
            // Normal hesaplar için sorgu
            var normalHesaplar = (from yb in db.Yevmiyelers.Where(p=>p.YevmiyeAlimKanunTurID.HasValue)
                                  where yb.YevmiyeTarih.Year == year
                                  from yhk in db.YevmiyelerHesapKodlaris
                                  where yhk.YevmiyeHesapKodTurID == HesapKoduTuru.YuzdeOnAsimHesapKodlari &&
                                        ((yhk.IsHesapKoduBaslangicEslesmesiYeterli && yb.HesapKod.StartsWith(yhk.HesapKod)) ||
                                         (!yhk.IsHesapKoduBaslangicEslesmesiYeterli && yb.HesapKod == yhk.HesapKod))
                                  group yb by yhk.HesapKod into g
                                  select new
                                  {
                                      HesapKodu = g.Key,
                                      Toplam = g.Sum(x => x.Alacak) - g.Sum(x => x.Borc)
                                  }).ToList();

            // 901'li hesaplar için sorgu
            var hesaplar901 = (from yb in db.Yevmiyelers
                               where yb.YevmiyeTarih.Year == year &&
                                     yb.HesapKod.StartsWith("901")
                               group yb by new { HesapKodu = "901." + yb.HesapKod.Substring(yb.HesapKod.Length - 5, 5), yb.HesapAdi } into g
                               select new
                               {
                                   g.Key.HesapKodu,
                                   g.Key.HesapAdi,
                                   Toplam = g.Sum(x => x.Alacak) - g.Sum(x => x.Borc)
                               }).ToList();



            // Memory'de eşleştirme ve gruplandırma
            var sonuclar = (from nh in normalHesaplar
                            from h901 in hesaplar901
                            where nh.HesapKodu.Substring(4, 5) == h901.HesapKodu.Substring(4, 5)
                            select new FrYevmiyeYuzdeOnAsimHesaplari
                            {
                                EslesenHesapKodu = h901.HesapKodu,
                                EslesenHesapAdi = h901.HesapAdi,
                                EslesenHesapToplam = h901.Toplam,
                                EslesenHesapToplamYuzde10 = h901.Toplam * (decimal)0.1,
                                HesapKodu = nh.HesapKodu,
                                Toplam = nh.Toplam,
                                Bakiye = (h901.Toplam * (decimal)0.1) - nh.Toplam,
                                GrupHesapKodu = GetHesapKodu(h901.HesapKodu),
                                GrupAdi = GetGrupAdi(h901.HesapKodu)
                            }).ToList();

            // Grup toplamlarını hesapla ve ekle
            var grupTotalleri = sonuclar
                .GroupBy(x => new { x.GrupAdi, x.GrupHesapKodu })
                .Select(g => new FrYevmiyeYuzdeOnAsimHesaplari
                {
                    EslesenHesapKodu = g.Key.GrupHesapKodu,
                    EslesenHesapAdi = g.Key.GrupAdi,
                    EslesenHesapToplam = g.Sum(x => x.EslesenHesapToplam),
                    EslesenHesapToplamYuzde10 = g.Sum(x => x.EslesenHesapToplamYuzde10),
                    HesapKodu = $"TOPLAM - {g.Key}",
                    Toplam = g.Sum(x => x.Toplam),
                    Bakiye = g.Sum(x => x.Bakiye),
                    GrupAdi = g.Key.GrupAdi,
                    IsGrupToplami = true
                })
                .ToList();

            // Sonuçları ve grup toplamlarını birleştir
            sonuclar.AddRange(grupTotalleri);
            return sonuclar;
        }

        // Grup adlarını belirle
        string GetGrupAdi(string hesapKodu)
        {
            if (hesapKodu.StartsWith("901.03.05"))
                return "Hizmet Alımları";
            if (hesapKodu.StartsWith("901.03"))
                return "Tüketim";
            if (hesapKodu.StartsWith("901.06"))
                return "Yapım İşleri";
            return "Diğer";
        }
        string GetHesapKodu(string hesapKodu)
        {
            if (hesapKodu.StartsWith("901.03.05"))
                return "901.03.05";

            return hesapKodu.Substring(0,6);

        }

    }
}