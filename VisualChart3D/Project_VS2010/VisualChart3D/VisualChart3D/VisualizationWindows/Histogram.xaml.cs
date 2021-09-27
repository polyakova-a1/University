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
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : Window, IVisualizationWindow
    {
        private double[,] _distMatrix;
        public double[,] DataMatrix { get => _distMatrix; set { _distMatrix = value; OnDataChanged(new VisualizationWindowEventArgs(_distMatrix, _graph)); } }

        private IList<int> _graph;
        public IList<int> Graph { get => _graph; set { _graph = value; OnDataChanged(new VisualizationWindowEventArgs(_distMatrix, _graph)); } }

        public event EventHandler DataChanged;

        protected virtual void OnDataChanged(VisualizationWindowEventArgs e)
        {
            DataChanged?.Invoke(this, e);
        }

        public Histogram()
        {
            InitializeComponent();
        }


        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HistPlot.Plot.Clear();

            if (   !(bool)e.NewValue
                || DataMatrix == null
                || Graph == null
                || DataMatrix.GetLength(0) != Graph.Count)
            {
                return;
            }

            BinsSlider.Maximum = Graph.Count;
            VisualizationUpdate();
        }

        private void BinsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VisualizationUpdate();
        }

        private void VisualizationUpdate()
        {
            if (   HistPlot == null
                || DataMatrix == null
                || Graph == null)
            {
                return;
            }

            HistPlot.Plot.Clear();

            var totalDist = Algorithms.GetBrokenLineLength(Graph, DataMatrix);
            var thresholdPerBin = totalDist / BinsSlider.Value;

            var values = new double[(int) BinsSlider.Value];
            ++values[0];

            var positions = Enumerable.Range(1, (int) BinsSlider.Value)
                                      .Select(i => (thresholdPerBin * (2 * i - 1)) / 2)
                                      .ToArray();

            var dist = 0.0;
            for (var i = 1; i < Graph.Count; ++i)
            {
                dist += DataMatrix[Graph[i - 1], Graph[i]];
                ++values[Math.Min((int) (dist / thresholdPerBin), values.Length - 1)];
            }

            var bar = HistPlot.Plot.AddBar(values, positions);
            if (bar.Xs.Length > 1)
            {
                bar.BarWidth = positions[1] - positions[0];
            }

            HistPlot.Render();
        }
    }
}
