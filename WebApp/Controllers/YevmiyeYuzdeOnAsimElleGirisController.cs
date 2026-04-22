
using BiskaUtil;
using Database;
using DevExpress.ClipboardSource.SpreadsheetML;
using Microsoft.Office.Interop.Excel;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize(Roles = RoleNames.YevmiyeYuzdeOnAsimElleGiris)]
    public class YevmiyeYuzdeOnAsimElleGirisController : Controller
    {
        private readonly MusskDBEntities db = new MusskDBEntities();

        public ActionResult Index()
        {
            return Index(new FmYevmiyeYuzdeOnAsimElleGiris());
        }

        [HttpPost]
        public ActionResult Index(FmYevmiyeYuzdeOnAsimElleGiris model, bool export = false)
        {
            var harcamaBirimIds = UserIdentity.Current.YevmiyeHarcamaBirimYetkileri;
            var q = from s in db.YevmiyelerYuzdeOnAsimHesapElleGirislers.Where(p => harcamaBirimIds.Contains(p.YevmiyeHarcamaBirimID)) select s;

            if (model.Yil.HasValue)
                q = q.Where(p => p.Tarih.Year == model.Yil);

            if (model.Tarih.HasValue)
                q = q.Where(p => p.Tarih.Date == model.Tarih.Value.Date);

            if (model.YevmiyeHarcamaBirimID.HasValue)
                q = q.Where(p => p.YevmiyeHarcamaBirimID == model.YevmiyeHarcamaBirimID);

            if (!model.HesapKod.IsNullOrWhiteSpace())
                q = q.Where(p => p.HesapKod.StartsWith(model.HesapKod) || p.HesapAdi.Contains(model.HesapAdi));

            if (!model.Aciklama.IsNullOrWhiteSpace())
                q = q.Where(p => p.Aciklama.Contains(model.Aciklama));

            if (model.IsAktif.HasValue)
                q = q.Where(p => p.IsAktif == model.IsAktif);
            model.RowCount = q.Count();

            q = q.OrderByDescending(x => x.YevmiyeYuzdeOnAsimHesapElleGirisID);

            var ps = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);

            model.Data = q.Select(x => new FrYevmiyeYuzdeOnAsimElleGiris
            {
                YevmiyeYuzdeOnAsimHesapElleGirisID = x.YevmiyeYuzdeOnAsimHesapElleGirisID,
                Tarih = x.Tarih,
                VergiKimlikNo = x.VergiKimlikNo,
                HarcamaBirimAdi = x.HarcamaBirimAdi,
                HesapKod = x.HesapKod,
                HesapAdi = x.HesapAdi,
                BrutTutar = x.BrutTutar,
                Aciklama = x.Aciklama,
                IsAktif = x.IsAktif
            }).Skip(ps.StartRowIndex).Take(model.PageSize).ToList();


            if (export)
            {
                var gv = new GridView();

                gv.DataSource = q.Select(x => new FrYevmiyeYuzdeOnAsimElleGiris
                {
                    YevmiyeYuzdeOnAsimHesapElleGirisID = x.YevmiyeYuzdeOnAsimHesapElleGirisID,
                    Tarih = x.Tarih,
                    VergiKimlikNo = x.VergiKimlikNo,
                    HarcamaBirimAdi = x.HarcamaBirimAdi,
                    HesapKod = x.HesapKod,
                    HesapAdi = x.HesapAdi,
                    BrutTutar = x.BrutTutar,
                    Aciklama = x.Aciklama,
                    Durum = x.IsAktif ? "Aktif" : "Pasif"
                }).ToList();
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);
                var fileName = "YevmiyeYuzdeOnAsimElleGirisListesi";
                if (model.Yil.HasValue) fileName += "_" + model.Yil;
                fileName += ".xls";
                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, fileName);

            }

            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.YevmiyeHarcamaBirimID);

            ViewBag.YevmiyeHarcamaBirimID = new SelectList(
                Management.CmbYevmiyelerBirim(true),
                "Value",
                "Caption",
                model.YevmiyeHarcamaBirimID
            );
            ViewBag.IndexModel = new MIndexBilgi { Toplam = model.RowCount };

            return View(model);
        }
        [Authorize(Roles = RoleNames.YevmiyeYuzdeOnAsimElleGirisKayitYetkisi)]

        public ActionResult Kayit(int? id)
        {
            var model = new YevmiyelerYuzdeOnAsimHesapElleGirisler();

            if (id.HasValue)
            {
                model = db.YevmiyelerYuzdeOnAsimHesapElleGirislers.Find(id);
            }
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbYevmiyelerBirim(), "Value", "Caption", model.YevmiyeHarcamaBirimID);

            return View(model);
        }

        [Authorize(Roles = RoleNames.YevmiyeYuzdeOnAsimElleGirisKayitYetkisi)]
        [HttpPost]
        public ActionResult Kayit(YevmiyelerYuzdeOnAsimHesapElleGirisler kModel, string dlgid = "")
        {
            var mmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };

            #region Kontrol

            if (kModel.Tarih == DateTime.MinValue)
            {
                mmMessage.Messages.Add("Tarih boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tarih" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Tarih" });

            if (kModel.YevmiyeHarcamaBirimID <= 0)
            {
                mmMessage.Messages.Add("Harcama birimi seçiniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeHarcamaBirimID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeHarcamaBirimID" });

            if (kModel.YevmiyeHesapKodHavuzID <= 0)
            {
                mmMessage.Messages.Add("Hesap kodu seçiniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeHesapKodHavuzID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeHesapKodHavuzID" });

            if (kModel.BrutTutar <= 0)
            {
                mmMessage.Messages.Add("Brüt tutar giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BrutTutar" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BrutTutar" });
            if (kModel.Aciklama.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Açıklama giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Aciklama" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Aciklama" });
            #endregion

            if (!mmMessage.Messages.Any())
            {
                var hesap = db.YevmiyelerHesapKodHavuzus
                    .First(x => x.YevmiyeHesapKodHavuzID == kModel.YevmiyeHesapKodHavuzID);

                kModel.HesapKod = hesap.HesapKod;
                kModel.HesapAdi = hesap.HesapAdi;

                var birim = db.YevmiyelerHarcamaBirimleris
                    .First(x => x.YevmiyeHarcamaBirimID == kModel.YevmiyeHarcamaBirimID);

                kModel.VergiKimlikNo = birim.VergiKimlikNo;
                kModel.HarcamaBirimAdi = birim.BirimAdi;

                if (db.YevmiyelerYuzdeOnAsimHesapElleGirislers.Any(a =>
                    a.YevmiyeHarcamaBirimID == kModel.YevmiyeHarcamaBirimID &&
                    a.YevmiyeHesapKodHavuzID == kModel.YevmiyeHesapKodHavuzID &&
                    a.YevmiyeYuzdeOnAsimHesapElleGirisID != kModel.YevmiyeYuzdeOnAsimHesapElleGirisID))
                {
                    mmMessage.Messages.Add("Bu harcama birimi ve hesap kodu daha önce tanımlanmıştır.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeHesapKodHavuzID" });
                }
            }

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;

                if (kModel.YevmiyeYuzdeOnAsimHesapElleGirisID <= 0)
                {
                    kModel.IsAktif = true;
                    db.YevmiyelerYuzdeOnAsimHesapElleGirislers.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerYuzdeOnAsimHesapElleGirislers
                        .First(p => p.YevmiyeYuzdeOnAsimHesapElleGirisID == kModel.YevmiyeYuzdeOnAsimHesapElleGirisID);

                    data.Tarih = kModel.Tarih;
                    data.YevmiyeHarcamaBirimID = kModel.YevmiyeHarcamaBirimID;
                    data.YevmiyeHesapKodHavuzID = kModel.YevmiyeHesapKodHavuzID;

                    data.HesapKod = kModel.HesapKod;
                    data.HesapAdi = kModel.HesapAdi;

                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.HarcamaBirimAdi = kModel.HarcamaBirimAdi;

                    data.BrutTutar = kModel.BrutTutar;
                    data.Aciklama = kModel.Aciklama;

                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            if (mmMessage.Messages.Count > 0)
            {
                var hesap = db.YevmiyelerHesapKodHavuzus
                    .FirstOrDefault(x => x.YevmiyeHesapKodHavuzID == kModel.YevmiyeHesapKodHavuzID);

                if (hesap != null)
                {
                    kModel.HesapKod = hesap.HesapKod;
                    kModel.HesapAdi = hesap.HesapAdi;
                }
            }
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbYevmiyelerBirim(), "Value", "Caption", kModel.YevmiyeHarcamaBirimID);
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }
        public ActionResult GetHesapKodlari(string term)
        {
            var data = db.YevmiyelerHesapKodHavuzus
                .Where(x => x.HesapKod.StartsWith("830") && (
                            x.HesapKod.StartsWith(term) ||
                            x.HesapAdi.Contains(term)))
                .Select(x => new
                {
                    id = x.YevmiyeHesapKodHavuzID,
                    text = x.HesapKod + " - " + x.HesapAdi,
                    x.HesapKod,
                    x.HesapAdi
                })
                .OrderBy(x => x.HesapKod)
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = RoleNames.YevmiyeYuzdeOnAsimElleGirisKayitYetkisi)]
        public ActionResult DurumDegistir(int id)
        {
            var data = db.YevmiyelerYuzdeOnAsimHesapElleGirislers.Find(id);

            if (data == null) return Json(new { success = false });

            if (!data.IsAktif)
            {
                if (!User.IsInRole(RoleNames.YevmiyeYuzdeOnAsimElleGirisAktifeAlmaYetkisi))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Bu kayıt pasife alınmıştır. Aktife alma yetkiniz yok!"
                    });
                }
            }

            data.IsAktif = !data.IsAktif;

            db.SaveChanges();

            return Json(new { success = true });
        }

        [Authorize(Roles = RoleNames.YevmiyeYuzdeOnAsimElleGirisKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var kayit = db.YevmiyelerYuzdeOnAsimHesapElleGirislers
                .FirstOrDefault(p => p.YevmiyeYuzdeOnAsimHesapElleGirisID == id);

            string message;
            var success = true;

            if (kayit != null)
            {
                try
                {
                    if (kayit.IsAktif && !RoleNames.YevmiyeYuzdeOnAsimElleGirisAktifeAlmaYetkisi.InRole())
                    {
                        message = "'" + kayit.HesapKod + " - " + kayit.HesapAdi + "' kadyı aktif durumda olduğu için silinemez. Önce kaydı pasif duruma alınız.";
                    }
                    else
                    {
                        message = "'" + kayit.HesapKod + " - " + kayit.HesapAdi + "' kaydı silindi.";

                        db.YevmiyelerYuzdeOnAsimHesapElleGirislers.Remove(kayit);
                        db.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    success = false;

                    message = "'" + kayit.HesapKod + "' kaydı silinemedi! <br/> Bilgi: " + ex.Message;

                    Management.SistemBilgisiKaydet(
                        message,
                        "YevmiyeYuzdeOnAsimElleGiris/Sil<br/><br/>" + ex.ToString(),
                        BilgiTipi.OnemsizHata
                    );
                }
            }
            else
            {
                success = false;
                message = "Kayıt bulunamadı!";
            }

            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}