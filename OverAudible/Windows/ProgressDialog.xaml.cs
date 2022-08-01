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

namespace OverAudible.Windows
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ProgressDialog), new PropertyMetadata(string.Empty));



        public ProgressDialog()
        {
            InitializeComponent();
        }

        public static async Task<T> ShowDialogAsync<T>(string title, string message, Func<Task<T>> operation) where T : class
        {
            ProgressDialog p = new()
            {
                Title = title,
                Message = message
            };
            p.Show();

            T result = await operation.Invoke();

            p.Close();

            return result;
        }
    }
}
