using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LocalNews.Models;
using LocalNews.BusinessLogic.Interface;

namespace LocalNews.BusinessLogic.Bll
{
    public class LoginHistoryBll : ILoginHistoryInterface
    {
        public void Add(long userId, string userStatus)
        {
            Login_History history = new Login_History();
            history.user_id = userId;
            history.date_time = System.DateTime.Now;
            using (var db = new LocalNewsDBEntities())
            {
                User_Status status = db.User_Status.Where(us => us.status_name == userStatus).FirstOrDefault();
                history.status_id = status.status_id;
                db.Login_History.Add(history);
                db.SaveChanges();
            }
        }
    }
}