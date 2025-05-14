using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using ChatClient.Services;

namespace ChatClient;

public partial class App : Application
{
    public IHost _host { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddHttpClient<IChatClientService, ChatClientService>(c =>
                    c.BaseAddress = new Uri("https://localhost:5000/"));

                services.AddSingleton<AuthorizationWindow>();
                services.AddSingleton<RegistrationWindow>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        _host.Start();

        var authWin = _host.Services.GetRequiredService<AuthorizationWindow>();
        authWin.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}


