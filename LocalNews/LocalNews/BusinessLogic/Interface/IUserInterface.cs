using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LocalNews.Models;

namespace LocalNews.BusinessLogic.Interface
{
    interface IUserInterface
    {
        IEnumerable<User> GetAll();
        bool ChangePassword(string username, string oldPassword, string newPassword);
        User Get(int id);
        User Get(string username);
        bool HasLocalAccount(string username);
        bool Login(LoginModel loginModel);
        User Register(RegisterModel registerModel);
        void Remove(int id);
        bool Update(User user);
        ResetPasswordModelStepTwo GetSecurityQuestion(string username);
        bool VerifySecurity(ResetPasswordModelStepTwo stepTwoModel);
    }
}
