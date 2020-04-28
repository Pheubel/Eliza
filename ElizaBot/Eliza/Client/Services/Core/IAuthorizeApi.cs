using Eliza.Shared;
using System.Threading.Tasks;

namespace Eliza.Client.Services.Core
{
    public interface IAuthorizeApi
    {
        Task<UserInfoDTO> GetUserInfo();
        Task Login();
        Task Logout();
    }
}
