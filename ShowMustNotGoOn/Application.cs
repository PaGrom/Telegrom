using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger _logger;

        public Application(ITelegramBotClient telegramBotClient,ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");
            var me = await _telegramBotClient.GetMeAsync();
            _logger.Information($"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );
        }
    }
}
