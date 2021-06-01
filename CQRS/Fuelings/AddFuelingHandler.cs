using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Fuelings
{
    public record AddFueling(string Name) : IRequest<Fueling>;

    public class AddFuelingHandler : IRequestHandler<AddFueling, Fueling>
    {
        private readonly DietTrackerDbContext _dbContext;
        public AddFuelingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Fueling> Handle(AddFueling request, CancellationToken cancellationToken)
        {
            var data = _dbContext.Fuelings
                .Add(new Fueling
                {
                    Name = request.Name
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return data.Entity;
        }
    }
}