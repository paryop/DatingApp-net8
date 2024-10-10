using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
    {
        public void AddLike(UserLike like)
        {
            context.Likes.Add(like);
        }

        public void DeleteLike(UserLike like)
        {
            context.Likes.Remove(like);
        }

        public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
        {
            return await context.Likes
                .Where(x => x.SourceUserId == currentUserId)
                .Select(x => x.TargetUserId)
                .ToListAsync();
        }

        public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
        {
             return await context.Likes
                .FindAsync(sourceUserId, targetUserId); 
        }

        public async Task<PagedList<MemberDto>> GetUserLikes(LikeParams likeParams)
        {
            var likes = context.Likes.AsQueryable();

            switch(likeParams.Predicate)
            {
                case "liked":                    
                    return await PagedList<MemberDto>.CreateAsync(likes
                        .Where(x => x.SourceUserId == likeParams.UserId)
                        .Select(x => x.TargetUser)
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider),
                            likeParams.PageNumber, likeParams.PageSize);
                case "likedBy":
                    return await PagedList<MemberDto>.CreateAsync(likes
                        .Where(x => x.TargetUserId == likeParams.UserId)
                        .Select(x => x.SourceUser)
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider),
                            likeParams.PageNumber, likeParams.PageSize);
                default:
                    var likeIds = await GetCurrentUserLikeIds(likeParams.UserId);

                    return await PagedList<MemberDto>.CreateAsync(likes
                        .Where(x => x.TargetUserId == likeParams.UserId && likeIds.Contains(likeParams.UserId))
                        .Select(x => x.SourceUser)
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider),
                            likeParams.PageNumber, likeParams.PageSize);
            }
        }

        public async Task<bool> SaveChanges()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}