// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// Представление точки для точечного графика
    /// </summary>
    class ScatterPlotItem : Vertex3D
    {
        /// <summary>
        /// Ширина точки, по xy направлениям
        /// </summary>
        public double W;

        /// <summary>
        /// Глубина точки, по z направлению.
        /// </summary>
        public double H;

        /// <summary>
        /// Тип фигуры
        /// </summary>
        public Shapes ShapeType;

    }
}

