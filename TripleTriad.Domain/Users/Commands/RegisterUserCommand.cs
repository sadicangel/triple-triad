using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace TripleTriad.Users.Commands;

public sealed class RegisterUserCommand : IRequest<IResult>
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    public sealed class Validator : AbstractValidator<RegisterUserCommand>
    {
        public Validator()
        {
            RuleFor(e => e.Username).NotEmpty();
            RuleFor(e => e.Password).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<RegisterUserCommand, IResult>
    {
        private readonly UserManager<User> _userManager;

        public Handler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = request.Username,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.Conflict(result.Errors.Select(e => e.Description));

            var token = await _userManager.GenerateUserTokenAsync(user, "Default", "AccessToken");
            return Results.Ok(token);
        }
    }
}
