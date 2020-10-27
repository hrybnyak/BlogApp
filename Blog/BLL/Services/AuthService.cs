using BLL.DTO;
using BLL.Exceptions;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        public readonly UserManager<User> UserManager;
        public readonly IJwtFactory JwtFactory;
        public readonly JwtIssuerOptions JwtOptions;

        public AuthService(UserManager<User> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            UserManager = userManager;
            JwtFactory = jwtFactory;
            JwtOptions = jwtOptions.Value;
        }

        public async Task<ClaimsIdentity> GetClaimsIdentity(UserDto user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.UserName == null) throw new ArgumentNullException(nameof(user.UserName));
            if (user.Password == null) throw new ArgumentNullException(nameof(user.Password));
            var userToVerify = await UserManager.FindByNameAsync(user.UserName);
            if (userToVerify == null)
            {
                userToVerify = await UserManager.FindByEmailAsync(user.UserName);
                if (userToVerify == null)
                {
                    throw new WrongCredentialsException();
                }
            }
            if (await UserManager.CheckPasswordAsync(userToVerify, user.Password))
            {
                return await JwtFactory.GenerateClaimsIdentity(userToVerify);
            }
            else
            {
                throw new WrongCredentialsException();
            }
        }

        public async Task<object> Authenticate(UserDto user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var identity = await GetClaimsIdentity(user);
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            string token = await JwtFactory.GenerateEncodedToken(user.UserName, identity);
            if (token == null) throw new ArgumentNullException(nameof(token));
            return new
            {
                id = identity.FindFirst(ClaimTypes.NameIdentifier).Value,
                auth_token = token,
                expires_in = (int)JwtOptions.ValidFor.TotalSeconds
            };
        }
    }
}
