using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CollectAdvice.Services
{
	/// <summary>
	/// Slack からメッセージを取得するサービスクラス
	/// </summary>
	public class SlackService
	{
		private readonly SlackAPIHelper m_SlackAPIHelper;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="token">Slack APIトークン</param>
		public SlackService(string token)
		{
			m_SlackAPIHelper = new SlackAPIHelper(token);
		}

		/// <summary>
		/// 指定された期間のSlackメッセージを取得する
		/// </summary>
		/// <param name="channelId">チャンネルID</param>
		/// <param name="startDate">開始日</param>
		/// <param name="endDate">終了日</param>
		/// <returns>メッセージのリスト</returns>
		public async Task<List<string>> GetMessagesAsync(string channelId, DateTime startDate, DateTime endDate)
		{
			// 終了日を23:59:59に設定して、終了日当日のメッセージも取得するようにする
			var endDateWithTime = endDate.Date.AddDays(1).AddSeconds(-1); 

			var startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds().ToString();
			var endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds().ToString();
			return await m_SlackAPIHelper.GetMessagesAsync(channelId, startTimestamp, endTimestamp);
		}
	}
}