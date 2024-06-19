using Application.Commands.Users.SignupUser;
using Application.Interfaces;
using Application.Mapper;
using Domain.Dtos;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Users.SigninUser
{
    public class SigninUserCommandHandler : IRequestHandler<SigninUserCommand, SigninDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;
        IPasswordHasher<SigninUserCommand> _passwordHasher;

        public SigninUserCommandHandler(IUserRepository UserRepository, TokenService TokenService, IPasswordHasher<SigninUserCommand> passwordHasher)
        {
            _userRepository = UserRepository;
            _tokenService = TokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<SigninDto> Handle(SigninUserCommand command, CancellationToken cancellationToken)
        {
            SigninDto result = new SigninDto();

            var _existing = (List<User>)await _userRepository.GetAllAsync();

            var _check = _existing.Where(x => x.Email == command.Email).FirstOrDefault();

            
            if (_check == null || _passwordHasher.VerifyHashedPassword(command, _check.Password, command.Password) == PasswordVerificationResult.Failed)
            {
                result.Status = 1;
                return result;
            }


            _check.LastSignIn = DateTime.UtcNow;
            _check.Updated = DateTime.UtcNow;

            _userRepository.Update(_check);
            _userRepository.SaveChanges();

            result.Token = _tokenService.GenerateToken(_check);
            result.User = _check;

            return result;
        }
    }
}
