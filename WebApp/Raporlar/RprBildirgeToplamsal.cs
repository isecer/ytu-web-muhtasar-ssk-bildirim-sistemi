using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using WebApp.Models;

namespace WebApp.Raporlar
{
    public partial class RprBildirgeToplamsal : DevExpress.XtraReports.UI.XtraReport
    {
        public RprBildirgeToplamsal(string DonemAdi)
        {
            InitializeComponent();
            lblRpTarih.Text = "Rapor Alma Tarihi: " + DateTime.Now.ToString("dd-MM-yyyy");
            lblDonemAdi.Text = DonemAdi;
        }

    }
}
