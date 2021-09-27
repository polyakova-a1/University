using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.DataBinding
{
    public class ColumnDataViewModel : BaseViewModel, System.ComponentModel.INotifyPropertyChanged
    {
        private ObservableCollection<string> _activeItems;
        private ObservableCollection<string> _ignoredItems;
        private ObservableCollection<string> _firstLineItems;

        public ObservableCollection<string> ActiveItems {
            get {
                if (_activeItems == null)
                {
                    _activeItems = new ObservableCollection<string>();
                }

                return _activeItems;
            }

            set {
                _activeItems = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> IgnoredItems {
            get {
                if (_ignoredItems == null)
                {
                    _ignoredItems = new ObservableCollection<string>();
                }

                return _ignoredItems;
            }

            set {
                _ignoredItems = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> FirstLineItems {
            get {
                if (_firstLineItems == null)
                {
                    _firstLineItems = new ObservableCollection<string>();
                }

                return _firstLineItems;
            }

            set {
                _firstLineItems = value;
                NotifyPropertyChanged();
            }
        }
    }
}
