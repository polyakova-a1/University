using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using StarMathLib;

namespace VisualChart3D.VisualizationWindows
{
    /// <summary>
    /// Interaction logic for DataGeneration.xaml
    /// </summary>
    public partial class DataGeneration : Window
    {
        public DataGeneration()
        {
            InitializeComponent();
            GenerationPlot.RightClicked -= GenerationPlot.DefaultRightClickEvent;

            plottable = new ScottPlot.Plottable.ScatterPlot(new double[] { 0.0 },
                                                            new double[] { 0.0 })
            {
                Color = System.Drawing.Color.OrangeRed,
                MarkerSize = 5,
                MarkerShape = ScottPlot.MarkerShape.filledCircle,
                LineStyle = ScottPlot.LineStyle.None,
                IsVisible = false
            };

            highlighted = new ScottPlot.Plottable.ScatterPlot(new double[] { 0.0 },
                                                              new double[] { 0.0 })
            {
                Color = System.Drawing.Color.Red,
                MarkerSize = 10,
                MarkerShape = ScottPlot.MarkerShape.openCircle,
                IsVisible = false
            };

            GenerationPlot.Plot.Add(plottable);
            GenerationPlot.Plot.Add(highlighted);
        }

        private DateTime lbLastMouseDown = DateTime.UtcNow;
        private DateTime rbLastMouseDown = DateTime.UtcNow;

        private ScottPlot.Plottable.ScatterPlot plottable;
        private ScottPlot.Plottable.ScatterPlot highlighted;

        private int lastHighlightedIndex = -1;

        private void AddPoint()
        {
            var (x, y) = GenerationPlot.GetMouseCoordinates();

            if (plottable.IsVisible)
            {
                var xs = new double[plottable.PointCount + 1];
                var ys = new double[plottable.PointCount + 1];
                Array.Copy(plottable.Xs, xs, xs.Length - 1);
                Array.Copy(plottable.Ys, ys, ys.Length - 1);
                xs[^1] = x;
                ys[^1] = y;

                plottable.Update(xs, ys);
            }
            else
            {
                plottable.IsVisible = true;
                plottable.Xs[0] = x;
                plottable.Ys[0] = y;
            }

            UpdateHighlighted(x, y);
            GenerationPlot.Render();
        }

        private void RemovePoint()
        {
            if (lastHighlightedIndex >= 0)
            {
                if (plottable.PointCount > 1)
                {
                    plottable.Update(plottable.Xs.RemoveVectorCell(lastHighlightedIndex),
                                     plottable.Ys.RemoveVectorCell(lastHighlightedIndex));
                    lastHighlightedIndex = -1;

                    var (x, y) = GenerationPlot.GetMouseCoordinates();
                    UpdateHighlighted(x, y);
                }
                else
                {
                    highlighted.IsVisible = plottable.IsVisible = false;
                    lastHighlightedIndex = -1;
                }

                GenerationPlot.Render();
            }
        }

        private void UpdateHighlighted(double x, double y)
        {
            if (plottable.IsVisible)
            {
                double xyRatio = GenerationPlot.Plot.XAxis.Dims.PxPerUnit / GenerationPlot.Plot.YAxis.Dims.PxPerUnit;
                (double x1, double y1, int idx) = plottable.GetPointNearest(x, y, xyRatio);

                if (lastHighlightedIndex != idx)
                {
                    lastHighlightedIndex = idx;
                    highlighted.Xs[0] = x1;
                    highlighted.Ys[0] = y1;
                    highlighted.IsVisible = true;
                }
            }
            else
            {
                highlighted.IsVisible = false;
            }
        }

        private void GenerationPlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lbLastMouseDown = DateTime.UtcNow;
        }

        private void GenerationPlot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            const long msThreshold = 100;
            if ((DateTime.UtcNow - lbLastMouseDown).TotalMilliseconds < msThreshold && e.Source == GenerationPlot)
            {
                AddPoint();
            }
        }

        private void GenerationPlot_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rbLastMouseDown = DateTime.UtcNow;
        }

        private void GenerationPlot_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            const long msThreshold = 100;
            if ((DateTime.UtcNow - rbLastMouseDown).TotalMilliseconds < msThreshold && e.Source == GenerationPlot)
            {
                RemovePoint();
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!plottable.IsVisible)
            {
                MessageBox.Show($"График пуст. Добавьте точек нажатием ЛКМ.", "Сохранение невозможно",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog()
            {
                Filter = "*.csv|Comma separated file",
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                var tmp = new double[plottable.PointCount, 2];
                tmp.SetColumn(0, plottable.Xs);
                tmp.SetColumn(1, plottable.Ys);

                using var csvWriter = new Accord.IO.CsvWriter(saveDialog.FileName);
                csvWriter.WriteHeaders("X", "Y");
                csvWriter.Write(tmp);
                MessageBox.Show($"Данные записаны в файл '{saveDialog.FileName}'", "Сгенерированные данные");
            }
        }

        private void ClearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            highlighted.IsVisible = plottable.IsVisible = false;
            plottable.Update(new double[] { 0.0 }, new double[] { 0.0 });
            GenerationPlot.Render();
        }

        private void GenerationPlot_MouseMove(object sender, MouseEventArgs e)
        {
            (double x, double y) = GenerationPlot.GetMouseCoordinates();
            UpdateHighlighted(x, y);
            GenerationPlot.Render();
        }
    }
}
