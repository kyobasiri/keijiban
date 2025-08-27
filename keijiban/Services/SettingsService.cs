using keijiban.Configuration;
using keijiban.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace keijiban.Services
{
    /// <summary>
    /// ユーザーの選択設定をJSONファイルとして永続化および取得するサービスの実装。
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly ILogger<SettingsService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public SettingsService(ILogger<SettingsService> logger)
        {
            _logger = logger;
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(appDataPath, "Keijiban");
                Directory.CreateDirectory(appFolder); // 存在しない場合は作成
                _settingsPath = Path.Combine(appFolder, "settings.json");
                _logger.LogInformation("Settings file path: {SettingsPath}", _settingsPath);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to create or access the settings directory.");
                // パスが取得できない場合、フォールバックとしてカレントディレクトリを使う
                _settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                _logger.LogWarning("Falling back to current directory for settings file: {SettingsPath}", _settingsPath);
            }
        }

        public async Task<int> GetSelectedGroupIdAsync()
        {
            var settings = await LoadSettingsAsync();
            return settings.SelectedGroupId ?? AppConstants.Defaults.ScheduleGroupId;
        }

        public async Task<int> GetSelectedDepartmentIdAsync()
        {
            var settings = await LoadSettingsAsync();
            // DepartmentのデフォルトIDは仕様として明確でないため、1を仮定。必要に応じてAppConstantsに追加。
            return settings.SelectedDepartmentId ?? 1;
        }

        public async Task<int> GetSelectedInfoTypeIdAsync()
        {
            var settings = await LoadSettingsAsync();
            return settings.SelectedInfoTypeId ?? AppConstants.Defaults.InfoTypeId;
        }

        public async Task SaveSelectedGroupIdAsync(int? groupId)
        {
            if (groupId == null) return;
            await SaveSettingAsync(settings => settings.SelectedGroupId = groupId);
            _logger.LogDebug("Saved SelectedGroupId: {GroupId}", groupId);
        }

        public async Task SaveSelectedDepartmentIdAsync(int? departmentId)
        {
            if (departmentId == null) return;
            await SaveSettingAsync(settings => settings.SelectedDepartmentId = departmentId);
            _logger.LogDebug("Saved SelectedDepartmentId: {DepartmentId}", departmentId);
        }

        public async Task SaveSelectedInfoTypeIdAsync(int infoTypeId)
        {
            await SaveSettingAsync(settings => settings.SelectedInfoTypeId = infoTypeId);
            _logger.LogDebug("Saved SelectedInfoTypeId: {InfoTypeId}", infoTypeId);
        }

        /// <summary>
        /// 設定ファイルを非同期に読み込みます。
        /// </summary>
        private async Task<AppSettings> LoadSettingsAsync()
        {
            if (!File.Exists(_settingsPath))
            {
                _logger.LogInformation("Settings file not found. Returning default settings.");
                return new AppSettings();
            }

            try
            {
                using var stream = File.OpenRead(_settingsPath);
                var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _jsonOptions);
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load or deserialize settings file. Returning default settings.");
                return new AppSettings(); // 読み込み失敗時はデフォルトを返す
            }
        }

        /// <summary>
        /// 指定されたアクションで設定を更新し、非同期にファイルに保存する共通メソッド。
        /// </summary>
        private async Task SaveSettingAsync(Action<AppSettings> updateAction)
        {
            try
            {
                var settings = await LoadSettingsAsync();
                updateAction(settings);

                using var stream = File.Create(_settingsPath);
                await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save settings file.");
            }
        }
    }
}
