using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using fyptest.Models;
using fyptest.SignalR.Hubs;


namespace fyptest.Controllers
{
  public class ChatController : Controller
  {
    // GET: Chat
    private ServerDBEntities db = new ServerDBEntities();

    //class constructorsessio
    public ActionResult Contact(string id)
    {
      ConversationModel model = new ConversationModel();
      //var job = db.Requests.Where(j => j.SId == id).FirstOrDefault();
      var role = Session["Role"].ToString();
      var email = Session["Email"].ToString();
      var connectionModel = new ChatConnectionModel();
      var seeker = new Seeker();
      var provider = new Provider();
      if (role == "Provider")
      {
        seeker = db.Seekers.Where(m => m.email == id).FirstOrDefault();
        provider = db.Providers.Where(m => m.email == email).FirstOrDefault();
      }
      else if(role=="Seeker")
      {
        seeker = db.Seekers.Where(m => m.email == email).FirstOrDefault();
        provider = db.Providers.Where(m => m.email == id).FirstOrDefault();
      }
      var provider_img = provider.profileImage;
      var seeker_img =seeker.profileImage;
      connectionModel.SeekerPhoto = "/UploadedDocument/user.jpg";
      connectionModel.ProviderPhoto = "/UploadedDocument/user.jpg";
      if (seeker.profileImage != null)
      {
        connectionModel.ProviderPhoto = provider_img;        
      }

      if (provider_img != null)
      {
        connectionModel.ProviderPhoto = provider_img;
      }
      var groupName = seeker.email.Split('@')[0] + "#" + provider.email.Split('@')[0];
      var connectionCount = db.ChatConnections.Where(m => m.seeker_email == seeker.email && m.provider_email == provider.email && m.group_name==groupName).ToList();
      var connection = db.ChatConnections.Where(m => m.seeker_email == seeker.email && m.provider_email == provider.email && m.group_name == groupName).FirstOrDefault();
      if (connectionCount.Count()<=0)
      {
        var newConnection = new ChatConnection();
        newConnection.seeker_email = seeker.email;
        newConnection.provider_email = provider.email;
        newConnection.first_chat_datetime = DateTime.Now;
        newConnection.last_chat_datetime = DateTime.Now;
        newConnection.group_name = seeker.email.Split('@')[0] + "#" + provider.email.Split('@')[0];
        db.ChatConnections.Add(newConnection);
        try
        {
          db.SaveChanges();
          connectionModel.ConnectionID = newConnection.connection_id;
          connectionModel.Seeker = newConnection.seeker_email;
          connectionModel.Provider = newConnection.provider_email;
          connectionModel.GroupName = newConnection.group_name;
        }
        catch (Exception ex)
        {
          throw ex;
        }
      }
      else
      {
        connectionModel.ConnectionID = connection.connection_id;
        connectionModel.Seeker = connection.seeker_email;
        connectionModel.Provider = connection.provider_email;
        connectionModel.GroupName = connection.group_name;

        connectionModel.ProviderPhoto = provider.profileImage;
        connectionModel.SeekerPhoto = seeker.profileImage;
        if (provider.profileImage == null)
          connectionModel.ProviderPhoto = "/UploadedDocument/user.jpg";
        if (seeker.profileImage == null)
          connectionModel.SeekerPhoto = "/UploadedDocument/user.jpg";

        var newConnection = db.ChatConnections.Where(m => m.connection_id == connection.connection_id).FirstOrDefault();
        newConnection.last_chat_datetime = DateTime.Now;
        db.SaveChanges();

      }

      if (connectionModel.Seeker == Session["Email"].ToString())
      {
        connectionModel.Receiver = connectionModel.Provider.Split('@')[0];
      }
      else
      {
        connectionModel.Receiver = connectionModel.Seeker.Split('@')[0];
      }

      List<Chat> conversationList = db.Chats.Where(m => m.group_name == connectionModel.GroupName).OrderBy(m => m.created_at).ToList();
      var messageList = new List<MessageInfo>();
      var date = "";
      foreach (var msg in conversationList)
      {
        var message = new MessageInfo();
        message.MessageId = msg.conversation_id.ToString();
        message.Message = msg.message;
        message.Sender = msg.sender_id;
        message.Receiver = msg.receiver_id;
        message.StartTime = Convert.ToDateTime(msg.created_at).ToString("hh:mm");
        message.MsgDate = Convert.ToDateTime(msg.created_at).ToString("MM/dd/yyyy");

        if (date != message.MsgDate)
        {

          messageList.Add(new MessageInfo() { MessageId = "Date Sep", MsgDate = message.MsgDate == DateTime.Now.ToString("MM/dd/yyyy") ? "Today" : message.MsgDate });
        }

        date = message.MsgDate;
        messageList.Add(message);
      }

      model.ConnectionModelObj = connectionModel;
      model.MessageList = messageList;
      //return View(model);
      return PartialView("_Contact", model);
    }

