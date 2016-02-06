using ChangeTracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChangeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            Globals.Init();
            _vm = this.DataContext as MainViewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
