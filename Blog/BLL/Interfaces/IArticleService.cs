using BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IArticleService
    {
        Task<ArticleDto> CreateArticle(ArticleDto article, string token);
        void DeleteArticle(int id, string token);
        void UpdateArticle(int id, ArticleDto article, string token);
        Task<ArticleDto> GetArticleById(int id);
        Task<ICollection<TegDto>> GetTegsByArticleId(int id);
        Task<ICollection<CommentDto>> GetCommentsByArticleId(int id);
        IEnumerable<ArticleDto> GetArticlesWihtTextFilter(string filter);
        IEnumerable<ArticleDto> GetAllArticles();
        IEnumerable<ArticleDto> GetArticlesWithTegFilter(IEnumerable<TegDto> tegs);
        IEnumerable<ArticleDto> GetArticlesWithTegFilter(string tegs);
    }
}
