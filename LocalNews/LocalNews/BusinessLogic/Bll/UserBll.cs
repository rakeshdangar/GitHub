using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LocalNews.Models;
using LocalNews.BusinessLogic;
using LocalNews.BusinessLogic.Interface;
using LocalNews.BusinessLogic.Security;

namespace LocalNews.BusinessLogic.Bll
{
    public class UserBll : IUserInterface
    {
        private List<User> users = new List<User>();
        static readonly ILoginHistoryInterface loginHistoryBll = new LoginHistoryBll();

        public UserBll()
        {
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                throw new ArgumentNullException("oldPassword");
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentNullException("newPassword");
            }

            long userId = -1;
            using (var db = new LocalNewsDBEntities())
            {
                User user = db.Users.Where(u => u.username == username).FirstOrDefault();
                if (null != user && user.password == CryptoUtil.ComputeHash(oldPassword))
                {
                    userId = user.user_id;
                    user.password = CryptoUtil.ComputeHash(newPassword);
                    db.SaveChanges();
                }
            }
            if (userId > 0)
            {
                loginHistoryBll.Add(userId, LocalNewsConstant.User_Status_Password_Reset);
                return true;
            }
            else
                return false;
            
        }

        public IEnumerable<User> GetAll()
        {
            return users;
        }

        public User Get(int id)
        {
            return users.Find(u => u.user_id == id);
        }

        public User Get(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            User user = null;
            using (var db = new LocalNewsDBEntities())
            {
                user = db.Users.Where(u => u.username == username).FirstOrDefault();
            }
            return user;
        }

        public bool HasLocalAccount(string username)
        {
            return null != Get(username) ? true : false;
        }

        public bool Login(LoginModel login)
        {
            string userStatus = LocalNewsConstant.User_Status_Login;
            if (login == null)
            {
                throw new ArgumentNullException("user");
            }
            User user = null;
            string password = CryptoUtil.ComputeHash(login.Password);
            using (var db = new LocalNewsDBEntities())
            {
                user = db.Users.Where(u => u.username == login.UserName && u.password == password).FirstOrDefault();
            }
            if (null == user)
            {
                user = Get(login.UserName);
                userStatus = LocalNewsConstant.User_Status_Wrong_Password;
                return false;
            }
            loginHistoryBll.Add(user.user_id, userStatus);
            return true;
        }

        public User Register(RegisterModel register)
        {
            if (register == null)
            {
                throw new ArgumentNullException("user");
            }
            
            User user = new User();
            user.first_name = register.FirstName;
            user.last_name = register.LastName;
            user.username = register.UserName;
            user.password = CryptoUtil.ComputeHash(register.Password);
            
            using (var db = new LocalNewsDBEntities())
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
            loginHistoryBll.Add(user.user_id, LocalNewsConstant.User_Status_New_User);
            return user;
        }

        public bool Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            using (var db = new LocalNewsDBEntities())
            {
                var userToUpdate = (from u in db.Users
                                    where u.username == user.username
                                    select u).FirstOrDefault();

                userToUpdate.username = user.username;
                int num = db.SaveChanges();
            }
            return true;
        }

        public void Remove(int id)
        {
            users.RemoveAll(u => u.user_id == id);
        }
    }
}