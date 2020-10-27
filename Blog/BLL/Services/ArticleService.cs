using AutoMapper;
using BLL.DTO;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtFactory _jwtFactory;
        private readonly IMapper _mapper;
        private UserManager<User> _userManager;
        
        public ArticleService(IUnitOfWork unitOfWork, IJwtFactory jwtFactory, UserManager<User> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtFactory = jwtFactory;
            _userManager = userManager;
            _mapper = mapper;
        }

        private async Task AddTegs(Article articleEntity, ArticleDto article)
        {
            if (article.Tegs != null && article.Tegs.Count > 0)
            {
                foreach (TegDto teg in article.Tegs)
                {
                    var tegEntity = _unitOfWork.TegRepository.Get(t => t.Name == teg.Name, includeProperties: "ArticleTegs").FirstOrDefault();
                    if (tegEntity == null)
                    {
                        tegEntity = _mapper.Map<Teg>(teg);
                        tegEntity.ArticleTegs = new List<ArticleTeg>();
                        _unitOfWork.TegRepository.Insert(tegEntity);
                        await _unitOfWork.SaveAsync();
                    }
                    var connection = new ArticleTeg
                    {
                        ArticleId = articleEntity.Id,
                        TegId = tegEntity.Id
                    };
                    tegEntity.ArticleTegs.Add(connection);
                    _unitOfWork.TegRepository.Update(tegEntity);
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        public IEnumerable<ArticleDto> GetArticlesWithTegFilter(string tegs)
        {
            if (tegs == null) throw new ArgumentNullException(nameof(tegs));
            List<TegDto> teg = new List<TegDto>();
            string[] names = tegs.Split(',');
            foreach(string name in names)
            {
                teg.Add(new TegDto { Name = name });
            }
            return GetArticlesWithTegFilter(teg);
        }

        public IEnumerable<ArticleDto> GetArticlesWithTegFilter(IEnumerable<TegDto> tegs)
        {
            IEnumerable<Article> articles = _unitOfWork.ArticleRepository.Get(includeProperties: "ArticleTegs");
            List<Article> result = new List<Article>();
            foreach (TegDto teg in tegs)
            {
                Teg tegEntity;
                if (teg.Name != null)
                {
                    tegEntity = _unitOfWork.TegRepository.Get(t => t.Name == teg.Name).FirstOrDefault();
                    if (tegEntity == null) throw new ArgumentNullException(nameof(tegEntity));
                }
                else
                {
                    throw new ArgumentNullException(nameof(teg));
                }
                var filtered = articles.Where(a => a.ArticleTegs.Contains(a.ArticleTegs.Where(at=>at.ArticleId == a.Id && at.TegId == tegEntity.Id).FirstOrDefault()));
                foreach (Article article in filtered) {
                    if (!result.Contains(article))
                    {
                        result.Add(article);
                    }
                }
            }
            return _mapper.Map<IEnumerable<ArticleDto>>(result);
        }
        public async Task<ArticleDto> CreateArticle(ArticleDto article, string token)
        {
            if (article == null) throw new ArgumentNullException(nameof(article));
            if (article.Name == null) throw new ArgumentNullException(nameof(article.Name));
            if (article.Content == null) throw new ArgumentNullException(nameof(article.Content));

            string ownerId = (await _unitOfWork.BlogRepository.GetByIdAsync(article.BlogId.GetValueOrDefault())).OwnerId;
            if (ownerId.CompareTo(_jwtFactory.GetUserIdClaim(token)) != 0) throw new NotEnoughtRightsException();
            var articleEntity = _mapper.Map<Article>(article);
            articleEntity.LastUpdate = DateTime.Now;

            _unitOfWork.ArticleRepository.Insert(articleEntity);
            await _unitOfWork.SaveAsync();
            await AddTegs(articleEntity, article);

            var result = _mapper.Map<ArticleDto>(articleEntity);

            if (articleEntity.ArticleTegs != null && articleEntity.ArticleTegs.Count > 0)
            {
                result.Tegs = new List<TegDto>();
                foreach (ArticleTeg teg in articleEntity.ArticleTegs)
                    result.Tegs.Add(_mapper.Map<TegDto>(_unitOfWork.TegRepository.GetById(teg.TegId)));
            }
            return result;
        }

        public void DeleteArticle(int id, string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            var entity = _unitOfWork.ArticleRepository.GetById(id);
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            string ownerId = _unitOfWork.BlogRepository.GetById(entity.BlogId).OwnerId;
            if (ownerId.CompareTo(_jwtFactory.GetUserIdClaim(token)) != 0)
            {
                if (_jwtFactory.GetUserRoleClaim(token).CompareTo("Moderator") != 0 ) throw new NotEnoughtRightsException();
            }
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _unitOfWork.ArticleRepository.Delete(entity);
            _unitOfWork.Save();
         }

        public void UpdateArticle(int id, ArticleDto article, string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (article == null) throw new ArgumentNullException(nameof(article));
            var entity = _unitOfWork.ArticleRepository.GetById(id);
            if (entity == null) throw new ArgumentNullException(nameof(article));
            string ownerId = _unitOfWork.BlogRepository.GetById(entity.BlogId).OwnerId;
            if (ownerId.CompareTo(_jwtFactory.GetUserIdClaim(token)) != 0) throw new NotEnoughtRightsException();
            entity.Name = article.Name;
            entity.Content = article.Content;
            entity.LastUpdate = DateTime.Now;
            _unitOfWork.ArticleRepository.Update(entity);
            _unitOfWork.Save();
        }

        public async Task<ArticleDto> GetArticleById(int id)
        {
            var article = _unitOfWork.ArticleRepository.Get(a => a.Id == id, includeProperties: "Comments,ArticleTegs").FirstOrDefault();
            if (article == null) throw new ArgumentNullException(nameof(article));
            var result = _mapper.Map<ArticleDto>(article);

            if (article.Comments!=null && article.Comments.Count > 0) 
            {
                result.Comments = new List<CommentDto>();
                foreach (Comment comment in article.Comments)
                {
                    CommentDto dto = _mapper.Map<CommentDto>(comment);
                    dto.CreatorUsername = (await _userManager.FindByIdAsync(comment.UserId)).UserName;
                    result.Comments.Add(dto);
                }
            }
            result.AuthorId = (await _unitOfWork.BlogRepository.GetByIdAsync(article.BlogId)).OwnerId;
            result.AuthorUsername = (await _userManager.FindByIdAsync(result.AuthorId)).UserName;
            if (article.ArticleTegs != null && article.ArticleTegs.Count > 0)
            {
                result.Tegs = new List<TegDto>();
                foreach (ArticleTeg teg in article.ArticleTegs)
                    result.Tegs.Add(_mapper.Map<TegDto>(await _unitOfWork.TegRepository.GetByIdAsync(teg.TegId)));
            }
            return result;   
        }

        public async Task<ICollection<CommentDto>> GetCommentsByArticleId (int id)
        {
            var article = await GetArticleById(id);
            return article.Comments;
        }

        public async Task<ICollection<TegDto>> GetTegsByArticleId (int id)
        {
            var article = await GetArticleById(id);
            return article.Tegs;
        }

        public IEnumerable<ArticleDto> GetArticlesWihtTextFilter(string filter)
        {
            var articles = _unitOfWork.ArticleRepository.Get(a => a.Content.Contains(filter) || a.Name.Contains(filter));
            if (articles == null) throw new ArgumentNullException(nameof(articles));
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }

        public IEnumerable<ArticleDto> GetAllArticles()
        {
            var articles = _unitOfWork.ArticleRepository.Get();
            if (articles == null) throw new ArgumentNullException(nameof(articles));
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }
    }
}
