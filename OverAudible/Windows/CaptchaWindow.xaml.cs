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
    /// Interaction logic for CaptchaWindow.xaml
    /// </summary>
    public partial class CaptchaWindow : Window
    {


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(CaptchaWindow));

        public string? Result { get; private set; } = null;


        public CaptchaWindow()
        {
            InitializeComponent();
            this.Closing += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(Result))
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "Please complete the captcha to continue");
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Result = txtBox.Text;
            this.Close();
        }

        public static string ShowCaptcha(ImageSource image)
        {
            CaptchaWindow window = new CaptchaWindow();
            window.Image = image;
            window.ShowDialog();

            return String.IsNullOrEmpty(window.Result) ? String.Empty : window.Result;
        }
    }
}
