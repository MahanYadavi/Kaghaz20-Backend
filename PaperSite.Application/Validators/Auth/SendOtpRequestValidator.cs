using FluentValidation;
using PaperSite.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaperSite.Application.Validators.Auth
{
    public class SendOtpRequestValidator:AbstractValidator<SendOtpRequest>
    {
        public SendOtpRequestValidator()
        {
            RuleFor(x => x.mobileNo)
                .NotEmpty().WithMessage("شماره موبایل الزامی است.")
                .Matches(@"^09[0-9]{9}$").WithMessage("شماره موبایل معتبر نیست.");
        }
    }
}
