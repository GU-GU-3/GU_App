using System.Windows;

namespace CollectAdvice
{
	/// <summary>
	/// ProgressDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ProgressDialog : Window
	{
		/// <summary>
		/// ダイアログに表示するメッセージ
		/// </summary>
		public string Message
		{
			get => m_MessageTextBlock.Text;
			set => m_MessageTextBlock.Text = value;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ProgressDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// ダイアログを表示する
		/// </summary>
		/// <param name="owner">親ウィンドウ</param>
		/// <param name="message">表示するメッセージ</param>
		public static ProgressDialog Show(Window owner, string message)
		{
			ProgressDialog progressDialog = new ProgressDialog();
			progressDialog.Owner = owner;

			// メッセージを設定
			progressDialog.m_MessageTextBlock.Text = message;

			// ダイアログを表示
			progressDialog.Show();
			return progressDialog;
		}
	}
}