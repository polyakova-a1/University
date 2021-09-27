// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using VisualChart3D.Common;
using VisualChart3D.Common.Multimedia;

namespace VisualChart3D
{
    /// <summary>
    /// Interaction logic for ListObjects.xaml
    /// </summary>
    /// 
    //-----------------------------------------------------
    public partial class ListObjects : Window
    {
        private const string GettingFileByNameErrorMessage = "Ошибка при выборке и проверке имени файла по имени объекта.";
        private const string DefaultPicturePath = "Resources/empty_picture.jpg";
        private const string ObjectsClassNameDispInfoFormat = "Объект №{0}\tКласс: {1}\tИмя: {2}";
        private const string ObjectsClassDispInfoFormat = "Объект №{0}\tКласс: {1}";

        private const int MuteSoundLevel = 0;
        private const int BadIndex = -1;

        private BaseMultimediaDataMaster _multimediaDataMaster;
        private string _adressPictureDirectory;
        private MultimediaLoadType _multimediaLoadingType;
        private List<int> _selectedObjectsID; //все кроме переменных - с большой буквой
        private List<string> _namesOfObjects;
        private List<string> _namesOfClasses;
        private List<string> countOfClasses;
        private List<SavedPictureInfo> _savedPathsToPictures;
        private Engine _settings;
        private double[,] _coords;
        private double _currentSoundLevel;
        private bool _isMuted;
        private Common.DataBinding.ObjectListViewModel _objectsListViewModel;

        private bool _isInformationLoaded; // Поймали ли в фокус объекты и будет ли отражена инфа о них
        private bool _isMultimediaPicture;
        private bool _isMultimediaAudio;

        //private bool _currentIndexFlag; // - для того, чтобы компенсировать баг сброса индекса при обновлении бокса

        /// <summary>
        /// Переменная-буфер
        /// </summary>
        public event EventHandler CloseEvent;
        //------------------------------------
        public bool IsClosing = false;

        public ListObjects()
        {
            InitializeComponent();

            _multimediaDataMaster = new BaseMultimediaDataMaster();
            _isMultimediaPicture = false;
            _isMultimediaAudio = false;
            //_currentIndexFlag = false;
            _isMuted = false;
            _adressPictureDirectory = null;

            _objectsListViewModel = new Common.DataBinding.ObjectListViewModel();
            DataContext = _objectsListViewModel;
        }

        public void setCoords(double[,] newCoords)
        {
            _coords = newCoords;
        }

        public void SetListObjects(Engine settFiles, int[] idxArr, double[,] coordsCurrent)
        {
            _coords = coordsCurrent;
            Picture.Source = new BitmapImage(new Uri(DefaultPicturePath, UriKind.Relative));
            _isInformationLoaded = false;
            /*_isPicturesByID = false;
            _isPicturesByName = false;
            _isPicturesByClassInterval = false;
            _isPicturesByClassStartObject = false;*/

            //ListBoxObjects.Items.Clear();
            //-------------------------------

            //-------------------------------
            if (idxArr == null)
            {
                _isInformationLoaded = false;
                return;
            }

            _isInformationLoaded = true;

            _selectedObjectsID = new List<int>();
            _namesOfObjects = new List<string>();
            _namesOfClasses = new List<string>();
            _objectsListViewModel.ObjectListItems.Clear();
            _settings = settFiles;

            countOfClasses = settFiles.classStartPosition;
            Title = "Список объектов. Количество:" + idxArr.Length;
            _selectedObjectsID.AddRange(idxArr);
            _savedPathsToPictures = new List<SavedPictureInfo>();

            _adressPictureDirectory = settFiles.multimediaFolderPath;
            _multimediaLoadingType = settFiles.MultimediaLoadingType;
            _isMultimediaPicture = settFiles.IsMultimediaPicture;
            _isMultimediaAudio = settFiles.IsMultimediaAudio;

            if (settFiles.NamesObjectSelected)
            {
                foreach (int i in idxArr)
                {
                    _namesOfObjects.Add(settFiles.NamesObjects[i]);
                    _namesOfClasses.Add(settFiles.ClassesName[i]);
                    _objectsListViewModel.ObjectListItems.Add(string.Format(ObjectsClassNameDispInfoFormat, i, settFiles.ClassesName[i],
                        settFiles.NamesObjects[i]));

                    //ListBoxObjects.Items.Add(string.Format("Объект №{0}\tКласс: {1}\tИмя: {2}", i, settFiles.ClassesName[i],
                    //    settFiles.NamesObjects[i]));
                    //даже если у нас есть имена объектов по текущей логике всеравно можно загрузить посредством айди
                }
            }
            else
            {
                foreach (int i in idxArr)
                {
                    _namesOfObjects.Add(settFiles.NamesObjects[i]);
                    _namesOfClasses.Add(settFiles.ClassesName[i]);
                    _objectsListViewModel.ObjectListItems.Add(string.Format(ObjectsClassDispInfoFormat, settFiles.NamesObjects[i], settFiles.ClassesName[i]));

                    //ListBoxObjects.Items.Add(string.Format("Объект №{0}\tКласс: {1}", settFiles.NamesObjects[i], settFiles.ClassesName[i]));

                }
            }

            ListBoxObjects.SelectedIndex = 0;
            ListBoxObjects.ScrollIntoView(ListBoxObjects.SelectedItem);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            CloseEvent?.Invoke(this, new EventArgs());
        }

