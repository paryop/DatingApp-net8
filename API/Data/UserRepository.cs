using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Data
{
    public class UserRepository(DataContext context, IMapper mapper, UserManager<AppUser> userManager) : IUserRepository
    {
        public async Task<MemberDto?> GetMemberAsync(string username)
        {
            return await userManager.Users
            .Where(x=>x.NormalizedUserName == username.ToUpper())
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {

            var query = userManager.Users.AsQueryable();

            query = query.Where(u => u.NormalizedUserName != userParams.CurrentUsername!.ToUpper());

            if(userParams.Gender != null)
            {
                query = query.Where(u => u.Gender == userParams.Gender);
            }

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge-1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(a => a.DateOfBirth >= minDob && a.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x=>x.Created),
                _ => query.OrderByDescending(x => x.LastActive) 
            };
            
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), 
            userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await userManager.Users.FirstAsync(x => x.Id == id);
        }

        public async Task<AppUser?> GetUserByUserameAsync(string username)
        {
            return await userManager.Users
            .Include(p => p.Photos)            
            .SingleOrDefaultAsync<AppUser>(u => u.UserName == username);

        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await userManager.Users
            .Include(p => p.Photos)
            .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }
    }
}