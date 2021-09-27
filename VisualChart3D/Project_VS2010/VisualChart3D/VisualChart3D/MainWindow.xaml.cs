// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using StarMathLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualChart3D.Common;
using VisualChart3D.Common.Visualization;
using VisualChart3D.ConfigWindow;
using VisualChart3D.VisualizationWindows;
using Segment = System.ValueTuple<double, int, int>;
using Statistics = System.ValueTuple<double,
                                     System.Collections.Generic.List<int>,
                                     double,
                                     double,
                                     double,
                                     System.ValueTuple<double, int, int>,
                                     System.Collections.Generic.List<int>>;

namespace VisualChart3D
{
    public partial class MainWindow
    {
        //Setting up offset for axes
        private const float AxeOffsetForFastMap = 0.07f;
        private const float StandartAxeOffset = 0.0f;
        private const int LowerSpaceDimensional = 3;
        private const int DoubleClick = 2;

        private const string AlgorithmAlreadySelectedMessage = "Алгоритм уже выбран.";
        private const string BadAlgorithmSelectedMessage = "Не задан алгоритм.";
        private const string WindowTitleFormat = "Visual Chart 3D. Alg - {0}, {1}";
        private const string PathToHelpFile = @"help\index.htm";
        private const string DataErrorMessage = "Исключительная ситуация. Ошибка исходных данных";
        private const string CreatingGraphicErrorMessage = "Ошибка при построении графика\n {0} \n {1} \n {2}";
        private const string FileNotFoundErrorMessage = "Файл \"{0}\" не найден";
        private const string CreatingFileErrorMessage = "Не удалось создать файл {0}";
        private const string NotInitializedDataErrorMessage = "Не заданы исходные данные!";
        private const string CannotLogDataMessage = "Внимание. Не удалось создать лог-файл {0}.";

        /// <summary>
        /// Расширение для файла настроек класса
        /// </summary>
        private const string ExtColorSchema = ".clrSchema";

        //Будет убрано в интерфейсное поле и использоваться согласно паттерну Strategy
        private DisSpace _dissimiliaritySpace;
        private IKohonen _kohonenProjection;
        private ISammon _sammonsProjection;
        private IFastMap _fastMapProjection;
        private IVisualizer _pcaProjection;

        /// <summary>
        /// Тип визуализации
        /// </summary>
        private TypePlot _typePlot = TypePlot.Main;

        /// <summary>
        /// Настройки классов
        /// </summary>
        private ClassVisualisationSettings _settingsClasses; //private

        /// <summary>
        /// Настройки исходных данных, используемых в данный момент
        /// </summary>
        private Engine _settFilesCurrent;
        /// <summary>
        /// Текущие координаты объектов
        /// </summary>
        private double[,] _projectionCoords;
        /// <summary>
        /// Настройки исходных данных, при визуализации основных компонентов
        /// </summary>
        private Engine _settFilesMain;

        /// <summary>
        /// Размер объектов, при визуализации основных компонентов
        /// </summary>
        private double _sizeMain;

        /// <summary>
        /// Трансформация модели
        /// </summary>
        private readonly TransformMatrix _transformMatrix = new TransformMatrix();

        /// <summary>
        /// Данные текущего 3d графика 
        /// </summary>
        private ScatterChart3D _3DChartCurrent;

        /// <summary>
        /// Прямоугольник выделения
        /// </summary>
        private readonly ViewportRect _selectRect = new ViewportRect();

        /// <summary>
        /// Координаты точек текущего графика
        /// </summary>
        private Vertex3D[] _coordCurrent;

        /// <summary>
        /// Координаты точек главного графика
        /// </summary>
        private Vertex3D[] _coordMain;

        /// <summary>
        /// Индекс для прямоугольника выделения
        /// </summary>
        private readonly int _nRectModelIndex = -1;

        /// <summary>
        /// индекс для Viewport3d
        /// </summary>
        private int _nChartModelIndex = -1;

        /// <summary>
        /// Массив выделенных индексов
        /// </summary>
        private int[] _selectedIndex = new int[0];

        /// <summary>
        /// Окно, для отображения информации о выделенных объектов
        /// </summary>
        private ListObjects _listObjectWindow;

        private readonly ShortestOpenPathPlot  _sopp = new ShortestOpenPathPlot();
        private readonly Histogram _hist = new Histogram();
        private readonly PathProjection _cumulative = new PathProjection();

        public MainWindow()
        {
            CallBackPoint.callbackEventHandler = new CallBackPoint.callbackEvent(this.LightCurrentObject);
            InitializeComponent();
            _selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            Model3D model3D = new Model3D();
            List<Mesh3D> meshs = _selectRect.GetMeshes();
            _nRectModelIndex = model3D.UpdateModel(meshs, null, _nRectModelIndex, MainViewport);
            
            _sopp.Closing += _sopp.OnClosing;
            _sopp.DataChanged += (sender, args) =>
            {
                if (args is not VisualizationWindowEventArgs viswinArgs)
                    return;

                MnShortestOpenPath_Visualization_Graph.IsEnabled = viswinArgs.DataSource != null && viswinArgs.Graph != null;
            };
            
            _hist.Closing += _hist.OnClosing;
            _hist.DataChanged += (sender, args) =>
            {
                if (args is not VisualizationWindowEventArgs viswinArgs)
                    return;

                MnShortestOpenPath_Visualization_Histogram.IsEnabled = viswinArgs.DataSource != null && viswinArgs.Graph != null;
            };

            _cumulative.Closing += _cumulative.OnClosing;
            _cumulative.DataChanged += (sender, args) =>
            {
                if (args is not VisualizationWindowEventArgs viswinArgs)
                    return;

                MnShortestOpenPath_Visualization_ProjectionOnPath.IsEnabled = viswinArgs.DataSource != null && viswinArgs.Graph != null;
            };
        }

        private void LightCurrentObject(int objectNumber)
        {
        }

        /// <summary>
        /// Вращения/Перемещение и Увеличения 3d графика
        /// </summary>
        private void TransformChart()
        {
            if (_nChartModelIndex == -1)
            {
                return;
            }

            ModelVisual3D visual3D = (ModelVisual3D)(MainViewport.Children[_nChartModelIndex]);
            if (visual3D.Content == null)
            {
                return;
            }

            Transform3DGroup _3dGroup = visual3D.Content.Transform as Transform3DGroup;
            if (_3dGroup != null)
            {
                _3dGroup.Children.Clear();
                _3dGroup.Children.Add(new MatrixTransform3D(_transformMatrix.GetTotalMatrix()));
            }
        }

