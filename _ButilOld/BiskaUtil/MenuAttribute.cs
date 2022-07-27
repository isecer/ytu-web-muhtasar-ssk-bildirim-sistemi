using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiskaUtil
{       
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class MenuAttribute : System.Attribute
    {
        public int MenuID { get; set; }
        public string MenuAdi { get; set; }
        public int BagliMenuID { get; set; }       
        public string ModulAdi { get; set; }
        public string MenuIconUrl { get; set; }
        public string MenuCssClass { get; set; }
        public string MenuUrl{ get; set; }
        public bool DilCeviriYap { get; set; }
        public bool YetkisizErisim { get; set; }
        public string AuthenticationControl { get; set; }
        public int SiraNo { get; set; }
        public string[] BagliRoller { get; set; }
        public MenuAttribute() {
            YetkisizErisim = false;
            BagliRoller = new string[0];
        }
    } 
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class RoleAttribute: System.Attribute
    {
        public int RolID { get; set; }
        public string RolAdi { get; set; }
        public string Aciklama { get; set; }
        public string ModulAdi { get; set; }
        public string Kategori { get; set; }
        public string GorunurAdi { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SettingAttribute : System.Attribute
    {
        public enum SettingAttributeType { Integer, Boolean, String, List, Date }
        public string Category { get; set; }
        public SettingAttributeType AttributeType { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Key { get; set; }
        public string DefaultValue { get; set; }
        public string DefaultValueList { get; set; }
        public string ModulName{ get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class CustomRoleAttribute : System.Attribute
    {
        public string ModulAdi{ get; set; }
        public string DisplayName { get; set; }
        public string PageUrl{ get; set; }
    }


    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    //public class BiskaAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    //{
    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
    //    {                   
    //        if (!base.AuthorizeCore(httpContext))
    //            return false;            
    //        bool retval = false;
    //        return retval;
    //    }
    //}
}