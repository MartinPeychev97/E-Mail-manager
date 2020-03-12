using System.Collections.Generic;
using System.Threading.Tasks;
using EmailManager.Data.Implementation;

namespace EmailManager.Services.Contracts
{
    public interface IUserServices
    {
        Task<User> BanUser(string userId);
        Task<IEnumerable<User>> GetAll(int currentPage);
        Task<User> GetUserById(string id);
        Task<IEnumerable<User>> SearchUsers(string search, int currentPage);
        Task<int> GetPageCount(int emailsPerPage);
    }
}