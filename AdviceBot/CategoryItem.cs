using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectAdvice
{
    public class CategoryItem : INotifyPropertyChanged
    {
        private string m_Name;

        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string Name
        {
            get => m_Name;
            set => SetProperty(ref m_Name, value);
        }

        private ObservableCollection<TaskItem> m_Tasks = new ObservableCollection<TaskItem>();

        /// <summary>
        /// 課題のリスト
        /// </summary>
        public ObservableCollection<TaskItem> Tasks
        {
            get => m_Tasks;
            set => SetProperty(ref m_Tasks, value);
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