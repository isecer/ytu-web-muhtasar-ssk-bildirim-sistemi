using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.Collections;
using System.Globalization;
using System.Security.AccessControl;

namespace BiskaUtil
{
    public class LocalMachine
    {
        public static bool IsUserMemberOfGroup(string userName, string groupName)
        {
            bool ret = false;

            try
            {
                DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName);
                DirectoryEntry userGroup = localMachine.Children.Find(groupName, "group");
                object members = userGroup.Invoke("members", null);
                foreach (object groupMember in (IEnumerable)members)
                {
                    DirectoryEntry member = new DirectoryEntry(groupMember);
                    if (member.Name.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }
        public static bool CreateUser(string userName,string Password,out Exception error)
        {
            error = null;
            try
            {                
                DirectoryEntry AD = new DirectoryEntry("WinNT://" +Environment.MachineName + ",computer");
                DirectoryEntry newUser = AD.Children.Add(userName, "user");
                newUser.Invoke("SetPassword", new object[] { Password });
                newUser.Invoke("Put", new object[] { "Description", "Bu kullanıcı Evrak Takip Sistemi'nin çalışması için gereklidir" });
                 //int val = (int)newUser.Properties["userAccountControl"].Value;                
                newUser.CommitChanges();                                
                //DirectoryEntry grp;
                //grp = AD.Children.Find("Guests", "group");
                //if (grp != null) { grp.Invoke("Add", new object[] { newUser.Path.ToString() }); }                

                AD.Close();
                newUser.Close();
                return true;
            }
            catch(Exception err)
            {
                error = err;
                return false;
            }
        }
        public static bool AddUserToLocalGroup(string userName, string groupName,out Exception error)
        {
            error = null;
            DirectoryEntry userGroup = null;
            try
            {
                string groupPath = String.Format(CultureInfo.CurrentUICulture, "WinNT://{0}/{1},group", Environment.MachineName, groupName);
                userGroup = new DirectoryEntry(groupPath);

                if ((null == userGroup) || (true == String.IsNullOrEmpty(userGroup.SchemaClassName)) || (0 != String.Compare(userGroup.SchemaClassName, "group", true, CultureInfo.CurrentUICulture)))
                    return false;

                String userPath = String.Format(CultureInfo.CurrentUICulture, "WinNT://{0},user", userName);
                userGroup.Invoke("Add", new object[] { userPath });
                userGroup.CommitChanges();

                return true;
            }
            catch (Exception err)
            {
                error = err;
                return false;
            }
            finally
            {
                if (null != userGroup) userGroup.Dispose();
            }
        }
        public static bool UserIsExists(string userName)
        {
            try
            {
                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntry user = AD.Children.Find(userName, "user");
                var result = user != null;
                AD.Close();
                if (user != null) user.Close();
                return result;
            }
            catch {
                return false;
            }
        }
        public static bool UserIsExists2(string userName)
        {
            
            using (System.DirectoryServices.AccountManagement.PrincipalContext pc = new System.DirectoryServices.AccountManagement.PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Machine))
            {
                System.DirectoryServices.AccountManagement.UserPrincipal up = System.DirectoryServices.AccountManagement.UserPrincipal.FindByIdentity(
                    pc,
                    System.DirectoryServices.AccountManagement.IdentityType.SamAccountName,
                    userName);
                bool UserExists = (up != null);
                return UserExists;
            }
        }
         
        public static bool AddUsersAndPermissions(string DirectoryName, string UserAccount, FileSystemRights UserRights, AccessControlType AccessType,out Exception error)
        {
            error = null;
            try
            {
                // Create a DirectoryInfo object.
                System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(DirectoryName);
                // Get security settings.
                DirectorySecurity dirSecurity = directoryInfo.GetAccessControl();
                // Add the FileSystemAccessRule to the security settings. 
                dirSecurity.AddAccessRule(new FileSystemAccessRule(UserAccount, UserRights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessType));
                // Set the access settings.
                directoryInfo.SetAccessControl(dirSecurity);
                return true;
            }
            catch(Exception err){
                error = err;
                return false;
            }
        }
    }     
}