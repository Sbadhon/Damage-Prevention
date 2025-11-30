using FluentValidation;
using TicketSvc.Api.Contracts.Tickets;

namespace TicketSvc.Api.Validators
{
    public class SubmitTicketRequestValidator : AbstractValidator<SubmitTicketRequest>
    {
        public SubmitTicketRequestValidator()
        {
            RuleFor(x => x.WorkType)
                .NotEmpty().WithMessage("WorkType is required.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.Lat)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Lon)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        }
    }
}
