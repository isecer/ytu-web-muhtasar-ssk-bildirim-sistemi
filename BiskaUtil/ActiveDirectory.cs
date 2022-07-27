using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Management;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using System.Reflection;

namespace BiskaUtil
{
    public class ActiveDirectory
    {
        public class ADSUser
        {
            public string Adspath { get; set; }

            public string Company { get; set; }

            public string Description { get; set; }

            public string DisplayName { get; set; }

            public string DistinguishedName { get; set; }

            public string GivenName { get; set; }

            public string Mail { get; set; }

            public string Manager { get; set; }

            public string Name { get; set; }

            public string ProxyAddresses { get; set; }

            public string SAMAccountName { get; set; }

            public string Title { get; set; }

        }
        public class ADSTreeObject
        {
            private ADSTreeObject parent = null;
            private List<NameValue> properties = new List<NameValue>();
            private List<ADSTreeObject> subs = new List<ADSTreeObject>();

            public string Name { get; set; }

            public ADSTreeObject Parent
            {
                get
                {
                    return this.parent;
                }
                set
                {
                    this.parent = value;
                }
            }

            public string Path { get; set; }

            public List<NameValue> Properties
            {
                get
                {
                    return this.properties;
                }
                set
                {
                    this.properties = value ?? new List<NameValue>();
                }
            }

            public List<ADSTreeObject> SubObjects
            {
                get
                {
                    return this.subs;
                }
                set
                {
                    if (value == null)
                    {
                        this.subs = new List<ADSTreeObject>();
                    }
                    else
                    {
                        this.subs = value;
                    }
                }
            }

            public object Tag { get; set; }

            public ADSTreeObjectType TreeType { get; set; }

            public enum ADSTreeObjectType
            {
                Computer,
                User,
                Group,
                OU,
                Another
            }            
        }

        public ActiveDirectory() 
        {
            //this.LdapNETBIOSName = Ayar.ActiveDirectoryDomainAdi.GetAyar();
            //this.LdapServerPath = Ayar.ActiveDirectoryPath.GetAyar();
            //this.LdapUserName = Ayar.ActiveDirectoryKullaniciAdi.GetAyar();
            //this.LdapPassword = Ayar.ActiveDirectoryKullaniciSifresi.GetAyar();
        }
        
        public Exception Error { get; private set; }

        public bool HasError { get; private set; }

        public string LdapNETBIOSName { get; set; }

        public string LdapPassword { get; set; }

        public string LdapServerPath { get; set; }

