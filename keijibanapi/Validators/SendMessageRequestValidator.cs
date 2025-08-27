// ===============================================
// Validators/SendMessageRequestValidator.cs
// ===============================================
using FluentValidation;
using keijibanapi.Models;

namespace keijibanapi.Validators
{
    public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
    {
        public SendMessageRequestValidator()
        {
            // "Subject"プロパティは空であってはならない。空の場合はエラーメッセージを返す。
            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("件名は必須入力です。")
                .MaximumLength(100).WithMessage("件名は100文字以内で入力してください。");

            // "ToDeptIds"リストは空であってはならない。
            RuleFor(x => x.ToDeptIds)
                .NotEmpty().WithMessage("宛先部署を最低一つは選択してください。");
        }
    }
}
