using OverAudible.API;
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
using OverAudible.Models;
using OverAudible.Services;
using OverAudible.EventMessages;
using OverAudible.ViewModels;

namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class NewCollectionModal : ShellPage
    {
        private readonly LibraryService _libraryService;

        public NewCollectionModal(LibraryService libraryService)
        {
            InitializeComponent();
            _libraryService = libraryService;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Shell.Current.CloseAndClearModal();
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShellUI.Controls.MessageBox.Show(Shell.Current, "Collection name cannot be blank", "Error");
                return;
            }

            var api = await ApiClient.GetInstance();

            var s = await api.CreateCollectionAsync(txtName.Text, txtDesc.Text, new(), true);

            string id = s["collection_id"].ToString();

            DateTime d = s.Value<DateTime>("creation_date");

            Collection c = new();
            c.Title = txtName.Text;
            c.CollectionId = id;
            c.Description = txtDesc.Text;
            c.CreationDate = d.ToString();
            c.Image1 = ApiClient.defaultCollectionURL;
            c.Image2 = ApiClient.defaultCollectionURL;
            c.Image3 = ApiClient.defaultCollectionURL;
            c.Image4 = ApiClient.defaultCollectionURL;

            _libraryService.AddCollection(c);
            Shell.Current.EventAggregator.Publish(new RefreshLibraryMessage(new NewCollectionMessage(c)));

            Shell.Current.CloseAndClearModal();

        }
    }
}
