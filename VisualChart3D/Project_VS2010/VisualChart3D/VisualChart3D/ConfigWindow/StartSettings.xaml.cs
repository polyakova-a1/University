// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using System.IO;
using VisualChart3D.Common;
using System.Collections.Generic;

namespace VisualChart3D.ConfigWindow
{

    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class StartSettings : Window
    {
        private const string ChooseMediaSubFoldersMessage = "Выберете каталог с подкаталогами мультимедиа объектов соответствующих классов";
        private const string ChooseMediaFolderMessage = "Выберете папку с мультимедиа объектов";
        private const string GettingClassDataErrorMessage = "Ошибка при выборке данных о классе! {0}";
        private const string DisplayingClassListErrorMessage = "Ошибка при выводе списка классов.";
        private const string ClassFilesChooseErrorMessage = "Ошибка! Файл с классами объектов не выбран.";
        private const string ClassDirNotFoundWarningMessage = "Внимание, не найдены директории для классов: {0}. \n Для данных классов выведение изображений объектов недоступно.";

        private const string NotImplementedMessage = "Внимание! Алгоритм не был реализован. Пожалуйста, свяжитесь с разработчиком для прояснения ситуации.";
        private const string BadInputFileType = "Ошибка чтения выбранного типа файла.";
        private const string ClassObjectsInfoFormat = "Имя класса - {0}; Количество: {1}.";

        private int[] _numberOfObjectsOfClass;

        /// <summary>
        /// Успешность принятия настройки
        /// </summary>
        /// 
        private bool _resultDialog = false;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="settFiles">настрока исходных данных</param>
		public StartSettings(Engine settFiles)
        {

            InitializeComponent();

            if (settFiles == null)
            {
                settFiles = new Engine();
            }

            SettFiles = settFiles;
            StartForm();
            UpdateInterface();
        }

        /// <summary>
        /// Обновить интерфейс в соответвии с настройками
        /// </summary>
        private void UpdateInterface()
        {
            if (SettFiles.UniversalReader == null)
            {
                return;
            }

            tbDataFilePath.Text = SettFiles.UniversalReader.SourceMatrixFile;
            switch (SettFiles.UniversalReader.InputFileType)
            {
                case InputFileType.Text:
                    cbStandartInput.IsChecked = true;
                    break;

                case InputFileType.SVM:
                    cbSVMInput.IsChecked = true;
                    break;

                case InputFileType.CSV:
                    cbCSVInput.IsChecked = true;

                    SwitchEnabledClass(!String.IsNullOrEmpty(SettFiles.UniversalReader.ClassNameColumn));
                    SwitchEnabledObject(!String.IsNullOrEmpty(SettFiles.UniversalReader.ObjectNameColumn));

                    //SwitchEnabledObjectAndClassIU(false);
                    break;

                default:
                    throw new NotImplementedException("");
            }

            if (SettFiles.MultimediaLoadingType == MultimediaLoadType.ByObjectID)
            {
                cbNamesPictures.IsChecked = true;
                rbMultimediaById.IsChecked = true;
                tbMultimediaFolderPath.IsEnabled = true;
                tbMultimediaFolderPath.Text = SettFiles.multimediaFolderPath;
            }

            if (SettFiles.MultimediaLoadingType == MultimediaLoadType.ByObjectName)
            {
                cbNamesPictures.IsChecked = true;
                rbMultimediaByObjectsName.IsChecked = true;
                tbMultimediaFolderPath.IsEnabled = true;
                tbMultimediaFolderPath.Text = SettFiles.multimediaFolderPath;
            }

            if (SettFiles.MultimediaLoadingType == MultimediaLoadType.ByClassInterval || SettFiles.MultimediaLoadingType == MultimediaLoadType.ByClassStartObjects)
            {
                cbNamesPictures.IsChecked = true;
                rbMultimediaByClassName.IsChecked = true;
                tbMultimediaFolderPath.IsEnabled = true;
                tbMultimediaFolderPath.Text = SettFiles.multimediaFolderPath;
            }

            if (SettFiles.ClassObjectSelected)
            {
                cbClassObject.IsChecked = true;
                tbClassObjectPath.Text = SettFiles.ClassObjectFile;
                switch (SettFiles.ClassObjectType)
                {
                    case ClassInfoType.OneToOne:
                        rbClassObjectOneToOne.IsChecked = true;
                        break;
                    case ClassInfoType.CountObj:
                        rbClassObjectCountObj.IsChecked = true;
                        break;
                    case ClassInfoType.StartObjects:
                        rbClassObjectStartObjects.IsChecked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (SettFiles.ClassEqualSelected && !string.IsNullOrEmpty(SettFiles.ClassEqualCountStr))
                {
                    cbClassEqual.IsChecked = true;
                    tbcbClassEqual.Text = SettFiles.ClassEqualCountStr;
                }
                else
                {
                    cbClassEqual.IsChecked = false;
                }
            }

            if (SettFiles.NamesObjectSelected)
            {
                cbNamesObject.IsChecked = true;
                tbNamesObjectPath.Text = SettFiles.NamesObjectFile;
            }
        }

        /// <summary>
        /// Обновление интерфейса при первом запуске
        /// </summary>
		private void StartForm()
        {

            if (SettFiles.ClassObjectSelected)
            {
                switch (SettFiles.ClassObjectType)
                {
                    case ClassInfoType.OneToOne:
                        rbClassObjectOneToOne.IsChecked = true;
                        break;

                    case ClassInfoType.CountObj:
                        rbClassObjectCountObj.IsChecked = true;
                        break;

                    case ClassInfoType.StartObjects:
                        rbClassObjectStartObjects.IsChecked = true;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                rbClassObject_Checked(rbClassObjectOneToOne, new RoutedEventArgs());
            }
            else
            {
                cbClassObject_Unchecked(cbClassObject, new RoutedEventArgs());

                if (SettFiles.ClassEqualSelected && !string.IsNullOrEmpty(SettFiles.ClassEqualCountStr))
                {
                    cbClassEqual.IsChecked = true;
                    tbcbClassEqual.Text = SettFiles.ClassEqualCountStr;
                }
                else
                {
                    cbClassEqual_Unchecked(cbClassEqual, new RoutedEventArgs());
                }
            }

            if (SettFiles.NamesObjectSelected)
            {
                cbNamesObject_Checked(cbClassObject, new RoutedEventArgs());
            }
            else
            {
                cbNamesObject_Unchecked(cbClassObject, new RoutedEventArgs());
            }

        }

        /// <summary>
        /// Считывание значений из формы в класс настроек
        /// </summary>
        /// <returns></returns>
        private Engine ReadValues()
        {
            //Engine result = new Engine(); // тут происходит запись параметров с формы

            Engine result = SettFiles;

            result.AlgorithmType = AlgorithmType.NoAlgorithm;

            if ((tbMultimediaFolderPath.Text != null) && (cbNamesPictures.IsChecked == true || cbNamesAudios.IsChecked == true))
            {
                if (((rbMultimediaById.IsChecked == true)
                    || (rbMultimediaByObjectsName.IsChecked == true)
                    || (rbMultimediaByClassName.IsChecked == true))
                        && (!String.IsNullOrEmpty(tbMultimediaFolderPath.Text)))
                {

                    result.multimediaFolderPath = tbMultimediaFolderPath.Text.ToString();


                    if ((bool)rbMultimediaById.IsChecked)
                    {
                        result.MultimediaLoadingType = MultimediaLoadType.ByObjectID;
                    }
                    else if ((bool)rbMultimediaByObjectsName.IsChecked)
                    {
                        result.MultimediaLoadingType = MultimediaLoadType.ByObjectName;
                    }
                    else if (rbMultimediaByClassName.IsChecked == true)
                    {
                        if ((bool)rbClassObjectCountObj.IsChecked)
                        {
                            result.MultimediaLoadingType = MultimediaLoadType.ByClassInterval;
                        }
                        else
                        {
                            result.MultimediaLoadingType = MultimediaLoadType.ByClassStartObjects;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    /*result.isMultimediaByObjectID = (bool)rbMultimediaById.IsChecked;
                    result.isMultimediaByObjectName = (bool)rbMultimediaByObjectsName.IsChecked;

                    if (rbMultimediaByClassName.IsChecked == true)
                    {
                        result.isMultimediaByClassInterval = (bool)rbClassObjectCountObj.IsChecked;
                        result.isMultimediaByClassStartObjects = (bool)rbClassObjectStartObjects.IsChecked;
                    }*/

                    result.IsMultimediaPicture = (bool)cbNamesPictures.IsChecked;
                    result.IsMultimediaAudio = (bool)cbNamesAudios.IsChecked;
                }
            }

            if (cbClassObject.IsChecked.Value)
            {
                result.ClassObjectSelected = true;
                result.ClassObjectFile = tbClassObjectPath.Text != null
                    ? tbClassObjectPath.Text.ToString()
                    : string.Empty;

                if (rbClassObjectOneToOne.IsChecked.Value)
                {
                    result.ClassObjectType = ClassInfoType.OneToOne;
                }

                if (rbClassObjectCountObj.IsChecked.Value)
                {
                    result.ClassObjectType = ClassInfoType.CountObj;
                }

                if (rbClassObjectStartObjects.IsChecked.Value)
                {
                    result.ClassObjectType = ClassInfoType.StartObjects;
                }
            }
            else
            {
                result.ClassObjectSelected = false;

                if (cbClassEqual.IsChecked.Value)
                {
                    result.ClassEqualSelected = true;
                    result.ClassEqualCountStr = tbcbClassEqual.Text;
                }
            }

            if (cbNamesObject.IsChecked.Value)
            {
                result.NamesObjectSelected = true;
                result.NamesObjectFile = tbNamesObjectPath.Text != null
                    ? tbNamesObjectPath.Text.ToString()
                    : string.Empty;
            }
            else if (SettFiles.UniversalReader.InputFileType != InputFileType.CSV)
            {
                result.NamesObjectSelected = false;
            }

            /*if (rbMetricEuclidean.IsChecked.Value)
            {
                result.Metrics = FastMapMetric.Euclidean;
            }
            else
            {
                if (rbMetricNonEuclidean.IsChecked.Value)
                {
                    result.Metrics = FastMapMetric.NonEuclidean;
                }
            }*/

            return result;
        }

        /// <summary>
        /// Открыть файл. 
        /// </summary>
        /// <param name="lb">отображение расположения</param>
        private bool OpenFile(System.Windows.Controls.TextBox lb, Boolean switchModeToClassInputChecking)
        {
            OpenFileDialog ofDlg = new OpenFileDialog
            {
                Multiselect = false
            };

            if (lb.Text != null && !String.IsNullOrEmpty(lb.Text.ToString()))
            {
                ofDlg.InitialDirectory = lb.Text.ToString();
            }

            ofDlg.RestoreDirectory = true;
            if (ofDlg.ShowDialog().Value)
            {
                lb.Text = ofDlg.FileName;

                if (switchModeToClassInputChecking)
                {
                    Engine temp = ReadValues();

                    string errors = temp.Validation();

                    if ((!String.IsNullOrEmpty(errors)) || (temp.UniqClassesName == null))
                    {
                        Utils.ShowErrorMessage(String.Format(GettingClassDataErrorMessage, errors));
                        lb.Text = String.Empty;

                        return false;
                    }

                    lbUniqueClasses.Items.Clear();

                    //SettFiles.UniqClassesName = new System.Collections.Generic.List<string>();
                    //SettFiles.UniqClassesName.AddRange(temp.UniqClassesName.ToArray());
                    //getNumberOfObjectsOfClass(temp);
                    lbUniqueClasses.IsEnabled = true;
                    _numberOfObjectsOfClass = getNumberOfObjectsOfClass(temp);

                    int k = 0;
                    foreach (string className in SettFiles.UniqClassesName)
                    {
                        lbUniqueClasses.Items.Add("Имя класса - " + className + " ; Количество: " + _numberOfObjectsOfClass[k]);
                        k++;
                    }

                    return true;
                }
            }

            return false;
        }

        private int[] getNumberOfObjectsOfClass(Engine temp)
        {
            int countOfClasses = temp.UniqClassesName.Count; //переделать лейблы в эдиты
            switch (temp.ClassObjectType)
            {
                case ClassInfoType.OneToOne:
                    _numberOfObjectsOfClass = temp.GetClassPositionsOnOneToOneMode(temp.UniqClassesName); // получаем длинну каждого класса 
                    break;

                case ClassInfoType.CountObj:
                    _numberOfObjectsOfClass = new int[countOfClasses];

                    //uniqueClassesNames
                    for (int i = 0; i < countOfClasses; i++)
                    {
                        _numberOfObjectsOfClass[i] = Int32.Parse(temp.ArrayClassesCountObj[i, 0]);
                    }

                    break;

                case ClassInfoType.StartObjects:
                    _numberOfObjectsOfClass = new int[countOfClasses];

                    //костылище. Будет убран как будет переписан интефрейс. 
                    var x = SettFiles.ClassesName;
                    //uniqueClassesNames
                    for (int i = 1; i < countOfClasses + 1; i++)
                    {
                        _numberOfObjectsOfClass[i - 1] = Int32.Parse(temp.classStartPosition[i]) - Int32.Parse(temp.classStartPosition[i - 1]);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(DisplayingClassListErrorMessage);
            }

            return _numberOfObjectsOfClass;
        }

        private void ClearPreviousClassObjectsSetings()
        {
            SettFiles.ClassesName = null;
            SettFiles.NamesObjects = null;

            ClearPreviousSelection();

            if (SettFiles.UniversalReader == null)
            {
                return;
            }

            if (SettFiles.UniversalReader.InputFileType == InputFileType.CSV)
            {
                SwitchEnabledClass(true);
                SwitchEnabledObject(true);
            }

            SwitchEnabledMultimedia(true);
        }

        private void SwitchEnabledMultimedia(bool v)
        {
            btMultimediaFolderBrowse.IsEnabled = v;
        }

        private void ClearPreviousSelection()
        {
            cbClassObject.IsChecked = false;
            cbNamesObject.IsChecked = false;
            //cbClassObject_Unchecked(cbClassObject, new RoutedEventArgs());
            //cbNamesObject_Unchecked(cbNamesObject, new RoutedEventArgs());
        }

        private void cbClassObject_Checked(object sender, RoutedEventArgs e)
        {
            tbClassObjectPath.IsEnabled = true;
            btClassObjectBrowse.IsEnabled = true;

            rbClassObjectOneToOne.IsEnabled = true;
            rbClassObjectCountObj.IsEnabled = true;
            rbClassObjectStartObjects.IsEnabled = true;

            cbClassEqual.IsChecked = false;
            cbClassEqual.IsEnabled = false;

            switch (SettFiles.ClassObjectType)
            {
                case ClassInfoType.OneToOne:
                    rbClassObjectOneToOne.IsChecked = true;
                    break;
                case ClassInfoType.CountObj:
                    rbClassObjectCountObj.IsChecked = true;
                    break;
                case ClassInfoType.StartObjects:
                    rbClassObjectStartObjects.IsChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void cbClassObject_Unchecked(object sender, RoutedEventArgs e)
        {
            tbClassObjectPath.Text = String.Empty;
            tbClassObjectPath.IsEnabled = false;

            btClassObjectBrowse.IsEnabled = false;

            rbClassObjectOneToOne.IsEnabled = false;
            rbClassObjectCountObj.IsEnabled = false;
            rbClassObjectStartObjects.IsEnabled = false;

            cbClassEqual.IsEnabled = true;

            rbMultimediaByClassName.IsEnabled = false;
            rbMultimediaByClassName.IsChecked = false;

            tbMultimediaFolderPath.Text = String.Empty;
        }

        private void cbNamesObject_Checked(object sender, RoutedEventArgs e)
        {
            tbNamesObjectPath.IsEnabled = true;
            btNamesObjectBrowse.IsEnabled = true;

            if (cbNamesPictures.IsChecked == true)
            {
                rbMultimediaByObjectsName.IsEnabled = true;
            }
        }

        private void cbNamesObject_Unchecked(object sender, RoutedEventArgs e)
        {
            tbNamesObjectPath.Text = String.Empty;
            tbNamesObjectPath.IsEnabled = false;
            btNamesObjectBrowse.IsEnabled = false;
            rbMultimediaByClassName.IsEnabled = false;

            if (cbNamesPictures.IsChecked == true)
            {
                rbMultimediaByObjectsName.IsEnabled = false;
            }
        }

        private void rbClassObject_Checked(object sender, RoutedEventArgs e)
        {

            tbClassObjectPath.Text = String.Empty;
            lbUniqueClasses.Items.Clear();

            if ((cbNamesPictures.IsChecked == true) && ((rbClassObjectCountObj.IsChecked == true) || (rbClassObjectStartObjects.IsChecked == true)))
            {
                rbMultimediaByClassName.IsEnabled = true;
                rbMultimediaByClassName.IsChecked = true;
            }
            if (rbClassObjectOneToOne.IsChecked.Value)
            {
                SettFiles.ClassObjectType = ClassInfoType.OneToOne;
            }
            else if (rbClassObjectCountObj.IsChecked.Value)
            {
                SettFiles.ClassObjectType = ClassInfoType.CountObj;
            }
            else if (rbClassObjectStartObjects.IsChecked.Value)
            {
                SettFiles.ClassObjectType = ClassInfoType.StartObjects;
            }
        }

        private void btClassObjectBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFile(tbClassObjectPath, true);
        }

        private void btNamesObjectBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFile(tbNamesObjectPath, false);
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            ResultDialog = false;
            this.Close();
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            if (((bool)cbClassObject.IsChecked) && (String.IsNullOrEmpty(tbClassObjectPath.Text)))
            {
                Utils.ShowErrorMessage(ClassFilesChooseErrorMessage);
                return;
            }

            if (SettFiles.UniversalReader == null)
            {
                Utils.ShowErrorMessage(ClassFilesChooseErrorMessage);
                return;
            }

            Engine temp = ReadValues();
            string settingErrorDescription = temp.Validation();

            if ((bool)cbClassObject.IsChecked)
            {
                _numberOfObjectsOfClass = getNumberOfObjectsOfClass(temp);
                temp.numberOfObjectsOfClass = new int[_numberOfObjectsOfClass.Length];
                Array.Copy(_numberOfObjectsOfClass, temp.numberOfObjectsOfClass, _numberOfObjectsOfClass.Length);
            }
            else if (!String.IsNullOrEmpty(temp.UniversalReader.ClassNameColumn))
            {
                temp.numberOfObjectsOfClass = new int[_numberOfObjectsOfClass.Length];
                Array.Copy(_numberOfObjectsOfClass, temp.numberOfObjectsOfClass, _numberOfObjectsOfClass.Length);

                temp.ClassObjectSelected = true;
                temp.ClassObjectFile = SettFiles.UniversalReader.SourceMatrixFile;
            }

            if (temp.UniqClassesName == null)
            {
                Utils.ShowErrorMessage(String.Format(GettingClassDataErrorMessage, settingErrorDescription));
                return;
            }

            if (!string.IsNullOrEmpty(settingErrorDescription))
            {
                Utils.ShowErrorMessage(settingErrorDescription);
            }
            else
            {
                SettFiles = temp;
                ResultDialog = true;
                DialogResult = true;
                Close();
            }
        }

        private void cbClassEqual_Checked(object sender, RoutedEventArgs e)
        {
            tbcbClassEqual.IsEnabled = true;
        }

        private void cbClassEqual_Unchecked(object sender, RoutedEventArgs e)
        {
            tbcbClassEqual.IsEnabled = false;
            tbcbClassEqual.Text = string.Empty;
        }

        private void cbDirPic_Checked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = true;
        }

        private void cbDirPic_Unchecked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = false;
        }

        private void btNameFolderPicBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();

            if ((rbMultimediaByClassName.IsEnabled == true) && (rbMultimediaByClassName.IsChecked == true))
            {
                FBD.Description = ChooseMediaSubFoldersMessage;
            }
            else
            {
                FBD.Description = ChooseMediaFolderMessage;
            }

            FBD.ShowDialog();
            if (FBD.SelectedPath != String.Empty)
            {
                if ((bool)rbMultimediaByClassName.IsChecked)
                {
                    CheckClassSubfoldersInPictureFolder(FBD.SelectedPath, SettFiles.UniqClassesName);
                }

                //на событие загрузки матрицы расстояний или попарных сравнений запилить обновление пути к картинкам.
                tbMultimediaFolderPath.Text = FBD.SelectedPath;
                if (rbMultimediaById.IsChecked == true)
                {
                    Utils.ToSerialize(
                        SettFiles.UniversalReader.SourceMatrixFile,
                        new Common.Logs.MultimediaLog((bool)cbNamesPictures.IsChecked, (bool)cbNamesAudios.IsChecked, FBD.SelectedPath, MultimediaLoadType.ByObjectID)
                    );

                    //WriteDataToPictureLog(SettFiles.UniversalReader.SourceMatrixFile, FBD.SelectedPath, "PicturesById");
                    return;
                }

                else if (rbMultimediaByObjectsName.IsChecked == true)
                {
                    Utils.ToSerialize(
                        SettFiles.UniversalReader.SourceMatrixFile,
                        new Common.Logs.MultimediaLog((bool)cbNamesPictures.IsChecked, (bool)cbNamesAudios.IsChecked, FBD.SelectedPath, MultimediaLoadType.ByObjectName)
                    );

                    //WriteDataToPictureLog(SettFiles.UniversalReader.SourceMatrixFile, FBD.SelectedPath, "PicturesByObjectsName");
                    return;
                }

                else if (rbMultimediaByClassName.IsChecked == true)
                {
                    if ((bool)rbClassObjectCountObj.IsChecked)
                    {
                        Utils.ToSerialize(
                            SettFiles.UniversalReader.SourceMatrixFile ,
                            new Common.Logs.MultimediaLog((bool)cbNamesPictures.IsChecked, (bool)cbNamesAudios.IsChecked, FBD.SelectedPath, MultimediaLoadType.ByClassInterval)
                        );
                    }
                    else
                    {
                        Utils.ToSerialize(
                            SettFiles.UniversalReader.SourceMatrixFile,
                            new Common.Logs.MultimediaLog((bool)cbNamesPictures.IsChecked, (bool)cbNamesAudios.IsChecked, FBD.SelectedPath, MultimediaLoadType.ByClassStartObjects)
                        );
                    }

                    //WriteDataToPictureLog(SettFiles.UniversalReader.SourceMatrixFile, FBD.SelectedPath, "PicturesByClassName");
                    return;
                }
            }
        }

        private void CheckClassSubfoldersInPictureFolder(String pictureFolderPath, System.Collections.Generic.List<string> uniqueClassesName)
        {
            string notFoundedClassDirs = String.Empty;
            bool isDirectoryFound;
            DirectoryInfo[] dirs = new DirectoryInfo(pictureFolderPath).GetDirectories();

            foreach (var className in uniqueClassesName)
            {
                isDirectoryFound = false;
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (Utils.CompareStrings(dirs[i].Name, className))
                    //if (string.Compare(dirs[i].Name, className, true) == 0)
                    {
                        isDirectoryFound = true;
                        break;
                    }
                }

                if (!isDirectoryFound)
                {
                    notFoundedClassDirs += className + ", ";
                }
            }

            if (!String.IsNullOrEmpty(notFoundedClassDirs))
            {
                Utils.ShowWarningMessage(String.Format(ClassDirNotFoundWarningMessage, notFoundedClassDirs.Remove(notFoundedClassDirs.Length - 2)));

            }
        }

        private void cbNamesPictures_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cbNamesAudios.IsChecked != false || cbNamesAudios.IsEnabled != false)
            {
                return;
            }

            rbMultimediaByClassName.IsEnabled = false;
            rbMultimediaById.IsEnabled = false;
            rbMultimediaById.IsChecked = false;
            rbMultimediaByObjectsName.IsEnabled = false;
            rbMultimediaByObjectsName.IsChecked = false;

            btMultimediaFolderBrowse.IsEnabled = false;

        }

