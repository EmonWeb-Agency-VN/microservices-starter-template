using Common.Application.Messaging;
using Common.Application.ValidationExtensions;
using Common.Domain.Entities.Users;
using Common.Domain.Interfaces;
using Common.SharedKernel;
using Common.SharedKernel.Errors;
using Common.SharedKernel.Extensions;
using Common.SharedKernel.LogProvider;
using Common.SharedKernel.Utilities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using System.Reflection;

namespace Modules.Web.Application.Commands.Account
{
    public class SignUpCommand : ICommand
    {
        public string DisplayName { get; set; }
        public string? Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        //public string? Code { get; set; }
        public string? Mobile { get; set; }
        public string? Photo { get; set; }
        public string DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public RoleType RoleType { get; set; }
        public LoginType LoginType { get; set; }
    }

    public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
    {
        public SignUpCommandValidator(IDBRepository dBRepository)
        {
            RuleFor(a => a.DisplayName)
                .NotEmpty().WithError(SignUpValidationErrors.DisplayNameIsRequired)
                .CouldBeVietnameseWord()
                .MaximumLength(50).WithError(SignUpValidationErrors.DisplayNameExceedMaxLength);

            RuleFor(a => a.Email)
                .EmailAddress()
                    .ApplyCurrentValidatorWhen(a => !string.IsNullOrEmpty(a.Email))
                    .WithError(SignUpValidationErrors.EmailFormatIsInvalid)
                .MaximumLength(50)
                    .ApplyCurrentValidatorWhen(a => !string.IsNullOrEmpty(a.Email))
                    .WithError(SignUpValidationErrors.InvalidEmailMaxLength);

            RuleFor(a => a.UserName)
                .NotEmpty().WithError(SignUpValidationErrors.UserNameIsRequired)
                .Must(a => !a.Contains(' ')).WithError(SignUpValidationErrors.UserNameCannotContainWhitespace)
                .MaximumLength(50).WithError(SignUpValidationErrors.UserNameExceedsMaxLength)
                .NotContainSpecialCharacter()
                .MustAsync(async (x, cancellationToken) =>
                {
                    var hasUser = await dBRepository.Context.Set<UserEntity>()
                        .AnyAsync(a => a.UserName.ToLower() == x.ToLower(), cancellationToken: cancellationToken);
                    if (!hasUser) return true;
                    return false;
                })
                    .ApplyCurrentValidatorWhenAsync(async (a, cancellationToken) => await Task.FromResult(!string.IsNullOrEmpty(a.UserName)))
                    .WithError(SignUpValidationErrors.UserNameAlreadyExisted);

            RuleFor(a => a.Password)
                .NotEmpty().WithError(SignUpValidationErrors.PasswordIsRequired)
                .MinimumLength(8).WithError(SignUpValidationErrors.PasswordMustBeAtLeast8CharactersLong)
                .Matches("[A-Z]").WithError(SignUpValidationErrors.PasswordMustContainAtLeast1UppercaseLetter)
                .Matches("[a-z]").WithError(SignUpValidationErrors.PasswordMustContainAtLeast1LowerLetter)
                .Matches("[0-9]").WithError(SignUpValidationErrors.PasswordMustContainAtLeast1Number);

            RuleFor(a => a.Mobile)
                .MaximumLength(13)
                    .ApplyCurrentValidatorWhen(a => !string.IsNullOrEmpty(a.Mobile))
                    .WithError(SignUpValidationErrors.MobileNumberExceedsLength)
                .Matches(Constants.MobileRegexPattern)
                    .ApplyCurrentValidatorWhen(a => !string.IsNullOrEmpty(a.Mobile))
                    .WithError(SignUpValidationErrors.MobileNumberIsInvalid)
                    //.MustAsync(async (m, cancellationToken) =>
                    //{
                    //    var mobileExist = await dBRepository.Context.Set<UserEntity>().AnyAsync(a => a.Mobile == m, cancellationToken: cancellationToken);
                    //    if (!mobileExist) return true;
                    //    return false;
                    //})
                    //    .ApplyCurrentValidatorWhen(a => !string.IsNullOrEmpty(a.Mobile))
                    //    .WithError(CreateAccountValidationErrors.MobileNumberAlreadyExisted)
                    ;

            RuleFor(a => a.DateOfBirth)
                .NotNull().NotEmpty().WithError(SignUpValidationErrors.DateOfBirthIsRequired)
                .Must(a => DateTimeExtension.TryConvertDateTime(a, out _, DateFormat.Default))
                    //.ApplyCurrentValidatorWhen(a => !string.IsNullOrEmpty(a.DateOfBirth))
                    .WithError(SignUpValidationErrors.DateOfBirthFormatIsNotCorrect);
        }
    }

    internal static class SignUpValidationErrors
    {
        internal static Error UserIdMustBeEmptyByDefault => new("CreateAccountValidation.UserIdMustBeEmptyByDefault", "Member id must be empty when create account.");
        internal static Error UserIdIsRequired => new("CreateAccountValidation.UserIdIsRequired", "Member id is required.");

