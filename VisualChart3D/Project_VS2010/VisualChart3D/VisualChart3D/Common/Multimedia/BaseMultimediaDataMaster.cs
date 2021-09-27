// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;

namespace VisualChart3D.Common.Multimedia
{
    public struct SavedPictureInfo
    {
        public string label;
        public string path;
        public SavedPictureInfo(string newLabel, string newPath)
        {
            label = newLabel;
            path = newPath;
        }
    }

    class BaseMultimediaDataMaster
    {
        private const int EmptyIndex = -1;

        private const string ObjectNameErrorMessage = "Ошибка при выборке и проверке имени файла по имени объекта.";
        private const string PicturesFolderNotFoundErrorMessage = "Не найдена папка с картинками по адресу '{0}'.{1} Возможно, удаление лог-файла '{2}' в папке с исходными данными и перезапуск программы поможет решить проблему.";
        private const string PicturesLogFileName = "PictureAdressLog.txt";
        private const string DefaultPicturePath = "Resources/empty_picture.jpg";
        private List<string> Pictures;
        private int cur_index;
        private MultimediaLoadType _multimediaLoadType;
        //private List<string> _savedPathsToPictures;
        //private List<SavedPictureInfo> _savedPathsToPictures;
        private String _caltulatedPath;

        public BaseMultimediaDataMaster()
        {
            Pictures = null;
            cur_index = EmptyIndex;
            //_savedPathsToPictures = new List<string>();
        }

        public string LastMultimediaSourceItemPath { get => _caltulatedPath; }
        protected virtual List<string> MultimediaExtenshions { get; } = new List<string> { ".jpg", ".jpeg", ".bmp", ".png" };


        /// <summary>
        /// Сравнение имени объекта сначала с именем файла без расширения, затем с именем файла с расширением.
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        private bool IsMultimediaExist(string adress, string objectName)
        {
            bool result;
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(adress);
                result = Utils.CompareStrings(objectName, fileName);

                if (!result)
                {
                    fileName = Path.GetFileName(adress);
                    result = Utils.CompareStrings(objectName, fileName);
                }

                return result;
            }
            catch (Exception)
            {
                Utils.ShowErrorMessage(ObjectNameErrorMessage);
                return false;
            }
        }

        /*DONE*/
        private List<string> GetMultimediaFileList(string[] All_Files)
        {
            List<string> List_of_Files = new List<string>();

            List_of_Files.AddRange(Array.FindAll(All_Files, p => MultimediaExtenshions.Contains(Path.GetExtension(p))));

            /*foreach (string file in All_Files)
            {
                foreach (string extension in ImageExtensions)
                {
                    //if (file.EndsWith(extension))
                    if(Utils.CompareStrings(Path.GetExtension(file), extension))
                    {
                        List_of_Files.Add(file);
                        break;
                    }
                }
            }*/

            return List_of_Files;
        }

        //DONE
        private string GetMultimediaAdress(List<string> Pics, string name_of_picture/*, Image Picture*/)
        {
            string picturePath = Pics.Find(pic => (IsMultimediaExist(pic, name_of_picture)));
            string result = picturePath ?? DefaultPicturePath;
            //Picture.Source = GetPictureFromSource(result);
            return result;
        }