        public void DisplayObjectCoords(int selectedInted)
        {
            tbCurrentObjectCoords.Text =
                    "x=" + _coords[(_selectedObjectsID[selectedInted]), 0] + Environment.NewLine +
                    "y=" + _coords[(_selectedObjectsID[selectedInted]), 1] + Environment.NewLine +
                    "z=" + _coords[(_selectedObjectsID[selectedInted]), 2];
        }

        private void ListBoxObjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            /*
Пофиксить вывод координат, не все оси соответствуют изображенным.
             
            if (_currentIndexFlag)
            {
                _currentIndexFlag = false;
                return;
            }*/

            if (mdMultimediaPlayer.Source != null && mdMultimediaPlayer.CanPause)
            {
                mdMultimediaPlayer.Pause();
            }

            if (!IsCorrectSelection())
            {
                return;
            }

            //if ((ListBoxObjects.SelectedIndex != -1) && ((string)ListBoxObjects.Items[ListBoxObjects.SelectedIndex] != String.Empty) && (_isInformationLoaded))

            DisplayObjectCoords(ListBoxObjects.SelectedIndex);
            CallBackPoint.callbackEventHandler(ListBoxObjects.SelectedIndex);

            if (!_isMultimediaPicture && !_isMultimediaAudio)
            {
                return;
            }


            if (_isMultimediaPicture)
            {
                string path = _multimediaDataMaster.GetPathToMultimediaBySelectedIndex(_selectedObjectsID[ListBoxObjects.SelectedIndex], _settings);
                Picture.Source = Utils.GetPictureFromSource(path);
            }

            if (_isMultimediaAudio)
            {
                //throw new NotImplementedException();
                MediaDataMaster mediaMaster = new MediaDataMaster();
                string path = mediaMaster.GetPathToMultimediaBySelectedIndex(_selectedObjectsID[ListBoxObjects.SelectedIndex], _settings);
                mdMultimediaPlayer.Source = new Uri(path);
            }


            /*if (_multimediaLoadingType == MultimediaLoadType.ByObjectID || _multimediaLoadingType == MultimediaLoadType.ByObjectName)
            {
                List<string> Pictures = Get_Pictures_Files_List(Directory.GetFiles(_adressPictureDirectory));
                if (_multimediaLoadingType == MultimediaLoadType.ByObjectID)
                {
                    Picture.Source = Add_Picture_On_Screen(Get_Picture_Adress(Pictures, _numberOfObjects[ListBoxObjects.SelectedIndex]));
                }

                //нужен рефакторинг массивов и флаговой структуры, все устарело.
                if (_multimediaLoadingType == MultimediaLoadType.ByObjectName)
                {
                    Picture.Source = Add_Picture_On_Screen(Get_Picture_Adress(Pictures, _namesOfObjects[ListBoxObjects.SelectedIndex]));
                }

            }

            if (_multimediaLoadingType == MultimediaLoadType.ByClassStartObjects || _multimediaLoadingType == MultimediaLoadType.ByClassInterval)
            {
                string result = _savedPathsToPictures.Find(path => path.label.Equals((Int32.Parse(_namesOfObjects[ListBoxObjects.SelectedIndex]) - 1).ToString())).path;
                if (result != null)
                {
                    Picture.Source = Add_Picture_On_Screen(result);
                    return;
                }

                String substring = String.Empty;
                int k;
                List<string> Pictures;
                int cur_index;

                if (Directory.Exists(_adressPictureDirectory))
                {
                    DirectoryInfo[] dirs = new DirectoryInfo(_adressPictureDirectory).GetDirectories();
                    foreach (var item in dirs)
                    {
                        if (item.Name.Equals(_namesOfClasses[ListBoxObjects.SelectedIndex]))
                        {
                            Pictures = Get_Pictures_Files_List(Directory.GetFiles(item.FullName));
                            cur_index = Int32.Parse(_namesOfObjects[ListBoxObjects.SelectedIndex]) - 1;

                            for (k = 0; k < countOfClasses.Count - 1; k++)
                            {
                                if ((cur_index >= Int32.Parse(countOfClasses[k])) && (cur_index < Int32.Parse(countOfClasses[k + 1])))
                                {
                                    cur_index -= Int32.Parse(countOfClasses[k]);
                                    break;
                                }
                            }

                            if ((Pictures.Count == (Int32.Parse(countOfClasses[k + 1]) - Int32.Parse(countOfClasses[k]))))
                            {
                                substring = Pictures[cur_index];
                                _savedPathsToPictures.Add(new SavedPictureInfo(cur_index.ToString(), substring));
                                Picture.Source = Add_Picture_On_Screen(substring);
                            }
                            else
                            {
                                substring = Get_Picture_Adress(Pictures, cur_index + 1);
                                Picture.Source = Add_Picture_On_Screen(substring);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    Utils.ShowErrorMessage(String.Format(PicturesFolderNotFoundErrorMessage, _adressPictureDirectory, Environment.NewLine, PicturesLogFileName));

                }*/

