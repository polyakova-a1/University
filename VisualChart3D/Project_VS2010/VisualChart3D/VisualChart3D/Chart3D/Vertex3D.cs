// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace VisualChart3D
{

    /// <summary>
    /// Точка в 3D пространстве
    /// </summary>
    public class Vertex3D
    {
        /// <summary>
        /// Цвет точки
        /// </summary>
        public System.Windows.Media.Color Color;

        /// <summary>
        /// расположение точки
        /// </summary>
        public double X, Y, Z;

        /// <summary>
        /// link to the viewport positions array index
        /// </summary>
        public int NMinI, NMaxI;

        /// <summary>
        /// Выбрана ли эта точка пользователем
        /// </summary>
        public bool Selected = false;

        static public Vertex3D ParseCoord(string strCoord)
        {
            string[] coord = strCoord.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return new Vertex3D()
            {
                X = double.Parse(coord[0]),
                Y = double.Parse(coord[1]),
                Z = double.Parse(coord[2])
            };
        }
    }
}
