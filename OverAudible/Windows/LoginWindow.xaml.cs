using OverAudible.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public object? Result { get; set; }

        public static string emailRegex = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                               + "@"
                               + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

        public LoginWindow()
        {
            InitializeComponent();
            
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string pass = txtPass.Visibility == Visibility.Visible ? txtPass.Password : txtPassVisible.Text;
            string email = txtEmail.Text;

            if (!ValidateEmail(email))
            {

            }
            if (!ValidatePassword(pass))
            {

            }

            try
            {
                var s = await ApiClient.GetInstance(email, pass);
                
            }
            catch (Exception ex)
            {

            }

            this.Result = true;
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var destinationurl = "https://www.audible.com/";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }

        public static bool ValidateEmail(string text)
        {
            if (text is null)
                return false;
            if (new Regex(emailRegex).Match(text).Success)
                return true;
            return false;
        }

        public static bool ValidatePassword(string password)
        {
            if (password is null)
                return false;
            if (password.Length > 3)
                return true;
            return false;
        }

        private void PackIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtPass.Visibility == Visibility.Visible)
            {
                txtPass.Visibility = Visibility.Collapsed;
                txtPassVisible.Visibility = Visibility.Visible;
                txtPassVisible.Text = txtPass.Password;
            }
            else
            {
                txtPass.Visibility = Visibility.Visible;
                txtPassVisible.Visibility = Visibility.Collapsed;
                txtPass.Password = txtPassVisible.Text;
            }

        }
    }
}
