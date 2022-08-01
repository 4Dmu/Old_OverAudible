using OverAudible.Models;
using OverAudible.Services;
using OverAudible.ViewModels;
using ShellUI.Attributes;
using ShellUI.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class AddToCollectionModal : ShellPage
    {
        public AddToCollectionModalViewModel viewModel => this.DataContext as AddToCollectionModalViewModel;

        public AddToCollectionModal(AddToCollectionModalViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            this.Loaded += OnLoad;
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoad;
            this.Unloaded -= OnUnloaded;
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            await viewModel.LoadAsync();
        }

        private void CollectionInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SelectCollectionCommand.Execute(null);
        }
    }
}
