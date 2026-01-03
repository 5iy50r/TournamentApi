using Data;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auth;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;

    public AuthService(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<User> Register(string firstName, string lastName, string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(x => x.Email == email);
        if (exists) throw new InvalidOperationException("Email jest już zajęty.");

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<(User user, string token)> Login(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null) throw new InvalidOperationException("Zły email lub hasło.");

        var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!ok) throw new InvalidOperationException("Zły email lub hasło.");

        var token = _tokens.CreateToken(user);
        return (user, token);
    }
}
