using BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAccountService
    {
        Task<UserDto> RegisterRegularUser(UserDto userDto);
        Task<UserDto> RegisterModerator(UserDto userDto);
        Task<IEnumerable<UserDto>> GetAllRegularUsers();
        Task<IEnumerable<UserDto>> GetAllUsers();
        Task<IEnumerable<UserDto>> GetAllModerators();
        Task<UserDto> GetUserById(string id, string token);
        Task<bool> DeleteUser(string id, string token);
        Task UpdateUser(string id, UserDto user, string token);
        Task ChangePassword(string id, PasswordDto password, string token);
        Task<IEnumerable<BlogDto>> GetAllBlogsByUserId(string id);
        Task<IEnumerable<CommentDto>> GetAllCommentsByUserId(string id);
    }
}
