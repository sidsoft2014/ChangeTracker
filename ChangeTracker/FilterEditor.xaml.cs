using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChangeTracker
{
    /// <summary>
    /// Interaction logic for FilterEditor.xaml
    /// </summary>
    public partial class FilterEditor : Window
    {
        public FilterEditor()
        {
            InitializeComponent();
        }

        private void btnAddOrSaveFilterMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnAddOrSaveFilterMode.Content.ToString() == "New")
            {
                cbxFilterName.IsEditable = true;
                cbxFilterName.Text = "";
                cbxFilterName.Focus();
            }
            else
            {
                cbxFilterName.SelectedItem = cbxFilterName.Text;
                cbxFilterName.IsEditable = false;
            }
        }
    }
}