        private void cbNamesPictures_Checked(object sender, RoutedEventArgs e)
        {
            if (cbNamesAudios.IsChecked == true && cbNamesAudios.IsEnabled == true)
            {
                return;
            }

            if (cbNamesObject.IsChecked == true)
            {
                rbMultimediaByObjectsName.IsEnabled = true;
            }

            rbMultimediaById.IsEnabled = true;

            if ((rbClassObjectStartObjects.IsChecked == true) || (rbClassObjectCountObj.IsChecked == true))
            {
                rbMultimediaByClassName.IsEnabled = true;
            }
        }

        private void rbPicturesById_Checked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = true;

            if ((rbMultimediaByObjectsName.IsChecked == true) && (rbMultimediaByObjectsName.IsEnabled == true))
            {
                rbMultimediaByObjectsName.IsChecked = false;
            }

            if ((rbMultimediaByClassName.IsChecked == true) && (rbMultimediaByClassName.IsEnabled == true))
            {
                rbMultimediaByClassName.IsChecked = false;
            }
            //Пометить отсюда в будущий класс с инфой вариант выбора.
        }

        private void rbPicturesByObjectsName_Checked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = true;
            if ((rbMultimediaById.IsChecked == true) && (rbMultimediaById.IsEnabled == true))
            {
                rbMultimediaById.IsChecked = false;
            }
            if ((rbMultimediaByClassName.IsChecked == true) && (rbMultimediaByClassName.IsEnabled == true))
            {
                rbMultimediaByClassName.IsChecked = false;
            }
            //Пометить отсюда в будущий класс с инфой вариант выбора.
        }

        private void rbPicturesById_Unchecked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = false;
        }

        private void rbPicturesByObjectsName_Unchecked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = false;
        }

        private void rbPicturesByClassName_Checked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = true;
        }

        private void rbPicturesByClassName_Unchecked(object sender, RoutedEventArgs e)
        {
            btMultimediaFolderBrowse.IsEnabled = false;
        }

        private void rbClassObjectCountObj_Unchecked(object sender, RoutedEventArgs e)
        {
            rbMultimediaByClassName.IsEnabled = false;
        }

        private void rbClassObjectStartObjects_Unchecked(object sender, RoutedEventArgs e)
        {
            rbMultimediaByClassName.IsEnabled = false;
        }

        private void cbDisSpaceMod_Checked(object sender, RoutedEventArgs e)
        {
            //rbObjectAttribute.IsEnabled = false;
            //tbMinkovskiDegree.IsEnabled = false;
            //rbMetricEuclidean.IsEnabled = false;
            //rbMetricNonEuclidean.IsEnabled = false;
        }

        private void cbDisSpaceMod_Unchecked(object sender, RoutedEventArgs e)
        {
            //rbObjectAttribute.IsEnabled = true;
            //tbMinkovskiDegree.IsEnabled = true;
            //rbMetricEuclidean.IsEnabled = true;
            //rbMetricNonEuclidean.IsEnabled = true;

        }

        /*private void cbFastMapAlg_Checked(object sender, RoutedEventArgs e)
        {
            gbSpaceType.IsEnabled = true;
        }

        private void cbFastMapAlg_Unchecked(object sender, RoutedEventArgs e)
        {
            gbSpaceType.IsEnabled = false;
        }*/

        private void btDataFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            InputFileType inputFileType;
            bool? showDialog;

            ReadInputFileType(out inputFileType);

            Visibility radioButtonsVisibility = Visibility.Visible;
            switch (inputFileType)
            {
                case InputFileType.SVM:
                    radioButtonsVisibility = Visibility.Collapsed;
                    goto case InputFileType.Text;
                case InputFileType.Text:
                    var dataInputStandartWindow = new InputDataWindows.StandartInputWindow(SettFiles.UniversalReader, inputFileType);

                             dataInputStandartWindow.rbDistanceMatrix.Visibility = radioButtonsVisibility;
                    dataInputStandartWindow.rbObjectAttributeMatrix3D.Visibility = radioButtonsVisibility;

                    showDialog = dataInputStandartWindow.ShowDialog();
                    if (!showDialog.GetValueOrDefault())
                    {
                        return;
                    }

                    SettFiles.UniversalReader = dataInputStandartWindow.Reader;

                    ClearPreviousClassObjectsSetings();

                    //SwitchEnabledClass(true);
                    //SwitchEnabledObject(true);

                    break;

                case InputFileType.CSV:
                    InputDataWindows.SCVInputWindow dataInputCSVWindow = new InputDataWindows.SCVInputWindow(SettFiles.UniversalReader);

                    showDialog = dataInputCSVWindow.ShowDialog();
                    if (!showDialog.GetValueOrDefault())
                    {
                        return;
                    }

                    lbUniqueClasses.Items.Clear();
                    SettFiles.UniversalReader = dataInputCSVWindow.Reader;

                    ClearPreviousClassObjectsSetings();

                    //SwitchCheckedClassCSV(true);

                    //Разделить на отруб функционала с классами и имен объектов отдельно.
                    //SwitchEnabledObjectAndClassIU(false);

                    if (IsClassesInitialized(SettFiles.UniversalReader))
                    {
                        var uniqueClassesNames = SettFiles.GetUniqClass();
                        SettFiles.UniqClassesName = uniqueClassesNames;
                        _numberOfObjectsOfClass = SettFiles.GetClassPositionsOnOneToOneMode(uniqueClassesNames);

                        int k = 0;
                        foreach (string className in uniqueClassesNames)
                        {
                            lbUniqueClasses.Items.Add(String.Format(ClassObjectsInfoFormat, className, _numberOfObjectsOfClass[k]));
                            k++;
                        }

                    }

                    if (IsObjectsInitialized(SettFiles.UniversalReader))
                    {
                        SettFiles.NamesObjectSelected = true;
                        SwitchEnabledObject(false);
                    }

                    SwitchEnabledClass(String.IsNullOrEmpty(SettFiles.UniversalReader.ClassNameColumn));

                    break;

                default:
                    throw new NotImplementedException();
            }

            tbDataFileType.Text = Enum.GetName(typeof(InputFileType), inputFileType);

            tbDataFilePath.Text = SettFiles.UniversalReader.SourceMatrixFile;


            cbNamesPictures.IsEnabled = true;
            cbNamesAudios.IsEnabled = true;
            rbMultimediaById.IsEnabled = true;
            rbMultimediaByObjectsName.IsEnabled = true;
            rbMultimediaByClassName.IsEnabled = true;

            GetDataFromPictureLogger();

            //Открыть вкладки. Проверить, все ли значения уйдут по нужным полям. 
            //Опосля перейти к механизму парсинга и сделать заглушку для сиэсви.
        }

        private bool IsObjectsInitialized(Common.DataReader.IUniversalReader reader)
        {
            return !String.IsNullOrEmpty(SettFiles.UniversalReader.ObjectNameColumn);
        }

        private bool IsClassesInitialized(Common.DataReader.IUniversalReader reader)
        {
            return !String.IsNullOrEmpty(SettFiles.UniversalReader.ClassNameColumn);
        }

        private void GetDataFromPictureLogger()
        {
            //string[] logData = ReadDataFromPictureLogFile(SettFiles.UniversalReader.SourceMatrixFile);
            Common.Logs.MultimediaLog multimediaLog = Utils.ToDeserialize<Common.Logs.MultimediaLog>(SettFiles.UniversalReader.SourceMatrixFile);
            if (multimediaLog == null)
            {
                return;
            }

            if (multimediaLog.IsPictures)
            {
                cbNamesPictures.IsEnabled = multimediaLog.IsPictures;
                cbNamesPictures.IsChecked = multimediaLog.IsPictures;
            }

            if (multimediaLog.IsMultimedia)
            {
                cbNamesAudios.IsEnabled = multimediaLog.IsMultimedia;
                cbNamesAudios.IsChecked = multimediaLog.IsMultimedia;
            }

            tbMultimediaFolderPath.Text = multimediaLog.DataPath;

            switch (multimediaLog.DataLoadType)
            {
                case MultimediaLoadType.ByClassInterval:
                    rbMultimediaByClassName.IsChecked = true;
                    break;
                case MultimediaLoadType.ByClassStartObjects:
                    rbMultimediaByClassName.IsChecked = true;
                    break;
                case MultimediaLoadType.ByObjectID:
                    rbMultimediaById.IsChecked = true;
                    break;
                case MultimediaLoadType.ByObjectName:
                    rbMultimediaByObjectsName.IsChecked = true;
                    break;
                default:
                    throw new NotImplementedException();
            }

            /*cbNamesPictures.IsEnabled = true;
            cbNamesPictures.IsChecked = true;
            tbMultimediaFolderPath.Text = logData[0];
            switch (logData[1])
            {
                case "PicturesById":
                    rbMultimediaById.IsChecked = true;
                    break;
                case "PicturesByObjectsName":
                    rbMultimediaByObjectsName.IsChecked = true;
                    break;
                case "PicturesByClassName":
                    rbMultimediaByClassName.IsChecked = true;
                    break;

                default:
                    break;
            }*/
        }

        private void SwitchCheckedClassCSV(bool isChecked)
        {
            cbClassObject.IsChecked = true;
            rbClassObjectOneToOne.IsChecked = true;
        }

        private void SwitchEnabledClass(bool isEnabled)
        {
            cbClassObject.IsEnabled = isEnabled;

            rbClassObjectOneToOne.IsEnabled = isEnabled;
            rbClassObjectStartObjects.IsEnabled = isEnabled;
            rbClassObjectCountObj.IsEnabled = isEnabled;

            cbClassEqual.IsEnabled = isEnabled;
            tbcbClassEqual.IsEnabled = isEnabled;
        }

        private void SwitchEnabledObject(bool isEnabled)
        {
            cbNamesObject.IsEnabled = isEnabled;
            btNamesObjectBrowse.IsEnabled = isEnabled;
        }

        private void ReadInputFileType(out InputFileType inputFileType)
        {
            if (cbStandartInput.IsChecked == true)
            {
                inputFileType = InputFileType.Text;
                return;
            }
            else if (cbCSVInput.IsChecked == true)
            {
                inputFileType = InputFileType.CSV;
                return;
            }
            else if (cbSVMInput.IsChecked == true)
            {
                inputFileType = InputFileType.SVM;
                return;
            }

            throw new ArgumentException(BadInputFileType);

        }

        public Engine SettFiles { get; private set; }
        public bool ResultDialog { get => _resultDialog; set => _resultDialog = value; }

        private void cbNamesAudios_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cbNamesPictures.IsChecked != false || cbNamesPictures.IsEnabled != false)
            {
                return;
            }

            rbMultimediaByClassName.IsEnabled = false;
            rbMultimediaById.IsEnabled = false;
            rbMultimediaById.IsChecked = false;
            rbMultimediaByObjectsName.IsEnabled = false;
            rbMultimediaByObjectsName.IsChecked = false;

            btMultimediaFolderBrowse.IsEnabled = false;
        }

        private void cbNamesAudios_Checked(object sender, RoutedEventArgs e)
        {
            if (cbNamesPictures.IsChecked == true && cbNamesPictures.IsEnabled == true)
            {
                return;
            }

            if (cbNamesObject.IsChecked == true)
            {
                rbMultimediaByObjectsName.IsEnabled = true;
            }

            rbMultimediaById.IsEnabled = true;

            if ((rbClassObjectStartObjects.IsChecked == true) || (rbClassObjectCountObj.IsChecked == true))
            {
                rbMultimediaByClassName.IsEnabled = true;
            }
        }
    }
}
