// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualChart3D.Common;

namespace VisualChart3D.ConfigWindow
{
    /// <summary>
    /// Окно для изменения настроек отрисовки
    /// </summary>
    public partial class ClassDisplayingConfigs
    {
        const string BadClassCountMessage = "SettingsClassesForms.ArrayClass count is less then 1";

        /// <summary>
        /// Настройки
        /// </summary>
        public ClassVisualisationSettings SettingsClassesForms { get; set; }

        /// <summary>
        /// Значения <see cref="Shapes"/>
        /// </summary>
        private readonly IEnumerable<Shapes> _arrShape = Enum.GetValues(typeof(Shapes)).Cast<Shapes>();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="stg">начальные настройки</param>
        public ClassDisplayingConfigs(ClassVisualisationSettings stg)
        {
            InitializeComponent();
            SettingsClassesForms = stg;
            CBshapes.Items.Clear();
            TbSizeObjects.Text = SettingsClassesForms.SizeObjectStr;
            NmCountPolig.Value = SettingsClassesForms.CountPoligon;

            foreach (Shapes shape in _arrShape)
            {
                CBshapes.Items.Add(shape.GetRusName());
            }

            foreach (AloneSettClass clss in SettingsClassesForms.ArrayClass)
            {
                cbClasses.Items.Add(clss.NameClass);
            }

            cbClasses.SelectedIndex = 0;

            //НАЙТИ ПРИЧИНУ
            if (SettingsClassesForms.ArrayClass.Length == 0)
            {
                throw new NotImplementedException(BadClassCountMessage);
            }

            CnvColor.Background = new SolidColorBrush(SettingsClassesForms.ArrayClass[0].ColorObject);
            CBshapes.SelectedItem = SettingsClassesForms.ArrayClass[0].Shape.GetRusName();
        }

        private Color? SelectColor(Canvas cnv)
        {
            System.Windows.Forms.ColorDialog clrDlg = new System.Windows.Forms.ColorDialog();

            if (clrDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color clr = clrDlg.Color.ToColor();
                SolidColorBrush b = new SolidColorBrush(clr);
                cnv.Background = b;
                return clr;
            }

            return null;
        }


        private void cnvColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (cbClasses.SelectedIndex == -1)
            {
                return;
            }

            Color? clr = SelectColor(CnvColor);
            if (clr.HasValue)
            {
                SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].ColorObject = clr.Value;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SettingsClassesForms.CountPoligonStr = NmCountPolig.Value.ToString();
            SettingsClassesForms.SizeObjectStr = TbSizeObjects.Text;

            string errorMessage = SettingsClassesForms.ValidationStrValues();

            if (string.IsNullOrEmpty(errorMessage))
            {
                DialogResult = true;
            }
            else
            {
                Utils.ShowWarningMessage(errorMessage);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CbClasses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbClasses.SelectedIndex == -1)
            {
                return;
            }

            CnvColor.Background = new SolidColorBrush(SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].ColorObject);
            CBshapes.SelectedItem = SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].Shape.GetRusName();
            cbIsClassVisible.IsChecked = SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].IsLiquid;
        }

        private void CBshapes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbClasses.SelectedIndex == -1)
            {
                return;
            }

            foreach (Shapes shape in _arrShape)
            {
                if (shape.GetRusName() == CBshapes.SelectedItem.ToString())
                {
                    SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].Shape = shape;
                    break;
                }
            }
        }

        private void cbIsClassVisible_Checked(object sender, RoutedEventArgs e)
        {
            SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].IsLiquid = true;
        }

        private void cbIsClassVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsClassesForms.ArrayClass[cbClasses.SelectedIndex].IsLiquid = false;
        }
    }
}
