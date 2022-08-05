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
    public partial class ManageIgnoreListModal : ShellPage
    {
        public ManageIgnoreListViewModel viewModel => this.DataContext as ManageIgnoreListViewModel;

        public ManageIgnoreListModal(ManageIgnoreListViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            this.Loaded += async (s, e) =>
            {
                await viewModel.LoadAsync();
            };

        }
    }
}