        public string LdapUserName { get; set; }
        public  DirectoryEntry GetDirectoryEntry()
        {
            //Uid = txtUid.Text;
            //Pwd = txtPwd.Text;            
            string server =LdapServerPath;
            DirectoryEntry de = new DirectoryEntry("LDAP://" + server + ""); 
            if (!String.IsNullOrEmpty(LdapUserName))
            {
                de.Username =LdapUserName;
                de.Password =LdapPassword;
            }
            //DirectoryEntry de = new DirectoryEntry();
            return de;
        }
        public DirectoryEntry GetDirectoryEntry(string path)
        {                        
            DirectoryEntry de = new DirectoryEntry(path.IsNullOrWhiteSpace()? "LDAP://" +LdapServerPath + "":path);  
            if (!String.IsNullOrEmpty(LdapUserName))
            {
                de.Username =LdapUserName;
                de.Password =LdapPassword;
            }            
            return de;
        }
        public DirectoryEntry GetDirectoryEntry(object ADSObject)
        {
            DirectoryEntry de = new DirectoryEntry(ADSObject);                       
            if (!String.IsNullOrEmpty(LdapUserName))
            {
                de.Username =LdapUserName;
                de.Password =LdapPassword;
            }
            return de;
        }    

      
        public  string[] GetComputerNames()
        {
            HasError = false;
            Error = null;

            List<string> ComputerNames = new List<string>();
            var de = GetDirectoryEntry();            
            try
            {
                DirectorySearcher ser = new DirectorySearcher(de);
                ser.SearchScope = SearchScope.Subtree;
                ser.PropertiesToLoad.Add("name");
                ser.SizeLimit = int.MaxValue;
                ser.PageSize = int.MaxValue;

                ser.Filter = "(&ObjectCategory=computer)"; //Only allows Computers to be returned in results.
                SearchResultCollection results = ser.FindAll();
                foreach (SearchResult res in results)
                {

                    string computerName = res.Properties["name"][0].ToString();
                    ComputerNames.Add(computerName);
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                Error = ex;
            }
            finally
            {
                de.Close();
                de.Dispose();//Clean up resources
            }
            return ComputerNames.OrderBy(o=>o).ToArray();
        }

        public  string GetFullName(string userAccountName)
        {
           
                HasError = false;
                Error = null;
                DirectoryEntry entry = GetDirectoryEntry();
                try
                {                    
                    var account = userAccountName.Split('\\').LastOrDefault();
                    DirectorySearcher search = new DirectorySearcher(entry);
                    search.Filter = "(SAMAccountName=" + account + ")";
                    search.PropertiesToLoad.Add("displayName");
                    SearchResult result = search.FindOne();
                    if (result != null)
                    {
                        return result.Properties["displayname"][0].ToString();
                    }
                    else
                    {
                        return "Unknown User";
                    }
                }
                catch (Exception ex)
                {
                    Error = ex;
                    HasError = true;
                }
                entry.Close();
                entry.Dispose();
                return "";           
        }
        public string GetUserOU(string userAccountName) 
        {
            HasError = false;
            Error = null;
            //var grps = GetUserMemberOfGroups(userAccountName);
            DirectoryEntry entry = GetDirectoryEntry();
            try
            {
               
                var account = userAccountName.Split('\\').LastOrDefault();
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + account + ")";
                //search.PropertiesToLoad.Add("displayName");
                SearchResult result = search.FindOne();
                if (result != null)
                {
                    var strOu=result.Properties["distinguishedname"][0].ToString();
                    try
                    {
                        var lst = strOu.Split(',').Where(p =>p.Split('=').Length>1 && p.Split('=')[0] == "OU").Select(s => s.Split('=')[1]).ToArray();
                        string ou = string.Join(" \\ ", lst);
                        return ou;
                    }
                    catch 
                    {
                        return strOu;
                    }
                }
                else
                {
                    return "Unknown User";
                }
            }
            catch (Exception ex)
            {
                Error = ex;
                HasError = true;
            }
            entry.Close();
            entry.Dispose();
            return "";     
        }
      

        public string[] GetUserGroupMembership(string userAccountName)
        {
            //StringCollection groups = new StringCollection();
            List<string> groups = new List<string>();
            DirectoryEntry obEntry = GetDirectoryEntry();
            var account = userAccountName.Split('\\').LastOrDefault();
            try
            {                
                DirectorySearcher srch = new DirectorySearcher(obEntry,
                    "(sAMAccountName=" + account + ")");
                SearchResult res = srch.FindOne();
                if (null != res)
                {
                    DirectoryEntry obUser = GetDirectoryEntry(res.Path);
                    // Invoke Groups method.
                    object obGroups = obUser.Invoke("Groups");
                    foreach (object ob in (IEnumerable)obGroups)
                    {
                        // Create object for each group.
                        DirectoryEntry obGpEntry = new DirectoryEntry(ob);
                        groups.Add(obGpEntry.Name.Replace("CN=",""));
                    }
                }
            }
            catch
            {
               
            }
            obEntry.Close();
            obEntry.Dispose();
            return groups.ToArray();
        }

        public List<string> GetUserGroups(string userAccountName) 
        { 
            List<string> Groups = new List<string>();
            //System.DirectoryServices.DirectoryEntry dirEntry = new System.DirectoryServices.DirectoryEntry(_path, username, password); 
            DirectoryEntry dirEntry = GetDirectoryEntry();
            DirectorySearcher dirSearcher = new DirectorySearcher(dirEntry);
            var account = userAccountName.Split('\\').LastOrDefault();
            dirSearcher.Filter = "(SAMAccountName=" + account + ")";
            dirSearcher.PropertiesToLoad.Add("memberOf"); 
            int propCount; 
            try { 
                SearchResult dirSearchResults = dirSearcher.FindOne(); 
                propCount = dirSearchResults.Properties["memberOf"].Count; 
                string dn; 
                int equalsIndex; 
                int commaIndex; 
                for (int i = 0; i <= propCount - 1; i++) 
                { 
                    dn = dirSearchResults.Properties["memberOf"][i].ToString();
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1); 
                    if (equalsIndex == -1) 
                    { 
                        return null; 
                    } 
                    if (!Groups.Contains(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1))) 
                    { 
                        Groups.Add(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1)); 
                    } 
                } 
            } 
            catch 
            { 
                 
            }
            dirEntry.Close();
            dirEntry.Dispose();
            return Groups; 
        }
        
        public List<string> GetGroupNames(string Path) 
        {        
            List<string> retwal= new List<string>();             
            string path="LDAP://tedas.gov.tr/OU=035-IZMIR,OU=Kullanıcılar,OU=TEDAS - EDM,DC=tedas,DC=gov,DC=tr";
            if (!string.IsNullOrEmpty(Path))
                path = Path;
            DirectoryEntry dirEntry = GetDirectoryEntry(path);

            SearchResultCollection results;
            DirectorySearcher srch = new DirectorySearcher(dirEntry);
            srch.Filter = "(objectClass=Group)";
            //srch.PropertiesToLoad.Add("displayName");
            results = srch.FindAll();
            #region lazım olabilir ,önemli
            //var paths = results.Cast<SearchResult>().Select(s => s.Path).ToList();
            //var displayNames = results.Cast<SearchResult>().Select(s => s.Properties["displayName"][0].ToString());
            //foreach (SearchResult item in results)
            //{
            //    foreach (string propertyKey in item.Properties.PropertyNames)
            //    {
            //        // Retrieve the value assigned to that property name 
            //        // in the ResultPropertyValueCollection.
            //        ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];

            //        // Iterate through values for each property name in each 
            //        // SearchResult.
            //        foreach (Object propertyValue in valueCollection)
            //        {
            //            if (propertyKey == "name")
            //            {
            //                string grpName = propertyValue.ToString();
            //                retwal.Add(grpName);
            //            }
            //        }
            //    }
            //}
            #endregion
            foreach (SearchResult item in results)
            {
                ResultPropertyValueCollection valueCollection = item.Properties["name"];
                // Iterate through values for each property name in each 
                // SearchResult.
                foreach (Object propertyValue in valueCollection)
                {                    
                        string grpName = propertyValue.ToString();
                        retwal.Add(grpName);                     
                }
            }
            dirEntry.Close();
            dirEntry.Dispose();
            return retwal;
          
        }
        public List<ADSTreeObject> GetGroups(string Path)
        {

            List<ADSTreeObject> retwal = new List<ADSTreeObject>();
            //            
            string path = "LDAP://tedas.gov.tr/OU=035-IZMIR,OU=Kullanıcılar,OU=TEDAS - EDM,DC=tedas,DC=gov,DC=tr";
            if (!string.IsNullOrEmpty(Path))
                path = Path;
            DirectoryEntry dirEntry = GetDirectoryEntry(path);

            SearchResultCollection results;
            DirectorySearcher srch = new DirectorySearcher(dirEntry);
            srch.Filter = "(objectClass=Group)";
            //srch.PropertiesToLoad.Add("displayName");
            results = srch.FindAll();

            //string[] keys = new string[] {"name","adspath" };
            foreach (SearchResult item in results)
            {

                ADSTreeObject objTree = new ADSTreeObject();
                objTree.TreeType = ADSTreeObject.ADSTreeObjectType.Group;
                //var keys= item.Properties.PropertyNames.Cast<string>().ToArray()
                foreach (string propertyKey in item.Properties.PropertyNames)
                {
                    // Retrieve the value assigned to that property name 
                    // in the ResultPropertyValueCollection.
                    ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];

                    // Iterate through values for each property name in each 
                    // SearchResult.
                    foreach (Object propertyValue in valueCollection)
                    {
                        if (propertyKey == "name")
                        {
                            string grpName = propertyValue.ToString();
                            objTree.Name = grpName;
                        }
                        else if (propertyKey == "adspath")
                        {
                            string adspath = propertyValue.ToString();
                            objTree.Path = adspath;
                        }
                        else
                        {
                            objTree.Properties.Add(new NameValue(propertyKey, propertyValue.ToString()));
                        }
                    }
                }
                retwal.Add(objTree);
            }
            dirEntry.Close();
            dirEntry.Dispose();
            
            //return retwal;
            return retwal;

        }
        public List<ADSTreeObject> GetUsers(string Path) 
        {
            List<ADSTreeObject> retwal = new List<ADSTreeObject>();
            //            
            //string path = "LDAP://tedas.gov.tr/OU=035-IZMIR,OU=Kullanıcılar,OU=TEDAS - EDM,DC=tedas,DC=gov,DC=tr";
            var path = LdapServerPath;
            if (!string.IsNullOrEmpty(Path))
                path = Path;
            DirectoryEntry dirEntry = GetDirectoryEntry(path);

            SearchResultCollection results;
            DirectorySearcher srch = new DirectorySearcher(dirEntry);
            srch.Filter = "(objectClass=user)";
            //srch.PropertiesToLoad.Add("displayName");
            results = srch.FindAll();

            //string[] keys = new string[] { "name", "adspath" };
            foreach (SearchResult item in results)
            {

                ADSTreeObject objTree = new ADSTreeObject();
                objTree.TreeType = ADSTreeObject.ADSTreeObjectType.User;
                foreach (string propertyKey in item.Properties.PropertyNames)
                {
                    // Retrieve the value assigned to that property name 
                    // in the ResultPropertyValueCollection.
                    ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];

                    // Iterate through values for each property name in each 
                    // SearchResult.
                    foreach (Object propertyValue in valueCollection)
                    {
                        if (propertyKey == "name")
                        {
                            string grpName = propertyValue.ToString();
                            objTree.Name = grpName;
                        }
                        else if (propertyKey == "adspath")
                        {
                            string adspath = propertyValue.ToString();
                            objTree.Path = adspath;
                        }
                        else
                        
                        objTree.Properties.Add(new NameValue(propertyKey, propertyValue.ToString()));
                        
                    }
                }
                retwal.Add(objTree);
            }
            dirEntry.Close();
            dirEntry.Dispose();
            //return retwal;
            return retwal;
        }
        public bool IsAuthenticated(string username, string pwd, out Exception error)
        {
            error = null;
            string str = "";
            string ldapNETBIOSName = this.LdapNETBIOSName;
            if (!(string.IsNullOrWhiteSpace( ldapNETBIOSName) || username.Contains(@"\")))
            {
                str = ldapNETBIOSName + @"\" + username;
            }
            else
            {
                str = username;
            }
            DirectoryEntry searchRoot = new DirectoryEntry(this.LdapServerPath, str, pwd);
            try
            {
                object nativeObject = searchRoot.NativeObject;
                DirectorySearcher searcher = new DirectorySearcher(searchRoot)
                {
                    Filter = "(SAMAccountName=" + username + ")"
                };
                searcher.PropertiesToLoad.Add("cn");
                SearchResult result = searcher.FindOne();
                if (null == result)
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                error = exception;
                return false;
            }
            return true;
        }
        public List<ADSUser> SearchUsers(string Path, string searchUser, int? TopCount)
        {
            TopCount = TopCount ?? 10;
            string username = "";
            string ldapNETBIOSName = this.LdapNETBIOSName;
            if (!(string.IsNullOrWhiteSpace(ldapNETBIOSName) || this.LdapUserName.Contains(@"\")))
            {
                username = ldapNETBIOSName + @"\" + this.LdapUserName;
            }
            else
            {
                username = this.LdapUserName;
            }
            if (string.IsNullOrWhiteSpace( Path))
            {
                Path = this.LdapServerPath;
            }
            List<ADSUser> list = new List<ADSUser>();
            DirectoryEntry searchRoot = new DirectoryEntry(Path, username, this.LdapPassword);
            try
            {
                object nativeObject = searchRoot.NativeObject;
                DirectorySearcher searcher = new DirectorySearcher(searchRoot) {
                    CacheResults = true,
                    SearchScope = SearchScope.Subtree,
                    Filter = "(&(&(objectCategory=person)(objectClass=user))(|(samaccountname=*" + searchUser + "*)(&(GivenName=*" + searchUser + "*)(SN=*" + searchUser + "*))))"
                };
                searcher.PropertiesToLoad.AddRange(new string[] { "DisplayName", "GivenName", "DistinguishedName", "Title", "manager", "adspath", "name", "mail"
                    , "physicalDeliveryOfficeName", "DirectReports", "Company", "Description", "SAMAccountName" ,"proxyaddresses"});
                 
                SearchResultCollection results = searcher.FindAll();
                PropertyInfo[] properties = typeof(ADSUser).GetProperties();
                int num = -1;
                foreach (SearchResult item in results)
                {
                    num++;                     
                    if (TopCount>0 && num>TopCount.Value)
                    {
                        break;
                    }
                    ADSUser user = new ADSUser();
                    var props = user.GetType().GetProperties();
                    foreach (string propertyKey in item.Properties.PropertyNames)
                    {                        
                        ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];                        
                        foreach (Object propertyValue in valueCollection)
                        {
                            if (propertyValue == null) continue;
                            var propVal = propertyValue.ToString();
                            if (propVal == "") continue;

                            if (propVal.StartsWith("smtp:")) propVal = propVal.Substring(5); 
                            var propx=props.Where(p => p.Name.ToLower() == propertyKey.ToLower()).FirstOrDefault();
                            if (propx != null) {
                                propx.SetValue(user, propVal, null);
                            }                            
                        }
                    }
                    user.Mail=string.IsNullOrWhiteSpace( user.Mail)?user.ProxyAddresses:user.Mail;
                    list.Add(user);
                }
                searchRoot.Close();
                searchRoot.Dispose();
            }
            catch (Exception)
            {
            }
            return list;
        }
        public List<ADSUser> GetMailList(string Path, string searchUser, int? TopCount)
        {
            TopCount = TopCount ?? 10;
            string username = "";
            string ldapNETBIOSName = this.LdapNETBIOSName;
            if (!(string.IsNullOrWhiteSpace( ldapNETBIOSName) || this.LdapUserName.Contains(@"\")))
            {
                username = ldapNETBIOSName + @"\" + this.LdapUserName;
            }
            else
            {
                username = this.LdapUserName;
            }
            if (string.IsNullOrWhiteSpace( Path))
            {
                Path = this.LdapServerPath;
            }
            List<ADSUser> list = new List<ADSUser>();
            DirectoryEntry searchRoot = new DirectoryEntry(Path, username, this.LdapPassword);
            try
            {
                object nativeObject = searchRoot.NativeObject;
                DirectorySearcher searcher = new DirectorySearcher(searchRoot)
                {
                    CacheResults = true,
                    SearchScope = SearchScope.Subtree,
                    Filter = "(&(&(objectCategory=person)(objectClass=user)(objectClass=group))(|(samaccountname=*" + searchUser + "*)(&(GivenName=*" + searchUser + "*)(SN=*" + searchUser + "*))))"
                };
                searcher.PropertiesToLoad.AddRange(new string[] { "DisplayName", "GivenName", "DistinguishedName", "Title", "manager", "adspath", "name", "mail"
                    , "physicalDeliveryOfficeName", "DirectReports", "Company", "Description", "SAMAccountName" ,"proxyaddresses"});

                SearchResultCollection results = searcher.FindAll();
                PropertyInfo[] properties = typeof(ADSUser).GetProperties();
                int num = -1;
                foreach (SearchResult item in results)
                {
                    num++;                    
                    if (TopCount>0 && num>TopCount.Value)
                    {
                        break;
                    }
                    ADSUser user = new ADSUser();
                    var props = user.GetType().GetProperties();
                    foreach (string propertyKey in item.Properties.PropertyNames)
                    {
                        ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];
                        foreach (Object propertyValue in valueCollection)
                        {
                            if (propertyValue == null) continue;
                            var propVal = propertyValue.ToString();
                            if (propVal == "") continue;

                            if (propVal.StartsWith("smtp:")) propVal = propVal.Substring(5);
                            var propx = props.Where(p => p.Name.ToLower() == propertyKey.ToLower()).FirstOrDefault();
                            if (propx != null)
                            {
                                propx.SetValue(user, propVal, null);
                            }
                        }
                    }
                    user.Mail = string.IsNullOrWhiteSpace( user.Mail) ? user.ProxyAddresses : user.Mail;
                    list.Add(user);
                }
                searchRoot.Close();
                searchRoot.Dispose();
            }
            catch (Exception)
            {
            }
            return list;
        }
        public List<ADSTreeObject> GetComputers(string Path)
        {
            List<ADSTreeObject> retwal = new List<ADSTreeObject>();
            //            
            string path = "LDAP://tedas.gov.tr/OU=035-IZMIR,OU=Kullanıcılar,OU=TEDAS - EDM,DC=tedas,DC=gov,DC=tr";
            if (!string.IsNullOrEmpty(Path))
                path = Path;
            DirectoryEntry dirEntry = GetDirectoryEntry(path);

            SearchResultCollection results;
            DirectorySearcher srch = new DirectorySearcher(dirEntry);
            srch.Filter = "(objectClass=computer)";
            //srch.PropertiesToLoad.Add("displayName");
            results = srch.FindAll();

            //string[] keys = new string[] { "name", "adspath" };
            foreach (SearchResult item in results)
            {

                ADSTreeObject objTree = new ADSTreeObject();
                objTree.TreeType = ADSTreeObject.ADSTreeObjectType.Computer;
                foreach (string propertyKey in item.Properties.PropertyNames)
                {
                    // Retrieve the value assigned to that property name 
                    // in the ResultPropertyValueCollection.
                    ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];

                    // Iterate through values for each property name in each 
                    // SearchResult.
                    foreach (Object propertyValue in valueCollection)
                    {
                        if (propertyKey == "name")
                        {
                            string grpName = propertyValue.ToString();
                            objTree.Name = grpName;
                        }
                        else if (propertyKey == "adspath")
                        {
                            string adspath = propertyValue.ToString();
                            objTree.Path = adspath;
                        }
                        else
                        {
                            objTree.Properties.Add(new NameValue(propertyKey, propertyValue.ToString()));
                        }
                    }
                }
                retwal.Add(objTree);
            }
            dirEntry.Close();
            dirEntry.Dispose();
            //return retwal;
            return retwal;
        }
        public List<ADSTreeObject> GetAllOU()
        {
            return GetAllOU("");
        }
        public List<ADSTreeObject> GetAllOU(string Path) 
        {

            List<ADSTreeObject> retwals = new List<ADSTreeObject>();

            DirectoryEntry de = string.IsNullOrEmpty(Path) ? GetDirectoryEntry() : GetDirectoryEntry(Path);
            SearchResultCollection results;
            DirectorySearcher srch = new DirectorySearcher(de);
            srch.Filter = "(objectClass=organizationalUnit)";
            srch.SearchScope = SearchScope.Subtree;            
            results = srch.FindAll();
            var paths= results.Cast<SearchResult>().Select(s => s.Path).ToArray();
            #region tmp func
            Func<string, ADSTreeObject> findParent = null;
            findParent = new Func<string, ADSTreeObject>(
                (AdsPath)=>
                    {
                        var parentStr = AdsPath.Substring(AdsPath.IndexOf(",")+1);
                        foreach (ADSTreeObject to in retwals)
                        {
                            var pth= to.Path.Substring(to.Path.IndexOf("/",8)+1);
                            if (pth == parentStr)
                                return to;
                        }
                        return null;
                    }                
                );
            #endregion
            #region  w
            //string[] keys = new string[] { "name", "adspath" };
            foreach (SearchResult item in results)
            {                
                ADSTreeObject objTree = new ADSTreeObject();
                objTree.TreeType = ADSTreeObject.ADSTreeObjectType.OU;
                foreach (string propertyKey in item.Properties.PropertyNames)
                {
                    // Retrieve the value assigned to that property name 
                    // in the ResultPropertyValueCollection.
                    ResultPropertyValueCollection valueCollection = item.Properties[propertyKey];

                    // Iterate through values for each property name in each 
                    // SearchResult.                    
                    foreach (Object propertyValue in valueCollection)
                    {
                        if (propertyKey == "name")
                        {
                            string grpName = propertyValue.ToString();
                            objTree.Name = grpName;
                        }
                        else if (propertyKey == "adspath")
                        {
                            string adspath = propertyValue.ToString();
                            objTree.Path = adspath;
                        }
                        else 
                        {
                            objTree.Properties.Add(new NameValue(propertyKey, propertyValue.ToString())); ;
                        }
                    }
                }
                //objTree.SubObjects = GetAllOU(objTree.Path).ToArray();                
                // string path = "LDAP://tedas.gov.tr/OU=035-IZMIR,OU=Kullanıcılar,OU=TEDAS - EDM,DC=tedas,DC=gov,DC=tr";
                var parent=retwals.Where(p => p.Path.Substring(p.Path.IndexOf("/", 8)+1) == 
                                  objTree.Path.Substring(objTree.Path.IndexOf(",")+1)
                              ).FirstOrDefault();
                //var pr = findParent(objTree.Path);
                objTree.Parent = parent;
                retwals.Add(objTree);
            }
            de.Dispose();
            #endregion 

            de.Close();
            de.Dispose();
            var roots=retwals.Where(p => p.Parent == null).ToList();
            return roots;
        }

        public List<string> GetGroups(string dc, string oupath) 
        {
            List<string> lst = new List<string>();
            //tedas.gov.tr/DC=tedas,DC=gov,DC=tr
            //PrincipalContext yourOU = new PrincipalContext(ContextType.Domain, "mycompany.local", "OU=Marketing,OU=Operations,OU=Applications,DC=mycompany,DC=local");            
            PrincipalContext yourOU = new PrincipalContext(ContextType.Domain,dc, oupath,LdapUserName,LdapPassword);
            GroupPrincipal findAllGroups = new GroupPrincipal(yourOU, "*");
            PrincipalSearcher ps = new PrincipalSearcher(findAllGroups);
            foreach (var group in ps.FindAll())
            {
               // Console.WriteLine(group.DistinguishedName);
                lst.Add(group.DistinguishedName);
            }
            //Console.ReadLine(); 
            return lst;
        }

        public List<ADSTreeObject> GetMembers(string Path) 
        {
            List<ADSTreeObject> retwal = new List<ADSTreeObject>();
            DirectoryEntry group = GetDirectoryEntry(Path);

            object members = group.Invoke("Members", null);
            foreach (object member in (IEnumerable)members)
            {
                DirectoryEntry x = GetDirectoryEntry(member);
                ADSTreeObject to = new ADSTreeObject();
                to.Name = x.Name.Replace("CN=","");
                to.Path = x.Path;
                retwal.Add(to); 
            }           
            return retwal;
        }
        public List<ADSTreeObject> GetMemberOf(string Path)
        {
            List<ADSTreeObject> retwal = new List<ADSTreeObject>();
            string strUserADsPath = Path;
            DirectoryEntry oUser;
            oUser = GetDirectoryEntry(strUserADsPath);           
            // Invoke IADsUser::Groups method.
            object groups = oUser.Invoke("Groups");
            foreach (object group in (IEnumerable)groups)
            {
                // Get the Directory Entry.
                DirectoryEntry groupEntry = new DirectoryEntry(group);
                ADSTreeObject to = new ADSTreeObject();
                to.Name = groupEntry.Name.Replace("CN=","");
                to.Path = groupEntry.Path;
                retwal.Add(to);
            }
            return retwal;
        }

        public Exception AddToGroup(string userPath, string groupPath)
        {
            Exception exp = null;
            try
            {
                DirectoryEntry dirEntry = GetDirectoryEntry(groupPath); // new DirectoryEntry("LDAP://" + groupDn);
                dirEntry.Properties["member"].Add(userPath);
                dirEntry.CommitChanges();
                dirEntry.Close();
                return exp;
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //doSomething with E.Message.ToString();
                return E;
            }
            catch(Exception ex){
                return ex;
            }
        }
        public Exception RemoveUserFromGroup(string userPath, string groupPath)
        {
            try
            {
                DirectoryEntry dirEntry = GetDirectoryEntry(groupPath);
                dirEntry.Properties["member"].Remove(userPath);
                dirEntry.CommitChanges();
                dirEntry.Close();
                return null;
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //doSomething with E.Message.ToString();
                return E;
            }
            catch (Exception exp) {
                return exp;
            }
        }
        
        public Exception  CreateUserAccount(string OuPath, string userName,string userPassword)
        {
            string oGUID = string.Empty;
            try
            {
                string connectionPrefix = OuPath;
                DirectoryEntry dirEntry = GetDirectoryEntry(connectionPrefix);
                DirectoryEntry newUser = dirEntry.Children.Add("CN=" + userName, "user");
                newUser.Properties["samAccountName"].Value = userName;
                newUser.CommitChanges();
                oGUID = newUser.Guid.ToString();
                newUser.Invoke("SetPassword", new object[] { userPassword });
                newUser.CommitChanges();
                dirEntry.Close();
                newUser.Close();
                return null;
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //DoSomethingwith --> E.Message.ToString();
                return E;

            }
            catch(Exception exp){
                return exp;
            }
            //return oGUID;            
        }
        public Exception ResetPassword(string userDn, string password)
        {
            try
            {
                DirectoryEntry uEntry = GetDirectoryEntry(userDn);
                uEntry.Invoke("SetPassword", new object[] { password });
                uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                uEntry.Close();
                return null;
            }
            catch(Exception exp)
            {
                return exp;
            }
        }

        public static bool IsAuthenticated(String ldapPath, String domain, String username, String pwd, out Exception error)
        {
            error = null;
            String domainAndUsername = "";
            if (string.IsNullOrWhiteSpace(domain) == false) domainAndUsername = domain + @"\" + username;
            else domainAndUsername = username;

            DirectoryEntry entry = new DirectoryEntry(ldapPath, domainAndUsername, pwd);
            try
            {	//Bind to the native AdsObject to force authentication.			
                Object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
            return true;
        }
        
    }

    
}
