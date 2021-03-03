using Domain;
using FluentValidation;

namespace Application.Activities
{
    public class ActivityValidator : AbstractValidator<Activity>
    {
        public ActivityValidator()
        {
            RuleFor(_ => _.Title).NotEmpty();
            RuleFor(_ => _.Category).NotEmpty();
            RuleFor(_ => _.City).NotEmpty();
            RuleFor(_ => _.Date).NotEmpty();
            RuleFor(_ => _.Description).NotEmpty();
            RuleFor(_ => _.Venue).NotEmpty();
        }
    }
}