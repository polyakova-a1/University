using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VisualChart3D.Common;
using VisualChart3D.Common.DataReader;
using System.Linq;
using System.Collections.ObjectModel;

namespace VisualChart3D.InputDataWindows
{
    /// <summary>
    /// Interaction logic for SCVInputWindow.xaml
    /// </summary>
    public partial class SCVInputWindow : Window
    {
        private const string NotImplementedSourceMatrixType = "Ошибка типа входной матрицы. ";
        private const string BadColumnChoiseMessage = "Ошибка выбора стобцов в качестве имен классов и объектов.";
        private const string BadParsingMessage = "Ошибка при чтении файла. Дальнейшее чтение невозможно.";
        private const string CannotIgnoreColumn = "Нельзя игнорировать уже выбранный в качестве имен объектов или имен классов столбец";
        private const string RefreshFilePathMessage = "Задайте путь к файлу";

        private const int SingleSelectedElementIndex = 0;

        private IUniversalReader _reader;
        private SourceFileMatrixType _inputMatrixType;

        /*/// <summary>
        /// Блокирование повторного вызова обработчика смены индекса комбобокса при возврате к старому индексу.
        /// </summary>
        private bool _loopBlocking;*/

        private Common.DataBinding.ColumnDataViewModel _columnDataViewModel;

        public SCVInputWindow(IUniversalReader reader = null,
                              InputFileType fileType = InputFileType.CSV)
        {
            InitializeComponent();
            //_loopBlocking = false;
            _columnDataViewModel = new Common.DataBinding.ColumnDataViewModel();

            DataContext = _columnDataViewModel;

            if (reader == null)
            {
                _reader = InitializeReader(fileType);
            }
            else
            {
                _reader = reader.InputFileType == fileType ? reader : InitializeReader(fileType);
                InitializeColumnData();
            }

            CheckFileVisibility(_reader);
            FillFileValues();
            InitializeColumnsData(_reader);
        }

        private void InitializeColumnData()
        {
            _columnDataViewModel.ActiveItems = new ObservableCollection<string>(_reader.FirstLine.Where(p => !_reader.IgnoredColumns.Contains(p)));
            _columnDataViewModel.IgnoredItems = new ObservableCollection<string>(_reader.IgnoredColumns);
        }

        private void btChooseFile_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            bool result = Utils.OpenFile(tbDataMatrixPath);

            if (!result)
            {
                return;
            }

            _inputMatrixType = GetChoosedMatrixType();
            if (_reader.CheckSourceMatrix(tbDataMatrixPath.Text, _inputMatrixType))
            {
                CheckFileVisibility(_reader);
                InitializeColumnsData(_reader);
            }
            else
            {
                tbDataMatrixPath.Text = RefreshFilePathMessage;
            }

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

        private void CheckFileVisibility(IUniversalReader universalReader)
        {
            if (universalReader.SourceMatrixType != SourceFileMatrixType.ObjectAttribute)
            {
                return;
            }

            if (String.IsNullOrEmpty(universalReader.SourceMatrixFile))
            {
                return;
            }

            tbMinkovskiDegree.IsEnabled = true;
        }

        private void InitializeColumnsData(IUniversalReader universalReader)
        {
            const int StartElement = 0;
            const int SecondElement = 1;

            if (universalReader.FirstLine == null)
            {
                return;
            }

            if (universalReader.FirstLine.Count == 0)
            {
                return;
            }

            //_columnDataViewModel.ActiveItems?.Clear();
            _columnDataViewModel.IgnoredItems?.Clear();

            _columnDataViewModel.ActiveItems = new ObservableCollection<string>(universalReader.FirstLine);


            _columnDataViewModel.FirstLineItems = new ObservableCollection<string>(universalReader.FirstLine);

            cbClassNumberColumn.IsEnabled = true;
            //cmbCLassNumberColumn.IsEnabled = true;

            cbObjectNameColumn.IsEnabled = true;
            //cmbObjectNameColumn.IsEnabled = true;

            if (universalReader.ClassNameColumn != null)
            {
                cmbCLassNumberColumn.SelectedIndex = universalReader.FirstLine.FindIndex(p => p == universalReader.ClassNameColumn);
                cbClassNumberColumn.IsChecked = true;
            }
            else
            {
                cmbCLassNumberColumn.SelectedIndex = StartElement;
                cmbCLassNumberColumn.SelectedValue = universalReader.FirstLine[StartElement];
            }

            //Если есть ферст лайн при инициализации и имена столбцов равны нулю, то нот чекед поставить.
            if (universalReader.ObjectNameColumn != null)
            {
                cmbObjectNameColumn.SelectedIndex = universalReader.FirstLine.FindIndex(p => p == universalReader.ObjectNameColumn);
                cbObjectNameColumn.IsChecked = true;
            }
            else
            {
                cmbObjectNameColumn.SelectedIndex = SecondElement;
                cmbObjectNameColumn.SelectedValue = universalReader.FirstLine[SecondElement];
            }
        }

