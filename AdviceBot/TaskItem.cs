using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CollectAdvice
{
    /// <summary>
    /// 課題を表すクラス
    /// </summary>
    public class TaskItem : INotifyPropertyChanged
    {
        private string m_Description;
        /// <summary>
        /// 課題の内容
        /// </summary>
        [JsonPropertyName("Description")]  // JSON のキー「Description」に対応
        public string Description
        {
            get => m_Description;
            set => SetProperty(ref m_Description, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}