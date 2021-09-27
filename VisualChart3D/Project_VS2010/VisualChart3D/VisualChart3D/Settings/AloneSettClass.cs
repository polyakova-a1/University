// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Media;
using VisualChart3D.Common;

namespace VisualChart3D
{
    /// <summary>
    /// Настройка класса объектов
    /// </summary>
    public class AloneSettClass
    {
        /// <summary>
        /// Получить или задать цвет объектов
        /// </summary>
        public Color ColorObject { get; set; }

        /// <summary>
        /// Тип объекта
        /// </summary>
        public Shapes Shape { get; set; }

        /// <summary>
        /// Имя класса
        /// </summary>
        public string NameClass { get; set; }
        public bool IsLiquid { get; set; }
    }
}
