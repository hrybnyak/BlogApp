using AutoMapper;
using BLL.DTO;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Article, ArticleDto>()
                .ForMember(a => a.Id, map => map.MapFrom(a => a.Id))
                .ForMember(a => a.BlogId, map => map.MapFrom(a => a.BlogId))
                .ForMember(a => a.Content, map => map.MapFrom(a => a.Content))
                .ForMember(a => a.Name, map => map.MapFrom(a => a.Name))
                .ForMember(a => a.LastUpdate, map => map.MapFrom(a => a.LastUpdate))

                .ReverseMap();

            CreateMap<Blog, BlogDto>()
                .ForMember(a => a.Id, map => map.MapFrom(a => a.Id))
                .ForMember(a => a.Name, map => map.MapFrom(a => a.Name))
                .ForMember(a => a.OwnerId, map => map.MapFrom(a => a.OwnerId))

                .ReverseMap();

            CreateMap<Comment, CommentDto>()
                .ForMember(a => a.Id, map => map.MapFrom(a => a.Id))
                .ForMember(a => a.Content, map => map.MapFrom(a => a.Content))
                .ForMember(a => a.LastUpdate, map => map.MapFrom(a => a.LastUpdated))
                .ForMember(a => a.ArticleId, map => map.MapFrom(a => a.ArticleId))
                .ForMember(a => a.CreatorId, map => map.MapFrom(a => a.UserId))

                .ReverseMap();

            CreateMap<Teg, TegDto>()
                .ForMember(a => a.Id, map => map.MapFrom(a => a.Id))
                .ForMember(a => a.Name, map => map.MapFrom(a => a.Name))

                .ReverseMap();

            CreateMap<User, UserDto>()
                .ForMember(a => a.Id, map => map.MapFrom(a => a.Id))
                .ForMember(a => a.UserName, map => map.MapFrom(a => a.Id))
                .ForMember(a => a.Email, map => map.MapFrom(a => a.Email))

                .ReverseMap();
        }
    }
}
