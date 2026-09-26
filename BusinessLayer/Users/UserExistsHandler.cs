using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Users;

public record UserExists(string UserId) : IRequest<bool>;
public class UserExistsHandler(DietTrackerDbContext dbContext) : IRequestHandler<UserExists, bool>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<bool> Handle(UserExists request, CancellationToken cancellationToken)
    {
        return await _dbContext
            .Users
            .AsNoTracking()
            .AnyAsync(user => user.UserId == request.UserId);
    }
}