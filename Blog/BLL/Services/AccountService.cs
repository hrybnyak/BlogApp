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
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private IJwtFactory _jwtFactory;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public AccountService(UserManager<User> userManager, IJwtFactory jwtFactory, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        private async Task<User> CreateUser(UserDto userDto)
        {
            if (await _userManager.FindByEmailAsync(userDto.Email) != null) throw new EmailIsAlreadyTakenException();
            if (await _userManager.FindByNameAsync(userDto.UserName) != null) throw new NameIsAlreadyTakenException();
            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email
            };
            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (result.Succeeded) return user;
            else return null;
        }
        public async Task<UserDto> RegisterRegularUser(UserDto userDto)
        {
            var user = await CreateUser(userDto);
            if (user == null) throw new ArgumentNullException(nameof(user),"Couldn't create user");
            else {
                await _userManager.AddToRoleAsync(user, "RegularUser");
            }
            return _mapper.Map<UserDto>(user);
        }
        public async Task<UserDto> RegisterModerator(UserDto userDto)
        {
            var user = await CreateUser(userDto);
            if (user == null) throw new ArgumentNullException(nameof(user), "Couldn't create user");
            else
            {
                await _userManager.AddToRoleAsync(user, "Moderator");
            }
            return _mapper.Map<UserDto>(user);
        } 
        public async Task<IEnumerable<UserDto>> GetAllRegularUsers()
        {
            return _mapper.Map<IEnumerable<UserDto>>((await _userManager.GetUsersInRoleAsync("RegularUser")).ToList());
        }
        public async Task<IEnumerable<UserDto>> GetAllModerators()
        {
            return _mapper.Map<IEnumerable<UserDto>>((await _userManager.GetUsersInRoleAsync("Moderator")).ToList());
        }
        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            List<UserDto> users = (await GetAllRegularUsers()).ToList();
            users.AddRange(_mapper.Map<IEnumerable<UserDto>>((await _userManager.GetUsersInRoleAsync("Moderator")).ToList()));
            return users;
        }
        public async Task<UserDto> GetUserById(string id, string token)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (token == null) throw new ArgumentNullException(nameof(token));
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new ArgumentNullException(nameof(user), "Couldn't find user with this id");

            string claimsId = _jwtFactory.GetUserIdClaim(token);
            if (claimsId == id) return _mapper.Map<UserDto>(user);
            string claimsRole = _jwtFactory.GetUserRoleClaim(token);
            if (claimsRole == "Moderator")
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any(r => r == "Moderator" || r == "Admin")) throw new NotEnoughtRightsException();
                else return _mapper.Map<UserDto>(user);
            }
            else if (claimsRole == "Admin")
            {
                return _mapper.Map<UserDto>(user);
            }
            else throw new NotEnoughtRightsException();
        }
        public async Task<bool> DeleteUser(string id, string token)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (token == null) throw new ArgumentNullException(nameof(token));
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new ArgumentNullException(nameof(user), "Couldn't find user with this id");

            string claimsId = _jwtFactory.GetUserIdClaim(token);
            if (claimsId == id) return (await _userManager.DeleteAsync(user)).Succeeded;
            string claimsRole = _jwtFactory.GetUserRoleClaim(token);
            if (claimsRole == "Moderator")
            {
                throw new NotEnoughtRightsException();
            }
            else if (claimsRole == "Admin") return (await _userManager.DeleteAsync(user)).Succeeded;
            else throw new NotEnoughtRightsException();
        }

        public async Task UpdateUser (string id, UserDto user, string token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.UserName == null && user.Email == null) throw new ArgumentNullException(nameof(user));

            string claimsId = _jwtFactory.GetUserIdClaim(token);

            var userEntity = await _userManager.FindByIdAsync(id);
            if (userEntity == null) throw new ArgumentNullException(nameof(userEntity), "Couldn't find user with this id");

            if (claimsId == id)
            {
                if (user.UserName != null && userEntity.UserName.CompareTo(user.UserName) != 0)
                {
                    var checkIfNameIsTaken = await _userManager.FindByNameAsync(user.UserName);
                    if (checkIfNameIsTaken != null) throw new NameIsAlreadyTakenException();
                    userEntity.UserName = user.UserName;
                }
                else if (user.Email != null && userEntity.Email.CompareTo(user.Email) != 0)
                {
                    var checkIfNameIsTaken = await _userManager.FindByEmailAsync(user.Email);
                    if (checkIfNameIsTaken != null) throw new NameIsAlreadyTakenException();
                    userEntity.Email = user.Email;
                }
                await _userManager.UpdateAsync(userEntity);
            }
            else throw new NotEnoughtRightsException();
        }
        public async Task ChangePassword(string id, PasswordDto password, string token)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (password.NewPassword == null) throw new ArgumentNullException(nameof(password.NewPassword));
            if (password.OldPassword == null) throw new ArgumentNullException(nameof(password.OldPassword));

            string claimsId = _jwtFactory.GetUserIdClaim(token);
            var userEntity = await _userManager.FindByIdAsync(id);
            if (userEntity == null) throw new ArgumentNullException(nameof(userEntity), "Couldn't find user with this id");

            if (claimsId == id)
            {
                bool checkPassword = await _userManager.CheckPasswordAsync(userEntity, password.OldPassword);
                if (checkPassword == false) throw new ArgumentException(nameof(password));
                else await _userManager.ChangePasswordAsync(userEntity, password.OldPassword, password.NewPassword);
            }
            else throw new NotEnoughtRightsException();
        }

        public async Task<IEnumerable<BlogDto>> GetAllBlogsByUserId(string id)
        {
            var userEntity = await _userManager.FindByIdAsync(id);
            if (userEntity == null) throw new ArgumentNullException(nameof(userEntity), "Couldn't find user with this id");
            var blogs = _unitOfWork.BlogRepository.Get(b => b.OwnerId == id);
            return _mapper.Map<IEnumerable<BlogDto>>(blogs);
        }

        public async Task<IEnumerable<CommentDto>> GetAllCommentsByUserId(string id)
        {
            if (id == null) throw new ArgumentNullException();
            var userEntity = await _userManager.FindByIdAsync(id);
            if (userEntity == null) throw new ArgumentNullException(nameof(userEntity), "Couldn't find user with this id");
            var comments = _unitOfWork.CommentRepository.Get(c => c.UserId == id);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}
