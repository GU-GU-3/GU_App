using System;
using System.Windows.Input;

namespace CollectAdvice.Commands
{
	/// <summary>
	/// コマンドを簡単に作成できるヘルパークラス
	/// </summary>
	public class RelayCommand : ICommand
	{
		#region フィールド

		/// <summary>
		/// コマンド実行時の処理
		/// </summary>
		private readonly Action _execute;

		/// <summary>
		/// コマンドが実行可能かどうかを判定する処理
		/// </summary>
		private readonly Func<bool> _canExecute;

		#endregion

		#region イベント

		/// <summary>
		/// コマンドの実行可否が変更されたときに発火するイベント
		/// </summary>
		public event EventHandler CanExecuteChanged;

		#endregion

		#region コンストラクタ

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="execute">実行するアクション</param>
		/// <param name="canExecute">実行可否を判定する関数</param>
		public RelayCommand(Action execute, Func<bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		#endregion

		#region メソッド

		/// <summary>
		/// コマンドを実行できるかどうか
		/// </summary>
		/// <param name="parameter">パラメータ</param>
		/// <returns>実行可能な場合は true、それ以外は false</returns>
		public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

		/// <summary>
		/// コマンドを実行する
		/// </summary>
		/// <param name="parameter">パラメータ</param>
		public void Execute(object parameter) => _execute();

		/// <summary>
		/// 実行可否を更新
		/// </summary>
		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

		#endregion
	}
}