// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VisualChart3D.Common;
using VisualChart3D.Common.Multimedia;
using VisualChart3D.Common.Visualization;

namespace VisualChart3D.ConfigWindow
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class DissimilaritySpaceConfigs : Window
    {
        private readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".bmp", ".png" };
        private const string ReadingDirAdressErrorMessage = "Не удалось чтение адреса директории с изображениями из лог-файла";
        private const string CreatingLogFileErrorMessage = "Не удалось создать лог-файл для сохранения выбранного путя к директории с изображениями.";
        private const string BadFirstIndexWarningMessage = "Неккоректный выбор первого объекта. Пожалуйста, повторите.";
        private const string BadSecondIndexWarningMessage = "Неккоректный выбор второго объекта. Пожалуйста, повторите.";
        private const string BadThirdIndexWarningMessage = "Неккоректный выбор третьего объекта. Пожалуйста, повторите.";
        private const int BadSelectedIndex = -1;

        private DisSpace _dissimilaritySpace;
        private BaseMultimediaDataMaster _multimediaDataMaster;
        private Engine _settFile;
        private Common.DataBinding.ReferencedObjectsViewModel _referencedObjectsViewModel;

        public DisSpace DissimilaritySpace { get => _dissimilaritySpace; }

        public DissimilaritySpaceConfigs(Engine CurrentSetFile, DisSpace disSpace)
        {
            InitializeComponent();

            _multimediaDataMaster = new BaseMultimediaDataMaster();

            _referencedObjectsViewModel = new Common.DataBinding.ReferencedObjectsViewModel();
            DataContext = _referencedObjectsViewModel;

            _settFile = CurrentSetFile;
            _dissimilaritySpace = disSpace;
            cbThirdClassChoose.IsEnabled = false;
            pcThirdSelectedObject.Visibility = Visibility.Hidden;

            cbFirstClassChoose.ItemsSource = _settFile.NamesObjects;
            //firstСhoosedОbject = firstObject;
            cbFirstClassChoose.SelectedIndex = disSpace.FirstBasisObject - 1;


            cbSecondClassChoose.ItemsSource = _settFile.NamesObjects;
            //secondChoosedObject = secondObject;
            cbSecondClassChoose.SelectedIndex = disSpace.SecondBasisObject - 1;

            cbThirdClassChoose.ItemsSource = _settFile.NamesObjects;
            //thirdChoosedObject = thirdObject;
            cbThirdClassChoose.SelectedIndex = disSpace.ThirdBasisObject - 1;

            //sizeOfSpace = spaceSize;

            if (disSpace.Space == Space.ThreeDimensional)
            {
                Is3DSpace.IsChecked = true;
            }

            cbBasisObjectColorMode.IsChecked = disSpace.BasicObjectsColorMode;
            //cbAutomaticlySettingUpOfReference.IsChecked = referencedObjectsMode;

            InitializeReferencedObjects();
        }

        private void btBackAndSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbFirstClassChoose.SelectedIndex.Equals(BadSelectedIndex))
            {
                Utils.ShowWarningMessage(BadFirstIndexWarningMessage);
                return;
            }

            if (cbSecondClassChoose.SelectedIndex.Equals(BadSelectedIndex))
            {
                Utils.ShowWarningMessage(BadSecondIndexWarningMessage);
                return;
            }

            if (cbThirdClassChoose.SelectedIndex.Equals(BadSelectedIndex))
            {
                Utils.ShowWarningMessage(BadThirdIndexWarningMessage);
                return;
            }

            if ((!cbFirstClassChoose.SelectedIndex.Equals(BadSelectedIndex)) && (!cbSecondClassChoose.SelectedIndex.Equals(BadSelectedIndex)))
            {
                _dissimilaritySpace.FirstBasisObject = cbFirstClassChoose.SelectedIndex + 1;
                _dissimilaritySpace.SecondBasisObject = cbSecondClassChoose.SelectedIndex + 1;
                _dissimilaritySpace.ThirdBasisObject = cbThirdClassChoose.SelectedIndex + 1;
                _dissimilaritySpace.BasicObjectsColorMode = (bool)cbBasisObjectColorMode.IsChecked;
                this.Close();
            }
        }

        private void btBackWithoutSaving_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cb_FirstClassChoose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFirstClassChoose.SelectedIndex == BadSelectedIndex)
            {
                return;
            }

            if (!_settFile.IsMultimediaPicture)
            {
                return;
            }

            string path = _multimediaDataMaster.GetPathToMultimediaBySelectedIndex(cbFirstClassChoose.SelectedIndex, _settFile);
            pcFirstSelectedObject.Source = Utils.GetPictureFromSource(path);
        }

        private void cb_SecondClassChoose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSecondClassChoose.SelectedIndex == BadSelectedIndex)
            {
                return;
            }

            if (!_settFile.IsMultimediaPicture)
            {
                return;
            }

            string path = _multimediaDataMaster.GetPathToMultimediaBySelectedIndex(cbSecondClassChoose.SelectedIndex, _settFile);
            pcSecondSelectedObject.Source = Utils.GetPictureFromSource(path);
        }

        private void cbThirdClassChoose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbThirdClassChoose.SelectedIndex == BadSelectedIndex)
            {
                return;
            }

            if (!_settFile.IsMultimediaPicture)
            {
                return;
            }

            string path = _multimediaDataMaster.GetPathToMultimediaBySelectedIndex(cbThirdClassChoose.SelectedIndex, _settFile);
            pcThirdSelectedObject.Source = Utils.GetPictureFromSource(path);
        }

        private void Is3DSpace_Checked(object sender, RoutedEventArgs e)
        {
            //sizeOfSpace = "3D";
            _dissimilaritySpace.Space = Space.ThreeDimensional;
            cbThirdClassChoose.IsEnabled = true;
            pcThirdSelectedObject.Visibility = Visibility.Visible;
        }

        private void Is3DSpace_Unchecked(object sender, RoutedEventArgs e)
        {
            //sizeOfSpace = "2D";
            _dissimilaritySpace.Space = Space.TwoDimensional;
            cbThirdClassChoose.IsEnabled = false;
            pcThirdSelectedObject.Visibility = Visibility.Hidden;
        }

       /* private void cbAutomaticlySettingUpOfReference_Checked(object sender, RoutedEventArgs e)
        {
            //обратно отправлять флажок с режимом центровки и с списком описания центровых объектов
            InitializeReferencedObjects();
            
        }*/

        private void InitializeReferencedObjects()
        {
            List<string> referedObjectsSavedInfo; //SettFile.SourceMatrixFile
            referedObjectsSavedInfo = GetDataFromReferenceLogFile(_settFile.UniversalReader.SourceMatrixFile);

            if (referedObjectsSavedInfo == null)
            {
                referedObjectsSavedInfo = _dissimilaritySpace.GetReferencedObjectsWithClassNames(_settFile);

                WriteDataToReferenceLog(_settFile.UniversalReader.SourceMatrixFile, referedObjectsSavedInfo);
                btRefreshReferenceObjects.IsEnabled = true;
            }

            _referencedObjectsViewModel.ReferedObjectsInfo = new ObservableCollection<string>(referedObjectsSavedInfo);
        }

        private List<string> GetDataFromReferenceLogFile(string pathToAnyMatrix)
        {
            List<string> data = new List<string>();
            string pathToLogFile = pathToAnyMatrix
                    .Remove(pathToAnyMatrix.LastIndexOf('\\') + 1) + "adressOfReferenceLogFile.txt";
            try
            {
                if (File.Exists(pathToLogFile))
                {
                    data.AddRange(File.ReadAllLines(pathToLogFile));
                    return data;
                }
            }
            catch
            {
                Utils.ShowErrorMessage(ReadingDirAdressErrorMessage);
            }
            return null;
        }

        private void WriteDataToReferenceLog(string pathToAnyMatrix, List<string> data)
        {
            string pathToPictureContentAdressLog = pathToAnyMatrix.Remove(pathToAnyMatrix.LastIndexOf('\\') + 1)
                + "adressOfReferenceLogFile.txt";
            try
            {
                using (WriteTextToFile file = new WriteTextToFile(pathToPictureContentAdressLog))
                {
                    foreach (string line in data)
                    {
                        file.WriteLine(line);
                    }
                }
            }
            catch
            {
                Utils.ShowErrorMessage(CreatingLogFileErrorMessage);
            }

        }

        /*private void cbAutomaticlySettingUpOfReference_Unchecked(object sender, RoutedEventArgs e)
        {
            btRefreshReferenceObjects.IsEnabled = false;
            cbAutomaticlySettingUpOfReference.IsChecked = false;
            lbReferencedObjects.Items.Clear();
        }*/

        private void btRefreshReferenceObjects_Click(object sender, RoutedEventArgs e)
        {
            List<string> referedObjectsSavedInfo = _dissimilaritySpace.GetReferencedObjectsWithClassNames(_settFile);

            _referencedObjectsViewModel.ReferedObjectsInfo = new ObservableCollection<string>(referedObjectsSavedInfo);

            WriteDataToReferenceLog(_settFile.UniversalReader.SourceMatrixFile, referedObjectsSavedInfo);
        }

        private void lbReferencedObjects_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}
