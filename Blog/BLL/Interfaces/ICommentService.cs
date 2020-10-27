using BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> GetCommentById(int id);
        void UpdateComment(int id, CommentDto comment, string token);
        void DeleteComment(int id, string token);
        Task<CommentDto> AddComment(CommentDto comment, string token);
        IEnumerable<CommentDto> GetAllComments();

    }
}
