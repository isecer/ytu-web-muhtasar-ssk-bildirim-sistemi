using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace BiskaUtil
{
    [Serializable]
    public class PagerOption
    {
        public bool Expand { get; set; }
        public string Sort { get; set; }
        public string Sender { get; set; }
        public int PageIndex { get; set; }
        public string SelectedIds { get; set; }

        int pageSize = 25;
        public int PageSize
        {
            get
            {
                return pageSize;
            }
            set
            {
                pageSize = value;
                if (!PageSizes.Contains(pageSize))
                {
                    var lst = PageSizes.ToList();
                    lst.Add(pageSize);
                    PageSizes = lst.OrderBy(o => o).ToList();
                }
            }
        }
        public int RowCount { get; set; }

        public int StartRowIndex
        {
            get
            {
                return (PageIndex - 1) * PageSize;
            }
        }
        public int PageCount
        {
            get
            {
                if (PageSize == 0) return 0;
                var pg = (RowCount / PageSize);
                if (RowCount % PageSize != 0) pg++;
                return pg;
            }
        }
        public int PrevPageIndex
        {
            get
            {
                return PageIndex > 0 ? (PageIndex - 1) : 1;
            }
        }
        public int NextPageIndex
        {
            get
            {
                return PageIndex < PageCount ? (PageIndex + 1) : PageCount;
            }
        }
        public int LastPageIndex
        {
            get
            {
                return PageCount;
            }
        }
        public bool CanPrev
        {
            get
            {
                return RowCount > 0 && PageIndex > 1;
            }
        }
        public bool CanNext
        {
            get
            {
                return RowCount > 0 && PageIndex < PageCount;
            }
        }
        public bool CanFirst
        {
            get
            {
                return RowCount > 0 && PageIndex > 1;
            }
        }
        public bool CanLast
        {
            get { return RowCount > 0 && PageIndex != PageCount; }
        }

        //public int[] PageSizes = { 10,15,20, 25, 30, 50, 100, 200, 500, 1000, 10000 };
        public List<int> PageSizes = new List<int>() { 10, 15, 20, 25, 30, 50 };

        public PagerOption()
        {
            PageIndex = 1;
            PageSize = 20;
            RowCount = 0;
        }
        public PagerOption Clone()
        {
            PagerOption po = new PagerOption();
            po.PageIndex = this.PageIndex;
            po.RowCount = this.RowCount;
            po.PageSize = this.PageSize;
            return po;
        }
        public Dictionary<string, object> GetValues()
        {
            var parentType = this.GetType().UnderlyingSystemType;
            var props = parentType.GetProperties();
            Dictionary<string, object> dicts = new Dictionary<string, object>();
            foreach (var prop in props)
            {
                try
                {
                    var pval = prop.GetValue(this, null);
                    dicts.Add(prop.Name, pval);
                }
                catch { }
            }
            return dicts;
        }

        public MvcHtmlString ToPagerString(string Culture = "tr_TR")
        {
            string str = string.Concat(new object[] { "<input type='hidden' id='Sort'      name='Sort' value='", this.Sort, "'/><input type='hidden' id='Sender'    name='Sender' value='", this.Sender, "'/><input type='hidden' id='PageIndex' name='PageIndex' value='", this.PageIndex, "'/><input type='hidden' id='PageSize'  name='PageSize' value='", this.PageSize, "'/><input type='hidden' id='RowCount'  name='RowCount' value='", this.RowCount, "'/><input type='hidden' id='SelectedIds'  name='SelectedIds' value='", this.SelectedIds, "'/>" });
            string str2 = "";
            foreach (int num in this.PageSizes)
            {
                object obj2 = str2;
                str2 = string.Concat(new object[] { obj2, "<option ", (num == this.PageSize) ? "selected=selected" : "", " value=", num, ">", num, "</option>" });
            }
            string str3 = "Listelenen:";
            if (Culture == "en_US")
            {
                str3 = "Listed:";
            }
            string str4 = string.Concat(new object[] { "<div style='width:220px;float:left;'>", str3, " (", (this.RowCount > 0) ? (this.StartRowIndex + 1) : this.StartRowIndex, "-", (this.PageSize < this.RowCount) ? (((this.StartRowIndex + this.PageSize) > this.RowCount) ? this.RowCount : (this.StartRowIndex + this.PageSize)) : this.RowCount, ")/", this.RowCount, "</div>" });
            return new MvcHtmlString(string.Concat(new object[] {
        str4, "<div class='dataTables_paginate paging_simple_numbers pgrBiska'>", str, "<a class='btn btn-default btn-xs  pgrIlk  ", ((this.PageIndex < 2) || (this.PageCount < 2)) ? "disabled" : "", "' href='javascript:void(0)'><i class='fa fa-fast-backward'></i></a><span><a class='btn btn-default btn-xs  pgrGeri ", ((this.PageIndex < 2) || (this.PageCount < 2)) ? "disabled" : "", "'><i class='fa fa-step-backward'></i></a><a class='btn btn-default btn-xs ' href='javascript:void(0)' style='padding: 0px;'><input type='text' class='pgrPageIndex' value=", this.PageIndex, " style='width: 30px;height:21px;'></a><a class='btn btn-default btn-xs  disabled' href='javascript:void(0)' style='padding-right: 1px; padding-left: 1px;'>/</a><a class='btn btn-default btn-xs  pgrToplamSayfa disabled' href='javascript:void(0)'>", this.PageCount, "</a><a class='btn btn-default btn-xs  pgrGit ", (this.PageCount < 2) ? "disabled" : "", "'><i class='fa fa-play'></i></a><a class='btn btn-default btn-xs  pgrIleri ", ((this.PageIndex >= this.PageCount) || (this.PageCount < 2)) ? "disabled" : "", "'><i class='fa fa-step-forward'></i></a></span><a class='btn btn-default btn-xs  pgrSon ",
        ((this.PageIndex >= this.PageCount) || (this.PageCount < 2)) ? "disabled" : "", " href='javascript:void(0)'><i class='fa fa-fast-forward'></i></a><a class='btn btn-default btn-xs ' style='padding: 0px;'><select class='pgrSatirSayisi' style='padding: 1px; width: 50px;height:21px;'>", str2, "</select></a></div>"
     }));
        }

    }

    public static class Helper
    {
        public static UrlHelper Url
        {
            get
            {
                return new UrlHelper(HttpContext.Current.Request.RequestContext);
            }
        }
    }
}