        //DONE
        public virtual string GetPathToMultimediaBySelectedIndex(/*Image repaintingImage,*/ int SelectedIndex, Engine SettFile)
        {
            /*bool isPicture = SettFile.IsMultimediaPicture;

            if ((SelectedIndex == EmptyIndex) || (!isPicture))
            {
                return;
            }*/

            if (!Directory.Exists(SettFile.multimediaFolderPath))
            {
                //(repaintingImage);
                Utils.ShowErrorMessage(String.Format(PicturesFolderNotFoundErrorMessage, SettFile.multimediaFolderPath, Environment.NewLine, PicturesLogFileName));
                return DefaultPicturePath;
            }

            _multimediaLoadType = SettFile.MultimediaLoadingType;

            /*if (Pictures != null && _multimediaLoadType == SettFile.MultimediaLoadingType)
            {
                //Ну, загрузим из уже полученных, позволит не как статик юзать.
                throw new NotImplementedException();
            }*/

            if (SettFile.MultimediaLoadingType == MultimediaLoadType.ByObjectID || SettFile.MultimediaLoadingType == MultimediaLoadType.ByObjectName)
            {
                Pictures = GetMultimediaFileList(Directory.GetFiles(SettFile.multimediaFolderPath));


                //Для имен объектов пусть так и будет. А для номеров объектов - либо есть пронумерованные картинки, либо пусть будет доставание из папки по номеру, если в ней 341 объект.
                _caltulatedPath = GetMultimediaAdress(Pictures, SettFile.NamesObjects[SelectedIndex]);

                if (SettFile.MultimediaLoadingType == MultimediaLoadType.ByObjectID && _caltulatedPath == DefaultPicturePath)
                {
                    if (Pictures.Count == SettFile.NamesObjects.Count)
                    {
                        _caltulatedPath = Pictures[SelectedIndex];
                    }
                    
                }

                return _caltulatedPath;
                //repaintingImage.Source = GetPictureFromSource(_caltulatedPath);

                /*if (SettFile.MultimediaLoadingType == MultimediaLoadType.ByObjectName)
                {
                    _caltulatedPath = Get_Picture_Adress(Pictures, SettFile.NamesObjects[SelectedIndex]);
                    repaintingImage.Source = GetPictureFromSource(_caltulatedPath);
                }*/

            }

            else if (SettFile.MultimediaLoadingType == MultimediaLoadType.ByClassStartObjects || SettFile.MultimediaLoadingType == MultimediaLoadType.ByClassInterval)
            {
                /*string result = _savedPathsToPictures.Find(path => path.label.Equals((Int32.Parse(_namesOfObjects[ListBoxObjects.SelectedIndex]) - 1).ToString())).path;
                if (result != null)
                {
                    repaintingImage.Source = GetPictureFromSource(result);
                    return;
                }*/

                int k;

                DirectoryInfo[] dirs = new DirectoryInfo(SettFile.multimediaFolderPath).GetDirectories();

                var dirWithNeedClassData = Array.Find(dirs, p => Utils.CompareStrings(p.Name, SettFile.ClassesName[SelectedIndex]));

                if (dirWithNeedClassData == null)
                {
                    //SetDefaultValue(repaintingImage);
                    return DefaultPicturePath;
                }

                Pictures = GetMultimediaFileList(Directory.GetFiles(dirWithNeedClassData.FullName));
                cur_index = Int32.Parse(SettFile.NamesObjects[SelectedIndex]);

                //Тут вычисляется номер объекта в диапозоне его класса. Тоесть если класс от 0 до ста, а айди 87, то и 87 вернет.
                for (k = 0; k < SettFile.classStartPosition.Count - 1; k++)
                {
                    if ((cur_index >= Int32.Parse(SettFile.classStartPosition[k])) && (cur_index <= Int32.Parse(SettFile.classStartPosition[k + 1])))
                    {
                        cur_index -= Int32.Parse(SettFile.classStartPosition[k]);
                        break;
                    }
                }

                //Если число объектов в папке равно числу объектов в классе, то и берем по айдишнику.
                if ((Pictures.Count == (Int32.Parse(SettFile.classStartPosition[k + 1]) - Int32.Parse(SettFile.classStartPosition[k]))))
                {
                    _caltulatedPath = Pictures[cur_index - 1];
                    //repaintingImage.Source = GetPictureFromSource(_caltulatedPath);
                }
                else
                {
                    //Если не равно, то по имени объекта. 
                    _caltulatedPath = GetMultimediaAdress(Pictures, SettFile.NamesObjects[cur_index]);
                    //repaintingImage.Source = GetPictureFromSource(_caltulatedPath);
                }

                return _caltulatedPath;

            }
            else
            {
                return DefaultPicturePath;
            }  
        }

        /*private void SetDefaultValue(Image repaintingImage)
        {
            repaintingImage.Source = GetPictureFromSource(DefaultPicturePath);
        }*/
    }
}

/* Из ListObjects-------------------------------------------------------------------
 * /*private bool Is_Picture_Exist(string adress, int number_of_picture)
{
    try
    {
        int end = adress.IndexOf('.');
        if (end == -1)
            return false;
        int start = end - 1;
        // char.IsDigit -  в рефракторинг
        while (adress[start] != '\\')
        {
            if (((byte)adress[start] < 48) && ((byte)adress[start] > 57))  // если в имени файла есть символы, то ошибка.
                return false;                                              // либо запилить остановку до символов и считывать такое - игрок99
            start--;
        }
        start++;
        String name_of_file = adress.Substring(start, end - start);
        if (number_of_picture == Int32.Parse(name_of_file))
            return true;
        else
            return false;
    }
    catch (Exception)
    {
        Utils.ShowErrorMessage(GettingFileByNameErrorMessage);
        //возможно стоит просто не информировать а просто по буллу выводить пустую картинку при ошибке
        return false;
    }
}*/

/*private string Get_Picture_Adress(List<string> Pics, int order_of_picture)
{

    string result = _savedPathsToPictures.Find(path => path.label.Equals(order_of_picture.ToString())).path;

    if (!String.IsNullOrEmpty(result))
    {
        Picture.Source = Add_Picture_On_Screen(result);
        return result;
    }//нужен тест!!!!

    result = Pics.Find(pic => (Is_Picture_Exist(pic, order_of_picture))) ?? "Resources / empty_picture.jpg";
    Picture.Source = Add_Picture_On_Screen(result);
    _savedPathsToPictures.Add(new SavedPictureInfo(order_of_picture.ToString(), result));
    return result;
}*/

