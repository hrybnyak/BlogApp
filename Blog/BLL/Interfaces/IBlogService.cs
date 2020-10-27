using BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IBlogService
    {
        Task<BlogDto> CreateBlog(BlogDto blog, string token);
        void DeleteBlog(int id, string token);
        void UpdateBlogName(int id, BlogDto blog, string token);
        BlogDto GetBlogById(int id);
        IEnumerable<BlogDto> GetAllBlogs();
        IEnumerable<ArticleDto> GetAllArticlesByBlogId(int id);
    }
}
