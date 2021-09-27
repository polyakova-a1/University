using System.Windows;

namespace VisualChart3D.ConfigWindow
{
    /// <summary>
    /// Interaction logic for SammonsMapConfigWindow.xaml
    /// </summary>
    public partial class KohonenMapConfigs : Window
    {
        private const string MaxIterationsFormat = "Количество итераций (max = {0})";

        private int _countOfIteration;

        public KohonenMapConfigs(int countOfIteration, int maxIterations)
        {
            InitializeComponent();
            _countOfIteration = countOfIteration;
            this.tbCountOfIterations.Value = _countOfIteration;
            MaxIterations = maxIterations;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _countOfIteration = this.tbCountOfIterations.Value.Value;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private int MaxIterations {
            set {

                this.tbCountOfIterations.Maximum = value;

                string MaxIteratiosMessage = string.Format(MaxIterationsFormat, value);
                Common.Utils.ChangeLabelTextBlockText(lbIterations, MaxIteratiosMessage);
            }
        }

        public int CountOfIteration { get => _countOfIteration; }
    }
}
