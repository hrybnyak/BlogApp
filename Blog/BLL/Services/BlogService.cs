using AutoMapper;
using BLL.DTO;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class BlogService : IBlogService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly IJwtFactory _jwtFactory;

        public BlogService(IJwtFactory jwtFactory, IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper)
        {
            _jwtFactory = jwtFactory;
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }

        private bool ConfigureRights(string token, string id)
        {
            string claimsId = _jwtFactory.GetUserIdClaim(token);
            if (claimsId == null) throw new ArgumentNullException(nameof(claimsId));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (claimsId.CompareTo(id) == 0) return true;
            else return false;
        }

        public async Task<BlogDto> CreateBlog (BlogDto blog, string token)
        {
            if (blog == null) throw new ArgumentNullException(nameof(blog));
            string claimsId = _jwtFactory.GetUserIdClaim(token);
            var blogEntity = _mapper.Map<Blog>(blog);
            blogEntity.OwnerId = claimsId;

            _unitOfWork.BlogRepository.Insert(blogEntity);
            await _unitOfWork.SaveAsync();
            blogEntity = _unitOfWork.BlogRepository.Get(b => b.Name == blog.Name, includeProperties:"Owner").FirstOrDefault();
            if (blogEntity == null) throw new ArgumentNullException(nameof(blogEntity));
            var result = _mapper.Map<BlogDto>(blogEntity);
            result.OwnerUsername = blogEntity.Owner.UserName;
            return result;
        }
        public void DeleteBlog(int id, string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            var blogEntity = _unitOfWork.BlogRepository.GetById(id);
            if (blogEntity == null) throw new ArgumentNullException(nameof(blogEntity), "This blog doesn't exist");
            if (ConfigureRights(token, blogEntity.OwnerId))
            {
                _unitOfWork.BlogRepository.Delete(blogEntity);
                _unitOfWork.Save();
            }
            else throw new NotEnoughtRightsException();
        }
        public void UpdateBlogName (int id, BlogDto blog, string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (blog == null) throw new ArgumentNullException(nameof(blog));
            var entity = _unitOfWork.BlogRepository.GetById(id);
            if (entity == null) throw new ArgumentNullException(nameof(entity), "This blog doesn't exist");
            if (!ConfigureRights(token,entity.OwnerId)) throw new NotEnoughtRightsException();
            if (_unitOfWork.BlogRepository.Get(b => b.Name == blog.Name).FirstOrDefault() != null) throw new NameIsAlreadyTakenException();
            entity.Name = blog.Name;
            _unitOfWork.BlogRepository.Update(entity);
            _unitOfWork.Save();
        }
        public BlogDto GetBlogById(int id)
        {
            var blog = _unitOfWork.BlogRepository.Get((b => b.Id == id), includeProperties: "Articles,Owner").FirstOrDefault();
            if (blog == null) throw new ArgumentNullException(nameof(blog), "Couldn't find blog by id");
            var dto = _mapper.Map<BlogDto>(blog);
            if (blog.Articles.Count>0) dto.Articles = _mapper.Map<IEnumerable<ArticleDto>>(blog.Articles);
            if (blog.Owner != null) dto.OwnerUsername = blog.Owner.UserName;
            return dto;
        }
        public IEnumerable<BlogDto> GetAllBlogs()
        {
            return _mapper.Map<IEnumerable<BlogDto>>(_unitOfWork.BlogRepository.Get());
        }

        public IEnumerable<ArticleDto> GetAllArticlesByBlogId(int id)
        {
            var blog = GetBlogById(id);
            return blog.Articles;
        }

    }
}