        private void PrincipalComponentAnalysisGeneration(PCA pca, bool firstGeneration = false)
        {
            if (!pca.ToProject())
            {
                return;
            }

            _projectionCoords = pca.Projection;
            //_projectionCoords = Utils.GetNormalizedData(rawCoors);

            int countCords = _projectionCoords.GetLength(0);
            _coordCurrent = new Vertex3D[countCords];

            if (!firstGeneration)
            {
                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }
            }
            else
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                SaveResultsAsFile(countCords);
            }
        }

        private void DissimilitarySpaceGeneration(DisSpace DissimiliaritySpace, bool firstGeneration = false)
        {
            double[,] rawCoors = DissimiliaritySpace.ToProject();
            _projectionCoords = Utils.GetNormalizedData(rawCoors);

            //_projectionCoords = DissimiliaritySpace.ToProject();
            int countCords = _projectionCoords.GetLength(0);
            _coordCurrent = new Vertex3D[countCords];

            if (!firstGeneration)
            {
                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }
            }
            else
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                SaveResultsAsFile(countCords);
            }
        }

        //МИХАЛЫЧ! НАДО НАКОНЕЦ ТО С МАССИВАМИ ДАННЫХ РАЗОБРАТЬСЯ, ДАБЫ ИЗБЕЖАТЬ ДУБЛИРОВАНИЕ КОДА!!!!!!!
        private bool KohonenMapGeneration(IVisualizer projection, bool firstGeneration = false)
        {
            if (!projection.ToProject())
            {
                return false;
            }

            int countCords = projection.Projection.GetLength(0);
            _projectionCoords = projection.Projection;
            _coordCurrent = new Vertex3D[countCords];

            if (!firstGeneration)
            {
                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }

            }
            else
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                SaveResultsAsFile(countCords);
            }

            return true;
        }
        
        private bool SammonsMapGeneration(IVisualizer visualizer, bool firstGeneration = false)
        {
            if (!_sammonsProjection.ToProject())
            {
                return false;
            }

            int countCords = visualizer.Projection.GetLength(0);
            _projectionCoords = Utils.GetNormalizedData(visualizer.Projection);

            _coordCurrent = new Vertex3D[countCords];

            if (!firstGeneration)
            {
                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }
            }
            else
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                SaveResultsAsFile(countCords);
            }

            return true;
        }

        private void SaveResultsAsFile(int countCords)
        {
            string fileName = _settFilesCurrent.UniversalReader.SourceMatrixFile + ".3D";

            try
            {
                using (WriteTextToFile file = new WriteTextToFile(fileName))
                {
                    file.WriteLineFormat("{0}", _settFilesCurrent.AlgorithmType);

                    for (int i = 0; i < countCords; i++)
                    {
                        file.WriteLineFormat("{0}\t{1}\t{2}\t{3}", _settFilesCurrent.ClassesName[i], _projectionCoords[i, 0],
                            _projectionCoords[i, 1], _projectionCoords[i, 2]);
                        _coordCurrent[i] = new Vertex3D
                        {
                            X = _projectionCoords[i, 0],
                            Y = _projectionCoords[i, 1],
                            Z = _projectionCoords[i, 2]
                        };
                    }
                }
            }
            catch
            {
                Utils.ShowWarningMessage(String.Format(CreatingFileErrorMessage, fileName));

                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }
            }

            //Костылище с осями.
            /*_coordCurrent[0] = new Vertex3D
            {
                X = 0,
                Y = 0,
                Z = 0
            };*/

        }

        private bool FastMapGeneration(bool firstGeneration = false)
        {
            int countCords = 0;
            /*FastMap fastMap = _settFilesCurrent.UniversalReader.SourceMatrixType == SourceFileMatrixType.MatrixDistance
                ? new FastMap(_settFilesCurrent.UniversalReader.ArraySource, _settFilesCurrent.Metrics)
                : new FastMap(CommonMatrix.ObjectAttributeToDistance(_settFilesCurrent.UniversalReader.ArraySource, _settFilesCurrent.CountObjects, _settFilesCurrent.UniversalReader.MinkovskiDegree), _settFilesCurrent.Metrics);
            */
            //_fastMapProjection = new FastMap(_settFilesCurrent.ArraySource, _settFilesCurrent.Metrics);
            //_projectionCoords = fastMap.GetCoordinates(fastMap.CountOfProjection);

            if (!_fastMapProjection.ToProject())
            {
                return false;
            }

            _projectionCoords = Utils.GetNormalizedData(_fastMapProjection.Projection);

            countCords = _fastMapProjection.Projection.GetLength(0);

            _coordCurrent = new Vertex3D[countCords];

            if (!firstGeneration)
            {
                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }
            }
            else
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                SaveResultsAsFile(countCords);
            }

            return true;
        }

        /// <summary>
        /// Определение оптимального размера объектов
        /// </summary>
        private void SizeDetection()
        {
            _settingsClasses.SizeObject = ClassVisualisationSettings.OptimalSize;
            //double nDataRange = _coordCurrent.Max(d => d.X);
            //_settingsClasses.SizeObject = nDataRange / 100;
        }

        private void canvasOn3D_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(MainViewport);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_3DChartCurrent == null)
                {
                    return;
                }

                if (Math.Abs(_selectRect.XMax - _selectRect.XMin) > 0.01)
                {
                    _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                    _selectedIndex = _3DChartCurrent.Select(_selectRect, _transformMatrix, MainViewport);
                    MeshGeometry3D meshGeometry = Model3D.GetGeometry(MainViewport, _nChartModelIndex);
                    _3DChartCurrent.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
                }

                _transformMatrix.OnMouseMove(pt, MainViewport);
                TransformChart();
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _selectRect.OnMouseMove(pt, MainViewport, _nRectModelIndex);
            }
            else
            {
                Point pt2 = _transformMatrix.VertexToScreenPt(new Point3D(0.5, 0.5, 0.3), MainViewport);
                string s1 = string.Format("Screen:({0:d},{1:d}), Predicated: ({2:d}, H:{3:d}), Window: {4}",
                    (int)pt.X, (int)pt.Y, (int)pt2.X, (int)pt2.Y, _typePlot.ToString());
                StatusPane.Text = s1;

            }
        }

        private void canvasOn3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _transformMatrix.OnLBtnUp();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (_nChartModelIndex == -1)
                {
                    return;
                }

                MeshGeometry3D meshGeometry = Model3D.GetGeometry(MainViewport, _nChartModelIndex);
                if (meshGeometry == null)
                {
                    return;
                }

                if (_typePlot == TypePlot.Main)
                {
                    _selectedIndex = _3DChartCurrent.Select(_selectRect, _transformMatrix, MainViewport);
                    _3DChartCurrent.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));

                    if (_listObjectWindow != null)
                    {
                        _listObjectWindow.SetListObjects(_settFilesCurrent, _selectedIndex, _projectionCoords);
                    }
                }
                else
                {
                    int[] selectedIndex = _3DChartCurrent.Select(_selectRect, _transformMatrix, MainViewport);
                    _3DChartCurrent.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));

                    if (_listObjectWindow != null)
                    {
                        _listObjectWindow.SetListObjects(_settFilesCurrent, selectedIndex, _projectionCoords);
                    }
                }
            }
        }

        private void canvasOn3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Math.Abs(_selectRect.XMax - _selectRect.XMin) > 0.01)
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                _selectedIndex = _3DChartCurrent.Select(_selectRect, _transformMatrix, MainViewport);
                MeshGeometry3D meshGeometry = Model3D.GetGeometry(MainViewport, _nChartModelIndex);
                _3DChartCurrent.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }

            _transformMatrix.OnWheel(e);
            TransformChart();
        }

        private void canvasOn3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_settFilesCurrent == null)
            {
                return;
            }

            if ((e.ClickCount == DoubleClick) 
                && (_settFilesCurrent.AlgorithmType != AlgorithmType.DisSpace))
            {
                if (_typePlot == TypePlot.Main && _selectedIndex.Length > 0)
                {
                    _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                    _typePlot = TypePlot.Subsidiary;
                    _settFilesMain = _settFilesCurrent;
                    _coordMain = _coordCurrent;
                    _sizeMain = _settingsClasses.SizeObject;

                    SubsidiaryEngine settTemp = new SubsidiaryEngine(_settFilesCurrent, _selectedIndex);
                    _settFilesCurrent = settTemp;

                    switch (_settFilesCurrent.AlgorithmType)
                    {
                        case AlgorithmType.FastMap:
                            _fastMapProjection = new FastMap(_settFilesCurrent.ArraySource);
                            FastMapGeneration();
                            break;

                        case AlgorithmType.KohonenMap:
                            _kohonenProjection = new KohonenProjection(_settFilesCurrent.ArraySource, LowerSpaceDimensional);
                            if (!KohonenMapGeneration(_kohonenProjection))
                            {
                                _settFilesCurrent = _settFilesMain;
                                _typePlot = TypePlot.Main;
                            }                            

                            break;

                        case AlgorithmType.SammonsMap:
                            _sammonsProjection = new SammonsProjection(_settFilesCurrent.ArraySource, LowerSpaceDimensional);
                            if (!SammonsMapGeneration(_sammonsProjection))
                            {
                                _settFilesCurrent = _settFilesMain;
                                _typePlot = TypePlot.Main;
                            }                            

                            break;

                        case AlgorithmType.NoAlgorithm:
                            NoAlgorithmGeneration();
                            break;

                        default:
                            break;
                    }


                    //DissimilitarySpaceGeneration(first_object_to_dis_space, second_object_to_dis_space,false);
                    SizeDetection();

                    DrawScatterPlot();
                    return;
                }
            }

            Point pt = e.GetPosition(MainViewport);

            if (e.ChangedButton == MouseButton.Left)
            {
                _transformMatrix.OnLBtnDown(pt);
            }

            else if (e.ChangedButton == MouseButton.Right)
            {
                _selectRect.OnMouseDown(pt, MainViewport, _nRectModelIndex);
            }
        }

        private void MnKohonenMapSett_Click(object sender, RoutedEventArgs e)
        {
            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if (_kohonenProjection == null)
            {
                Utils.ShowWarningMessage(BadAlgorithmSelectedMessage);
                return;
            }

            KohonenMapConfigs kohonenMapConfigWindow = new KohonenMapConfigs(_kohonenProjection.IterationsCount, _kohonenProjection.IterationLimit);
            bool? showDialog = kohonenMapConfigWindow.ShowDialog();

            if ((bool)showDialog)
            {
                _kohonenProjection.IterationsCount = kohonenMapConfigWindow.CountOfIteration;
                KohonenMapGeneration(_kohonenProjection, true);
                DrawScatterPlot();
            }
        }

        private void MnDisSpaceSett_Click(object sender, RoutedEventArgs e)
        {
            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if(_dissimiliaritySpace == null)
            {                
                Utils.ShowWarningMessage(BadAlgorithmSelectedMessage);
                return;
            }

            DissimilaritySpaceConfigs DisSpcWin = new DissimilaritySpaceConfigs(_settFilesCurrent, _dissimiliaritySpace);

            bool? showDialog = DisSpcWin.ShowDialog();

            // создаем окошко и передаем туда сеттингфайлз, получаем -
            DissimilitarySpaceGeneration(_dissimiliaritySpace, true);
            DrawScatterPlot();

            if (MnListObjects.IsChecked)
            {
                _listObjectWindow.setCoords(_projectionCoords);
                _listObjectWindow.DisplayObjectCoords(_listObjectWindow.ListBoxObjects.SelectedIndex);
                //MnListObjects.IsChecked = false; //допилить сохранение координат и размера. А в теории -
                //запилить просто без обновления перерасчет координат!
            }
            //MnListObjects.IsChecked = true;
        }

        private void MnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            ClassDisplayingConfigs stWin = new ClassDisplayingConfigs(_settingsClasses);
            bool? showDialog = stWin.ShowDialog();
            if (showDialog != null && showDialog.Value)
            {
                _settingsClasses = stWin.SettingsClassesForms;
                if (_settFilesCurrent.ClassObjectSelected)
                {
                    System.Xml.Serialization.XmlSerializer writer =
                        new System.Xml.Serialization.XmlSerializer(typeof(ClassVisualisationSettings));
                    var path = _settFilesCurrent.ClassObjectFile + ExtColorSchema;

                    try
                    {
                        FileStream file = File.Create(path);
                        writer.Serialize(file, _settingsClasses);
                        file.Close();
                    }
                    catch
                    {
                        Utils.ShowWarningMessage(string.Format(CannotLogDataMessage, path));
                    }
                }

                DrawScatterPlot();
            }
        }

        private void MnOpen_Click(object sender, RoutedEventArgs e)
        {
            StartSettings startSettingsWindow = new StartSettings(_settFilesCurrent);
            bool? showDialog = startSettingsWindow.ShowDialog();

            if (showDialog.GetValueOrDefault())
            {
                _typePlot = TypePlot.Main;
                _settFilesCurrent = startSettingsWindow.SettFiles;

                SetActiveAlgorithms(_settFilesCurrent.UniversalReader.SourceMatrixType);
                //InitializeAlgorithm();
                CreateLogOfProjection();
                InitializeWindow();
            }
            else
                _settFilesCurrent = null;
        }

        private void InitializeWindow()
        {
            Title = String.Format(WindowTitleFormat, _settFilesCurrent.AlgorithmType.ToString(), Path.GetFileName(_settFilesCurrent.UniversalReader.SourceMatrixFile));
            DrawScatterPlot();
        }

        private void SetActiveAlgorithms(SourceFileMatrixType sourceMatrixType)
        {
            switch (sourceMatrixType)
            {
                case SourceFileMatrixType.MatrixDistance:
                    MnDisSpace.IsEnabled = true;
                    MnFastMap.IsEnabled = true;
                    MnSammonMap.IsEnabled = true;
                    MnKohonenMap.IsEnabled = true;
                    break;

                case SourceFileMatrixType.ObjectAttribute:
                    MnDisSpace.IsEnabled = true;
                    MnFastMap.IsEnabled = true;
                    MnSammonMap.IsEnabled = true;
                    MnKohonenMap.IsEnabled = true;
                    break;

                case SourceFileMatrixType.ObjectAttribute3D:
                    MnDisSpace.IsEnabled = false;
                    MnFastMap.IsEnabled = false;
                    MnSammonMap.IsEnabled = false;
                    MnKohonenMap.IsEnabled = false;

                    //Если данные не требуют обработки, то пусть сразу выведутся.
                    InitializeAlgorithm();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void CreateLogOfProjection()
        {
            try
            {
                string pathXml = _settFilesCurrent.ClassObjectSelected
                    ? _settFilesCurrent.ClassObjectFile + ExtColorSchema
                    : String.Empty;

                if (File.Exists(pathXml))
                {
                    System.Xml.Serialization.XmlSerializer reader =
                        new System.Xml.Serialization.XmlSerializer(typeof(ClassVisualisationSettings));
                    StreamReader file = new StreamReader(
                        pathXml);
                    _settingsClasses = new ClassVisualisationSettings();
                    _settingsClasses = (ClassVisualisationSettings)reader.Deserialize(file);

                    if ((_settingsClasses.ArrayClass.Length == 0) && (_settFilesCurrent?.UniqClassesName.Count > 0))
                    {
                        //Потеря данных ((( Исправить на точечное исправление именно названий классов
                        _settingsClasses = new ClassVisualisationSettings(_settFilesCurrent.UniqClassesName);
                    }

                    file.Close();

                }
                else
                {
                    _settingsClasses = new ClassVisualisationSettings(_settFilesCurrent.UniqClassesName);
                    SizeDetection();
                }
            }
            catch
            {
                _settingsClasses = new ClassVisualisationSettings(_settFilesCurrent.UniqClassesName);
                SizeDetection();
            }
        }

        private void InitializeAlgorithm()
        {
            switch (_settFilesCurrent.AlgorithmType)
            {
                case AlgorithmType.FastMap:
                    _fastMapProjection = new FastMap(_settFilesCurrent.ArraySource);
                    FastMapGeneration(true);
                    break;

                case AlgorithmType.DisSpace:
                    _dissimiliaritySpace = new DisSpace(_settFilesCurrent.ArraySource, _settFilesCurrent.CountObjects);
                    DissimilitarySpaceGeneration(_dissimiliaritySpace, true);
                    break;

                case AlgorithmType.KohonenMap:
                    _kohonenProjection = new KohonenProjection(_settFilesCurrent.ArraySource, LowerSpaceDimensional);
                    KohonenMapGeneration(_kohonenProjection, true);
                    break;

                case AlgorithmType.SammonsMap:
                    _sammonsProjection = new SammonsProjection(_settFilesCurrent.ArraySource, LowerSpaceDimensional);
                    SammonsMapGeneration(_sammonsProjection, true);
                    break;

                case AlgorithmType.PrincipalComponentAnalysis:
                    _pcaProjection = new PCA(_settFilesCurrent.UniversalReader.SourceMatrix, LowerSpaceDimensional);
                    PrincipalComponentAnalysisGeneration((PCA)_pcaProjection, true);
                    break;

                case AlgorithmType.NoAlgorithm:
                    NoAlgorithmGeneration(true);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void MnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseListObjects(object sender, EventArgs e)
        {
            MnListObjects.IsChecked = false;
        }

        private void MnListObjects_Checked(object sender, RoutedEventArgs e)
        {
            _listObjectWindow = new ListObjects();
            _listObjectWindow.CloseEvent += CloseListObjects;
            _listObjectWindow.Show();
        }

        private void MnListObjects_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_listObjectWindow != null && !_listObjectWindow.IsClosing)
            {
                _listObjectWindow.CloseEvent -= CloseListObjects;
                _listObjectWindow.Close();
            }

            _listObjectWindow = null;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MnListObjects.IsChecked = false;

            _sopp.Closing -= _sopp.OnClosing;
            _hist.Closing -= _hist.OnClosing;
            _cumulative.Closing -= _cumulative.OnClosing;

            _sopp.Close();
            _hist.Close();
            _cumulative.Close();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_typePlot == TypePlot.Subsidiary && e.Key == Key.Back)
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                _typePlot = TypePlot.Main;
                _settFilesCurrent = _settFilesMain;
                _coordCurrent = _coordMain;

                switch (_settFilesCurrent.AlgorithmType)
                {
                    case AlgorithmType.FastMap:
                        _fastMapProjection = new FastMap(_settFilesCurrent.ArraySource);
                        FastMapGeneration();
                        break;

                    case AlgorithmType.KohonenMap:
                        _kohonenProjection = new KohonenProjection(_settFilesCurrent.ArraySource, LowerSpaceDimensional);
                        KohonenMapGeneration(_kohonenProjection);
                        break;

                    case AlgorithmType.SammonsMap:
                        _sammonsProjection = new SammonsProjection(_settFilesCurrent.ArraySource, LowerSpaceDimensional);
                        SammonsMapGeneration(_sammonsProjection);
                        break;

                    case AlgorithmType.NoAlgorithm:
                        NoAlgorithmGeneration();
                        break;

                    default:
                        throw new NotImplementedException();
                }

                _settingsClasses.SizeObject = _sizeMain;
                DrawScatterPlot();
            }
            else
            {
                _transformMatrix.OnKeyDown(e);
                TransformChart();
            }
        }

        private void NoAlgorithmGeneration(bool firstGeneration = false)
        {

            int countCords = _settFilesCurrent.ArraySource.GetLength(0);
            _projectionCoords = Utils.GetNormalizedData(_settFilesCurrent.ArraySource);

            _coordCurrent = new Vertex3D[countCords];

            if (!firstGeneration)
            {
                for (int i = 0; i < countCords; i++)
                {
                    _coordCurrent[i] = new Vertex3D
                    {
                        X = _projectionCoords[i, 0],
                        Y = _projectionCoords[i, 1],
                        Z = _projectionCoords[i, 2]
                    };
                }
            }
            else
            {
                _selectRect.OnMouseDown(new Point(0, 0), MainViewport, _nRectModelIndex);
                SaveResultsAsFile(countCords);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(PathToHelpFile))
            {
                Process.Start(PathToHelpFile);
            }
            else
            {
                Utils.ShowErrorMessage(String.Format(FileNotFoundErrorMessage, PathToHelpFile));

            }
        }

        /// <summary>
        ///  Отображение точек на графике
        /// </summary>
        public void DrawScatterPlot()
        {
            if (_coordCurrent == null || _coordCurrent.Length == 0)
            {
                return;
            }

            if (_settFilesCurrent == null)
            {
                Utils.ShowErrorMessage(DataErrorMessage);
                return;
            }

            switch (_settFilesCurrent.AlgorithmType)
            {
                case AlgorithmType.DisSpace:
                    MnDisSpaceSett.IsEnabled = true;
                    MnSamMapSett.IsEnabled = false;
                    MnKohonenMapSett.IsEnabled = false;

                    break;

                case AlgorithmType.KohonenMap:
                    MnKohonenMapSett.IsEnabled = true;
                    MnDisSpaceSett.IsEnabled = false;
                    MnSamMapSett.IsEnabled = false;
                    break;

                case AlgorithmType.FastMap:
                    MnSamMapSett.IsEnabled = false;
                    MnDisSpaceSett.IsEnabled = false;
                    MnKohonenMapSett.IsEnabled = false;
                    break;

                case AlgorithmType.SammonsMap:
                    MnSamMapSett.IsEnabled = true;
                    MnDisSpaceSett.IsEnabled = false;
                    MnKohonenMapSett.IsEnabled = false;
                    break;

                case AlgorithmType.NoAlgorithm:
                    MnDisSpaceSett.IsEnabled = false;
                    MnFastMapSett.IsEnabled = false;
                    MnSamMapSett.IsEnabled = false;
                    MnKohonenMapSett.IsEnabled = false;
                    break;

                case AlgorithmType.PrincipalComponentAnalysis:
                       MnFastMapSett.IsEnabled = false;
                      MnDisSpaceSett.IsEnabled = false;
                        MnSamMapSett.IsEnabled = false;
                    MnKohonenMapSett.IsEnabled = false;
                    break;

                default:
                    throw new NotImplementedException();
            }

            // Установить данные разброса графика
            _3DChartCurrent = new ScatterChart3D(true, _settingsClasses.CountPoligon);
            _3DChartCurrent.SetDataNo(_coordCurrent.Length);

            // установить свойство каждой точки (размер, положение, форма, цвет)
            for (int i = 0; i < _coordCurrent.Length; i++)
            {
                try
                {
                    AloneSettClass classSet =
                        _settingsClasses.GetSettingClass(_settFilesCurrent.ClassesName[i]);
                    bool isClassSetNull = classSet == null;

                    if (!isClassSetNull)
                    {
                        if (classSet.IsLiquid)
                        {
                            continue;
                        }
                    }

                    ScatterPlotItem plotItem = new ScatterPlotItem
                    {
                        ShapeType = (isClassSetNull) ? Shapes.Ellipse3D : classSet.Shape,
                        Color = (isClassSetNull) ? Color.FromRgb(255, 0, 13) : classSet.ColorObject,   //classSet.ColorObject Color.FromRgb(255,0,13)

                        W = _settingsClasses.SizeObject,
                        H = _settingsClasses.SizeObject,
                        X = _coordCurrent[i].X,
                        Y = _coordCurrent[i].Y,
                        Z = _coordCurrent[i].Z
                    };

                    _3DChartCurrent.SetVertex(i, plotItem);
                }
                catch (Exception ex)
                {
                    Utils.ShowErrorMessage(String.Format(CreatingGraphicErrorMessage, ex.Message, ex, ex.StackTrace));
                    return;
                }
            }

            if (_settFilesCurrent.AlgorithmType == AlgorithmType.DisSpace)
            {
                if (_dissimiliaritySpace.BasicObjectsColorMode)
                {
                    int[] basicArray = _dissimiliaritySpace.BasicObjectsArray;
                    for (int i = 0; i < _dissimiliaritySpace.BasicObjectsNumber; i++)
                    {
                        if (_3DChartCurrent.Get(basicArray[i] - 1) != null)
                        {
                            _3DChartCurrent.Get(basicArray[i] - 1).Color = Color.FromRgb((byte)(255 - _3DChartCurrent.Get(basicArray[i] - 1).Color.R), // тут косяк, три разных объекта, что-то нулевое
                        (byte)(255 - _3DChartCurrent.Get(basicArray[i] - 1).Color.G), (byte)(255 - _3DChartCurrent.Get(basicArray[i] - 1).Color.B));
                        }
                    }
                }

            }

            // Задать оси
            if (_settFilesCurrent.AlgorithmType == AlgorithmType.FastMap)
            {
                _3DChartCurrent.SetAxes(Colors.LightSkyBlue, AxeOffsetForFastMap);
            }
            else
            {
                _3DChartCurrent.SetAxes(Colors.LightSkyBlue, StandartAxeOffset);
            }

            // Получить массив трёхмерных точек на графике
            List<Mesh3D> meshs = _3DChartCurrent.GetMeshes();

            // вывести график в ViewPoint3D
            Model3D model3D = new Model3D();
            _nChartModelIndex = model3D.UpdateModel(meshs, null, _nChartModelIndex, MainViewport);

            // Задать проекцию
            double viewRange;

            if (_settFilesCurrent.AlgorithmType == AlgorithmType.KohonenMap)
            {
                //Utils.GetMinAndMax(_projectionCoords, _projectionCoords.GetLength(0), _projectionCoords.GetLength(1), out double min, out double max);
                Utils.GetMinAndMaxByDimensions(_projectionCoords, _projectionCoords.GetLength(0), _projectionCoords.GetLength(1),
                    out double maxX, out double maxY, out double maxZ,
                    out double minX, out double minY, out double minZ);
                //viewRange = _3DChartCurrent.ViewRange * 10;
                _transformMatrix.CalculateProjectionMatrix(new Point3D(minX, minY, minZ),
                    new Point3D(maxX, maxY, maxZ), 0.5);
            }
            else
            {
                viewRange = _3DChartCurrent.ViewRange;
                _transformMatrix.CalculateProjectionMatrix(new Point3D(0, 0, 0),
                    new Point3D(viewRange, viewRange, viewRange), 0.5);
            }
            //double viewRange = _3DChartCurrent.ViewRange*10;
            //double viewRange = _3DChartCurrent.ViewRange;
            //_transformMatrix.CalculateProjectionMatrix(new Point3D(0, 0, 0),
            //    new Point3D(viewRange, viewRange, viewRange), 0.5);
            TransformChart();
        }

        private void MnSamMapSett_Click(object sender, RoutedEventArgs e)
        {
            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if (_sammonsProjection == null)
            {
                Utils.ShowWarningMessage(BadAlgorithmSelectedMessage);
                return;
            }

            SammonsMapConfigs sammonsMapConfigs = new SammonsMapConfigs(_sammonsProjection);
            bool? showDialog = sammonsMapConfigs.ShowDialog();

            if ((bool)showDialog)
            {
                _sammonsProjection = sammonsMapConfigs.SamProjection;
                SammonsMapGeneration(_sammonsProjection, true);
                DrawScatterPlot();
            }

            /* KohonenMapConfigs kohonenMapConfigWindow = new KohonenMapConfigs(_kohonenProjection.MaxIterations);
            bool? showDialog = kohonenMapConfigWindow.ShowDialog();

            if ((bool)showDialog)
            {
                _kohonenProjection.MaxIterations = kohonenMapConfigWindow.MaxIteration;
                KohonenMapGeneration(_kohonenProjection, true);
                DrawScatterPlot();
            }*/

        }

        private bool IsAlgorithmChangeAvailable(AlgorithmType algorithm)
        {
            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return false;
            }

            if (_settFilesCurrent.AlgorithmType == algorithm)
            {
                Utils.ShowWarningMessage(AlgorithmAlreadySelectedMessage);
                return false;
            }

            return true;
        }

        private void ApplyNewAlgorithmType(AlgorithmType buttonAlgorithm)
        {
            _settFilesCurrent.AlgorithmType = buttonAlgorithm;

            InitializeAlgorithm();
            CreateLogOfProjection();
            InitializeWindow();
        }

        private void ChangeAlgorithm(AlgorithmType buttonAlgorithm)
        {
            if (!IsAlgorithmChangeAvailable(buttonAlgorithm))
            {
                return;
            }

            ApplyNewAlgorithmType(buttonAlgorithm);
        }

        private void MnFastMap_Click(object sender, RoutedEventArgs e)
        {
            const AlgorithmType ButtonAlgorithm = AlgorithmType.FastMap;
            ChangeAlgorithm(ButtonAlgorithm);

            /*if (!IsAlgorithmChangeAvailable(ButtonAlgorithm))
            {
                return;
            }

            ApplyNewAlgorithmType(ButtonAlgorithm);
            _settFilesCurrent.AlgorithmType = ButtonAlgorithm;

            InitializeAlgorithm();
            CreateLogOfProjection();
            InitializeWindow();*/
        }

        private void MnDisSpace_Click(object sender, RoutedEventArgs e)
        {
            const AlgorithmType ButtonAlgorithm = AlgorithmType.DisSpace;
            ChangeAlgorithm(ButtonAlgorithm);

            /*if (!IsAlgorithmChangeAvailable(ButtonAlgorithm))
            {
                return;
            }

            ApplyNewAlgorithmType(ButtonAlgorithm);

            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if (_settFilesCurrent.AlgorithmType == AlgorithmType.DisSpace)
            {
                Utils.ShowWarningMessage(AlgorithmAlreadySelectedMessage);
                return;
            }

            _settFilesCurrent.AlgorithmType = AlgorithmType.DisSpace;

            InitializeAlgorithm();
            CreateLogOfProjection();
            InitializeWindow();*/
        }

        private void MnSammonMap_Click(object sender, RoutedEventArgs e)
        {
            const AlgorithmType ButtonAlgorithm = AlgorithmType.SammonsMap;
            ChangeAlgorithm(ButtonAlgorithm);

            /*if (!IsAlgorithmChangeAvailable(ButtonAlgorithm))
            {
                return;
            }

            ApplyNewAlgorithmType(ButtonAlgorithm);

            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if (_settFilesCurrent.AlgorithmType == AlgorithmType.SammonsMap)
            {
                Utils.ShowWarningMessage(AlgorithmAlreadySelectedMessage);
                return;
            }

            _settFilesCurrent.AlgorithmType = AlgorithmType.SammonsMap;

            InitializeAlgorithm();
            CreateLogOfProjection();
            InitializeWindow();*/
        }

        private void MnKohonenMap_Click(object sender, RoutedEventArgs e)
        {
            const AlgorithmType ButtonAlgorithm = AlgorithmType.KohonenMap;
            ChangeAlgorithm(ButtonAlgorithm);

            /*if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if (_settFilesCurrent.AlgorithmType == AlgorithmType.KohonenMap)
            {
                Utils.ShowWarningMessage(AlgorithmAlreadySelectedMessage);
                return;
            }

            _settFilesCurrent.AlgorithmType = AlgorithmType.KohonenMap;

            InitializeAlgorithm();
            CreateLogOfProjection();
            InitializeWindow();*/
        }

        private void MnFastMapSett_Click(object sender, RoutedEventArgs e)
        {
            //ВЫВЕСТИ ВСЕ ВЫЗОВЫ ОКОН В ОДИН УНИВЕРСАЛЬНЫЙ МЕТОД!!!!!
            if (_settFilesCurrent == null)
            {
                Utils.ShowWarningMessage(NotInitializedDataErrorMessage);
                return;
            }

            if (_fastMapProjection == null)
            {
                Utils.ShowWarningMessage(BadAlgorithmSelectedMessage);
                return;
            }

            VisualChart3D.ConfigWindow.FastMapConfigs fastMapConfigs = new FastMapConfigs(this._fastMapProjection);
            bool? showDialog = fastMapConfigs.ShowDialog();
            if ((bool)showDialog)
            {
                this._fastMapProjection = fastMapConfigs.FastMapSetts;
                FastMapGeneration(true);
                DrawScatterPlot();
            }
            /*            SammonsMapConfigs sammonsMapConfigs = new SammonsMapConfigs(_sammonsProjection);
            bool? showDialog = sammonsMapConfigs.ShowDialog();

            if ((bool)showDialog)
            {
                _sammonsProjection = sammonsMapConfigs.SamProjection;
                SammonsMapGeneration(_sammonsProjection, true);
                DrawScatterPlot();
            }*/

        }

        private void MnPca_Click(object sender, RoutedEventArgs e)
        {
            const AlgorithmType ButtonAlgorithm = AlgorithmType.PrincipalComponentAnalysis;
            ChangeAlgorithm(ButtonAlgorithm);
        }

        private void UpdateVisualizationTab(IVisualizationWindow wnd, double[,] sourceMatrix, IList<int> brokenLine)
        {
            if (wnd != null && sourceMatrix != null)
            {
                double[,] dataMatrix;
                switch (wnd)
                {
                    case ShortestOpenPathPlot:
                        if (sourceMatrix.GetLength(1) > 2)
                        {
                            var pca = new PCA(sourceMatrix, 2);
                            dataMatrix = pca.ToProject() ? pca.Projection : null;
                        }
                        else
                        {
                            dataMatrix = sourceMatrix;
                        }
                        break;
                    case Histogram:
                        dataMatrix = sourceMatrix;
                        break;
                    case PathProjection:
                        dataMatrix = sourceMatrix;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                
                wnd.DataMatrix = dataMatrix;
                wnd.Graph = brokenLine;
            }
        }

        private void MnShortestOpenPath_Greedy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var distMatrix = _settFilesCurrent.ArraySource;

                var timer = new Stopwatch();
                timer.Start();
                var graph = Algorithms.GetShortestOpenPath(distMatrix);
                timer.Stop();
                
                var brokenLine = Algorithms.AdjacentListToBrokenLine(graph);

                var (brokenLineLen, avgDist, twoEndsDist, diff, minSegmDist, maxSegmDist) = Algorithms.GetBrokenLineStats(distMatrix, brokenLine);
                
                var minBrokenLine = new Algorithms.BrokenLineStats()
                {
                    BrokenLine = brokenLine,
                    Length = brokenLineLen,
                    TwoEndsDistance = twoEndsDist,
                    Difference = diff,
                    SegmentAverageDistance = avgDist,
                    MinSegment = minSegmDist,
                    MaxSegment = maxSegmDist
                };

                using var file = new StreamWriter(dialog.OpenFile());
                file.WriteLine(PrepareSummary(minBrokenLine));

                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                       timer.Elapsed.Minutes,
                                                                       timer.Elapsed.Seconds,
                                                                       timer.Elapsed.Milliseconds);

                MessageBox.Show($@"Граф записан в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Жадный алгоритм");

                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, brokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, brokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, brokenLine);
            }
        }

        private void MnShortestOpenPath_NonGreedy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var distMatrix = _settFilesCurrent.ArraySource;
                
                var timer = new Stopwatch();
                timer.Start();
                var graph = Algorithms.GetNonGreedyShortestOpenPath(distMatrix);
                timer.Stop();
                
                var brokenLine = Algorithms.AdjacentListToBrokenLine(graph);
                var (brokenLineLen, avgDist, twoEndsDist, diff, minSegmDist, maxSegmDist) = Algorithms.GetBrokenLineStats(distMatrix, brokenLine);

                var minBrokenLine = new Algorithms.BrokenLineStats()
                {
                    BrokenLine = brokenLine,
                    Length = brokenLineLen,
                    TwoEndsDistance = twoEndsDist,
                    Difference = diff,
                    SegmentAverageDistance = avgDist,
                    MinSegment = minSegmDist,
                    MaxSegment = maxSegmDist
                };


                using var file = new StreamWriter(dialog.OpenFile());
                file.WriteLine(PrepareSummary(minBrokenLine));

                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);

                MessageBox.Show($@"Граф записан в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Жадный алгоритм");

                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, brokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, brokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, brokenLine);
            }
        }

        private void MnShortestOpenPath_NonGreedy_BruteForce_Pairs_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var scheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
                var taskFactory = new TaskFactory(scheduler);

                const int MaxTasks = 1024;
                var tasks = new List<Task>(MaxTasks);

                var distMatrix = _settFilesCurrent.ArraySource;
                var mutexMinBrokenLine = new object();
                var mutexMaxTwoEndsDist = new object();

                var timer = new Stopwatch();
                Algorithms.BrokenLineStats minBrokenLine = null, minDiff = null;

                timer.Start();
                for (var i = 0; i < distMatrix.GetLength(0) - 1; ++i)
                {
                    for (var j = i + 1; j < distMatrix.GetLength(1); ++j)
                    {
                        tasks.Add(taskFactory.StartNew((state) =>
                        {
                            var (i_, j_) = (ValueTuple<int, int>)state;
                            var graph = Algorithms.GetNonGreedyShortestOpenPath(distMatrix, (d) => new HashSet<(double value, int i, int j)> { (d[i_, j_], i_, j_) });

                            var brokenLine = Algorithms.AdjacentListToBrokenLine(graph);

                            var (brokenLineLen, avgDist, twoEndsDist, diff, minSegmDist, maxSegmDist) = Algorithms.GetBrokenLineStats(distMatrix, brokenLine);

                            var stats = new Algorithms.BrokenLineStats()
                            {
                                BrokenLine = brokenLine,
                                Length = brokenLineLen,
                                TwoEndsDistance = twoEndsDist,
                                Difference = diff,
                                SegmentAverageDistance = avgDist,
                                MinSegment = minSegmDist,
                                MaxSegment = maxSegmDist
                            };

                            Interlocked.CompareExchange(ref minBrokenLine, stats, null);
                            if (brokenLineLen < minBrokenLine.Length)
                            {
                                lock (mutexMinBrokenLine)
                                {
                                    if (brokenLineLen < minBrokenLine.Length)
                                    {
                                        Interlocked.Exchange(ref minBrokenLine, stats);
                                    }
                                }
                            }

                            Interlocked.CompareExchange(ref minDiff, stats, null);
                            if (diff < minDiff.Difference)
                            {
                                lock (mutexMaxTwoEndsDist)
                                {
                                    if (diff < minDiff.Difference)
                                    {
                                        Interlocked.Exchange(ref minDiff, stats);
                                    }
                                }
                            }
                        },
                        (i, j),
                        TaskCreationOptions.AttachedToParent));

                        if (tasks.Count >= MaxTasks)
                        {
                            Task.WhenAll(tasks).Wait();
                            tasks = new List<Task>(MaxTasks);
                        }
                    }
                }

                Task.WhenAll(tasks).Wait();
                timer.Stop();

                using (var file = new StreamWriter(dialog.OpenFile()))
                {
                    file.WriteLine(PrepareSummary(minBrokenLine, minDiff));
                }

                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);

                MessageBox.Show($@"Данные по графам записаны в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Перебор начальных решений для жадного алгоритма");

                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
            }
        }

        private void MnShortestOpenPath_Greedy_BruteForce_Lines_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var scheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
                var taskFactory = new TaskFactory(scheduler);

                const int MaxTasks = 1024;
                var tasks = new List<Task>(MaxTasks);
                
                var distMatrix = _settFilesCurrent.ArraySource;
                var mutexMinBrokenLine = new object();
                var mutexMaxTwoEndsDist = new object();
                
                var timer = new Stopwatch();
                Algorithms.BrokenLineStats minBrokenLine = null, minDiff = null;
                timer.Start();
                for (int i = 0; i < distMatrix.GetLength(0) - 1; ++i)
                {
                    tasks.Add(taskFactory.StartNew((state) =>
                    {
                        var i_ = (int) state;

                        (double value, int i, int j) minFunc(double[,] matr)
                        {
                            var (minValue, min_j) = matr.GetRow(i_).Skip(i_ + 1).MinIndex();
                            return (minValue, i_, min_j + i_ + 1);
                        }

                        var graph = Algorithms.GetShortestOpenPath(distMatrix, minFunc);
                        var brokenLine = Algorithms.AdjacentListToBrokenLine(graph);

                        var (brokenLineLen, avgDist, twoEndsDist, diff, minSegmDist, maxSegmDist) = Algorithms.GetBrokenLineStats(distMatrix, brokenLine);

                        var stats = new Algorithms.BrokenLineStats()
                        {
                            LineIndex = i_,
                            BrokenLine = brokenLine,
                            Length = brokenLineLen,
                            TwoEndsDistance = twoEndsDist,
                            Difference = diff,
                            SegmentAverageDistance = avgDist,
                            MinSegment = minSegmDist,
                            MaxSegment = maxSegmDist
                        };

                        Interlocked.CompareExchange(ref minBrokenLine, stats, null);
                        if (brokenLineLen < minBrokenLine.Length)
                        {
                            lock (mutexMinBrokenLine)
                            {
                                if (brokenLineLen < minBrokenLine.Length)
                                {
                                    Interlocked.Exchange(ref minBrokenLine, stats);
                                }
                            }
                        }

                        Interlocked.CompareExchange(ref minDiff, stats, null);
                        if (diff < minDiff.Difference)
                        {
                            lock (mutexMaxTwoEndsDist)
                            {
                                if (diff < minDiff.Difference)
                                {
                                    Interlocked.Exchange(ref minDiff, stats);
                                }
                            }
                        }
                    },
                    i,
                    TaskCreationOptions.AttachedToParent));

                    if (tasks.Count >= MaxTasks)
                    {
                        Task.WhenAll(tasks).Wait();
                        tasks = new List<Task>(MaxTasks);
                    }
                }

                Task.WhenAll(tasks).Wait();
                timer.Stop();

                using (var file = new StreamWriter(dialog.OpenFile()))
                {
                    file.WriteLine(PrepareSummary(minBrokenLine, minDiff));
                }

                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);

                MessageBox.Show($@"Графы записаны в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Минимум по строке");
                
                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
            }
        }

        private void MnShortestOpenPath_BruteForce_BrokenLines_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var timer = new Stopwatch();

                var distMatrix = _settFilesCurrent.ArraySource;
                
                var scheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
                var taskFactory = new TaskFactory(scheduler);

                timer.Start();

                var tasks = Algorithms.GetStartPermutationsForParallel(distMatrix)
                                      .Select(perm => taskFactory.StartNew((state) =>
                    {
                        Algorithms.BrokenLineStats minBrokenLine = null;
                        Algorithms.BrokenLineStats minDiff = null;
                        var brokenLine = state as int[];
                        Array.Sort(brokenLine, 1, brokenLine.Length - 1);

                        do
                        {
                            var brokenLineLen = Algorithms.GetBrokenLineLength(brokenLine, distMatrix);
                            var twoEndsDistance = Algorithms.GetDistBetweenTwoEnds(brokenLine, distMatrix);
                            var diff = Math.Abs(brokenLineLen - twoEndsDistance);


                            if (brokenLineLen < (minBrokenLine != null ? minBrokenLine.Length : double.MaxValue))
                            {
                                minBrokenLine = new Algorithms.BrokenLineStats()
                                {
                                    BrokenLine = (int[])brokenLine.Clone(),
                                    Length = brokenLineLen,
                                };
                            }

                            if (diff < (minDiff != null ? minDiff.Difference : double.MaxValue))
                            {
                                minDiff = new Algorithms.BrokenLineStats()
                                {
                                    BrokenLine = (int[])brokenLine.Clone(),
                                    Difference = diff,
                                };
                            }
                        }
                        while (Algorithms.NextPermutation(new ArraySegment<int>(perm, 1, perm.Length - 1)));
                        
                        return (minBrokenLine, minDiff);
                    },
                    perm,
                    TaskCreationOptions.AttachedToParent))
                                      .ToArray();

                Task.WaitAll(tasks);

                var (minBrokenLine, minDiff) = tasks.First().Result;
                foreach (var (currBrokenLine, currDiff) in tasks.Skip(1).Select(task => task.Result))
                {
                    if (currBrokenLine.Length < minBrokenLine.Length)
                    {
                        minBrokenLine = currBrokenLine;
                    }

                    if (currDiff.Difference < minDiff.Difference)
                    {
                        minDiff = currDiff;
                    }
                }

                (minBrokenLine.Length,
                 minBrokenLine.SegmentAverageDistance,
                 minBrokenLine.TwoEndsDistance,
                 minBrokenLine.Difference,
                 minBrokenLine.MinSegment,
                 minBrokenLine.MaxSegment) = Algorithms.GetBrokenLineStats(distMatrix, minBrokenLine.BrokenLine);

                (minDiff.Length,
                 minDiff.SegmentAverageDistance,
                 minDiff.TwoEndsDistance,
                 minDiff.Difference,
                 minDiff.MinSegment,
                 minDiff.MaxSegment) = Algorithms.GetBrokenLineStats(distMatrix, minDiff.BrokenLine);

                timer.Stop();

                using (var file = new StreamWriter(dialog.OpenFile()))
                {
                    file.WriteLine(PrepareSummary(minBrokenLine, minDiff));
                }


                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);
                MessageBox.Show($@"Графы записаны в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Полный перебор ломанных");

                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
            }
        }

        private void MnShortestOpenPath_BruteForce_BrokenLines_Simple_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var distMatrix = _settFilesCurrent.ArraySource;

                var minBrokenLine = new Algorithms.BrokenLineStats()
                {
                    BrokenLine = null,
                    Length = double.MaxValue,
                    TwoEndsDistance = double.MinValue,
                    Difference = double.NaN,
                    SegmentAverageDistance = double.NaN,
                    MinSegment = default,
                    MaxSegment = default
                };
                
                var minDiff = new Algorithms.BrokenLineStats()
                {
                    BrokenLine = null,
                    Length = double.MaxValue,
                    TwoEndsDistance = double.MinValue,
                    Difference = double.MaxValue,
                    SegmentAverageDistance = double.NaN,
                    MinSegment = default,
                    MaxSegment = default
                };

                var timer = new Stopwatch();
                timer.Start();
                var perm = Enumerable.Range(0, distMatrix.GetLength(0)).ToArray();
                do
                {
                    var brokenLine = perm;
                    var (brokenLineLen, avgDist, twoEndsDist, diff, minSegmDist, maxSegmDist) = Algorithms.GetBrokenLineStats(distMatrix, brokenLine);
                    
                    if (brokenLineLen < minBrokenLine.Length)
                    {
                        minBrokenLine.BrokenLine = (int[]) brokenLine.Clone();
                        minBrokenLine.Length = brokenLineLen;
                        minBrokenLine.TwoEndsDistance = twoEndsDist;
                        minBrokenLine.Difference = diff;
                        minBrokenLine.SegmentAverageDistance = avgDist;
                        minBrokenLine.MinSegment = minSegmDist;
                        minBrokenLine.MaxSegment = maxSegmDist;
                    }
                    
                    if (diff < minDiff.Difference)
                    {
                        minDiff.BrokenLine = (int[]) brokenLine.Clone();
                        minDiff.Length = brokenLineLen;
                        minDiff.TwoEndsDistance = twoEndsDist;
                        minDiff.Difference = diff;
                        minDiff.SegmentAverageDistance = avgDist;
                        minDiff.MinSegment = minSegmDist;
                        minDiff.MaxSegment = maxSegmDist;
                    }
                }
                while (Algorithms.NextPermutation<int>(perm));
                timer.Stop();

                using (var file = new StreamWriter(dialog.OpenFile()))
                {
                    file.WriteLine(PrepareSummary(minBrokenLine, minDiff));
                }

                // UpdateVisualizationTab(_settFilesCurrent?.UniversalReader?.SourceMatrix, minBrokenLine.BrokenLine);
                // 
                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);
                MessageBox.Show($@"Графы записаны в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Полный перебор ломанных");

                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
            }
        }

        private void MnPermutations_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var distMatrix = _settFilesCurrent.ArraySource;
                using var file = TextWriter.Synchronized(new StreamWriter(dialog.OpenFile()));

                var timer = new Stopwatch();
                timer.Start();
                foreach (var perm in new Combinatorics.Collections.Permutations<int>(Enumerable.Range(0, distMatrix.GetLength(0)).ToArray(),
                                                                                     Combinatorics.Collections.GenerateOption.WithoutRepetition))
                {
                    file.WriteLine($"{{{string.Join(" ", perm.Select(val => Convert.ToString(val + 1)))}}}");
                }

                timer.Stop();

                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);

                MessageBox.Show($@"Перестановки записаны в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Перестановки");
            }
        }

        private void MnShortestOpenPath_Greedy_BruteForce_Pairs_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (dialog.ShowDialog() == true)
            {
                var scheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
                var taskFactory = new TaskFactory(scheduler);

                const int MaxTasks = 1024;
                var tasks = new List<Task>(MaxTasks);

                Algorithms.BrokenLineStats minBrokenLine = null, minDiff = null;

                var distMatrix = _settFilesCurrent.ArraySource;
                var mutexMinBrokenLine = new object();
                var mutexMaxTwoEndsDist = new object();

                var timer = new Stopwatch();
                timer.Start();
                for (var i = 0; i < distMatrix.GetLength(0) - 1; ++i)
                {
                    for (var j = i + 1; j < distMatrix.GetLength(1); ++j)
                    {
                        tasks.Add(taskFactory.StartNew((state) =>
                        {
                            var (i_, j_) = (ValueTuple<int, int>)state;
                            var graph = Algorithms.GetShortestOpenPath(distMatrix, (d) => (value: d[i_, j_], i_, j_));

                            var brokenLine = Algorithms.AdjacentListToBrokenLine(graph);

                            var (brokenLineLen, avgDist, twoEndsDist, diff, minSegmDist, maxSegmDist) = Algorithms.GetBrokenLineStats(distMatrix, brokenLine);

                            var stats = new Algorithms.BrokenLineStats()
                            {
                                BrokenLine = brokenLine,
                                Length = brokenLineLen,
                                TwoEndsDistance = twoEndsDist,
                                Difference = diff,
                                SegmentAverageDistance = avgDist,
                                MinSegment = minSegmDist,
                                MaxSegment = maxSegmDist
                            };

                            Interlocked.CompareExchange(ref minBrokenLine, stats, null);
                            if (brokenLineLen < minBrokenLine.Length)
                            {
                                lock (mutexMinBrokenLine)
                                {
                                    if (brokenLineLen < minBrokenLine.Length)
                                    {
                                        Interlocked.Exchange(ref minBrokenLine, stats);
                                    }
                                }
                            }

                            Interlocked.CompareExchange(ref minDiff, stats, null);
                            if (diff < minDiff.Difference)
                            {
                                lock (mutexMaxTwoEndsDist)
                                {
                                    if (diff < minDiff.Difference)
                                    {
                                        Interlocked.Exchange(ref minDiff, stats);
                                    }
                                }
                            }
                        },
                        (i, j),
                        TaskCreationOptions.AttachedToParent));

                        if (tasks.Count >= MaxTasks)
                        {
                            Task.WhenAll(tasks).Wait();
                            tasks = new List<Task>(MaxTasks);
                        }
                    }
                }

                Task.WhenAll(tasks).Wait();
                timer.Stop();

                using (var file = new StreamWriter(dialog.OpenFile()))
                {
                    file.WriteLine(PrepareSummary(minBrokenLine, minDiff));
                }

                var elapsed = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours,
                                                                           timer.Elapsed.Minutes,
                                                                           timer.Elapsed.Seconds,
                                                                           timer.Elapsed.Milliseconds);

                MessageBox.Show($@"Данные по графам записаны в файл ""{dialog.FileName}""{Environment.NewLine}Общее время: {elapsed}",
                                  "Перебор начальных решений для жадного алгоритма");

                UpdateVisualizationTab(_sopp, _settFilesCurrent?.UniversalReader?.SourceMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_hist, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
                UpdateVisualizationTab(_cumulative, _settFilesCurrent?.UniversalReader?.DistMatrix, minBrokenLine.BrokenLine);
            }
        }

        private static string PrepareSummary(Algorithms.BrokenLineStats minBrokenLine)
        {
            return $@"-----------------------------
Граф с минимальной длиной - {string.Join(" ", minBrokenLine.BrokenLine.Select(vertex => Convert.ToString(vertex + 1)))}
Длина - {minBrokenLine.Length:F3}
Расстояние между терминальными точками - {minBrokenLine.TwoEndsDistance:F3}
Разница между длиной графа и расстоянием между терминальными точками - {minBrokenLine.Difference:F3}
Минимальный сегмент - {{{minBrokenLine.MinSegment.First + 1}, {minBrokenLine.MinSegment.Second + 1}}}, длина - {minBrokenLine.MinSegment.Distance:F3}
Максимальный сегмент - {{{minBrokenLine.MaxSegment.First + 1}, {minBrokenLine.MaxSegment.Second + 1}}}, длина - {minBrokenLine.MaxSegment.Distance:F3}
Средняя длина сегмента - {minBrokenLine.Length / (minBrokenLine.BrokenLine.Count - 1):F3}";
        }

        private static string PrepareSummary(Algorithms.BrokenLineStats minBrokenLine, Algorithms.BrokenLineStats minDiff)
        {
            return $@"-----------------------------
Граф с минимальной длиной - {string.Join(" ", minBrokenLine.BrokenLine.Select(vertex => Convert.ToString(vertex + 1)))}
Длина - {minBrokenLine.Length:F3}
Расстояние между терминальными точками - {minBrokenLine.TwoEndsDistance:F3}
Разница между длиной графа и расстоянием между терминальными точками - {minBrokenLine.Difference:F3}
Минимальный сегмент - {{{minBrokenLine.MinSegment.First + 1}, {minBrokenLine.MinSegment.Second + 1}}}, длина - {minBrokenLine.MinSegment.Distance:F3}
Максимальный сегмент - {{{minBrokenLine.MaxSegment.First + 1}, {minBrokenLine.MaxSegment.Second + 1}}}, длина - {minBrokenLine.MaxSegment.Distance:F3}
Средняя длина сегмента - {minBrokenLine.Length / (minBrokenLine.BrokenLine.Count - 1):F3}

Граф с минимальной разницей (d - dT) - {string.Join(" ", minDiff.BrokenLine.Select(vertex => Convert.ToString(vertex + 1)))}
Длина - {minDiff.Length:F3}
Расстояние между терминальными точками - {minDiff.TwoEndsDistance:F3}
Разница между длиной графа и расстоянием между терминальными точками - {minDiff.Difference:F3}
Средняя длина сегмента - {minDiff.SegmentAverageDistance:F3}
Минимальный сегмент - {{{minDiff.MinSegment.First + 1}, {minDiff.MinSegment.Second + 1}}}, длина - {minDiff.MinSegment.Distance:F3}
Максимальный сегмент - {{{minDiff.MaxSegment.First + 1}, {minDiff.MaxSegment.Second + 1}}}, длина - {minDiff.MaxSegment.Distance:F3}";
        }

        private void MnShortestOpenPath_Visualization_Graph_Click(object sender, RoutedEventArgs e)
        {
            _sopp.Show();
            _sopp.Focus();
        }

        private void MnGenerateDataOnPlot_Click(object sender, RoutedEventArgs e)
        {
            new DataGeneration().ShowDialog();
        }

        private void MnShortestOpenPath_Visualization_Histogram_Click(object sender, RoutedEventArgs e)
        {
            _hist.Show();
            _hist.Focus();
        }

        private void MnShortestOpenPath_Visualization_ProjectionOnPath_Click(object sender, RoutedEventArgs e)
        {
            _cumulative.Show();
            _cumulative.Focus();
        }
    }
}