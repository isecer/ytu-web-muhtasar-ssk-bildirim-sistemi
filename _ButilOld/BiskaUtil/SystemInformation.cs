using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiskaUtil
{
    public class SystemInformation
    {
        public enum SystemInformationType { Information=0,Warning=1,Error=2}

        public SystemInformationType InfoType { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public string StackTrace { get; set; }

        public delegate void OnSystemEventHandler(SystemInformation info);
        public static event OnSystemEventHandler OnEvent;


        public static void Add(SystemInformationType infoType, string message, string category,string stackTrace)
        {
            //Management.AddMessage(new SystemInformation { Category = category, InfoType = infoType, Message = message, StackTrace = stackTrace });
            if (OnEvent != null) {
                OnEvent(new SystemInformation { Category = category, InfoType = infoType, Message = message, StackTrace = stackTrace });
            }
        }

        public static void Add(SystemInformationType infoType,string Message)
        {
            Add(infoType, Message, "","");
        }       
        public static void AddInfo(string Information)
        {
            Add(SystemInformationType.Information, Information);
        }
        public static void AddInfo(string Information, string Category)
        {
            Add(SystemInformationType.Information, Information,Category,"");
        }
        public static void AddWarning(string WarningMessage) 
        {
            Add(SystemInformationType.Warning, WarningMessage);
        }
        public static void AddWarning(string WarningMessage,string Category) 
        {
            Add(SystemInformationType.Warning, WarningMessage,Category,"");
        }
        public static void Add(Exception[] Errors)
        {
            if (Errors != null && Errors.Length > 0)
            {                
                foreach (var err in Errors)
                {
                    Add(SystemInformationType.Error, err.Message, "", err.StackTrace);
                }
            }
        }
        public static void Add(Exception Error)
        {
            Exception err = Error;
            string Msg = err.Message;
            string StackTrace = err.StackTrace;
            while((err=err.InnerException)!=null)
            {
               Msg +="\r\n"+err.Message;
               StackTrace += "\r\n" + err.Message + ":" + err.StackTrace;
            }
            Add(SystemInformationType.Error, Msg, "", StackTrace);
        }
        public static void AddError(string ErrorMessage)
        {
            Add(SystemInformationType.Error, ErrorMessage);
        }
        public static void AddError(string ErrorMessage,string Category)
        {
            Add(SystemInformationType.Error, ErrorMessage, "", "");
        }
        public static void AddError(string ErrorMessage,string Category,string StackTrace)
        {
            Add(SystemInformationType.Error, ErrorMessage, "", StackTrace);
        }
        public static void AddError(string ErrorMessage,Exception innerException)
        {
            Add(SystemInformationType.Error, ErrorMessage, "", innerException.StackTrace);
        }
    }
}