        private void FillFileValues()
        {
            if (_reader.SourceMatrixType == SourceFileMatrixType.MatrixDistance)
            {
                rbDistanceMatrix.IsChecked = true;
            }
            else if (_reader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute)
            {
                rbObjectAttributeMatrix.IsChecked = true;
            }
            else if (_reader.SourceMatrixType == SourceFileMatrixType.ObjectAttribute3D)
            {
                rbObjectAttributeMatrix3D.IsChecked = true;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (!String.IsNullOrEmpty(_reader.SourceMatrixFile))
            {
                tbDataMatrixPath.Text = _reader.SourceMatrixFile;
            }

            tbMinkovskiDegree.Value = _reader.MinkovskiDegree;
        }

        private IUniversalReader InitializeReader(InputFileType fileType)
        {
            IUniversalReader reader = new FastSCVDataReader(fileType);
            return reader;
        }

        private void rbDistanceMatrix_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
            //if (tbMinkovskiDegree == null)
            {
                return;
            }

            tbDataMatrixPath.Text = RefreshFilePathMessage;

            if (tbMinkovskiDegree.IsEnabled)
            {
                tbMinkovskiDegree.IsEnabled = false;
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            //bool isBadChoise = CheckSelectedIndexes();

            Reader.MinkovskiDegree = (int)tbMinkovskiDegree.Value;

            bool isIndexNotInitialize = CheckIInitialzedIndexes();

            if (isIndexNotInitialize)
            {
                Utils.ShowErrorMessage(BadColumnChoiseMessage);
                return;
            }

            if (cbClassNumberColumn.IsChecked == true)
            {
                Reader.ClassNameColumn = (string)cmbCLassNumberColumn.SelectedItem;
            }

            if (cbObjectNameColumn.IsChecked == true)
            {
                Reader.ObjectNameColumn = (string)cmbObjectNameColumn.SelectedItem;
            }

            Reader.IgnoredColumns = _columnDataViewModel.IgnoredItems.ToList<string>();

            if (Reader.DistMatrix == null)
            {
                Utils.ShowErrorMessage(BadParsingMessage);

                _reader.ClassNameColumn = null;
                _reader.ObjectNameColumn = null;

                return;
            }

            if (Utils.CheckSourceMatrix(Reader.DistMatrix, Reader.SourceMatrixType))
            {
                Utils.ShowErrorMessage(Utils.BadMatrixType);
                return;
            }

            DialogResult = true;
        }

        private bool IsBothColumnsChecked()
        {
            if (cbClassNumberColumn.IsChecked == false)
            {
                return false;
            }

            if (cbObjectNameColumn.IsChecked == false)
            {
                return false;
            }

            return true;
        }

        private bool CheckIInitialzedIndexes()
        {
            if (!IsBothColumnsChecked())
            {
                return false;
            }

            return (cmbCLassNumberColumn.SelectedIndex == -1) || (cmbObjectNameColumn.SelectedIndex == -1);
        }

        private bool CheckIdenticalSelection()
        {
            if (!IsBothColumnsChecked())
            {
                return false;
            }

            return (cmbCLassNumberColumn.SelectedValue == cmbObjectNameColumn.SelectedValue);
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void IgnoreSelectedItem(object sender, RoutedEventArgs e)
        {
            if (lbAllColumns.SelectedIndex == -1)
            {
                return;
            }

            string transferedColumn = _columnDataViewModel.ActiveItems[lbAllColumns.SelectedIndex];

            if ((bool)cbClassNumberColumn.IsChecked || (bool)cbObjectNameColumn.IsChecked)
            {
                if (Utils.CompareStrings(transferedColumn, cmbCLassNumberColumn.SelectedValue.ToString()))
                //if (transferedColumn.CompareTo(cmbCLassNumberColumn.SelectedValue.ToString()) == 0)
                {
                    Utils.ShowWarningMessage(CannotIgnoreColumn);
                    return;
                }
            }

            /*if ((bool)cbObjectNameColumn.IsChecked)
            {
               if (transferedColumn.CompareTo(cmbCLassNumberColumn.SelectedValue.ToString()) == 0)
                {
                    Utils.ShowWarningMessage(CannotIgnoreColumn);
                    return;
                }
            }*/

            _columnDataViewModel.ActiveItems.Remove(transferedColumn);
            _columnDataViewModel.IgnoredItems.Add(transferedColumn);

            return;
        }

        private void ActivateSelectedItem(object sender, RoutedEventArgs e)
        {
            if (lbIgnoredColumns.SelectedIndex == -1)
            {
                return;
            }

            string transferedColumn = _columnDataViewModel.IgnoredItems[lbIgnoredColumns.SelectedIndex];

            _columnDataViewModel.IgnoredItems.Remove(transferedColumn);
            _columnDataViewModel.ActiveItems.Add(transferedColumn);

            return;
        }

        public IUniversalReader Reader {
            get {
                return _reader;
            }
        }

        private void Column_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            string removedItem;
            string addedItem;

            //if (_loopBlocking)
            //{
            //    _loopBlocking = false;
            //    return;
            //}

            if ((e.RemovedItems.Count != SingleSelectedElementIndex) && (e.AddedItems.Count != SingleSelectedElementIndex))
            {
                removedItem = e.RemovedItems[SingleSelectedElementIndex].ToString();
                bool isIdenticalColumns = CheckIdenticalSelection();

                if (isIdenticalColumns)
                {
                    var x = sender as ComboBox;
                    x.SelectionChanged -= Column_SelectionChanged;

                    Utils.ShowErrorMessage(BadColumnChoiseMessage);
                    //_loopBlocking = true;


                    RestoreLastValue(sender as ComboBox, removedItem);
                    x.SelectionChanged += Column_SelectionChanged;
                    return;
                }

                addedItem = e.AddedItems[SingleSelectedElementIndex].ToString();

                RestoreItem(removedItem);
                DeleteItemFromItems(addedItem);
                SetToolTip(sender, addedItem);

                return;
            }

            if (e.AddedItems.Count != SingleSelectedElementIndex)
            {
                addedItem = e.AddedItems[SingleSelectedElementIndex].ToString();

                //_loopBlocking = true;

                DeleteItemFromItems(addedItem);
                SetToolTip(sender, addedItem);
            }
        }

        private void RestoreLastValue(ComboBox comboBox, string lastItem)
        {
            comboBox.SelectedItem = lastItem;
            DeleteItemFromItems(lastItem);
        }

        private void AddItem(ObservableCollection<string> observableCollection, string item)
        {
            observableCollection.Add(item);
        }

        private void DeleteItemFromItems(string deletedItem)
        {
            RemoveItem(_columnDataViewModel.ActiveItems, deletedItem);
            RemoveItem(_columnDataViewModel.IgnoredItems, deletedItem);
        }

        private void RemoveItem(ObservableCollection<string> observableCollection, string item)
        {
            if (observableCollection.Contains(item))
            {
                observableCollection.Remove(item);
            }
        }

        private void ToolTip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            string toolTipText = e.AddedItems[0].ToString();
            SetToolTip(sender, toolTipText);
        }

