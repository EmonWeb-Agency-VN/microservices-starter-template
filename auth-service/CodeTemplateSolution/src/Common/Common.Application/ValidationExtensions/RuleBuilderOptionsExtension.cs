using Common.Application.Errors;
using Common.Domain.Filters;
using Common.SharedKernel;
using Common.SharedKernel.Errors;
using FluentValidation;

namespace Common.Application.ValidationExtensions
{
    public static class RuleBuilderOptionsExtension
    {
        public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Error error) =>
            rule.WithErrorCode(error.Code).WithMessage(error.Message);

        public static IRuleBuilderOptions<T, TProperty> ApplyCurrentValidatorWhen<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Func<T, bool> predicate, ApplyConditionTo condition = ApplyConditionTo.CurrentValidator) =>
            rule.When(predicate, condition);
        public static IRuleBuilderOptions<T, TProperty> ApplyCurrentValidatorWhenAsync<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Func<T, CancellationToken, Task<bool>> predicate, ApplyConditionTo condition = ApplyConditionTo.CurrentValidator) =>
            rule.WhenAsync(predicate, condition);

        public static IRuleBuilderOptions<T, TProperty> ValidateFilterModel<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule) where TProperty : FilterModel =>
            rule.Must(a =>
            {
                if (a.PageIndex < 0 || a.PageSize <= 0) return false;
                return true;
            }).WithError(CommonErrors.InvalidFilterModel);

        public static IRuleBuilderOptions<T, string> NotContainSpecialCharacter<T>(this IRuleBuilderOptions<T, string> rule) =>
            rule.Matches("^[a-zA-Z0-9]+$").WithError(CommonErrors.NotContainSpecialCharacters);

        public static IRuleBuilderOptions<T, string> MatchVietnameseName<T>(this IRuleBuilderOptions<T, string> rule) =>
            rule.Matches(Constants.VietnameseNameRegexPattern).WithError(CommonErrors.NotMatchVietnameseName);

        public static IRuleBuilderOptions<T, string> CouldBeVietnameseWord<T>(this IRuleBuilderOptions<T, string> rule) =>
            rule.Matches(Constants.VietnameseWordRegexPattern).WithError(CommonErrors.NotMatchVietnameseName);
    }

    public static class RuleBuilderInitialExtension
    {
        public static IRuleBuilderInitial<T, TProperty> StopCascadeIfFailed<T, TProperty>(this IRuleBuilderInitial<T, TProperty> rule) =>
            rule.Cascade(CascadeMode.Stop);
    }

}
