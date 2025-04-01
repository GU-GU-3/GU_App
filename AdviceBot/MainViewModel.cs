using CollectAdvice.Commands;
using CollectAdvice.Services;
using CollectAdvice.Utilities;
using CollectAdvice.ViewModels;
using CollectAdvice;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows;

/// <summary>
/// VM
/// </summary>
public class MainViewModel : ViewModelBase
{
	#region フィールド

	/// <summary>
	/// Slack との連携を行うサービス
	/// </summary>
	private readonly SlackService m_SlackService;

	/// <summary>
	/// Google Gemini API を使用してタスクのカテゴリ分類とアドバイス生成を行うサービス
	/// </summary>
	private readonly GeminiService m_GeminiService;

	/// <summary>
	/// 設定情報を管理するサービス
	/// </summary>
	private readonly SettingsService m_SettingsService;

	/// <summary>
	/// 取得対象の開始日
	/// </summary>
	private DateTime? m_StartDate;

	/// <summary>
	/// 取得対象の終了日
	/// </summary>
	private DateTime? m_EndDate;

	/// <summary>
	/// 最近の課題に対するアドバイス
	/// </summary>
	private string m_AdviceforRecentTask;

	#endregion

	#region プロパティ

	/// <summary>
	/// Slack の API キー
	/// </summary>
	public string SlackAPIKey { get; set; }

	/// <summary>
	/// Slack のチャンネル ID
	/// </summary>
	public string SlackChannelID { get; set; }

	/// <summary>
	/// Google Gemini API のキー
	/// </summary>
	public string GeminiAPIKey { get; set; }

    /// <summary>
    /// 課題カテゴリを格納するコレクション
    /// </summary>
    private ObservableCollection<CategoryItem> m_Categories = new ObservableCollection<CategoryItem>();
    public ObservableCollection<CategoryItem> Categories
    {
        get => m_Categories;
        private set => SetProperty(ref m_Categories, value);  // SetPropertyで変更を通知
    }

    /// <summary>
    /// 最近の課題に対するアドバイスを取得または設定する
    /// </summary>
    public string AdviceforRecentTask
    {
        get => m_AdviceforRecentTask ?? "";
        set
        {
            m_AdviceforRecentTask = value;
            OnPropertyChanged(nameof(AdviceforRecentTask)); 
        }
    }

    /// <summary>
    /// 開始日を取得または設定する
    /// </summary>
    public DateTime? StartDate
    {
        get => m_StartDate;
        set => SetProperty(ref m_StartDate, value);
    }

    /// <summary>
    /// 終了日を取得または設定する
    /// </summary>
    public DateTime? EndDate
    {
        get => m_EndDate;
        set => SetProperty(ref m_EndDate, value);
    }

    #endregion

    /// <summary>
    /// タスクをロードするコマンド
    /// </summary>
    public ICommand LoadTasksCommand { get; }

    /// <summary>
    /// コンストラクタ - サービスの初期化とコマンドの設定
    /// </summary>
    public MainViewModel()
    {
        var slackApiToken = ApiKeyProvider.GetApiKey("SlackAPIKey");
        m_SlackService = new SlackService(slackApiToken);
        m_GeminiService = new GeminiService();
        m_SettingsService = new SettingsService();

        LoadTasksCommand = new RelayCommand(async () => await LoadTasksAsync());

        LoadSettings();  // 設定のロード
    }

    /// <summary>
    /// 設定情報を読み込む
    /// </summary>
    private void LoadSettings()
    {
        // 設定ファイルから情報を読み込み
        var settings = m_SettingsService.LoadSettings();
        SetProperty(ref m_StartDate, settings.StartDate);
        SetProperty(ref m_EndDate, settings.EndDate);

        // API キーの読み込み
        var apiKeys = ApiKeyProvider.LoadApiKeys();
        GeminiAPIKey = apiKeys.ContainsKey("GeminiAPIKey") ? apiKeys["GeminiAPIKey"] : string.Empty;
        SlackAPIKey = apiKeys.ContainsKey("SlackAPIKey") ? apiKeys["SlackAPIKey"] : string.Empty;
        SlackChannelID = apiKeys.ContainsKey("SlackChannelID") ? apiKeys["SlackChannelID"] : string.Empty;
    }

    /// <summary>
    /// Slackのメッセージを取得し、タスクを抽出・分類して表示する
    /// </summary>
    internal async Task LoadTasksAsync()
    {
        ProgressDialog progressDialog = null;
        try
        {
            var owner = Application.Current.MainWindow;
            progressDialog = ProgressDialog.Show(owner, "Slackのメッセージを取得中...");

            var slackChannelId = ApiKeyProvider.GetApiKey("SlackChannelID");
            var startTimestamp = (StartDate ?? DateTime.UtcNow.AddDays(-7));
            var endTimestamp = (EndDate ?? DateTime.UtcNow);

            var messages = await m_SlackService.GetMessagesAsync(slackChannelId, startTimestamp, endTimestamp);

            var taskItems = messages
                .Where(m => m.Contains("課題"))
                .Select(m =>
                {
                    int startIndex = m.IndexOf("課題") + "課題".Length;
                    string extractedText = m.Substring(startIndex).Trim();
                    return Regex.Replace(extractedText, @"^\(.*?\)\s*\n", "").Trim();
                })
                .ToList();

            // タスクをカテゴリごとに分類
            var categorizedTasks = await m_GeminiService.CategorizeTasksAsync(taskItems);

            // 最近の課題として課題リストの最後の10%を取得
            int recentTaskCount = (int)(taskItems.Count * 0.10);
            var recentTasks = taskItems.Skip(taskItems.Count - recentTaskCount).ToList();

            // その10%に対応する最近の課題に対するアドバイスを取得
            var newAdvice = await m_GeminiService.GetAdviceAsync(recentTasks);
            AdviceforRecentTask = newAdvice;

            // 新しい ObservableCollection に更新
            var newCategories = new ObservableCollection<CategoryItem>();
            foreach (var category in categorizedTasks)
            {
                newCategories.Add(new CategoryItem
                {
                    Name = category.Key,
                    Tasks = new ObservableCollection<TaskItem>(category.Value.Select(t => new TaskItem { Description = t }))
                });
            }

            // Categoriesプロパティを更新し、UI に変更を通知
            Categories = newCategories;  
        }
        catch (Exception ex)
        {
            MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            progressDialog?.Close();
        }
    }

    /// <summary>
    /// 設定を保存する
    /// </summary>
    public void SaveSettings()
    {
        // 現在の設定を取得し、保存する
        var settings = new DateSettings
        {
            StartDate = this.StartDate ?? DateTime.Now.AddDays(-7), 
            EndDate = this.EndDate ?? DateTime.Now 
        };

        m_SettingsService.Save(settings);

        // APIキーの設定を保存
        var apiKeys = new Dictionary<string, string>
        {
            { "SlackAPIKey", this.SlackAPIKey },
            { "SlackChannelID", this.SlackChannelID },
            { "GeminiAPIKey", this.GeminiAPIKey }
        };

        ApiKeyProvider.SaveApiKeys(apiKeys);
    }
}
