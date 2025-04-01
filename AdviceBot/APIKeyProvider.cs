using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CollectAdvice.Utilities
{
    /// <summary>
    /// APIキーを管理し、取得するユーティリティクラス
    /// </summary>
    public static class ApiKeyProvider
    {
        private static readonly string m_ApiKeyFilePath = GetApiKeyFilePath();

        /// <summary>
        /// APIキーのファイルパスを取得する
        /// </summary>
        /// <returns>APIキーを保存するファイルのパス</returns>
        private static string GetApiKeyFilePath()
        {
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataDirectory, "AdviceBot", "apikeys.json");
        }

        /// <summary>
        /// 指定されたAPIキーを取得する
        /// </summary>
        /// <param name="keyName">取得するAPIキーの名前</param>
        /// <returns>APIキーの文字列</returns>
        /// <exception cref="FileNotFoundException">APIキーのファイルが見つからない場合</exception>
        /// <exception cref="ArgumentException">指定されたキーが見つからない場合</exception>
        public static string GetApiKey(string keyName)
        {
            var apiKeys = LoadApiKeys();
            if (apiKeys.TryGetValue(keyName, out string apiKey))
            {
                return apiKey;
            }
            throw new ArgumentException($"指定されたキーが見つかりません: {keyName}");
        }

        /// <summary>
        /// APIキーをすべてロードする
        /// </summary>
        /// <returns>APIキーの辞書</returns>
        public static Dictionary<string, string> LoadApiKeys()
        {
            if (!File.Exists(m_ApiKeyFilePath))
            {
                CreateApiKeyFile();
            }

            var jsonString = File.ReadAllText(m_ApiKeyFilePath);
            try
            {
                var apiKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                return apiKeys ?? new Dictionary<string, string>();
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("APIキーのファイルの形式が不正です。", ex);
            }
        }

        /// <summary>
        /// APIキーを保存する
        /// </summary>
        /// <param name="apiKeys">APIキーの辞書</param>
        public static void SaveApiKeys(Dictionary<string, string> apiKeys)
        {
            var jsonString = JsonSerializer.Serialize(apiKeys, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(m_ApiKeyFilePath, jsonString);
        }

        /// <summary>
        /// APIキーのファイルが存在しない場合、新しく作成する
        /// </summary>
        private static void CreateApiKeyFile()
        {
            string directory = Path.GetDirectoryName(m_ApiKeyFilePath) ?? string.Empty;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var initialApiKeys = new Dictionary<string, string>
            {
                { "SlackAPIKey", "your-slack-api-key" },
                { "SlackChannelID", "your-slack-channel-id" },
                { "GeminiAPIKey", "your-gemini-api-key" }
            };

            SaveApiKeys(initialApiKeys);
        }
    }
}
