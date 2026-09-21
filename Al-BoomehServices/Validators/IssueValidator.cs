using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class IssueValidator : AbstractValidator<IssueDTO>
    {
        public IssueValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.CustomerId)
                .GreaterThan(0);

            RuleFor(x => x.IssueId)
                .GreaterThan(0);
        }
    }

}
