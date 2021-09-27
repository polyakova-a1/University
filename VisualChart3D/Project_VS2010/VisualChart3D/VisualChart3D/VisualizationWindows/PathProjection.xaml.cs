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
using VisualChart3D.Common;

namespace VisualChart3D.VisualizationWindows
{
    /// <summary>
    /// Interaction logic for CumulativePlot.xaml
    /// </summary>
    public partial class PathProjection : Window, IVisualizationWindow
    {
        public PathProjection()
        {
            InitializeComponent();
        }

        private double[,] _distMatrix;
        public double[,] DataMatrix { get => _distMatrix; set { _distMatrix = value; OnDataChanged(new VisualizationWindowEventArgs(_distMatrix, _graph)); } }

        private IList<int> _graph;
        public IList<int> Graph { get => _graph; set { _graph = value; OnDataChanged(new VisualizationWindowEventArgs(_distMatrix, _graph)); } }

        public event EventHandler DataChanged;

        protected virtual void OnDataChanged(VisualizationWindowEventArgs e)
        {
            DataChanged?.Invoke(this, e);
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CumulativePlot.Plot.Clear();

            if (   !(bool)e.NewValue
                || DataMatrix == null
                || Graph == null
                || DataMatrix.GetLength(0) != Graph.Count)
            {
                return;
            }

            VisualizationUpdate();
        }

        private void VisualizationUpdate()
        {
            if (   CumulativePlot == null
                || DataMatrix == null
                || Graph == null)
            {
                return;
            }

            CumulativePlot.Plot.Clear();

            var xs = new double[Graph.Count];
            var ys = new double[Graph.Count];

            for (var i = 1; i < Graph.Count; ++i)
            {
                xs[i] = xs[i - 1] + DataMatrix[Graph[i - 1], Graph[i]];
                ys[i] = DataMatrix[Graph.First(), Graph[i]];
            }

            CumulativePlot.Plot.AddScatter(xs, ys,
                                           System.Drawing.Color.Violet);
            CumulativePlot.Render();
        }
    }
}
