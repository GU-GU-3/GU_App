using CollectAdvice.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CollectAdvice.Services
{
    /// <summary>
    /// Google Gemini API を使用してタスクのカテゴリ分類とアドバイス生成を行うサービス
    /// </summary>
    public class GeminiService
    {
        #region フィールド

        /// <summary>
        /// Gemini APIのエンドポイント
        /// </summary>
        private readonly string m_ApiUrl;

        /// <summary>
        /// HTTPクライアント
        /// </summary>
        private readonly HttpClient m_Client;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GeminiService()
        {
            string apiKey = ApiKeyProvider.GetApiKey("GeminiAPIKey");
            m_ApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
            m_Client = new HttpClient();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// タスクをカテゴリごとに分類し、アドバイスを生成する
        /// </summary>
        /// <param name="taskItems">タスクのリスト</param>
        /// <returns>カテゴリごとに分類されたタスクとアドバイス</returns>
        public async Task<Dictionary<string, List<string>>> CategorizeTasksAsync(List<string> taskItems)
        {
            var tasksWithAdvice = new List<string>();
            foreach (var task in taskItems)
            {
                string advice = await GenerateAdviceAsync(task);
                tasksWithAdvice.Add(advice);
            }
            return await CategorizeTasksWithAdviceAsync(taskItems, tasksWithAdvice);
        }

        /// <summary>
        /// 指定されたタスクに対するアドバイスをGemini APIから取得する
        /// </summary>
        /// <param name="task">タスク内容</param>
        /// <returns>生成されたアドバイス</returns>
        private async Task<string> GenerateAdviceAsync(string task)
        {
	        string prompt = $@"
あなたのタスクを、以下のカテゴリのいずれかに分類し、適切なアドバイス(200文字程度)を提供してください。

カテゴリ:
1. 技術的課題
2. プロセス改善
3. コミュニケーションとコラボレーション
4. ビジネスとドメイン知識
5. 自己成長とキャリア

タスク: {task}

出力フォーマット:
カテゴリ: (カテゴリ名)
アドバイス: (アドバイス内容)
";
	        var requestData = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };

	        string jsonRequest = JsonSerializer.Serialize(requestData);
	        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
	        HttpResponseMessage response = await m_Client.PostAsync(m_ApiUrl, content);
	        response.EnsureSuccessStatusCode();

	        string responseJson = await response.Content.ReadAsStringAsync();
	        using JsonDocument doc = JsonDocument.Parse(responseJson);
	        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        }

        /// <summary>
        /// タスクとアドバイスをカテゴリごとに分類する
        /// </summary>
        /// <param name="taskItems">タスクのリスト</param>
        /// <param name="tasksWithAdvice">アドバイスのリスト</param>
        /// <returns>カテゴリごとに分類されたタスクとアドバイス</returns>
        private async Task<Dictionary<string, List<string>>> CategorizeTasksWithAdviceAsync(List<string> taskItems, List<string> tasksWithAdvice)
        {
            var categorizedTasks = new Dictionary<string, List<string>>();
            for (int index = 0; index < taskItems.Count; index++)
            {
                var task = taskItems[index];
                var advice = tasksWithAdvice[index];
                var lines = advice.Split('\n');

                string category = "未分類";
                if (lines.Length > 0 && lines[0].StartsWith("カテゴリ:"))
                {
                    category = lines[0].Replace("カテゴリ:", "").Trim();
                }

                var taskAdvice = string.Join("\n", lines[1..]).Trim();
                if (taskAdvice.StartsWith("アドバイス:"))
                {
                    taskAdvice = taskAdvice.Replace("アドバイス:", "").Trim();
                }

                if (!categorizedTasks.ContainsKey(category))
                {
                    categorizedTasks[category] = new List<string>();
                }
                categorizedTasks[category].Add($"{task}\nアドバイス: {taskAdvice}");
            }
            return categorizedTasks;
        }

        /// <summary>
        /// 与えたタスク全体に対するアドバイスを取得する
        /// </summary>
        /// <param name="recentTasks">最近のタスクのリスト</param>
        internal async Task<string> GetAdviceAsync(List<string> recentTasks)
        {
	        try
	        {
		        string taskList = string.Join("\n", recentTasks);
        
		        string prompt = $@"
以下のタスク全体に対して、適切なアドバイスを提供してください（400字程度）:

タスク一覧:
{taskList}

出力フォーマット:
アドバイス: (アドバイス内容)
";
		        var requestData = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };

		        string jsonRequest = JsonSerializer.Serialize(requestData);
		        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
		        HttpResponseMessage response = await m_Client.PostAsync(m_ApiUrl, content);
		        response.EnsureSuccessStatusCode();

		        string responseJson = await response.Content.ReadAsStringAsync();
		        using JsonDocument doc = JsonDocument.Parse(responseJson);
		        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
	        }
	        catch (Exception ex)
	        {
		        Console.Error.WriteLine($"アドバイスの取得中にエラーが発生しました: {ex.Message}");
		        return null;
	        }
        }
        #endregion
    }
}
