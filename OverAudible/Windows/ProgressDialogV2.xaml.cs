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
    /// Interaction logic for ProgressDialogV2.xaml
    /// </summary>
    public partial class ProgressDialogV2 : Window
    {

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public bool EnableCancelButton
        {
            get { return (bool)GetValue(EnableCancelButtonProperty); }
            set { SetValue(EnableCancelButtonProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(ProgressDialogV2), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(ProgressDialogV2), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty EnableCancelButtonProperty = DependencyProperty.Register("EnableCancelButton", typeof(bool), typeof(ProgressDialogV2), new PropertyMetadata(false));

        public Action CancelButtonAction { get; set; }


        public ProgressDialogV2()
        {
            InitializeComponent();
            CancelButtonAction = () => { };
        }

        public ProgressDialogV2(Action cancelButtonAction, bool enableCancelButton = false)
        {
            InitializeComponent();
            CancelButtonAction = cancelButtonAction;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelButtonAction();
        }
    }
}
