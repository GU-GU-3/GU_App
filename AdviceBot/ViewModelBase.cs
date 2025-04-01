using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectAdvice.ViewModels
{
    /// <summary>
    /// ViewModel の基本クラス。プロパティ変更通知の機能を提供する。
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更時に発火するイベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティの値を変更し、変更通知を発火する
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
	        if (field is string strField && value is string strValue)
	        {
		        if (string.Equals(strField, strValue, StringComparison.Ordinal)) return false;
	        }
	        else
	        {
		        if (Equals(field, value)) return false;
	        }

	        field = value;
	        OnPropertyChanged(propertyName);
	        return true;
        }


        /// <summary>
        /// プロパティが変更されたことを通知する
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}