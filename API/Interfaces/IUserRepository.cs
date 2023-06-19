﻿using API.Models;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    
    Task<User> GetByIdAsync(string id);
    
    Task AddAsync(User user);
    
    Task<bool> UpdateAsync(User user);
    
    Task<bool> DeleteAsync(string id);
}