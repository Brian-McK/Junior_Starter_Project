﻿using API.Models;

namespace API.Interfaces;

public interface IRefreshTokensRepository
{
    Task AddAsync(RefreshTokenStore refreshToken);
    Task<bool> UpdateAsync(RefreshTokenStore refreshToken);
    Task DeleteAsync(string id);
    Task<bool> RefreshTokenExists(string id);
}