// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// Класс для 2D прямоугольника в  Viewport3D.
    /// </summary>
    /// <remarks>    
    ///  0  -------------------- 1
    ///  |   -----------------   |
    ///  |   | 4            5 |  |
    ///  |   |                |  |
    ///  |   |                |  |
    ///  |   | 7            6 |  |
    ///  |   -----------------   |
    /// 3  -------------------- 2
    /// </remarks>
    sealed class ViewportRect : Mesh3D
    {
        /// <summary>
        /// Получить минимальный X
        /// </summary>
        public double XMin
        {
            get { return _firstPoint.X < _secondPoint.X ? _firstPoint.X : _secondPoint.X; }
        }

        /// <summary>
        /// Получить максимальный X
        /// </summary>
        public double XMax
        {
            get { return _firstPoint.X < _secondPoint.X ? _secondPoint.X : _firstPoint.X; }
        }

        /// <summary>
        /// Получить минимальный Y
        /// </summary>
        public double YMin
        {
            get { return _firstPoint.Y < _secondPoint.Y ? _firstPoint.Y : _secondPoint.Y; }
        }

        /// <summary>
        /// Получить максимальный Y
        /// </summary>
        public double YMax
        {
            get { return _firstPoint.Y < _secondPoint.Y ? _secondPoint.Y : _firstPoint.Y; }
        }

        /// <summary>
        /// ширина линии прямоугольника (% от ширины окна)
        /// </summary>
        private const double LineWidth = 0.005;

        /// <summary>
        /// Значение Z прямоугольника
        /// </summary>
        private const double ZLevel = 1.0;

        /// <summary>
        /// Координаты одного из углов прямоугольника
        /// </summary>
        private Point _firstPoint = new Point(0, 0);

        /// <summary>
        /// Координаты противоположенной <see cref="_firstPoint"/> точки
        /// </summary>
        private Point _secondPoint = new Point(0, 0);

        public ViewportRect()
        {
            SetSize(8, 8);
            SetTriangle(0, 0, 4, 1);
            SetTriangle(1, 1, 4, 5);
            SetTriangle(2, 1, 5, 2);
            SetTriangle(3, 2, 5, 6);
            SetTriangle(4, 2, 6, 3);
            SetTriangle(5, 3, 6, 7);
            SetTriangle(6, 0, 3, 7);
            SetTriangle(7, 0, 7, 4);
            SetColor(255, 0, 0);
        }

        /// <summary>
        /// Задать прямоугольник
        /// </summary>
        /// <param name="pt1">Координаты одного из углов прямоугольника</param>
        /// <param name="pt2">Координаты противоположенного <paramref name="pt1"/> угла</param>
        public void SetRect(Point pt1, Point pt2)
        {
            _firstPoint = pt1;
            _secondPoint = pt2;
            SetRect();
        }

        /// <summary>
        /// Получить массив сетки прямоугольника 
        /// </summary>
        /// <returns>Список сетки прямоугольника</returns>
        /// <remarks>для отображения в Viewport3D</remarks>
        public List<Mesh3D> GetMeshes()
        {
            List<Mesh3D> meshs = new List<Mesh3D> { this };

            int nVertNo = VertexNo;
            for (int i = 0; i < nVertNo; i++)
            {
                VertIndices[i] = i;
            }

            return meshs;
        }

        /// <summary>
        /// Реагирование на нажатие клавиши мыши
        /// </summary>
        /// <param name="point">координаты курсора</param>
        /// <param name="viewport3D">окно просмотра</param>
        /// <param name="nModelIndex">индекс модели прямоугольника в окне просмотра</param>
        public void OnMouseDown(Point point, Viewport3D viewport3D, int nModelIndex)
        {
            if (nModelIndex == -1)
            {
                return;
            }

            MeshGeometry3D meshGeometry = Model3D.GetGeometry(viewport3D, nModelIndex);

            if (meshGeometry == null)
            {
                return;
            }

            Point viewPoint = TransformMatrix.ScreenPointToViewportPoint(point, viewport3D);

            SetRect(viewPoint, viewPoint);
            UpdatePositions(meshGeometry);
        }

        /// <summary>
        /// Реагирование на перемещение мыши
        /// </summary>
        /// <param name="pt">координаты курсора</param>
        /// <param name="viewport3D">окно просмотра</param>
        /// <param name="nModelIndex">индекс модели прямоугольника в окне просмотра</param>
        public void OnMouseMove(Point pt, Viewport3D viewport3D, int nModelIndex)
        {
            if (nModelIndex == -1)
            {
                return;
            }

            MeshGeometry3D meshGeometry = Model3D.GetGeometry(viewport3D, nModelIndex);

            if (meshGeometry == null)
            {
                return;
            }

            Point pt2 = TransformMatrix.ScreenPointToViewportPoint(pt, viewport3D);
            _secondPoint = pt2;
            SetRect();
            UpdatePositions(meshGeometry);
        }

        /// <summary>
        /// Задать прямоугольник
        /// </summary>
        /// <param name="pntCenter">координаты центра прямоугольника</param>
        /// <param name="w">ширина</param>
        /// <param name="h">высота</param>
        private void SetRect(Point pntCenter, double w, double h)
        {
            SetPoint(0, pntCenter.X - w / 2, pntCenter.Y + h / 2, ZLevel);
            SetPoint(1, pntCenter.X + w / 2, pntCenter.Y + h / 2, ZLevel);
            SetPoint(2, pntCenter.X + w / 2, pntCenter.Y - h / 2, ZLevel);
            SetPoint(3, pntCenter.X - w / 2, pntCenter.Y - h / 2, ZLevel);
            SetPoint(4, pntCenter.X - w / 2 + LineWidth, pntCenter.Y + h / 2 - LineWidth, ZLevel);
            SetPoint(5, pntCenter.X + w / 2 - LineWidth, pntCenter.Y + h / 2 - LineWidth, ZLevel);
            SetPoint(6, pntCenter.X + w / 2 - LineWidth, pntCenter.Y - h / 2 + LineWidth, ZLevel);
            SetPoint(7, pntCenter.X - w / 2 + LineWidth, pntCenter.Y - h / 2 + LineWidth, ZLevel);
        }

        /// <summary>
        /// Задать прямоугольник
        /// </summary>
        private void SetRect()
        {
            Point centerPoint = new Point
            {
                X = (_firstPoint.X + _secondPoint.X) / 2,
                Y = (_firstPoint.Y + _secondPoint.Y) / 2
            };
            double w = System.Math.Abs(_firstPoint.X - _secondPoint.X);
            double h = System.Math.Abs(_firstPoint.Y - _secondPoint.Y);
            SetRect(centerPoint, w, h);
        }
    }
}