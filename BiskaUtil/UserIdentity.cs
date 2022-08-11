using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;

namespace BiskaUtil
{

    [Serializable()]
    public partial class UserIdentity : IIdentity
    {
        public enum GenderType { None = 0, Male = 1, Female = 2 }
        /*--------------Interface Props-----------------------------*/
        public string AuthenticationType
        {
            get { return "Forms"; }
        }
        private bool isAuthenticated = true;
        public bool IsAuthenticated
        {
            get { return isAuthenticated; }
        }
        private string userName = "";
        public string Name
        {
            get
            {
                return userName;
            }
        }
        /*-------------End Of Interface Props-----------------------*/

        /**/
        List<string> roles = new List<string>();
        Dictionary<string, object> informations = new Dictionary<string, object>();
        public int Id { get; set; }
        public int YetkiGrupID { get; set; }
        public string AdSoyad { get; set; }
        public string EMail { get; set; }
        public string Description { get; set; }
        public bool IsActiveDirectoryUser { get; set; }
        public bool? IsActiveDirectoryImpersonateWorking { get; set; }
        public string Theme { get; set; }
        public string ImagePath { get; set; }
        public string CurrentModule { get; set; }
        public GenderType Gender { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }
        public int BirimID { get; set; } 
        public List<int> BirimYetkileri { get; set; }
        public List<int> BirimYetkileriRapor { get; set; }
        public List<int> YevmiyeHesapKodTurYetkileri { get; set; }
        public Dictionary<string, int?> SeciliBirimID { get; set; }
        public Dictionary<string, int?> SeciliVASurecID { get; set; }
        public Dictionary<string, int?> SeciliYil { get; set; }
        public Dictionary<string, int?> SeciliAyID { get; set; }



        public Dictionary<string, object> Informations { get { return informations; } set { informations = value; } }

        public List<string> Roles
        {
            get { return roles; }
            set { roles = value; }
        }
        public List<string> YGRoles
        {
            get { return roles; }
            set { roles = value; }
        }
        public UserIdentity(string name)
        {
            this.userName = name;
        }
        public UserIdentity(string name, bool isAuthenticated)
        {
            this.userName = name;
            this.isAuthenticated = isAuthenticated;
        }
        public UserIdentity(string name, string[] roles)
        {
            this.userName = name;
            if (roles != null)
                this.Roles.AddRange(roles);
        }
        public UserPrincipal ToPrincipal()
        {
            UserPrincipal prensip = new UserPrincipal(this);
            return prensip;
        }

        public bool IsAdmin { get; set; }
        public bool HasToChahgePassword { get; set; }
        public bool IsSuperAdmin { get; set; }
        public static string Ip
        {
            get
            {
                try
                {
                    string ip = "";
                    var forwarderFor = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrWhiteSpace(forwarderFor) || forwarderFor.ToLower().Contains("unknown"))
                        ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    else if (forwarderFor.Contains(","))
                    {
                        ip = forwarderFor.Substring(0, forwarderFor.IndexOf(","));
                    }
                    else if (forwarderFor.Contains(";"))
                    {
                        ip = forwarderFor.Substring(0, forwarderFor.IndexOf(";"));
                    }
                    else ip = forwarderFor;
                    var len = ip.Length > 30 ? 30 : ip.Length;
                    return ip.Substring(0, len).Trim();
                }
                catch
                {
                    return "";
                }
            }
        }

        public void Impersonate()
        {
            #region Impersonate
            if (this.IsActiveDirectoryUser)
            {
                if (this.IsActiveDirectoryImpersonateWorking == true)
                {
                    try
                    {
                        WinImpersonate personate = new WinImpersonate();
                        Exception error = null;
                        var identity = personate.GetWindowsIdentity(this.Name, this.Domain, this.Password, out error);
                        if (identity != null)
                        {
                            GenericPrincipal principal = new GenericPrincipal(identity, this.Roles.ToArray());
                            HttpContext.Current.User = principal;
                            this.IsActiveDirectoryImpersonateWorking = true;
                        }
                    }
                    catch { }
                    if (this.IsActiveDirectoryImpersonateWorking == false)
                    {
                        BiskaUtil.SystemInformation.Add(new Exception("Active Directory Kullanıcısı Kullarak İşlem Yapma İşlemi Başarasız Oldu.Sunucunun Active Directory Kullanıcıları İle Oturum Açılabildiğinden Emin Olunuz.Kullanıcı Adı:" + this.Name));
                    }
                }
                else
                {
                    HttpContext.Current.User = this.ToPrincipal();
                }
            }
            else
            {
                HttpContext.Current.User = this.ToPrincipal();
            }
            if (this.IsActiveDirectoryImpersonateWorking == false)
                HttpContext.Current.User = this.ToPrincipal();
            #endregion
        }


