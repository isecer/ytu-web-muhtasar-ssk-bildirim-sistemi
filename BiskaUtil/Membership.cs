using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
namespace BiskaUtil
{
    /// <summary>
    /// use RoleAttribute
    /// </summary>
    public interface IRoleName
    {
    }

    /// <summary>
    /// use  MenuAttribute
    /// </summary>   
    public interface IMenu
    {

    }

    /// <summary>
    /// protected void Application_Start()
    /// {
    ///
    ///        1-Eğer Kullanıcı Bilgileri Tekrar Gerekirse Nerden Alınacak?
    ///        BiskaUtil.Membership.OnRequireUserIdentity += Membership_OnRequireUserIdentity;                
    ///        
    ///        2-Aşağıdaki Roller ve Menüller DB'ye İşlenir
    ///        Management.Update();
    ///        ----- Membership.RoleFields(); veya Roles()
    ///        ----- Membership.MenuFields(); veya Menus()
    ///        3-Sistem Hatalarının Takibi İçin
    ///        BiskaUtil.SystemInformation.OnEvent += SystemInformation_OnEvent;
    ///  }
    ///  void Membership_OnRequireUserIdentity(string UserName, ref UserIdentity userIdentity)
    ///  {
    ///          // DB'den Çekilen Bilgiler Aktarılıyor
    ///       userIdentity= Management.GetUserIdentity(UserName); 
    ///
    ///  }
    ///  void SystemInformation_OnEvent(SystemInformation info)
    ///  {
    ///       Management.AddMessage(info);
    ///  }
    ///  protected void Application_AcquireRequestState(Object sender, EventArgs e)
    ///  {
    ///         4-BiskaUtil.UserIdentity.SetCurrent();//Zorunlu
    ///  }
    /// </summary>
    public class Membership
    {
        public delegate void OnRequireUserIdentityEventHandler(string UserName,ref UserIdentity userIdentity);
        public static event OnRequireUserIdentityEventHandler OnRequireUserIdentity;

        public static FieldInfo[] RoleFields()
        {
            var type = typeof(IRoleName);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p)).ToArray();
            List<FieldInfo> infos = new List<FieldInfo>();
            foreach (var typex in types)
            {
                var fields = typex.GetFields().Where(p=>p.IsLiteral);
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(null);
                    if (fieldValue != null)
                        infos.Add(field);
                    //var attr = field.GetCustomAttributes<MenuAttribute>();
                }
            }
            return infos.ToArray();
            //var type = typeof(IRoleName);
            //return AppDomain.CurrentDomain.GetAssemblies()
            //    .Where(p => type.IsAssignableFrom(p.GetType()))
            //    .SelectMany(s => s.GetType().GetFields())
            //    .Where(p => p.IsLiteral)
            //    .AsEnumerable();
        }
        public static IEnumerable<FieldInfo> MenuFields()
        {
            //var type = typeof(IMenu);
            //return AppDomain.CurrentDomain.GetAssemblies()
            //    .Where(p => type.IsAssignableFrom(p.GetType()))
            //    .SelectMany(s => s.GetType().GetFields())
            //    .Where(p => p.IsLiteral)
            //    .AsEnumerable();
            var type = typeof(IMenu);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p)).ToArray();
            List<FieldInfo> infos = new List<FieldInfo>();
            foreach (var typex in types)
            {
                var fields = typex.GetFields().Where(p => p.IsLiteral);
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(null);
                    if (fieldValue != null)
                        infos.Add(field);
                    //var attr = field.GetCustomAttributes<MenuAttribute>();
                }
            }
            return infos.ToArray();
        }
        public static RoleAttribute[] Roles()
        {
            var fields = RoleFields();
            List<RoleAttribute> roles = new List<RoleAttribute>();
            foreach (var field in fields) 
            {
                //var attr = field.GetCustomAttribute<RoleAttribute>(); 
                var oAttr = Attribute.GetCustomAttribute(field, typeof(RoleAttribute));
                if (oAttr != null)
                {
                    RoleAttribute attr = (RoleAttribute)oAttr;  
                    
                    var oVal = field.GetValue(null);
                    if (oVal == null) continue;
                    var key = string.IsNullOrWhiteSpace(attr.RolAdi) ? oVal.ToString() : attr.RolAdi;
                    attr.RolAdi = key;
                    var rolKey = field.DeclaringType.FullName + "." + field.Name;
                    if (attr.RolID == 0) attr.RolID = 1000000000 + rolKey.ToCrc16();
                    roles.Add(attr);
                }
            }
            return roles.ToArray();
        }
        public static MenuAttribute[] Menus()
        {
            var fields = MenuFields();
            var allRoles = Roles();
            List<MenuAttribute> menus = new List<MenuAttribute>();
            foreach (var field in fields)
            {
                //var attr = field.GetCustomAttribute<MenuAttribute>();                                
                var omenuAttr = Attribute.GetCustomAttribute(field, typeof(MenuAttribute));
                var oroleAttr = Attribute.GetCustomAttribute(field, typeof(RoleAttribute));
                if (omenuAttr != null)
                {
                    MenuAttribute attr = (MenuAttribute)omenuAttr;                    
                    var oVal = field.GetValue(null);
                    if (oVal == null) continue;
                    var key = string.IsNullOrWhiteSpace(attr.MenuAdi) ? oVal.ToString() : attr.MenuAdi;
                    attr.MenuAdi = key;
                    var menuKey = field.DeclaringType.FullName + "." + field.Name;
                    if (attr.MenuID == 0) attr.MenuID = 1000000000+menuKey.ToCrc16();
                    #region otomatik ilişki koy
                    if (oroleAttr != null) 
                    {
                        var roleAttr = (RoleAttribute)oroleAttr;
                        var attrx = allRoles.Where(p => p.RolAdi == oVal.ToString()).FirstOrDefault();
                        var lst = attr.BagliRoller.ToList();
                        lst.Add(oVal.ToString());
                        attr.BagliRoller = lst.ToArray();
                    }
                    #endregion
                    menus.Add(attr);
                }
            }
            return menus.ToArray();
        }

        public static UserIdentity GetUserIdentity(string UserName)
        {
            if (OnRequireUserIdentity != null)
            {
                UserIdentity uid = new UserIdentity(UserName);
                OnRequireUserIdentity(UserName, ref uid);
                return uid;
            }
            return new UserIdentity(UserName, false);
        }        
    }
}