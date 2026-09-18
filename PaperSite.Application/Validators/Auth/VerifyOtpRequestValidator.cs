using FluentValidation;
using PaperSite.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.Validators.Auth
{
    public class VerifyOtpRequestValidator:AbstractValidator<VerifyOtpRequest>
    {
        public VerifyOtpRequestValidator()
        {
            RuleFor(x => x.Mobile)
                .NotEmpty().WithMessage("شماره موبایل الزامی است.")
                .Matches(@"^09[0-9]{9}$").WithMessage("شماره موبایل معتبر نیست.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد تایید الزامی است.")
                .Matches(@"^\d{6}$").WithMessage("کد تایید باید ۶ رقم باشد.");
        }
    }
}
