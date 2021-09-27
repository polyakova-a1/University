using System;
using System.Windows;
using VisualChart3D.Common;
using VisualChart3D.Common.DataReader;

namespace VisualChart3D.InputDataWindows
{
    /// <summary>
    /// Interaction logic for StandartInputWindow.xaml
    /// </summary>
    public partial class StandartInputWindow : Window
    {
        private const string NotImplementedSourceMatrixType = "Ошибка типа входной матрицы. ";
        private const string InputFileChooseMessage = "Не указан файл \"Матрица расстояний\" или \"Матрица объект-признак\". ";
        private const string IncorrectFileStructureMessage = "Некорректная структура файла \"Матрица расстояний\" или \"Матрица объект-признак\". ";

        private IUniversalReader _reader;
        private SourceFileMatrixType _inputMatrixType;

        public StandartInputWindow(IUniversalReader reader = null, InputFileType fileType = InputFileType.Text)
        {
            InitializeComponent();

            if (reader == null)
            {
                _reader = InitializeReader(fileType);
            }
            else
            {                
                _reader = reader.InputFileType == fileType ? reader : InitializeReader(fileType); 
            }

            CheckVisibility(_reader.SourceMatrixType, _reader.SourceMatrixFile);
            FillValues();
        }

        private IUniversalReader InitializeReader(InputFileType fileType)
        {
            IUniversalReader reader = new StandartDataReader(fileType);
            return reader;
        }

        private void btChooseFile_Click(object sender, RoutedEventArgs e)
        {
            bool result = Utils.OpenFile(tbDataMatrixPath);

            if (!result)
            {
                return;
            }

            _inputMatrixType = GetChoosedMatrixType();
            CheckVisibility(_inputMatrixType, tbDataMatrixPath.Text);
        }

        private void CheckVisibility(SourceFileMatrixType sourceFileMatrixType, string sourceMatrixPath)
        {
            if (sourceFileMatrixType != SourceFileMatrixType.ObjectAttribute)
            {
                return;
            }

            if (String.IsNullOrEmpty(sourceMatrixPath))
            {
                return;
            }

            tbMinkovskiDegree.IsEnabled = true;
        }

        private void FillValues()
        {
            if (_reader.SourceMatrixType == SourceFileMatrixType.MatrixDistance)
            {
                rbDistanceMatrix.IsChecked = true;
            }
            else if (_reader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute)
            {
                rbObjectAttributeMatrix.IsChecked = true;
            }

            if (!String.IsNullOrEmpty(_reader.SourceMatrixFile))
            {
                tbDataMatrixPath.Text = _reader.SourceMatrixFile;
            }

            tbMinkovskiDegree.Value = _reader.MinkovskiDegree;
        }

        private SourceFileMatrixType GetChoosedMatrixType()
        {
            if (rbDistanceMatrix.IsChecked == true)
            {
                return SourceFileMatrixType.MatrixDistance;
            }
            else if (rbObjectAttributeMatrix.IsChecked == true)
            {
                return SourceFileMatrixType.ObjectAttribute;
            }
            else if (rbObjectAttributeMatrix3D.IsChecked == true)
            {
                return SourceFileMatrixType.ObjectAttribute3D;
            }

            throw new NotImplementedException(NotImplementedSourceMatrixType);
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            string errors = String.Empty;
            _reader.MinkovskiDegree = (int)tbMinkovskiDegree.Value;

            if (string.IsNullOrEmpty(tbDataMatrixPath.Text))
            {
                errors+=InputFileChooseMessage;
            }
            else if (!_reader.CheckSourceMatrix(tbDataMatrixPath.Text, _inputMatrixType))
            {
                errors+=IncorrectFileStructureMessage;
            }

            
            
            if (!String.IsNullOrEmpty(errors))
            {
                Utils.ShowErrorMessage(errors);
                return;
            }

            //_reader.SourceMatrixType = _inputMatrixType;
            //_reader.SourceMatrixFile = tbDataMatrixPath.Text;
            DialogResult = true;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Проверка на корректность исходной матрицы
        /// </summary>
        /// <returns></returns>
        /*private bool CheckSourceMatrix(string SourceMatrixFile, SourceFileMatrixType SourceMatrixType, out double[,] dataMatrix)
        {
            dataMatrix = null;
            if (string.IsNullOrEmpty(SourceMatrixFile) || !File.Exists(SourceMatrixFile))
            {
                return false;
            }

            try
            {
                if (SourceMatrixType == SourceFileMatrixType.MatrixDistance)
                {
                    dataMatrix = CommonMatrix.ReadMatrixDistance(SourceMatrixFile);
                }

                if (SourceMatrixType == SourceFileMatrixType.ObjectAttribute)
                {
                    dataMatrix = CommonMatrix.ReadMatrixAttribute(SourceMatrixFile);
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }*/

        public IUniversalReader Reader { get => _reader; }

        private void rbDistanceMatrix_Checked(object sender, RoutedEventArgs e)
        {
            if (tbMinkovskiDegree == null)
            {
                return;
            }

            if (tbMinkovskiDegree.IsEnabled)
            {
                tbMinkovskiDegree.IsEnabled = false;
            }
        }
    }
}