        private void SetToolTip(object uiObject, string toolTip)
        {
            Control uiControl = uiObject as Control;
            uiControl.ToolTip = toolTip;
        }

        private void cbClassNumberColumn_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            DeleteItemFromItems(cmbCLassNumberColumn.SelectedItem.ToString());
            cmbCLassNumberColumn.IsEnabled = true;
        }

        private void cbClassNumberColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            string selectedItem = cmbCLassNumberColumn.SelectedItem.ToString();
            RestoreItem(selectedItem);
            cmbCLassNumberColumn.IsEnabled = false;
        }

        private void RestoreItem(string item)
        {
            if (!_columnDataViewModel.ActiveItems.Contains(item))
            {
                AddItem(_columnDataViewModel.ActiveItems, item);
            }
        }

        private void cbObjectNameColumn_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            DeleteItemFromItems(cmbObjectNameColumn.SelectedItem.ToString());
            cmbObjectNameColumn.IsEnabled = true;
        }

        private void cbObjectNameColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            string selectedItem = cmbObjectNameColumn.SelectedItem.ToString();
            RestoreItem(selectedItem);
            cmbObjectNameColumn.IsEnabled = false;
        }

        private void rbObjectAttributeMatrix_Checked(object sender, RoutedEventArgs e)
        {
            tbDataMatrixPath.Text = RefreshFilePathMessage;
        }

        private void rbObjectAttributeMatrix3D_Checked(object sender, RoutedEventArgs e)
        {
            tbDataMatrixPath.Text = RefreshFilePathMessage;
        }
    }
}
