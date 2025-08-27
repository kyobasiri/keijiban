// ========================================================
// Validators/UpdateDepartmentMasterRequestValidator.cs
// ========================================================
using FluentValidation;
using keijibanapi.Models;

namespace keijibanapi.Validators
{
    public class UpdateDepartmentMasterRequestValidator : AbstractValidator<UpdateDepartmentMasterRequest>
    {
        public UpdateDepartmentMasterRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("IDは必須です。");

        }
    }
}
