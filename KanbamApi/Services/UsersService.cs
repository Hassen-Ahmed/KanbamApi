using KanbamApi.Data.Interfaces;
using KanbamApi.Dtos.Update;
using KanbamApi.Models;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.Interfaces;
using MongoDB.Driver;

namespace KanbamApi.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepo _usersRepo;

    private readonly IKanbamDbContext _kanbamDbContext;

    public UsersService(IUsersRepo usersRepo, IKanbamDbContext kanbamDbContext)
    {
        _usersRepo = usersRepo;
        _kanbamDbContext = kanbamDbContext;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _usersRepo.GetAll();
    }

    public async Task<List<User>> GetByIdAsync(string id)
    {
        return await _usersRepo.GetById(id);
    }

    public Task<string?> GetUsernameByIdAsync(string id)
    {
        return _usersRepo.GetUsernameById(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _usersRepo.GetUserByEmail(email);
    }

    public async Task<string?> CreateAsync(User newUser)
    {
        try
        {
            return await _usersRepo.Create(newUser);
        }
        catch (MongoWriteException ex)
            when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            Console.WriteLine("Duplicate key error: " + ex.Message);
            return null;
        }
        catch (MongoWriteException ex)
        {
            Console.WriteLine("Write error: " + ex.Message);
            return null;
        }
        catch (MongoConnectionException ex)
        {
            Console.WriteLine("Connection error: " + ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unknown error occurred: " + ex.Message);
            return null;
        }
    }

    public async Task<bool> PatchByIdAsync(string userId, DtoUsersUpdate updateUser)
    {
        User user = new() { Email = updateUser.Email, UserName = updateUser.UserName, };
        return await _usersRepo.Patch(userId, user);
    }

    // public async Task<bool> RemoveById(string id)
    // {
    //     return await _UserRepo.RemoveById(id);
    // }
}
