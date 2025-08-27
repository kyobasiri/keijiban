// MessageManager/App.axaml.cs

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using MessageManager.Configuration;
using MessageManager.Services;
using MessageManager.ViewModels;
using MessageManager.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.Extensions.Options; // ★ IOptionsのために追加


namespace MessageManager
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ConfigureServices();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // トップページから起動する
                desktop.MainWindow = new TopWindow();
            }

            base.OnFrameworkInitializationCompleted();
            
        }

        private void ConfigureServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var services = new ServiceCollection();

            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            services.AddSingleton<IConfiguration>(configuration);

            services.AddLogging(logging =>
            {
                logging.AddConfiguration(configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            });

            // ★ 変更点: IHttpClientFactoryをDIコンテナに登録
            services.AddHttpClient();

            // ★ 変更点: APIサービスをHttpClientと共に登録
            // これにより、各サービスはDIから最適化されたHttpClientインスタンスを受け取れるようになります。
            services.AddSingleton<IMessageApiService, MessageApiService>();
            services.AddSingleton<IEmergencyNoticeApiService, EmergencyNoticeApiService>();

            // ★ 新規追加: Window管理サービスを登録
            services.AddSingleton<IWindowManager, WindowManager>();

            // ViewModelも登録
            services.AddTransient<MainViewModel>();
            services.AddTransient<EmergencyNoticeViewModel>(); // ★ 新規追加
            // ★ 今後の改善で使用するため、他のViewModelも登録しておきます。
            services.AddTransient<TopViewModel>();
            services.AddTransient<DepartmentMaintenanceViewModel>();

            // ★ 新規追加: View(Window)もDIコンテナに登録
            services.AddTransient<TopWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<EmergencyNoticeWindow>();
            services.AddTransient<DepartmentMaintenanceWindow>();
            services.AddTransient<NewMessageViewModel>();
            services.AddTransient<MessageListViewModel>();

            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

            ServiceProvider = services.BuildServiceProvider();

            // 起動時ログ
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            logger.LogInformation($"MessageManager started: {AppConfig.GetConfigSummary()}");
        }
    }
}
