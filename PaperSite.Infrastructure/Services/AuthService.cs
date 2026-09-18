using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaperSite.Application.Common.Responses;
using PaperSite.Application.DTOs.Auth;
using PaperSite.Application.Interfaces;
using System.Security.Cryptography;
using PaperSite.Domain.Entities;

namespace PaperSite.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<OtpCode> _otpRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly PasswordHasher<OtpCode> _otpHasher = new();
    private readonly IJWtService _jwtService;
    private readonly ISmsService _smsService;

    public AuthService(IUnitOfWork unitOfWork, IRepository<User> userRepository, IRepository<Role> roleRepository, IJWtService jwtService,ISmsService smsService,IRepository<OtpCode> otpRepository)
    {
        _otpRepository = otpRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _smsService = smsService;
    }

    public async Task<BaseResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user == null)
        {
            return BaseResponse<AuthResponse>.Failure("ایمیل یا گذرواژه نامعتبر است");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return BaseResponse<AuthResponse>.Failure("ایمیل یا گذرواژه نامعتبر است");
        }

        var token = _jwtService.GenerateToken(user);
        return BaseResponse<AuthResponse>.Success(new AuthResponse
        {
            UserId = user.Id,
            Role = user.Role.Name,
            Token = token
        }, "ورود با موفقیت انجام شد");
    }

    public async Task<BaseResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedPhone = request.PhoneNumber.Trim();
        var existingEmail = await _userRepository.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (existingEmail != null)
        {
            return BaseResponse<AuthResponse>.Failure("ایمیل قبلاً ثبت شده است");
        }

        var existingPhone = await _userRepository.FirstOrDefaultAsync(x => x.PhoneNumber == normalizedPhone);
        if (existingPhone != null)
        {
            return BaseResponse<AuthResponse>.Failure("شماره تلفن قبلاً ثبت شده است");
        }

        var customerRole = await _roleRepository.FirstOrDefaultAsync(x => x.Name == Role.Customer);
        if (customerRole == null)
        {
            return BaseResponse<AuthResponse>.Failure("نقش مشتری یافت نشد");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = normalizedEmail,
            PhoneNumber = normalizedPhone,
            RoleId = customerRole.Id,
            Role = customerRole
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        return BaseResponse<AuthResponse>.Success(new AuthResponse
        {
            UserId = user.Id,
            Role = customerRole.Name,
            Token = token
        }, "ثبت‌نام با موفقیت انجام شد");
    }
    public async Task<BaseResponse<bool>> SendOtpAsync(string mobile)
    {
        mobile = MobileNumber.Normalize(mobile);

        if (string.IsNullOrWhiteSpace(mobile))
        {
            return BaseResponse<bool>.Failure(
                "شماره موبایل الزامی است"
            );
        }

        var now = DateTime.UtcNow;

        var code = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            PhoneNumber = mobile,
            ExpiresAt = now.AddMinutes(2)
        };

        otp.CodeHash = _otpHasher.HashPassword(otp, code);

        await _otpRepository.AddAsync(otp);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            var smsResult = await _smsService.SendOtpAsync(mobile, code);
            if (!smsResult.IsSuccess)
            {
                _otpRepository.Delete(otp);
                await _unitOfWork.SaveChangesAsync();
                return BaseResponse<bool>.Failure("ارسال پیامک ناموفق بود");
            }
        }
        catch
        {
            _otpRepository.Delete(otp);
            await _unitOfWork.SaveChangesAsync();
            return BaseResponse<bool>.Failure("ارسال پیامک ناموفق بود");
        }

        return BaseResponse<bool>.Success(
            true,
            "کد تایید ارسال شد"
        );
    }
    public async Task<BaseResponse<VerifyOtpResponse>> VerifyOtpAsync(
    string mobile,
    string code)
    {
        mobile = MobileNumber.Normalize(mobile);

        var now = DateTime.UtcNow;

        // فقط آخرین OTP معتبر را قبول می‌کنیم
        var otp = await _otpRepository.Query()
            .Where(x =>
                x.PhoneNumber == mobile &&
                x.UsedAt == null &&
                x.ExpiresAt >= now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return BaseResponse<VerifyOtpResponse>.Failure(
                "کد تایید اشتباه یا منقضی شده است"
            );
        }

        // جلوگیری از Brute Force
        if (otp.FailedAttempts >= 5)
        {
            return BaseResponse<VerifyOtpResponse>.Failure(
                "تعداد تلاش‌های ناموفق بیش از حد مجاز است. کد جدید دریافت کنید."
            );
        }

        var verifyResult =
            _otpHasher.VerifyHashedPassword(
                otp,
                otp.CodeHash,
                code
            );

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            await _otpRepository.Query()
                .Where(x => x.Id == otp.Id && x.UsedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.FailedAttempts, x => x.FailedAttempts + 1)
                    .SetProperty(x => x.UpdatedAt, now));

            return BaseResponse<VerifyOtpResponse>.Failure(
                "کد تایید اشتباه یا منقضی شده است"
            );
        }

        // مصرف اتمیک OTP؛ درخواست هم‌زمان دوم دیگر موفق نمی‌شود
        var consumed = await _otpRepository.Query()
            .Where(x => x.Id == otp.Id && x.UsedAt == null && x.FailedAttempts < 5)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.UsedAt, now)
                .SetProperty(x => x.UpdatedAt, now));

        if (consumed == 0)
        {
            return BaseResponse<VerifyOtpResponse>.Failure(
                "کد تایید قبلاً استفاده شده یا نامعتبر است"
            );
        }

        // آیا کاربر قبلاً ثبت نام کرده؟
        var user = await _userRepository.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == mobile
            );

        // کاربر قدیمی
        if (user != null)
        {
            var accessToken =
                _jwtService.GenerateToken(user);

            return BaseResponse<VerifyOtpResponse>.Success(
                new VerifyOtpResponse
                {
                    IsRegistered = true,
                    AccessToken = accessToken,
                    RegistrationToken = null,
                    UserId = user.Id,
                    Role = user.Role.Name,
                    RequiresProfileCompletion = false
                },
                "ورود با موفقیت انجام شد"
            );
        }

        // کاربر جدید تا قبل از تکمیل اطلاعات، Access Token دریافت نمی‌کند.
        var registrationToken = _jwtService.GenerateRegistrationToken(mobile);

        return BaseResponse<VerifyOtpResponse>.Success(
            new VerifyOtpResponse
            {
                IsRegistered = false,
                AccessToken = null,
                RegistrationToken = registrationToken,
                UserId = null,
                Role = null,
                RequiresProfileCompletion = true
            },
            "شماره موبایل تایید شد. برای ورود، تکمیل اطلاعات حساب الزامی است."
        );
    }
    public async Task<BaseResponse<VerifyOtpResponse>>
    
        
        CompleteRegistrationAsync(
        CompleteRegistrationRequest request)
    {
        // شماره موبایل فقط از توکن استخراج می‌شود
        var mobile =
            _jwtService.ValidateRegistrationToken(
                request.RegistrationToken
            );

        if (string.IsNullOrWhiteSpace(mobile))
        {
            return BaseResponse<VerifyOtpResponse>.Failure(
                "توکن ثبت‌نام نامعتبر یا منقضی شده است. دوباره کد تایید دریافت کنید."
            );
        }

        // نباید قبلاً User ساخته شده باشد
        var existingUser = await _userRepository
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == mobile
            );

        if (existingUser != null)
        {
            return BaseResponse<VerifyOtpResponse>.Failure(
                "این شماره قبلاً ثبت‌نام شده است. دوباره وارد شوید."
            );
        }

        string email;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            email = request.Email.Trim().ToLowerInvariant();

            var existingEmail =
                await _userRepository.FirstOrDefaultAsync(
                    x => x.Email == email
                );

            if (existingEmail != null)
            {
                return BaseResponse<VerifyOtpResponse>.Failure(
                    "این ایمیل قبلاً استفاده شده است."
                );
            }
        }
        else
        {
            // چون ساختار فعلی دیتابیس Email را Required کرده
            // فعلاً یک Email داخلی و Unique می‌سازیم
            email = $"{mobile}@customer.invalid";
        }

        var customerRole =
            await _roleRepository.FirstOrDefaultAsync(
                x => x.Name == Role.Customer
            );

        if (customerRole == null)
        {
            return BaseResponse<VerifyOtpResponse>.Failure(
                "نقش مشتری در سیستم یافت نشد."
            );
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            FirstName = request.FirstName.Trim(),

            LastName = request.LastName.Trim(),

            // Username دیگر از کاربر گرفته نمی‌شود
            UserName = mobile,

            Email = email,

            PhoneNumber = mobile,

            RoleId = customerRole.Id,

            Role = customerRole
        };

        // Customer اصلاً این Password را نمی‌داند.
        // فقط برای سازگاری با ساختار فعلی User ساخته می‌شود.
        var internalPassword =
            Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32)
            );

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                internalPassword
            );

        await _userRepository.AddAsync(user);

        await _unitOfWork.SaveChangesAsync();

        var accessToken =
            _jwtService.GenerateToken(user);

        return BaseResponse<VerifyOtpResponse>.Success(
            new VerifyOtpResponse
            {
                IsRegistered = true,
                AccessToken = accessToken,
                RegistrationToken = null,
                UserId = user.Id,
                Role = customerRole.Name,
                RequiresProfileCompletion = false
            },
            "ثبت‌نام با موفقیت انجام شد"
        );
    }
}