/*private string Get_Picture_Adress(List<string> Pics, string name_of_picture)
{
    string result = _savedPathsToPictures.Find(path => path.label.Equals(name_of_picture)).path;

    if (result != null)
    {
        Picture.Source = Add_Picture_On_Screen(result);
        return result;
    }//нужен тест!!!!

    result = Pics.Find(pic => (Is_Picture_Exist(pic, name_of_picture))) ?? "Resources / empty_picture.jpg";
    Picture.Source = Add_Picture_On_Screen(result);
    _savedPathsToPictures.Add(new SavedPictureInfo(name_of_picture.ToString(), result));
    return result;
}*/


//----------------------------------------------------------------------------------------
/*ИЗ DISSPACE--------------------------------------------------------------------------
private void Paining(Image Picture, int SelectedIndex)
{
    bool is_Picture = settFile.isMultimediaByClassInterval || settFile.isMultimediaByClassStartObjects || settFile.isMultimediaByObjectID || settFile.isMultimediaByObjectName;
    if ((SelectedIndex != -1) && (is_Picture))
    {
        if ((settFile.isMultimediaByObjectID) || (settFile.isMultimediaByObjectName))
        {
            List<string> Pictures = Get_Pictures_Files_List(Directory.GetFiles(settFile.multimediaFolderPath));
            if (settFile.isMultimediaByObjectID)
            {  //эксперимент - проверка надобности отдельной функции типа инт для выборки картинки. Вместо инт посылаем стринг
                Picture.Source = Add_Picture_On_Screen(Get_Picture_Adress(Pictures, settFile.NamesObjects[SelectedIndex], Picture));
            }

            if (settFile.isMultimediaByObjectName)
            {
                Picture.Source = Add_Picture_On_Screen(Get_Picture_Adress(Pictures, settFile.NamesObjects[SelectedIndex], Picture));
            }
        }

        if ((settFile.isMultimediaByClassStartObjects) || (settFile.isMultimediaByClassInterval))
        {
            String substring;
            int k;
            List<string> Pictures;
            int cur_index;
            DirectoryInfo[] directories = new DirectoryInfo(settFile.multimediaFolderPath).GetDirectories();

            foreach (var directory in directories)
            {
                if (directory.Name.Equals(settFile.ClassesName[SelectedIndex]))
                {
                    Pictures = Get_Pictures_Files_List(Directory.GetFiles(directory.FullName));
                    cur_index = Int32.Parse(settFile.NamesObjects[SelectedIndex]);

                    for (k = 0; k < settFile.classStartPosition.Count - 1; k++)
                    {
                        if ((cur_index >= Int32.Parse(settFile.classStartPosition[k])) && (cur_index <= Int32.Parse(settFile.classStartPosition[k + 1])))
                        {
                            cur_index -= Int32.Parse(settFile.classStartPosition[k]);
                            break;
                        }
                    }

                    if ((Pictures.Count == (Int32.Parse(settFile.classStartPosition[k + 1]) - Int32.Parse(settFile.classStartPosition[k]))))
                    {
                        substring = Pictures[cur_index - 1];
                        Picture.Source = Add_Picture_On_Screen(substring);
                    }
                    else
                    {
                        substring = Get_Picture_Adress(Pictures, cur_index, Picture);
                        Picture.Source = Add_Picture_On_Screen(substring);
                    }

                    break;
                }
            }
        }
    }
}
 * private bool Is_Picture_Exist(string adress, int number_of_picture)
{
    try
    {
        int end = adress.IndexOf('.');

        if (end == -1)
        {
            return false;
        }

        int start = end - 1;

        // char.IsDigit -  в рефракторинг
        while (adress[start] != '\\')
        {
            //if (((byte)adress[start] < 48) && ((byte)adress[start] > 57))  // если в имени файла есть символы, то ошибка.
            if (char.IsDigit(adress[start]))
            {  // если в имени файла есть символы, то ошибка.
                return false;
            }// либо запилить остановку до символов и считывать такое - игрок99
            start--;
        }

        start++;
        String name_of_file = adress.Substring(start, end - start);

        if (number_of_picture == Int32.Parse(name_of_file))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    catch (Exception)
    {
        return false;
    }
}

private BitmapImage Add_Picture_On_Screen(string adress)
{
    BitmapImage bi = new BitmapImage();
    bi.BeginInit();
    bi.UriSource = new Uri(adress, UriKind.RelativeOrAbsolute);  //urisource тупит и картинка не показывается дефолтная
    bi.EndInit();
    return bi;
}

private string Get_Picture_Adress(List<string> Pics, int order_of_picture, Image Picture)
{
    string result = Pics.Find(pic => (Is_Picture_Exist(pic, order_of_picture))) ?? "Resources / empty_picture.jpg";
    Picture.Source = Add_Picture_On_Screen(result);
    return result;
}

-------------------------------------------------------------------------------------------*/
