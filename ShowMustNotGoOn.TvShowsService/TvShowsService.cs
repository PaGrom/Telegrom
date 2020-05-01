using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot.Types;

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

        public async Task<TvShow> AddNewTvShowAsync(TvShow tvShow, CancellationToken cancellationToken)
        {
            TvShow show;
            var existingShow = await _dbContext.TvShows
                .FindAsync(new object[] { tvShow.Id }, cancellationToken);
            if (existingShow != null)
            {
                _logger.LogInformation($"TV Show '{existingShow.Title}' (Id: {existingShow.Id}) already exists in db");
                show = existingShow;
            }
            else
            {
                _logger.LogInformation($"Adding TV Show '{tvShow.Title}' (Id: {tvShow.Id}) to db");
                show = (await _dbContext.TvShows.AddAsync(tvShow, cancellationToken)).Entity;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return show;
        }

        public Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name, CancellationToken cancellationToken)
        {
            return _myShowsService.SearchTvShowsAsync(name);
        }

        public Task<TvShow> GetTvShowFromMyShowsAsync(int myShowsId, CancellationToken cancellationToken)
        {
            return _myShowsService.GetTvShowAsync(myShowsId);
        }

        public async Task<TvShow> GetTvShowAsync(int tvShowId, CancellationToken cancellationToken)
        {
            return await _dbContext.TvShows.FindAsync(new object[] { tvShowId }, cancellationToken);
        }

        public async Task<TvShow> GetTvShowByMyShowsIdAsync(int myShowsId, CancellationToken cancellationToken)
        {
            return await _dbContext.TvShows.FindAsync(new object[] { myShowsId }, cancellationToken);
        }

        public Task<Subscription> GetUserSubscriptionToTvShowAsync(User user, TvShow show, SubscriptionType subscriptionType, CancellationToken cancellationToken)
        {
            return _dbContext.Subscriptions
                .SingleOrDefaultAsync(s => s.UserId == user.Id
                               && s.TvShowId == show.Id
                               && s.SubscriptionType == subscriptionType,
                    cancellationToken: cancellationToken);
        }

        public async Task<Subscription> SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType, CancellationToken cancellationToken)
        {
            tvShow = await AddNewTvShowAsync(tvShow, cancellationToken);

            var existedSubscription = await GetUserSubscriptionToTvShowAsync(user, tvShow, subscriptionType, cancellationToken);

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
                }, cancellationToken)).Entity;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"User {user.Id} successfully subscribed to show {tvShow.Id} with subscription type {subscriptionType}");

            return subscription;
        }

        public async Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType, CancellationToken cancellationToken)
        {
            var existedSubscription = await GetUserSubscriptionToTvShowAsync(user, tvShow, subscriptionType, cancellationToken);

            if (existedSubscription == null)
            {
                _logger.LogInformation($"User {user.Id} not subscribed to show {tvShow.Id} with subscription type {subscriptionType}");
                return;
            }

            _dbContext.Subscriptions.Remove(existedSubscription);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"User {user.Id} successfully unsubscribed to show {tvShow.Id} with subscription type {subscriptionType}");
        }

        public Task<List<Subscription>> GetUserSubscriptionsAsync(User user, CancellationToken cancellationToken)
        {
            return _dbContext.Subscriptions.Where(s => s.UserId == user.Id).ToListAsync(cancellationToken);
        }
    }
}
