// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// Класс для точечного 3D графика
    /// </summary>
    internal class ScatterChart3D : Chart3D
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="useAxis">true - нужно рисовать ось</param>
        public ScatterChart3D(bool useAxis) : base(useAxis)
        {
            CountPolygon = 13;
        }

        public ScatterChart3D(bool useAxis, int countPolygon)
            : base(useAxis)
        {
            CountPolygon = countPolygon;
        }

        /// <summary>
        /// Кол-во фигур
        /// </summary>
        public const int ShapeNo = 5;

        private int _countPolygon;

        public int CountPolygon {
            get { return _countPolygon; }
            set {
                if (value > 0 && value < 21)
                    _countPolygon = value;
                else
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Получить точку с указанным индексом
        /// </summary>
        /// <param name="n">индекс</param>
        /// <returns>точка</returns>
        public ScatterPlotItem Get(int n)
        {
            return (ScatterPlotItem)Vertices[n];
        }

        /// <summary>
        /// Задать точку
        /// </summary>
        /// <param name="n">индекс</param>
        /// <param name="value">значение</param>
        public void SetVertex(int n, ScatterPlotItem value)
        {
            Vertices[n] = value;
        }


        /// <summary>
        /// преобразовать 3D точечный график в список <see cref="Mesh3D" />
        /// </summary>
        /// <returns>Список сеток</returns>
        public List<Mesh3D> GetMeshes()
        {
            int nDotNo = GetDataNo();

            if (nDotNo == 0)
            {
                return null;
            }

            List<Mesh3D> meshs = new List<Mesh3D>();

            int nVertIndex = 0;

            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = Get(i);

                if (plotItem == null)
                {
                    continue;
                }

                double w = plotItem.W;
                double h = plotItem.H;
                Mesh3D dot;
                Vertices[i].NMinI = nVertIndex;

                switch (plotItem.ShapeType)
                {
                    case Shapes.Bar3D:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                    case Shapes.Cone3D:
                        dot = new Cone3D(w, w, h, _countPolygon);
                        break;
                    case Shapes.Cylinder3D:
                        dot = new Cylinder3D(w, w, h, _countPolygon);
                        break;
                    case Shapes.Ellipse3D:
                        dot = new Ellipse3D(w, w, h, _countPolygon);
                        break;
                    case Shapes.Pyramid3D:
                        dot = new Pyramid3D(w, w, h);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                nVertIndex += dot.VertexNo;
                Vertices[i].NMaxI = nVertIndex - 1;

                TransformMatrix.Transform(dot, new Point3D(plotItem.X, plotItem.Y, plotItem.Z), 0, 0);
                dot.SetColor(plotItem.Color);
                meshs.Add(dot);
            }

            AddAxesMeshes(meshs);

            return meshs;
        }

        /// <summary>
        /// Выделение
        /// </summary>
        /// <param name="rect">область выделения</param>
        /// <param name="matrix">матрица трансформации</param>
        /// <param name="viewport3D">поле просмотра</param>
        public override int[] Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3D)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0)
            {
                return null;
            }

            double xMin = rect.XMin;
            double xMax = rect.XMax;
            double yMin = rect.YMin;
            double yMax = rect.YMax;
            List<int> result = new List<int>();

            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = Get(i);
                if (plotItem == null)
                {
                    continue;
                }

                Point pt = matrix.VertexToViewportPt(new Point3D(plotItem.X, plotItem.Y, plotItem.Z),
                    viewport3D);

                if ((pt.X > xMin) && (pt.X < xMax) && (pt.Y > yMin) && (pt.Y < yMax))
                {
                    result.Add(i);
                    Vertices[i].Selected = true;
                }
                else
                {
                    Vertices[i].Selected = false;
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Визуализация выделения
        /// </summary>
        /// <param name="meshGeometry">геометрия сетки</param>
        /// <param name="selectColor">Цвет выделения</param>
        public override void HighlightSelection(MeshGeometry3D meshGeometry, Color selectColor)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0)
            {
                return;
            }

            for (int i = 0; i < nDotNo; i++)
            {
                if (Vertices[i] == null)
                {
                    continue;
                }

                Point mapPt = TextureMapping.GetMappingPosition
                    (Vertices[i].Selected
                    ? selectColor
                    : Vertices[i].Color, false);
                int nMin = Vertices[i].NMinI;
                int nMax = Vertices[i].NMaxI;

                for (int j = nMin; j <= nMax; j++)
                {
                    meshGeometry.TextureCoordinates[j] = mapPt;
                }
            }
        }
    }
}
