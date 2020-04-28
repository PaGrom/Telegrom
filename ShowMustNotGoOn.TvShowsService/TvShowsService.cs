using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.TvShowsService
{
    public class TvShowsService : ITvShowsService
    {
        private readonly IMyShowsService _myShowsService;
        private readonly DatabaseContext.DatabaseContext _dbContext;
        private readonly ILogger<TvShowsService> _logger;

        public TvShowsService(IMyShowsService myShowsService,
            DatabaseContext.DatabaseContext dbContext,
            ILogger<TvShowsService> logger)
        {
            _myShowsService = myShowsService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<TvShow> AddNewTvShowAsync(TvShow tvShow)
        {
            TvShow show;
            var existingShow = await _dbContext.TvShows.SingleOrDefaultAsync(s => s.MyShowsId == tvShow.MyShowsId);
            if (existingShow != null)
            {
                _logger.LogInformation($"TV Show '{existingShow.Title}' (Id: {existingShow.MyShowsId}) already exists in db");
                show = existingShow;
            }
            else
            {
                _logger.LogInformation($"Adding TV Show '{tvShow.Title}' (Id: {tvShow.MyShowsId}) to db");
                show = (await _dbContext.TvShows.AddAsync(tvShow)).Entity;
                await _dbContext.SaveChangesAsync();
            }

            return show;
        }

        public Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name)
        {
            return _myShowsService.SearchTvShowsAsync(name);
        }

        public Task<TvShow> GetTvShowFromMyShowsAsync(int myShowsId)
        {
            return _myShowsService.GetTvShowAsync(myShowsId);
        }

        public Task<TvShow> GetTvShowAsync(int tvShowId)
        {
            return _dbContext.TvShows.SingleOrDefaultAsync(s => s.Id == tvShowId);
        }

        public Task<TvShow> GetTvShowByMyShowsIdAsync(int myShowsId)
        {
            return _dbContext.TvShows.SingleOrDefaultAsync(s => s.MyShowsId == myShowsId);
        }

        public Task<Subscription> GetUserSubscriptionToTvShowAsync(User user, TvShow show, SubscriptionType subscriptionType)
        {
            return _dbContext.Subscriptions
                .SingleOrDefaultAsync(s => s.UserId == user.Id
                               && s.TvShowId == show.Id
                               && s.SubscriptionType == subscriptionType);
        }

        public async Task<Subscription> SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
            tvShow = await AddNewTvShowAsync(tvShow);

            var existedSubscription = await GetUserSubscriptionToTvShowAsync(user, tvShow, subscriptionType);

            if (existedSubscription != null)
            {
                _logger.LogInformation($"User {user.Id} already subscribed to show {tvShow.Id} with subscription type {subscriptionType}");
                return existedSubscription;
            }

            var subscription = (await _dbContext.Subscriptions
                .AddAsync(new Subscription
                {
                    UserId = user.Id,
                    TvShowId = tvShow.Id,
                    SubscriptionType = subscriptionType
                })).Entity;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"User {user.Id} successfully subscribed to show {tvShow.Id} with subscription type {subscriptionType}");

            return subscription;
        }

        public async Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
            var existedSubscription = await GetUserSubscriptionToTvShowAsync(user, tvShow, subscriptionType);

            if (existedSubscription == null)
            {
                _logger.LogInformation($"User {user.Id} not subscribed to show {tvShow.Id} with subscription type {subscriptionType}");
                return;
            }

            _dbContext.Subscriptions.Remove(existedSubscription);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"User {user.Id} successfully unsubscribed to show {tvShow.Id} with subscription type {subscriptionType}");
        }

        public Task<List<Subscription>> GetUserSubscriptionsAsync(User user)
        {
            return _dbContext.Subscriptions.Where(s => s.UserId == user.Id).ToListAsync();
        }
    }
}