    [HttpPost]
    public ActionResult GetNotification(string room, string userName, string message)
    {
      var objHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
      objHub.Clients.Group(room).getMessages(userName, message, "You have a new message from " + userName.Split('@')[0], room.Replace('#', '_'));
      //return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
      return new JsonResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
    }

    [HttpPost]
    public JsonResult DeleteMessage(string messageId)
    {
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var message = db.Chats.Where(m => m.conversation_id.ToString() == messageId).FirstOrDefault();

        if (message != null)
        {
          db.Chats.Remove(message);
          db.SaveChanges();
          return new JsonResult { Data = "Deleted", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        return new JsonResult { Data = "Failed " + messageId, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
      }
      //return new JsonResult { Data = "Failed ", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
    }
    [HttpPost]
    public JsonResult UploadFileToChat(HttpPostedFileBase item)
    {
      var groupname = Request["groupId"];
      var path = "~/UploadedDocument/" + groupname;
      var docPath = path;
      var docs = "";
      var message = "";
      if (Request.Files.Count > 0)
      {
        HttpFileCollectionBase files = Request.Files;
        for (int i = 0; i < files.Count; i++)
        {

          HttpPostedFileBase file = files[i];
          var extension = Path.GetExtension(file.FileName).ToLower();
          if (extension == ".jpg" || extension == ".png" || extension == ".pdf" || extension == ".docx" || extension == ".txt")
          {
            if (!Directory.Exists(Server.MapPath(docPath)))
            {
              Directory.CreateDirectory(Server.MapPath(docPath));
            }

            path = Path.Combine(Server.MapPath(docPath), file.FileName);
            file.SaveAs(path);
            if (extension == ".jpg" || extension == ".png")
            {
              message = "<img src='" + (docPath + "/" + file.FileName).Replace("#", "%23").Replace("~", "") + "' width='200px' />";
            }
            else
            {
              message = "<btn class='btn btn-sm btn-warning' onclick='viewMediaMessage(\"" + docPath + "/" + file.FileName + "\")'><i class='fa fa-file-alt'></i> " + file.FileName + "</a>";
            }
            docs += file.FileName + "#";
            ViewBag.UploadSuccess = true;
          }
        }

      }

      var receiver = "";
      var provider = "";
      var seeker = "";
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var msg = new Chat();
        msg.created_at = DateTime.Now;
        msg.message = message;
        var userName = Request["userName"];
        msg.sender_id = userName;
        msg.group_name = groupname;
        var connection = db.ChatConnections.Where(m => m.group_name == groupname).FirstOrDefault();
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
        msg.is_media = "Yes";
        db.Chats.Add(msg);
        db.SaveChanges();
      }
      return new JsonResult { Data = message, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
    }

    public ActionResult ChatList(string id)
    {
      var email = Session["Email"];
      var chatList = new List<ChatList>();
      var chatRoom = new ChatRoom();
      if (email!= null)
      {
        if (Session["Role"].ToString() == "Provider")
        {
          var connections = db.ChatConnections.Where(m => m.provider_email == email.ToString()).ToList();
          if(connections!= null)
          {
            foreach(var i in connections)
            {
              var seeker = db.Seekers.Where(m => m.email == i.seeker_email).FirstOrDefault();
              var chat = new ChatList() {
                ReceiverEmail=seeker.email,
                ReceiverName = seeker.name,
                ProfilePic = seeker.profileImage
              };
              chatList.Add(chat);
              chatRoom.chatList = chatList;
            }
          }
        }
        else if (Session["Role"].ToString() == "Seeker")
        {
          var connections = db.ChatConnections.Where(m => m.seeker_email == email.ToString()).ToList();
          if (connections != null)
          {
            foreach (var i in connections)
            {
              var provider = db.Providers.Where(m => m.email == i.provider_email).FirstOrDefault();
              var chat = new ChatList()
              {
                ReceiverEmail = provider.email,
                ReceiverName = provider.name,
                ProfilePic = "/Image/Profile/"+provider.profileImage
              };
              chatList.Add(chat);
              chatRoom.chatList = chatList;
            }
          }
        }
      }
      var conversations = GetConversation(id);
      chatRoom.message = conversations;
      return View(chatRoom);
    }

    public ConversationModel GetConversation(string id)
    {
      ConversationModel model = new ConversationModel();
      //var job = db.Requests.Where(j => j.SId == id).FirstOrDefault();
      var role = Session["Role"].ToString();
      var email = Session["Email"].ToString();
      var connectionModel = new ChatConnectionModel();
      var seeker = new Seeker();
      var provider = new Provider();
      if (role == "Provider")
      {
        seeker = db.Seekers.Where(m => m.email == id).FirstOrDefault();
        provider = db.Providers.Where(m => m.email == email).FirstOrDefault();
      }
      else if (role == "Seeker")
      {
        seeker = db.Seekers.Where(m => m.email == email).FirstOrDefault();
        provider = db.Providers.Where(m => m.email == id).FirstOrDefault();
      }
      if(provider != null && seeker != null)
      {
        var provider_img = provider.profileImage;
        var seeker_img = seeker.profileImage;
        connectionModel.SeekerPhoto = "/UploadedDocument/user.jpg";
        connectionModel.ProviderPhoto = "/UploadedDocument/user.jpg";
        if (seeker.profileImage != null)
        {
          connectionModel.ProviderPhoto = provider_img;
        }

        if (provider_img != null)
        {
          connectionModel.ProviderPhoto = provider_img;
        }
        var groupName = seeker.email.Split('@')[0] + "#" + provider.email.Split('@')[0];
        var connectionCount = db.ChatConnections.Where(m => m.seeker_email == seeker.email && m.provider_email == provider.email && m.group_name == groupName).ToList();
        var connection = db.ChatConnections.Where(m => m.seeker_email == seeker.email && m.provider_email == provider.email && m.group_name == groupName).FirstOrDefault();
        if (connectionCount.Count() <= 0)
        {
          var newConnection = new ChatConnection();
          newConnection.seeker_email = seeker.email;
          newConnection.provider_email = provider.email;
          newConnection.first_chat_datetime = DateTime.Now;
          newConnection.last_chat_datetime = DateTime.Now;
          newConnection.group_name = seeker.email.Split('@')[0] + "#" + provider.email.Split('@')[0];
          db.ChatConnections.Add(newConnection);
          try
          {
            db.SaveChanges();
            connectionModel.ConnectionID = newConnection.connection_id;
            connectionModel.Seeker = newConnection.seeker_email;
            connectionModel.Provider = newConnection.provider_email;
            connectionModel.GroupName = newConnection.group_name;
          }
          catch (Exception ex)
          {
            throw ex;
          }
        }
        else
        {
          connectionModel.ConnectionID = connection.connection_id;
          connectionModel.Seeker = connection.seeker_email;
          connectionModel.Provider = connection.provider_email;
          connectionModel.GroupName = connection.group_name;

          connectionModel.ProviderPhoto = provider.profileImage;
          connectionModel.SeekerPhoto = seeker.profileImage;
          if (provider.profileImage == null)
            connectionModel.ProviderPhoto = "/UploadedDocument/user.jpg";
          if (seeker.profileImage == null)
            connectionModel.SeekerPhoto = "/UploadedDocument/user.jpg";

          var newConnection = db.ChatConnections.Where(m => m.connection_id == connection.connection_id).FirstOrDefault();
          newConnection.last_chat_datetime = DateTime.Now;
          db.SaveChanges();

        }

        if (connectionModel.Seeker == Session["Email"].ToString())
        {
          connectionModel.Receiver = connectionModel.Provider.Split('@')[0];
        }
        else
        {
          connectionModel.Receiver = connectionModel.Seeker.Split('@')[0];
        }

        List<Chat> conversationList = db.Chats.Where(m => m.group_name == connectionModel.GroupName).OrderBy(m => m.created_at).ToList();
        var messageList = new List<MessageInfo>();
        var date = "";
        foreach (var msg in conversationList)
        {
          var message = new MessageInfo();
          message.MessageId = msg.conversation_id.ToString();
          message.Message = msg.message;
          message.Sender = msg.sender_id;
          message.Receiver = msg.receiver_id;
          message.StartTime = Convert.ToDateTime(msg.created_at).ToString("hh:mm");
          message.MsgDate = Convert.ToDateTime(msg.created_at).ToString("MM/dd/yyyy");

          if (date != message.MsgDate)
          {

            messageList.Add(new MessageInfo() { MessageId = "Date Sep", MsgDate = message.MsgDate == DateTime.Now.ToString("MM/dd/yyyy") ? "Today" : message.MsgDate });
          }

          date = message.MsgDate;
          messageList.Add(message);
        }

        model.ConnectionModelObj = connectionModel;
        model.MessageList = messageList;
      }
      else
      {
        model.ConnectionModelObj = new ChatConnectionModel() { GroupName="00"};
        model.MessageList = new List<MessageInfo>();
      }
      return model;
    }
  }
}
