using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace TripleTriad.Users.Commands;

public sealed class LoginUserCommand : IRequest<IResult>
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    public sealed class Validator : AbstractValidator<LoginUserCommand>
    {
        public Validator()
        {
            RuleFor(e => e.Username).NotEmpty();
            RuleFor(e => e.Password).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<LoginUserCommand, IResult>
    {
        private readonly UserManager<User> _userManager;

        public Handler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByNameAsync(request.Username) is not User user)
                return Results.Unauthorized();

            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValidPassword)
                return Results.Unauthorized();


            var token = await _userManager.GenerateUserTokenAsync(user, "Default", "AccessToken");
            return Results.Ok(token);
        }
    }
}
