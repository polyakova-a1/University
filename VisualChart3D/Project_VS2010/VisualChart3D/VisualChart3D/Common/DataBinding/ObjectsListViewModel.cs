using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChart3D.Common.DataBinding
{
    class ObjectListViewModel: BaseViewModel, System.ComponentModel.INotifyPropertyChanged
    {
        private ObservableCollection<string> _objectListItems;
        private string _selectedItem;

        public ObservableCollection<string> ObjectListItems {
            get {
                if (_objectListItems == null)
                {
                    _objectListItems = new ObservableCollection<string>();
                }

                return _objectListItems;
            }

            set {
                _objectListItems = value;
                NotifyPropertyChanged();
            }
        }

        public string SelectedObject {
            get {
                return _selectedItem;
            }

            set {
                _selectedItem = value;
                NotifyPropertyChanged(nameof(SelectedObject));
            }
        }
    }
}
