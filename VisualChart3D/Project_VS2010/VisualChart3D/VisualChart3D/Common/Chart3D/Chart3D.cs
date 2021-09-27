// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// 3D график
    /// </summary>
    abstract class Chart3D
    {

        /// <summary>
        /// Индексатор точек
        /// </summary>
        /// <param name="n">индекс</param>
        /// <returns>точка в 3D пространстве</returns>
        public Vertex3D this[int n] {
            get {
                return Vertices[n];
            }
            set {
                Vertices[n] = value;
            }
        }

        public double MaxLenghtAxis {
            get { return Math.Max(AxisLenght.X, Math.Max(AxisLenght.Y, AxisLenght.Z)); }
        }

        public double ViewRange {
            get { return Math.Max(Math.Max(AxisLenght.X, AxisLenght.Y), AxisLenght.Z); }
        }

        /// <summary>
        /// Массив точек в графике
        /// </summary>
        protected Vertex3D[] Vertices;

        /// <summary>
        /// Минимальная точка
        /// </summary>
        protected Point3D MinPoint3D = new Point3D();

        /// <summary>
        /// Максимальная точка
        /// </summary>
        protected Point3D MaxPoint3D = new Point3D();

        /// <summary>
        /// длины осей
        /// </summary>
        protected Point3D AxisLenght = new Point3D();

        /// <summary>
        /// координаты цетра оси
        /// </summary>
        protected Point3D AxisCenter = new Point3D();

        /// <summary>
        /// длина оси
        /// </summary>
        private const float AxisLengthWidthRatio = 200;

        /// <summary>
        /// true - рисовать ось
        /// </summary>
        private readonly bool _useAxes;

        /// <summary>
        /// Цвет оси
        /// </summary>
        private Color _axisColor;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="useAxis">true - рисовать ось</param>
        protected Chart3D(bool useAxis)
        {
            _useAxes = useAxis;
        }

        /// <summary>
        /// Задать кол-во точек
        /// </summary>
        /// <param name="nSize">кол-во точек</param>
        public void SetDataNo(int nSize)
        {
            Vertices = new Vertex3D[nSize];
        }

        /// <summary>
        /// Задать оси
        /// </summary>
        public void SetAxes(Color clrAxis, float offset)
        {
            _axisColor = clrAxis;
            CalcDataRange();
            SetAxes(offset);
            //if (offset)
            //    SetAxes(0.0f);
            //else
            //    SetAxes(0.07f);
        }

        /// <summary>
        /// Получить кол-во точек
        /// </summary>
        /// <returns>кол-во точек</returns>
        protected int GetDataNo()
        {
            return Vertices.Length;
        }

        /// <summary>
        /// Вычислить значения для рисования оси
        /// </summary>
        private void CalcDataRange()
        {
            int nDataNo = GetDataNo();
            if (nDataNo == 0) return;
            MinPoint3D = new Point3D(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            //MinPoint3D = new Point3D(0d, 0d, 0d);
            MaxPoint3D = new Point3D(Single.MinValue, Single.MinValue, Single.MinValue);

            for (int i = 0; i < nDataNo; i++)
            {
                Vertex3D tempVert = this[i];
                if (tempVert == null)
                {
                    continue;
                }

                Point3D currentPoint = new Point3D(tempVert.X, tempVert.Y, tempVert.Z);
                if (MinPoint3D.X > currentPoint.X) MinPoint3D.X = currentPoint.X;
                if (MinPoint3D.Y > currentPoint.Y) MinPoint3D.Y = currentPoint.Y;
                if (MinPoint3D.Z > currentPoint.Z) MinPoint3D.Z = currentPoint.Z;
                if (MaxPoint3D.X < currentPoint.X) MaxPoint3D.X = currentPoint.X;
                if (MaxPoint3D.Y < currentPoint.Y) MaxPoint3D.Y = currentPoint.Y;
                if (MaxPoint3D.Z < currentPoint.Z) MaxPoint3D.Z = currentPoint.Z;
            }
        }

        /// <summary>
        /// Задать ось
        /// </summary>
        /// <param name="margin">Расстояние от осей до графика</param>
        private void SetAxes(float margin)
        {
            double xRange = MaxPoint3D.X - MinPoint3D.X;
            double yRange = MaxPoint3D.Y - MinPoint3D.Y;
            double zRange = MaxPoint3D.Z - MinPoint3D.Z;

            Point3D center = new Point3D
            {
                X = MinPoint3D.X - margin * xRange,
                Y = MinPoint3D.Y - margin * yRange,
                Z = MinPoint3D.Z - margin * zRange
            };

            Point3D lenght = new Point3D
            {
                X = (1 + 2 * margin) * xRange,
                Y = (1 + 2 * margin) * yRange,
                Z = (1 + 2 * margin) * zRange
            };

            AxisLenght = lenght;
            AxisCenter = center;
        }

        /// <summary>
        /// добавить сетку осей в <paramref name="meshs"/>
        /// </summary>
        /// <param name="meshs">Список сеток</param>
        protected void AddAxesMeshes(List<Mesh3D> meshs)
        {
            if (!_useAxes) return;

            const int countPolygan = 15;

            double radius = (AxisLenght.X + AxisLenght.Y + AxisLenght.Z) / (3 * AxisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, AxisLenght.X, countPolygan);
            xAxisCylinder.SetColor(_axisColor);
            TransformMatrix.Transform(xAxisCylinder, new Point3D(AxisCenter.X + AxisLenght.X / 2, AxisCenter.Y, AxisCenter.Z), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, countPolygan);
            xAxisCone.SetColor(_axisColor);
            TransformMatrix.Transform(xAxisCone, new Point3D(AxisCenter.X + AxisLenght.X, AxisCenter.Y, AxisCenter.Z), 0, 90);
            meshs.Add(xAxisCone);

            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, AxisLenght.Y, countPolygan);
            yAxisCylinder.SetColor(_axisColor);
            TransformMatrix.Transform(yAxisCylinder, new Point3D(AxisCenter.X, AxisCenter.Y + AxisLenght.Y / 2, AxisCenter.Z), 90, 90);
            meshs.Add(yAxisCylinder);

            Mesh3D yAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, countPolygan);
            yAxisCone.SetColor(_axisColor);
            TransformMatrix.Transform(yAxisCone, new Point3D(AxisCenter.X, AxisCenter.Y + AxisLenght.Y, AxisCenter.Z), 90, 90);
            meshs.Add(yAxisCone);

            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, AxisLenght.Z, countPolygan);
            zAxisCylinder.SetColor(_axisColor);
            TransformMatrix.Transform(zAxisCylinder, new Point3D(AxisCenter.X, AxisCenter.Y, AxisCenter.Z + AxisLenght.Z / 2), 0, 0);
            meshs.Add(zAxisCylinder);

            Mesh3D zAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, countPolygan);
            zAxisCone.SetColor(_axisColor);
            TransformMatrix.Transform(zAxisCone, new Point3D(AxisCenter.X, AxisCenter.Y, AxisCenter.Z + AxisLenght.Z), 0, 0);
            meshs.Add(zAxisCone);

        }

        /// <summary>
        /// Выделение
        /// </summary>
        /// <param name="rect">прямоугольник выделения</param>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="viewport3D">поле просмотра</param>
        public virtual int[] Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3D)
        {
            return null;
        }

        /// <summary>
        /// Визуализация выделения
        /// </summary>
        /// <param name="meshGeometry">геометрия сетки</param>
        /// <param name="selectColor">Цвет выделения</param>
        public virtual void HighlightSelection(MeshGeometry3D meshGeometry, Color selectColor)
        {
        }

    }
}

