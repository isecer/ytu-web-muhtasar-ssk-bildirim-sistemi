using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Configuration;

namespace BiskaUtil
{
    public sealed class WinImpersonate
    {
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        #region imports
        //[DllImport("advapi32.dll", SetLastError = true)]
        //private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref  IntPtr phToken);

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern bool CloseHandle(IntPtr handle);

        //[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public extern static bool DuplicateToken(IntPtr existingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr duplicateTokenHandle);

        #endregion
        #region logon consts
        //// logon types       
        //const int LOGON32_LOGON_INTERACTIVE = 2;
        //const int LOGON32_LOGON_NETWORK = 3;
        //const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        //// logon providers        
        //const int LOGON32_PROVIDER_DEFAULT = 0;
        //const int LOGON32_PROVIDER_WINNT50 = 3;
        //const int LOGON32_PROVIDER_WINNT40 = 2;
        //const int LOGON32_PROVIDER_WINNT35 = 1;

        #endregion
        /*
        public bool Impersonate(string username, string password, string domain, Action action,out Exception error)
        {
            IntPtr token = IntPtr.Zero;
            error = null;
            try
            {
                bool isSuccess = LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref token);
                if (isSuccess)
                {
                    using (WindowsImpersonationContext person = new WindowsIdentity(token).Impersonate())
                    {
                        try
                        {
                            if (action != null)
                                action();
                        }
                        catch(Exception err1){
                            error = err1;
                        }
                        person.Undo();
                    }
                }
                return isSuccess;
            }
            catch(Exception err)
            {
                error = err;
            }
            return false;
    
        public bool impersonateValidUser(String userName, String domain, String password,out Exception impersonateError)
        {
            impersonateError = null;
            try
            {
                WindowsIdentity tempWindowsIdentity;
                IntPtr token = IntPtr.Zero;
                IntPtr tokenDuplicate = IntPtr.Zero;

                if (RevertToSelf())
                {
                    if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                        {                            
                            tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                            impersonationContext = tempWindowsIdentity.Impersonate();                            
                            if (impersonationContext != null)
                            {
                                CloseHandle(token);
                                CloseHandle(tokenDuplicate);                                
                                return true;
                            }
                        }
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
                return false;
            }
            catch(Exception error){
                impersonateError = error;
                return false;
            }
        }
    }*/

        public WindowsIdentity GetWindowsIdentity(String userName, String password)
        {
            return GetWindowsIdentity(userName, "", password);
        }
        public WindowsIdentity GetWindowsIdentity(String userName, String domain, String password)
        {
            Exception error=null;
            return GetWindowsIdentity(userName, domain, password, out error);
        }
        public WindowsIdentity GetWindowsIdentity(String userName, String domain, String password,out Exception errorx)
        {
            errorx = null;
            WindowsIdentity tempWindowsIdentity;
            try
            {
                IntPtr token = IntPtr.Zero;
                IntPtr tokenDuplicate = IntPtr.Zero;

                if (RevertToSelf())
                {
                    if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                        {
                            tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                            return tempWindowsIdentity;
                        }
                    }
                }
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (tokenDuplicate != IntPtr.Zero)
                    CloseHandle(tokenDuplicate);
                return null;
            }
            catch (Exception error)
            {
                errorx = error;
                return null;
            }
        }

        public Exception RunAs(WindowsIdentity identity,Action action)
        {
            Exception err = null;
            if (identity != null)
            {
                #region impersonate
                var tempUserName = UserIdentity.Current.Name;
                var tempRoles = UserIdentity.Current.Roles.ToArray();
                GenericPrincipal principal = new GenericPrincipal(identity, null);
                HttpContext.Current.User = principal;
                impersonationContext = identity.Impersonate();
                #endregion
                try
                {
                    action();
                }
                catch (Exception error) { err = error; }

                #region undo impersonate
                try
                {
                    if (impersonationContext != null)
                    {
                        try
                        {
                            impersonationContext.Undo();
                        }
                        catch { }
                    }

                    principal = new GenericPrincipal(new GenericIdentity(tempUserName), tempRoles);
                    HttpContext.Current.User = principal;
                }
                catch { }
                #endregion
            }
            return err;
        }
        public Exception RunAs(string domain, string userName, string password, Action action)
        {
            Exception err = null;             
            WinImpersonate personate = new WinImpersonate();
            Exception impersonateError = null;

           var identity = personate.GetWindowsIdentity(userName, domain, password,out impersonateError);
           if (identity != null)
           {
               #region impersonate
               var tempUserName = UserIdentity.Current.Name;
               var tempRoles = UserIdentity.Current.Roles.ToArray();
               GenericPrincipal principal = new GenericPrincipal(identity, null);
               HttpContext.Current.User = principal;
               //info += "Indetity Name:" + HttpContext.Current.User.Identity.Name;
               var impersonationContext = identity.Impersonate();
               #endregion
               try
               {
                   action();
               }
               catch (Exception error)
               {
                   err = error;
               }
               #region undo impersonate
               try
               {
                   if (impersonationContext != null)
                   {
                       try
                       {
                           impersonationContext.Undo();
                       }
                       catch { }
                   }
                   principal = new GenericPrincipal(new GenericIdentity(tempUserName), tempRoles);
                   HttpContext.Current.User = principal;
               }
               catch { }
               #endregion
           }
           else
           {
               try
               {
                   action();
               }
               catch(Exception err2){
                   err = new Exception(err2.Message, impersonateError);
               }
               //err = impersonateError;
           }
           return err;
        }
        
        public void undoImpersonation()
        {
            impersonationContext.Undo();
        }

        
    }
}