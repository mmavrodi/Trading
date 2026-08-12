using Trading.DataAccess;

namespace Trading.Services
{
    public class BaseService
    {
        protected readonly ITradingDbContext _dbContext;

        public BaseService(ITradingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
