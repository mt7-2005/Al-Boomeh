using Al_BoomehDAL.Classes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Validators
{
    public class DriverIssueValidator : AbstractValidator<DriverIssueDTO>
    {
        public DriverIssueValidator()
        {
            RuleFor(x => x.DriverId)
                .GreaterThan(0);

            RuleFor(x => x.IssueId)
                .GreaterThan(0);
        }
    }
}