        internal static Error DisplayNameIsRequired => new("CreateAccountValidation.DisplayNameIsRequired", "Display name is required.");
        internal static Error DisplayNameExceedMaxLength => new("CreateAccountValidation.DisplayNameExceedMaxLength", "Display name exceeds max length.");
        internal static Error EmailFormatIsInvalid => new("CreateAccountValidation.EmailFormatIsInvalid", "The input email is not in correct format.");
        internal static Error InvalidEmailMaxLength => new("CreateAccountValidation.InvalidEmailMaxLength", "The input email exceeds max length.");
        internal static Error UserNameIsRequired => new("CreateAccountValidation.UserNameIsRequired", "Username is required.");
        internal static Error UserNameExceedsMaxLength => new("CreateAccountValidation.UserNameExceedsMaxLength", "Username exceeds max length.");
        internal static Error UserNameCannotContainWhitespace => new("CreateAccountValidation.UserNameCannotContainWhitespace", "Username cannot contain whitespaces.");
        internal static Error InvalidUserName => new("CreateAccountValidation.InvalidUserName", "Username cannot have special characters.");
        internal static Error UserNameAlreadyExisted => new("CreateAccountValidation.UserNameAlreadyExisted", "The username is already existed.");

        internal static Error PasswordIsRequired => new("CreateAccountValidation.PasswordIsRequired", "Password is required.");
        internal static Error PasswordMustBeAtLeast8CharactersLong => new("CreateAccountValidation.PasswordMustBeAtLeast8CharactersLong", "Password must be at least 8 characters long.");
        internal static Error PasswordMustContainAtLeast1UppercaseLetter => new("CreateAccountValidation.PasswordMustContainAtLeast1UppercaseLetter", "Password must contain at least one uppercase letter.");
        internal static Error PasswordMustContainAtLeast1LowerLetter => new("CreateAccountValidation.PasswordMustContainAtLeast1LowerLetter", "Password must contain at least one lowercase letter.");
        internal static Error PasswordMustContainAtLeast1Number => new("CreateAccountValidation.PasswordMustContainAtLeast1Number", "Password must contain at least one number.");

        internal static Error MobileNumberIsInvalid => new("CreateAccountValidation.MobileNumberIsInvalid", "The input mobile number is not in correct format.");
        internal static Error MobileNumberExceedsLength => new("CreateAccountValidation.MobileNumberExceedsLength", "The input mobile number exceeds max length.");
        internal static Error MobileNumberAlreadyExisted => new("CreateAccountValidation.MobileNumberAlreadyExisted", "The mobile number is already existed.");

        internal static Error DateOfBirthIsRequired => new("CreateAccountValidation.DateOfBirthIsRequired", "Date of birth is required.");
        internal static Error DateOfBirthFormatIsNotCorrect => new("CreateAccountValidation.DateOfBirthFormatIsNotCorrect", "Date of birth format is not correct.");

        internal static Error AddressIsRequired => new("CreateAccountValidation.AddressIsRequired", "Address is required.");
        internal static Error GenderIsNotDefined => new("CreateAccountValidation.GenderIsNotDefined", "Current gender is not defined.");

        internal static Error RoleTypeIsNotDefined => new("CreateAccountValidation.RoleTypeIsNotDefined", "Current role type is not defined.");
        internal static Error RoleTypeCannotBeUsed => new("CreateAccountValidation.RoleTypeCannotBeUsed", "Current role type contains inappropriate type.");

        internal static Error LoginTypeIsNotDefined => new("CreateAccountValidation.LoginTypeIsNotDefined", "Login type is not defined.");
        internal static Error ParentInvalidLoginType => new("CreateAccountValidation.ParentInvalidLoginType", "Parent cannot log in via web.");
    }

    public class CreateAccountCommandHandler(IDBRepository dBRepository, IUserService userService, ISender sender) : ICommandHandler<SignUpCommand>
    {
        private readonly string _delimiter = " - ";
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public async Task<Result> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newUser = new UserEntity
                {
                    DisplayName = request.DisplayName,
                    Email = string.IsNullOrEmpty(request.Email) ? string.Empty : request.Email,
                    UserName = request.UserName,
                    Mobile = string.IsNullOrEmpty(request.Mobile) ? string.Empty : request.Mobile,
                    Gender = request.Gender,
                    RoleType = request.RoleType,
                    UserType = request.RoleType.ToUserType(),
                    Password = PasswordUtils.HashPassword(request.Password, out var userSalt),
                    PasswordSalt = userSalt,
                    LoginType = request.LoginType,
                    UserStatus = UserStatus.Active,
                    CreatedOn = DateTimeOffset.UtcNow,
                };
                await dBRepository.AddAsync(newUser);

                await dBRepository.SaveChangesAsync(cancellationToken: cancellationToken);
                var listRoleIds = UserRolePermissionUtils.GetRoleIdsFromRoleType(request.RoleType, request.RoleType.ToUserType());

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[CreateAccountCommand] Error running command. Message: {ex.Message}");
                throw;
            }
        }
    }

    internal static class CreateAccountErrors
    {
        internal static Error CannotFindRoleAssociatedWithUser => new("CreateAccount.CannotFindRoleAssociatedWithUser", "Cannot find role associated with user.");
    }
}
