// Utils/LocalStorageHelper.cs
using System;
using System.IO;
using System.Text.Json;

namespace MessageManager.Utils
{
    public static class LocalStorageHelper
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MessageManager",
            "settings.json"
        );

        private static readonly object _lock = new object();

        public static void SaveSelectedDepartment(int? departmentId)
        {
            if (departmentId == null)
            {
                Console.WriteLine($"部署設定の保存に失敗しました: 部署idがnull値");
                return;
            }
            try
            {
                lock (_lock)
                {
                    var settings = LoadSettings();
                    settings.SelectedDepartmentId = departmentId;
                    SaveSettings(settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"部署設定の保存に失敗しました: {ex.Message}");
            }
        }

        public static int? LoadSelectedDepartment()
        {
            try
            {
                lock (_lock)
                {
                    var settings = LoadSettings();
                    return settings.SelectedDepartmentId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"部署設定の読み込みに失敗しました: {ex.Message}");
                return null;
            }
        }

        private static AppSettings LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            try
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        private static void SaveSettings(AppSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定ファイルの保存に失敗しました: {ex.Message}");
            }
        }

        private class AppSettings
        {
            public int? SelectedDepartmentId { get; set; }
        }
    }
}
