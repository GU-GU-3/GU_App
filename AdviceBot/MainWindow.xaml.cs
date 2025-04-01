using System.Windows;

namespace CollectAdvice
{
	/// <summary>
	/// MainWindow.xaml のコードビハインド
	/// </summary>
	public partial class MainWindow : Window
	{
		#region フィールド

		/// <summary>
		/// ViewModel
		/// </summary>
		private MainViewModel m_ViewModel;
		
		#endregion
		
		/// <summary>
		/// MainWindow のコンストラクタ
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			m_ViewModel = new MainViewModel();
			this.DataContext = m_ViewModel; // ViewModelをViewにバインド
		}

		#region イベントハンドラ

		/// <summary>
		/// 課題取得ボタンクリックイベントハンドラ
		/// </summary>
		private async void GetAdviceButton_OnClick(object sender, RoutedEventArgs e)
		{
			// LoadTasksCommand を実行
			await m_ViewModel.LoadTasksAsync();
		}

		/// <summary>
		/// 設定保存ボタンクリックイベントハンドラ
		/// </summary>
		private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			// 設定を保存する処理
			m_ViewModel.SaveSettings(); 
		}
		

		#endregion
		
	}
}