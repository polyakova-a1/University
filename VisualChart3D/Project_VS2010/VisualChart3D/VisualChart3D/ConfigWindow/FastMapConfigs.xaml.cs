using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VisualChart3D.ConfigWindow
{
    /// <summary>
    /// Interaction logic for FastMapSettings.xaml
    /// </summary>
    public partial class FastMapConfigs : Window
    {
        private Common.Visualization.IFastMap _fastMapSetts;
        public Common.Visualization.IFastMap FastMapSetts {
            get {
                if ((bool)rbMetricEuclidean.IsChecked)
                {
                    _fastMapSetts.Metric = Common.FastMapMetricType.Euclidean;
                }
                else if ((bool)rbMetricNonEuclidean.IsChecked)
                {
                    _fastMapSetts.Metric = Common.FastMapMetricType.NonEuclidean;
                }
                else
                {
                    throw new NotImplementedException();
                }

                return _fastMapSetts;
            }
            set {
                switch (value.Metric)
                {
                    case Common.FastMapMetricType.Euclidean:
                        rbMetricEuclidean.IsChecked = true;
                        break;
                    case Common.FastMapMetricType.NonEuclidean:
                        rbMetricNonEuclidean.IsChecked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                _fastMapSetts = value;
            }
        }
        public FastMapConfigs(Common.Visualization.IFastMap fastMap)
        {
            InitializeComponent();
            FastMapSetts = fastMap;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


    }
}