        public static void SetUserIdentityOnSession(HttpSessionStateBase session)
        {
            if (session == null)
            {
                SetCurrent();
            }
            else
            {
                if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    UserIdentity kimlik = null;
                    if ((session != null) && (session["UserIdentity"] != null))
                    {
                        kimlik = (UserIdentity)session["UserIdentity"];
                        kimlik.Impersonate();
                    }
                    else if (session != null && HttpContext.Current.User != null)
                    {
                        if (!(HttpContext.Current.User.Identity is NotAuthenticatedUser))
                        {
                            //kimlik = AccountModel.GetKimlik(HttpContext.Current.User.Identity.Name);
                            kimlik = Membership.GetUserIdentity(HttpContext.Current.User.Identity.Name);
                            if (kimlik.Id > 0)
                            {
                                kimlik.Impersonate();
                                session["Kimlik"] = kimlik;
                            }
                            else
                            {
                                session["Kimlik"] = null;
                                FormsAuthenticationUtil.SignOut();
                                HttpContext.Current.User = new GenericPrincipal(new NotAuthenticatedUser(), new string[0]);
                                //IPrincipal user = HttpContext.Current.User;
                            }
                        }
                    }
                }
            }
        }

        public static void SetCurrent(string UserName = null)
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                UserIdentity kimlik = null;
                HttpSessionState session = HttpContext.Current.Session;
                if (UserName != null)
                {
                    kimlik = Membership.GetUserIdentity(UserName);
                    if (kimlik.Id > 0)
                    {
                        kimlik.Impersonate();
                        session["UserIdentity"] = kimlik;
                    }
                    else
                    {
                        session["UserIdentity"] = null; 
                        //IPrincipal user = HttpContext.Current.User;
                    }
                }
                else if ((HttpContext.Current.Session != null) && (HttpContext.Current.Session["UserIdentity"] != null))
                {
                    kimlik = (UserIdentity)session["UserIdentity"];
                    kimlik.Impersonate(); 
                  
                }
                else if (HttpContext.Current.Session != null && HttpContext.Current.User != null)
                {
                    if (!(HttpContext.Current.User.Identity is NotAuthenticatedUser))
                    {
                        //kimlik = AccountModel.GetKimlik(HttpContext.Current.User.Identity.Name);
                        kimlik = Membership.GetUserIdentity(HttpContext.Current.User.Identity.Name);
                        if (kimlik.Id > 0)
                        {
                            kimlik.Impersonate();
                            session["UserIdentity"] = kimlik;
                        }
                        else
                        {
                            session["UserIdentity"] = null;
                            FormsAuthenticationUtil.SignOut();
                            HttpContext.Current.User = new GenericPrincipal(new NotAuthenticatedUser(), new string[0]);
                            //IPrincipal user = HttpContext.Current.User;
                        }
                    }
                }
            }
        }

        public static UserIdentity Current
        {
            get
            {
                if (HttpContext.Current.User.Identity is UserIdentity)
                    return (UserIdentity)HttpContext.Current.User.Identity;
                else
                {
                    if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        return Membership.GetUserIdentity(HttpContext.Current.User.Identity.Name);
                    return new UserIdentity("None", false);
                    //return AccountModel.GetKimlik(HttpContext.Current.User.Identity.Name);
                }
            }
        }
         
    }
    [Serializable()]
    public class UserPrincipal : IPrincipal
    {
        UserIdentity kimlik;
        public IIdentity Identity
        {
            get { return kimlik; }
        }
        public bool IsInRole(string role)
        {
            return kimlik.Roles.IndexOf(role) >= 0;
        }
        internal UserPrincipal(UserIdentity kimlik)
        {
            this.kimlik = kimlik;
        }
    }
}