using System;
using System.IO;
using System.Text.Json;

namespace CollectAdvice.Services
{
	/// <summary>
	/// 設定の保存・読み込みを行うサービスクラス
	/// </summary>
	public class SettingsService
	{
		private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AdviceBot");
		private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

		/// <summary>
		/// 設定をJSONファイルから読み込む
		/// </summary>
		/// <returns>読み込んだ設定情報</returns>
		public DateSettings LoadSettings()
		{
			if (!File.Exists(SettingsFilePath))
			{
				return new DateSettings { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now };
			}

			var json = File.ReadAllText(SettingsFilePath);
			return JsonSerializer.Deserialize<DateSettings>(json) ?? new DateSettings();
		}

		/// <summary>
		/// 設定をJSONファイルに保存する
		/// </summary>
		/// <param name="settings">保存する設定情報</param>
		public void SaveSettings(DateSettings settings)
		{
			EnsureSettingsDirectoryExists();
			var json = JsonSerializer.Serialize(settings);
			File.WriteAllText(SettingsFilePath, json);
		}

		/// <summary>
		/// Slackのメッセージ取得対象の開始日を読み込み
		/// </summary>
		internal DateTime? LoadStartDate()
		{
			var settings = LoadSettings();
			return settings?.StartDate;
		}

		/// <summary>
		/// Slackのメッセージ取得対象の終了日を読み込み
		/// </summary>
		internal DateTime? LoadEndDate()
		{
			var settings = LoadSettings();
			return settings?.EndDate;
		}

		/// <summary>
		/// 設定をJSONファイルに保存する
		/// </summary>
		/// <param name="settings">保存する設定情報</param>
		public void Save(DateSettings settings)
		{
			EnsureSettingsDirectoryExists();
			var json = JsonSerializer.Serialize(settings);
			File.WriteAllText(SettingsFilePath, json);
		}

		/// <summary>
		/// 設定ファイルのディレクトリを作成する
		/// </summary>
		private void EnsureSettingsDirectoryExists()
		{
			if (!Directory.Exists(SettingsDirectory))
			{
				Directory.CreateDirectory(SettingsDirectory);
			}
		}
	}
}
