using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LocalNews.Models;

namespace LocalNews.BusinessLogic.Interface
{
    interface ILoginHistoryInterface
    {
        void Add(long userId, string userStatus);
    }
}
