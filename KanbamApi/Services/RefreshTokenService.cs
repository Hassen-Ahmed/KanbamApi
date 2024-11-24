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
        // preliminary validation
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("UserId is invalid.");
            return Result<bool>.Failure(new Error(400, "UserId is required."));
        }

        if (refreshToken == default(Guid))
        {
            _logger.LogWarning("RefreshToken is invalid.");
            return Result<bool>.Failure(new Error(400, "Token must be a valid GUID."));
        }

        var refreshTokenNew = new RefreshToken()
        {
            UserId = userId,
            Token = refreshToken,
            TokenExpiryTime = expirationDate,
        };

        var validationResult = await _refreshTokenValidator.ValidateAsync(refreshTokenNew);

        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join(
                "; ",
                validationResult.Errors.Select(e => e.ErrorMessage)
            );
            _logger.LogWarning("Validation failed: {Errors}", errorMessages);
            _logger.LogWarning($"UserId failed: {userId}", errorMessages);
            _logger.LogWarning($"Token failed: {refreshToken}", errorMessages);

            var err = new Error(400, $"Validation failed: {errorMessages}");
            return Result<bool>.Failure(err);
        }

        return await _refreshTokenRepo.SaveRefreshToken(refreshTokenNew);
    }

    public async Task<Result<bool>> UpdateRefreshTokenByIdAsync(string id, Guid newToken)
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

    public async Task<Result<bool>> DeleteRefreshTokenByUserIdAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning($"Invalid userId : {userId}", userId);

            var err = new Error(400, "userId cannot be null or empty.");
            return Result<bool>.Failure(err);
        }

        return await _refreshTokenRepo.DeleteRefreshTokenByUserId(userId);
    }
}
