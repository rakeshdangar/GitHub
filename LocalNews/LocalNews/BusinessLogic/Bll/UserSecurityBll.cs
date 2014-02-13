using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using LocalNews.Models;
using LocalNews.BusinessLogic.Interface;

namespace LocalNews.BusinessLogic.Bll
{
    public class UserSecurityBll : IUserSecurityInterface
    {
        public Question GetQuestion(long id)
        {
            Question question = null;
            using (var db = new LocalNewsDBEntities())
            {
                question = db.Questions.Where(q => q.question_id == id).FirstOrDefault();
            }
            return question;
        }

        public Security GetUserSecurity(long id)
        {
            Security security = null;
            using (var db = new LocalNewsDBEntities())
            {
                security = db.Securities.Where(q => q.security_id == id).FirstOrDefault();
            }
            return security;
        }
    }
}