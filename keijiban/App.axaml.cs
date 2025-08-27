using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using keijiban.Configuration;
using keijiban.Services;
using keijiban.ViewModels;
using keijiban.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace keijiban
{
    public partial class App : Application
    {
        /// <summary>
        /// DIコンテナのサービスプロバイダー。
        /// アプリケーション全体で依存関係を解決するために使用します。
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // DIコンテナのセットアップ
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // DIコンテナから MainViewModel のインスタンスを生成
                var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();

                // MainWindow を生成し、DataContext に ViewModel を設定
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };

                var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
                logger.LogInformation("Application main window created and assigned DataContext.");
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // --- 構成(Configuration)のセットアップ ---
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // --- ログ(Logging)のセットアップ ---
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            // --- 設定クラス(Options)のセットアップ ---
            // appsettings.jsonの"ApiSettings"セクションをApiSettingsクラスに紐付け
            services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));

            // --- サービス(Services)の登録 ---
            // HttpClientFactoryの登録 (IHttpClientFactory を使えるようにする)
            services.AddHttpClient();

            // シングルトン: アプリケーション中で常に単一のインスタンスを共有
            services.AddSingleton<IApiService, ApiService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISignalRService, SignalRService>();

            // --- ViewModelの登録 ---
            // 推移的(Transient): 毎回新しいインスタンスを生成
            services.AddTransient<MainViewModel>();
        }
    }
}
