using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CollectAdvice
{
    /// <summary>
    /// SlackAPIとの通信を行うクラス
    /// </summary>
    public class SlackAPIHelper
    {
        #region フィールド

        /// <summary>
        /// Slackのトークン
        /// </summary>
        private readonly string m_Token;

        /// <summary>
        /// HTTPクライアント
        /// </summary>
        private readonly HttpClient m_Client;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="token">Slack APIトークン</param>
        public SlackAPIHelper(string token)
        {
            m_Token = token ?? throw new ArgumentNullException(nameof(token));
            m_Client = new HttpClient();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// Slackチャンネルから指定期間のメッセージを非同期で取得する
        /// </summary>
        /// <param name="channelId">メッセージを取得する対象のSlackチャンネルID</param>
        /// <param name="startTimestamp">開始日時のUNIXタイムスタンプ</param>
        /// <param name="endTimestamp">終了日時のUNIXタイムスタンプ</param>
        /// <returns>取得したメッセージのリスト</returns>
        public async Task<List<string>> GetMessagesAsync(string channelId, string startTimestamp, string endTimestamp)
        {
            if (string.IsNullOrEmpty(channelId))
                throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

            var messages = await FetchMessagesFromSlack(channelId, startTimestamp, endTimestamp);
            return messages ?? new List<string>();
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// Slack APIから指定期間のメッセージを取得する
        /// </summary>
        /// <param name="channelId">SlackチャンネルID</param>
        /// <param name="startTimestamp">開始日時のUNIXタイムスタンプ</param>
        /// <param name="endTimestamp">終了日時のUNIXタイムスタンプ</param>
        /// <returns>メッセージのリスト</returns>
        private async Task<List<string>> FetchMessagesFromSlack(string channelId, string startTimestamp, string endTimestamp)
        {
            var url = $"https://slack.com/api/conversations.history?channel={channelId}&oldest={startTimestamp}&latest={endTimestamp}&inclusive=true";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {m_Token}");

            try
            {
                var response = await m_Client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SlackResponse>(json);

                return result?.messages?.ConvertAll(msg => msg.text) ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                // ネットワークエラーやレスポンスが異常な場合の処理
                throw new InvalidOperationException("Failed to fetch messages from Slack.", ex);
            }
        }

        #endregion
    }
}
