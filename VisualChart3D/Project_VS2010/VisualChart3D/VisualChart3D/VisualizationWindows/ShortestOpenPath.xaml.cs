using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using StarMathLib;

using VisualChart3D.Common;

namespace VisualChart3D.VisualizationWindows
{
    /// <summary>
    /// Interaction logic for Canvas.xaml
    /// </summary>
    public partial class ShortestOpenPathPlot : Window, IVisualizationWindow
    {
        private double[,] _dataMatrix;
        public double[,] DataMatrix { get => _dataMatrix; set { _dataMatrix = value; OnDataChanged(new VisualizationWindowEventArgs(_dataMatrix, _graph)); } }
        private IList<int> _graph;

        public IList<int> Graph { get => _graph; set { _graph = value; OnDataChanged(new VisualizationWindowEventArgs(_dataMatrix, _graph)); } }

        public event EventHandler DataChanged;

        public ShortestOpenPathPlot()
        {   
            InitializeComponent();
        }

        private void OnDataChanged(EventArgs e)
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
            GraphPlot.Plot.Clear();
            
            if (   !(bool) e.NewValue
                || DataMatrix == null
                || Graph == null
                || DataMatrix.GetLength(1) != 2
                || DataMatrix.GetLength(0) != Graph.Count)
            {
                return;
            }

            var x = DataMatrix.GetColumn(0);
            var y = DataMatrix.GetColumn(1);

            GraphPlot.Plot.AddScatterPoints(x, y, System.Drawing.Color.Violet, markerSize: 10, markerShape: ScottPlot.MarkerShape.filledSquare);

            for (var i = 1; i < Graph.Count; ++i)
            {
                GraphPlot.Plot.AddLine(x[Graph[i - 1]], y[Graph[i - 1]], x[Graph[i]], y[Graph[i]], System.Drawing.Color.Pink);
            }
        }
    }
}
