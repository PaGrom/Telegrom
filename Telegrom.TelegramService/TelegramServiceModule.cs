using System.Net;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegrom.Core;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Telegrom.TelegramService;

public sealed class TelegramServiceModule : Module
{
    private const string HttpClientName = "typicode";
        
    protected override void Load(ContainerBuilder builder)
    {
        var proxy = CreateProxy();

        builder.Register(ctx =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient(HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new SocketsHttpHandler();
                    if (proxy != null)
                    {
                        handler.Proxy = proxy;
                        handler.UseProxy = true;
                    }
                    return handler;
                });

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IHttpClientFactory>();
        }).SingleInstance();
            
        builder.Register(c =>
            {
                var httpClientFactory = c.Resolve<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(HttpClientName);
                return new TelegramBotClient(TelegramOptions.Current.TelegramApiToken, httpClient);
            })
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<TelegramUpdateReceiver>()
            .As<ITelegramUpdateReceiver>()
            .SingleInstance();
    }
        
    private static WebProxy? CreateProxy()
    {
        if (IsHttpProxyConfigured())
        {
            return CreateHttpProxy();
        }

        if (IsSocks5ProxyConfigured())
        {
            return CreateSocks5Proxy();
        }

        return null;
    }

    private static bool IsHttpProxyConfigured()
    {
        return !string.IsNullOrEmpty(TelegramOptions.Current.ProxyAddress);
    }

    private static WebProxy CreateHttpProxy()
    {
        return new WebProxy(TelegramOptions.Current.ProxyAddress)
        {
            UseDefaultCredentials = true
        };
    }

    private static bool IsSocks5ProxyConfigured()
    {
        return !string.IsNullOrEmpty(TelegramOptions.Current.Socks5HostName)
            && TelegramOptions.Current.Socks5Port.HasValue;
    }

    private static WebProxy CreateSocks5Proxy()
    {
        var socks5Proxy = new WebProxy(TelegramOptions.Current.Socks5HostName,
            TelegramOptions.Current.Socks5Port.Value);

        if (IsSocks5CredentialsConfigured())
        {
            socks5Proxy.Credentials = new NetworkCredential(TelegramOptions.Current.Socks5Username,
                TelegramOptions.Current.Socks5Password);
        }

        return socks5Proxy;
    }

    private static bool IsSocks5CredentialsConfigured()
    {
        return !string.IsNullOrEmpty(TelegramOptions.Current.Socks5Username)
            && !string.IsNullOrEmpty(TelegramOptions.Current.Socks5Password);
    }
}
