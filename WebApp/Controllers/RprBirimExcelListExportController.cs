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
using WebApp.Raporlar;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = false, Duration = 4, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.BirimVeriCiktilariniAl)]
    public class RprBirimExcelListExportController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: RprBirimExcelListExport
        public ActionResult Index()
        {
            var VsData = Management.CmbVASurecler(false);
            var VASurecID = (VsData.Any(a => a.Value.HasValue) ? VsData.First().Value.Value : 0);
            ViewBag.VASurecID = new SelectList(VsData, "Value", "Caption", null);
            ViewBag.AyID = new SelectList(Management.CmbVASurecleriAylar(VASurecID), "Value", "Caption", DateTime.Now.Month);
            ViewBag.Birimler = Management.CmbKullaniciRaporAnaBirimlerTree(false);

            return View();
        }
        [HttpPost]
        public ActionResult Index(int VASurecID, int AyID, List<int> BirimID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = true;

            BirimID = BirimID ?? new List<int>();


            if (BirimID.Count <= 0)
            {
                MmMessage.Messages.Add("Çıktı alabilmek için en az bir birim seçilmelidir.");

                MmMessage.IsSuccess = false;
                var VsData = Management.CmbVASurecler(false);
                ViewBag.VASurecID = new SelectList(VsData, "Value", "Caption", null);
                ViewBag.AyID = new SelectList(Management.CmbVASurecleriAylar(VASurecID), "Value", "Caption", DateTime.Now.Month);
                ViewBag.Birimler = Management.CmbKullaniciRaporAnaBirimlerTree(false);
            }
            if (MmMessage.IsSuccess)
            {

                var qeData = (from s in db.VASurecleriBirimVerileris
                              join vb in db.VASurecleriBirims on s.VASurecleriBirimID equals vb.VASurecleriBirimID
                              where vb.VASurecID == VASurecID && s.AyID == AyID && (BirimID.Contains(vb.UstBirimID ?? vb.BirimID)) //&& UserIdentity.Current.BirimYetkileriRapor.Contains(vb.UstBirimID ?? vb.BirimID)
                              select new RpVASurecleriBirimVerileri
                              {
                                  BelgeMahiyetTipKodu = s.BelgeMahiyetTipKodu,
                                  BelgeTurKodu = s.BelgeTurKodu,
                                  DEsasKanunNo = s.DEsasKanunNo,
                                  YeniUniteKodu = s.YeniUniteKodu,
                                  EskiUniteKodu = s.EskiUniteKodu,
                                  IsyeriSiraNumarasi = s.IsyeriSiraNumarasi,
                                  IlKodu = s.IlKodu,
                                  AltIsverenNumarasi = s.AltIsverenNumarasi,
                                  SSKSicilNo = s.SSKSicilNo,
                                  TcKimlikNo = s.TcKimlikNo,
                                  Ad = s.Ad,
                                  Soyad = s.Soyad,
                                  PrimOdemeGun = s.PrimOdemeGun,
                                  UzaktanCalismaGun = s.UzaktanCalismaGun,
                                  HakEdilenUcret = s.HakEdilenUcret,
                                  PrimIkramiyeIstihkak = s.PrimIkramiyeIstihkak,
                                  IseGirisGunStr = s.IseGirisGunStr,
                                  IseGirisAyStr = s.IseGirisAyStr,
                                  IstenCikisGunStr = s.IstenCikisGunStr,
                                  IstenCikisAyStr = s.IstenCikisAyStr,
                                  IstenCikisNedenKodu = s.IstenCikisNedenKodu,
                                  EksikGunSayisi = s.EksikGunSayisi,
                                  EksikGunNedenKodu = s.EksikGunNedenKodu,
                                  MeslekTurKodu = s.MeslekTurKodu,
                                  IstirahatSurecindeCalismamistir = s.IstirahatSurecindeCalismamistir,
                                  TahakkukNedeni = s.TahakkukNedeni,
                                  HizmetDonemAy = s.HizmetDonemAy,
                                  HizmetDonemYil = s.HizmetDonemYil,
                                  GvDenMuafmi = s.GvDenMuafmi,
                                  // AsgariGecimIndirimi = s.AsgariGecimIndirimi,
                                  GvMatrahi = s.GvMatrahi,
                                  GvEngellilikOrani = s.GvEngellilikOrani,
                                  HesaplananGv = s.HesaplananGv,
                                  AsgariGecimIstisnaGvTutar = s.AsgariGecimIstisnaGvTutar,
                                  GvKesinti = s.GvKesinti,
                                  AsgariGecimIstisnaDvTutar = s.AsgariGecimIstisnaDvTutar,
                                  DvKesintisi = s.DvKesintisi


                              }).ToList();




                var rpr = new RprBirimVeriCiktisi();

                rpr.DataSource = qeData;
                var donemBilid = Management.GetVASurecKontrol(VASurecID, AyID);
                rpr.DisplayName = donemBilid.Yil + "-" + donemBilid.SecilenAyBilgi.AyAdi + " MUSSK Bildirimi Birimlere Göre Girilen Veriler ";
                var ms = new MemoryStream();
                rpr.ExportToXlsx(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return File(ms, "application/ms-excel", "Mussk Bildirim Exceli " + donemBilid.Yil + "-" + donemBilid.SecilenAyBilgi.AyAdi + ".xls");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
                return View();
            }
        }


        public ActionResult getAyBilgisi(int VASurecID)
        {
            var AyData = Management.CmbVASurecleriAylar(VASurecID);
            return AyData.ToJsonResult();
        }
    }
}