            //ДатаБиндинг! И тогда не будет костыля ниже!!!
            if ((!ListBoxObjects.SelectedValue.ToString().Contains("\tФайл:"))/* && (ListBoxObjects.SelectedIndex != BadIndex)*/)
            {
                //_objectsListViewModel.ObjectListItems[ListBoxObjects.SelectedIndex] += string.Format(" \tФайл: " + _multimediaDataMaster.LastMultimediaSourceItemPath);
                //Изменить селектед валюе можно только на значение, существующее в источнике данных. 

                ListBoxObjects.SelectionChanged -= ListBoxObjects_SelectionChanged;
                _objectsListViewModel.ObjectListItems[ListBoxObjects.SelectedIndex] += string.Format(" \tФайл: " + _multimediaDataMaster.LastMultimediaSourceItemPath);
                ListBoxObjects.SelectionChanged += ListBoxObjects_SelectionChanged;
                //_objectsListViewModel.SelectedObject = value;
                /*_currentIndexFlag = true;
                int current_index = ListBoxObjects.SelectedIndex;
                ListBoxObjects.Items[ListBoxObjects.SelectedIndex] += string.Format(" \tФайл: " + _multimediaDataMaster.LastMultimediaSourceItemPath);
                _currentIndexFlag = true;
                ListBoxObjects.SelectedIndex = current_index; //ПЕРЕДЕЛАТЬ КОНСТРУКЦИЮ!!! Двойной список, полностью повторяющий вывод картинки и инфы о ней*/
            }
            //}

        }

        private bool IsCorrectSelection()
        {
            if (ListBoxObjects.SelectedIndex == BadIndex)
            {
                return false;
            }

            if (String.IsNullOrEmpty((string)ListBoxObjects.Items[ListBoxObjects.SelectedIndex]))
            {
                return false;
            }

            if (!_isInformationLoaded)
            {
                return false;
            }

            return true;
        }

        private void btMultiMediaPlay_Click(object sender, RoutedEventArgs e)
        {
            if (mdMultimediaPlayer.Source == null)
            {
                return;
            }

            mdMultimediaPlayer.Play();
        }

        private void btMultiMediaPause_Click(object sender, RoutedEventArgs e)
        {
            if (mdMultimediaPlayer.Source == null)
            {
                return;
            }

            mdMultimediaPlayer.Pause();
        }

        private void btMultiMediaMute_Click(object sender, RoutedEventArgs e)
        {
            if (mdMultimediaPlayer.Source == null)
            {
                return;
            }

            _isMuted = !_isMuted;

            if (_isMuted)
            {
                _currentSoundLevel = mdMultimediaPlayer.Volume;
                mdMultimediaPlayer.Volume = MuteSoundLevel;
                btMultiMediaMute.Content = "Включить звук";
                //_isMuted = false;
            }
            else
            {
                mdMultimediaPlayer.Volume = _currentSoundLevel;
                btMultiMediaMute.Content = "Заглушить";
                //_isMuted = true;
            }
        }

        private void mdMultimediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            mdMultimediaPlayer.Stop();
            //mdMultimediaPlayer.Position = TimeSpan.FromSeconds(0);
        }

        /*private void ListBoxObjects_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject _do = (DependencyObject)e.MouseDevice.DirectlyOver;
            if (!IsLoaded)
            {
                return;
            }
        }*/

        private void ListBoxObjects_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsCorrectSelection())
            {
                return;
            }

            if (!_isMultimediaAudio)
            {
                return;
            }

            btMultiMediaPlay_Click(sender, new RoutedEventArgs());
        }
    }
}
