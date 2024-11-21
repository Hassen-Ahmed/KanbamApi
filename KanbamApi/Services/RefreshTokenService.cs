using FluentValidation;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util;

namespace KanbamApi.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepo _refreshTokenRepo;
    private readonly IValidator<RefreshToken> _refreshTokenValidator;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IRefreshTokenRepo refreshTokenRepo,
        IValidator<RefreshToken> refreshTokenValidator,
        ILogger<RefreshTokenService> logger
    )
    {
        _refreshTokenRepo = refreshTokenRepo;
        _refreshTokenValidator = refreshTokenValidator;
        _logger = logger;
    }

    public async Task<Result<RefreshToken>> GetRefreshTokensByTokenAsync(Guid token)
    {
        if (string.IsNullOrEmpty(token.ToString()))
        {
            _logger.LogWarning($"Invalid token input: {token}", token);
            var err = new Error(400, "Token cannot be null or empty.");
            return Result<RefreshToken>.Failure(err);
        }

        return await _refreshTokenRepo.GetRefreshTokensByToken(token);
    }

    public async Task<Result<bool>> SaveRefreshTokenAsync(
        string userId,
        Guid refreshToken,
        DateTime expirationDate
    )
    {
        var refreshTokenNew = new RefreshToken()
        {
            UserId = userId,
            Token = refreshToken,
            TokenExpiryTime = expirationDate,
        };

        var result = await _refreshTokenValidator.ValidateAsync(refreshTokenNew);

        if (!result.IsValid)
        {
            var ErrorMessage = result.Errors.Select(e => e.ErrorMessage).ToList()[0];

            _logger.LogWarning($"Validation failed: {result.Errors}", ErrorMessage);

            var err = new Error(400, $"Validation failed: {ErrorMessage}");
            return Result<bool>.Failure(err);
        }

        return await _refreshTokenRepo.SaveRefreshToken(refreshTokenNew);
    }

    public async Task<Result<bool>> Update_RefreshToken_ById_Async(string id, Guid newToken)
    {
        return await _refreshTokenRepo.Update_RefreshToken_ById(id, newToken);
    }

    public async Task<Result<bool>> DeleteRefreshTokenAsync(Guid token)
    {
        if (string.IsNullOrEmpty(token.ToString()))
        {
            _logger.LogWarning($"Invalid token input: {token}", token);
            var err = new Error(400, "Token cannot be null or empty.");
            return Result<bool>.Failure(err);
        }

        return await _refreshTokenRepo.DeleteRefreshToken(token);
    }
}
