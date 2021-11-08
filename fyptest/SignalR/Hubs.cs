using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using fyptest.Models;
using Microsoft.AspNet.SignalR;

namespace fyptest.SignalR.Hubs
{
  public class ChatHub : Hub
  {
    public void Hello()
    {
      Clients.All.hello();
    }

    static List<ChatConnectionModel> UsersList = new List<ChatConnectionModel>();
    static List<MessageInfo> MessageList = new List<MessageInfo>();

    //-->>>>> ***** Receive Request From Client [  Connect  ] *****
    public void Connect(string userName, string role)
    {
      var id = Context.ConnectionId;
      var email = "";
      var db = new ServerDBEntities();

      var userInfo = new List<ChatConnection>();
      if (role == "Seeker" && db.ChatConnections.Where(m => m.seeker_email == userName).ToList().Count > 0)
      {
        userInfo = db.ChatConnections.Where(m => m.seeker_email == userName).ToList();
        email = userInfo.FirstOrDefault().seeker_email;
        Groups.Add(Context.ConnectionId, "Seeker");
        Clients.Caller.onConnected(id, userName, email, "Seeker");
      }
      else if (role == "Provider" && db.ChatConnections.Where(m => m.provider_email == userName).ToList().Count > 0)
      {
        Groups.Add(Context.ConnectionId, "Provider");
        Clients.Caller.onConnected(id, userName, email, "Provider");
        userInfo = db.ChatConnections.Where(m => m.provider_email == userName).ToList();
        email = userInfo.FirstOrDefault().provider_email;
      }

      foreach (var item in userInfo)
      {
        var groupName = item.group_name;
        Groups.Add(Context.ConnectionId, groupName);
        Clients.Caller.onConnected(id, userName, email, groupName);
      }
    }

    public void SendMessageToGroup(string groupName, string userName, string message)
    {
      var connection = new ChatConnection();
      using (ServerDBEntities db = new ServerDBEntities())
      {
        connection = db.ChatConnections.Where(m => m.group_name == groupName).FirstOrDefault();
      }
      var receiver = "";
      var provider = "";
      var seeker = "";
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var msg = new Chat();
        msg.created_at = DateTime.Now;
        msg.message = message;
        msg.sender_id = userName;
        msg.group_name = connection.group_name;
        if (connection.provider_email == userName)
        {
          receiver = connection.seeker_email;
          seeker = receiver;
          provider = userName;
        }
        else if (connection.seeker_email == userName)
        {
          receiver = connection.provider_email;
          provider = receiver;
          seeker = userName;
        }
        msg.receiver_id = receiver;
        db.Chats.Add(msg);
        db.SaveChanges();
      }

      var jobId = "";
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var job = db.Requests.Where(m => m.Seeker == seeker && m.Provider == provider).FirstOrDefault();
        jobId = job.SId;
      }
      var link = "<li><a href='/Chat/Contact/" + jobId + "'>You have a new message from " + userName.Split('@')[0] + "</a></li>";
      Clients.Group(groupName).getMessages(userName, message, link, groupName.Replace('#', '_'));
    }
    public void SendMediaToGroup(string groupName, string userName, string message)
    {
      var connection = new ChatConnection();
      using (ServerDBEntities db = new ServerDBEntities())
      {
        connection = db.ChatConnections.Where(m => m.group_name == groupName).FirstOrDefault();
      }
      var receiver = "";
      var provider = "";
      var seeker = "";
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var msg = new Chat();
        msg.created_at = DateTime.Now;
        msg.message = message;
        msg.sender_id = userName;
        msg.group_name = connection.group_name;
        if (connection.provider_email == userName)
        {
          receiver = connection.seeker_email;
          seeker = receiver;
          provider = userName;
        }
        else if (connection.seeker_email == userName)
        {
          receiver = connection.provider_email;
          provider = receiver;
          seeker = userName;
        }
        msg.receiver_id = receiver;
        db.Chats.Add(msg);
        db.SaveChanges();
      }
      var jobId = "";
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var job = db.Requests.Where(m => m.Seeker == seeker && m.Provider == provider).FirstOrDefault();
        jobId = job.SId;
      }
      var link = "<li><a href='/Chat/Contact/" + jobId + "'>You have a new message from " + userName.Split('@')[0] + "</a></li>";
      Clients.Group(groupName).getMessages(userName, message, link, groupName.Replace('#', '_'));
    }


  }

}
