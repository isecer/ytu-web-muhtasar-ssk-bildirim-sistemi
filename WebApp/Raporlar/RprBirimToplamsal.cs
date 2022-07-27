using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace WebApp.Raporlar
{
    public partial class RprBirimToplamsal : DevExpress.XtraReports.UI.XtraReport
    {
        public RprBirimToplamsal()
        {
            InitializeComponent();
            lblRpTarih.Text = "Rapor Alma Tarihi: " + DateTime.Now.ToString("dd-MM-yyyy");
        }

    }
}
