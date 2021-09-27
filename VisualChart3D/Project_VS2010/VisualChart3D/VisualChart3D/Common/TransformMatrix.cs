// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace VisualChart3D.Common
{
    /// <summary>
    /// класс для 3d вращения, перемещения и зума.
    /// </summary>
    public class TransformMatrix
    {
        private Matrix3D _viewMatrix = new Matrix3D();
        private Matrix3D _projMatrix = new Matrix3D();
        private Matrix3D _totalMatrix;

        public Matrix3D GetTotalMatrix() => _totalMatrix;

        /// <summary>
        /// чувствительность для увеличения
        /// </summary>
        public const double ScaleFactor = 1.3;

        /// <summary>
        /// Нажата ли кнопка мыши
        /// </summary>
        private bool _isMouseDown;

        /// <summary>
        /// Предыдущее месторасположение мыши
        /// </summary>
        private Point _movePoint;

        public void ResetView()
        {
            _viewMatrix.SetIdentity();
        }

        public void OnLBtnDown(Point pt)
        {
            _isMouseDown = true;
            _movePoint = pt;
        }

        public void OnMouseMove(Point pt, System.Windows.Controls.Viewport3D viewPort)
        {
            if (!_isMouseDown) return;

            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;
            
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                double shiftX = 2 * (pt.X - _movePoint.X) / (width);
                double shiftY = -2 * (pt.Y - _movePoint.Y) / (width);
                _viewMatrix.Translate(new Vector3D(shiftX, shiftY, 0));
                _movePoint = pt;
            }

            else
            {
                double aY = 180 * (pt.X - _movePoint.X) / width;
                double aX = 180 * (pt.Y - _movePoint.Y) / height;

                _viewMatrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), aX));
                _viewMatrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), aY));
                _movePoint = pt;
            }
            _totalMatrix = Matrix3D.Multiply(_projMatrix, _viewMatrix);
        }

        public void OnLBtnUp()
        {
            _isMouseDown = false;
        }

        public void OnKeyDown(KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Home:
                    _viewMatrix.SetIdentity();
                    break;
                case Key.OemPlus:
                    _viewMatrix.Scale(new Vector3D(ScaleFactor, ScaleFactor, ScaleFactor));
                    break;
                case Key.OemMinus:
                    _viewMatrix.Scale(new Vector3D(1 / ScaleFactor, 1 / ScaleFactor, 1 / ScaleFactor));
                    break;
                default:
                    return;
            }
            _totalMatrix = Matrix3D.Multiply(_projMatrix, _viewMatrix);
        }

        public void OnWheel(MouseWheelEventArgs e)
        {
            double coef = (double)e.Delta / 100;
            _viewMatrix.Scale(coef > 0 ? new Vector3D(coef, coef, coef) : new Vector3D(1 / -coef, 1 / -coef, 1 / -coef));
            _totalMatrix = Matrix3D.Multiply(_projMatrix, _viewMatrix);
        }

        public static Point3D Transform(Point3D pt1, Point3D center, double aX, double aZ)
        {
            double angleX = 3.1415926f * aX / 180;
            double angleZ = 3.1415926f * aZ / 180;

            // вращение z-оси
            double x2 = pt1.X * Math.Cos(angleZ) + pt1.Z * Math.Sin(angleZ);
            double y2 = pt1.Y;
            double z2 = -pt1.X * Math.Sin(angleZ) + pt1.Z * Math.Cos(angleZ);

            double x3 = center.X + x2 * Math.Cos(angleX) - y2 * Math.Sin(angleX);
            double y3 = center.Y + x2 * Math.Sin(angleX) + y2 * Math.Cos(angleX);
            double z3 = center.Z + z2;

            return new Point3D(x3, y3, z3);
        }

        public static void Transform(Mesh3D model, Point3D center, double aX, double aZ)
        {
            double angleX = 3.1415926f * aX / 180;
            double angleZ = 3.1415926f * aZ / 180;

            int nVertNo = model.VertexNo;
            for (int i = 0; i < nVertNo; i++)
            {
                Point3D pt1 = model.GetPoint(i);
                // вращение z-оси
                double x2 = pt1.X * Math.Cos(angleZ) + pt1.Z * Math.Sin(angleZ);
                double y2 = pt1.Y;
                double z2 = -pt1.X * Math.Sin(angleZ) + pt1.Z * Math.Cos(angleZ);

                double x3 = center.X + x2 * Math.Cos(angleX) - y2 * Math.Sin(angleX);
                double y3 = center.Y + x2 * Math.Sin(angleX) + y2 * Math.Cos(angleX);
                double z3 = center.Z + z2;

                model.SetPoint(i, x3, y3, z3);
            }
        }

        public void CalculateProjectionMatrix(Mesh3D mesh, double scaleFactor)
        {
            CalculateProjectionMatrix(mesh.MinPoint, mesh.MaxPoint, scaleFactor);
        }

        public void CalculateProjectionMatrix(double min, double max, double scaleFactor)
        {
            CalculateProjectionMatrix(new Point3D(min, min, min), new Point3D(max, max, max), scaleFactor);
        }

        public void CalculateProjectionMatrix(Point3D min, Point3D max, double scaleFactor)
        {
            double xC = (min.X + max.X) / 2;
            double yC = (min.Y + max.Y) / 2;
            double zC = (min.Z + max.Z) / 2;

            double xRange = (max.X - min.X) / 2;
            double yRange = (max.Y - min.Y) / 2;
            double zRange = (max.Z - min.Z) / 2;

            _projMatrix.SetIdentity();
            _projMatrix.Translate(new Vector3D(-xC, -yC, -zC));

            if (xRange < 1e-10) return;

            double sX = scaleFactor / xRange;
            double sY = scaleFactor / yRange;
            double sZ = scaleFactor / zRange;
            _projMatrix.Scale(new Vector3D(sX, sY, sZ));

            _totalMatrix = Matrix3D.Multiply(_projMatrix, _viewMatrix);
        }

        /// <summary>
        /// получить положение экрана из оригинальной вершины
        /// </summary>
        public Point VertexToScreenPt(Point3D point, System.Windows.Controls.Viewport3D viewPort)
        {
            Point3D pt2 = _totalMatrix.Transform(point);

            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            double x3 = width / 2 + (pt2.X) * width / 2;
            double y3 = height / 2 - (pt2.Y) * width / 2;

            return new Point(x3, y3);
        }

        public static Point ScreenPointToViewportPoint(Point point, System.Windows.Controls.Viewport3D viewPort)
        {
            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            double x3 = point.X;
            double y3 = point.Y;
            double x2 = (x3 - width / 2) * 2 / width;
            double y2 = (height / 2 - y3) * 2 / width;

            return new Point(x2, y2);
        }

        public Point VertexToViewportPt(Point3D point, System.Windows.Controls.Viewport3D viewPort)
        {
            Point3D pt2 = _totalMatrix.Transform(point);

            return new Point(pt2.X, pt2.Y);
        }
    }
}