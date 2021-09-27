using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VisualChart3D.Common.Visualization;

namespace VisualChart3D.ConfigWindow
{
    /// <summary>
    /// Interaction logic for SammonsMapConfigs.xaml
    /// Логика взаимодействия - если изменил начальные данные, то пока не перерисуешь график - не сохранишь их.
    /// </summary>
    public partial class SammonsMapConfigs : Window
    {
        const string IterationStepLimitFormat = "Шаг дробления (max = {0})";
        const string IterationLimitFormat = "Число итераций (max = {0})";

        public event PropertyChangedEventHandler PropertyChanged;

        private ISammon _sammonProjection;
        //private double _maxHeight;

        /*public double PlotHeight {
            get {
                return _maxHeight;
            }
            set {
                _maxHeight = value;
                OnPropertyChanged("PlotHeight");
            }
        }*/

        public SammonsMapConfigs(ISammon sammonProjection)
        {
            InitializeComponent();

            idIterationNumber.ValueChanged += ddAll_ValueChanged;
            ddUpperBound.ValueChanged += ddAll_ValueChanged;
            SamProjection = sammonProjection;
            SetMaxUpperBound();
            SetMaxIteration();
            RepaintChart();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnRecalculate_Click(object sender, RoutedEventArgs e)
        {
            Recalculate();
            RepaintChart();

            if (BtnSave.IsEnabled == false)
            {
                BtnSave.IsEnabled = true;
                BtnRecalculate.IsEnabled = false;
            }
        }

        private void SetValues()
        {
            ddUpperBound.Value = SamProjection.IterationStep;
            idIterationNumber.Value = SamProjection.IterationNumber;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void RepaintChart()
        {
            circles.Plot.AddScatterPoints(Enumerable.Range(1, SamProjection.CalculatedCriteria.Count)
                                                    .Select(i => (double) i)
                                                    .ToArray(),
                                          SamProjection.CalculatedCriteria.ToArray(),
                                          System.Drawing.Color.Blue,
                                          markerSize: 10);
            circles.Render();
        }

        private void Recalculate()
        {
            SamProjection.IterationStep = (double)ddUpperBound.Value;
            SamProjection.IterationNumber = (int)idIterationNumber.Value;

            SamProjection.ToProject();
        }

        private void ddAll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SamProjection.IterationStep != ddUpperBound.Value)
            {
                SwitchButtons();
                return;
            }

            if (SamProjection.IterationNumber != idIterationNumber.Value)
            {
                SwitchButtons();
                return;
            }

            BtnSave.IsEnabled = true;
        }

        private void SwitchButtons()
        {
            BtnSave.IsEnabled = false;
            BtnRecalculate.IsEnabled = true;
        }

        private void SetMaxUpperBound()
        {            
            this.ddUpperBound.Maximum = this._sammonProjection.IterationStepLimit;
            string iterationStepLimitMessage = string.Format(IterationStepLimitFormat, this._sammonProjection.IterationStepLimit);
            Common.Utils.ChangeLabelTextBlockText(this.lbIterationStepLimit, iterationStepLimitMessage);

            /*this.tbCountOfIterations.Maximum = value;

            string MaxIteratiosMessage = string.Format(MaxIterationsFormat, value);
            Common.Utils.ChangeLabelTextBlockText(lbIterations, MaxIteratiosMessage);*/
        }


        private void SetMaxIteration()
        {
            this.idIterationNumber.Maximum = this._sammonProjection.IterationLimit;
            string iterationLimitMessage = string.Format(IterationLimitFormat, this._sammonProjection.IterationLimit);
            Common.Utils.ChangeLabelTextBlockText(this.lbIterationLimit, iterationLimitMessage);

            /*this.tbCountOfIterations.Maximum = value;

            string MaxIteratiosMessage = string.Format(MaxIterationsFormat, value);
            Common.Utils.ChangeLabelTextBlockText(lbIterations, MaxIteratiosMessage);*/
        }


        public ISammon SamProjection {
            get {
                return _sammonProjection;
            }

            private set {
                _sammonProjection = value;
                SetValues();
            }
        }
    }
}
