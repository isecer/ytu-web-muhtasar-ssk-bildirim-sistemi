using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiskaUtil
{
    public static class MessageBox
    {
        public class  Msg
        {
           public string[] Message { get; set; }
           public string Title { get; set; }
           public MessageType MsgType { get; set; }            
        }
        public enum MessageType { Information,Error,Warning,Custom,Success}
      
        #region Queue
        public static List<Msg> Queues {
            get
            {
                if (!UserIdentity.Current.Informations.ContainsKey("Queues"))
                {
                    List<Msg> msgsx = new List<Msg>();
                    UserIdentity.Current.Informations["Queues"] = msgsx;
                }
                var msgs = (List<Msg>)UserIdentity.Current.Informations["Queues"];
                return msgs;
            }
        }

        public static void Show(string Message)
        {
            Show(Message, "", MessageType.Information);
        }
        public static void Show(string Message, string Title) 
        {
            Show(Message, Title, MessageType.Information);
        }
        public static void Show(string Message,MessageType MsgType)
        {
            Show(Message, "", MsgType);
        }
        public static void Show(string Message, string Title, MessageType MsgType)
        {
            Queues.Add(new Msg { Message =new string[]{Message}, Title = Title, MsgType = MsgType });
        }
        public static void Show(string Title, MessageType MsgType,params string[] Messages)
        {
            Queues.Add(new Msg { Message = Messages, Title = Title, MsgType = MsgType });
        }
        public static Msg GetShow()
        {
            var fi=Queues.FirstOrDefault();
            if (fi != null) Queues.RemoveAt(0); 
            return fi;
        }
        public static Msg[] GetShows()
        {
            var lst = Queues.ToArray();
            Queues.Clear();
            return lst;
        }
        #endregion

    }
}
