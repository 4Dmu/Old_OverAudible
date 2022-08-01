using AudibleApi;
using OverAudible.Windows;
using ShellUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OverAudible.API
{
    public class LoginCallback : ILoginCallback
    {
        private readonly string _email;
        private readonly string _pass;

        public LoginCallback(string email, string pass)
        {
            _email = email;
            _pass = pass;
        }


        public async Task<string> Get2faCode()
        {
            await Task.Delay(1);

            return TextDialog.GetMessage("Two-Step Verification Code", "Please enter your 2fa code");
        }

        public async Task<string> GetCaptchaAnswer(byte[] captchaImage)
        {
            await Task.Delay(1);
            BitmapSource bitmapSource = BitmapSource.Create(2, 2, 300, 300, PixelFormats.Indexed8, BitmapPalettes.Gray256, captchaImage, 2);
            return CaptchaWindow.ShowCaptcha(bitmapSource);
        }

        public async Task<(string email, string password)> GetLogin()
        {
            await Task.Delay(1);

            return (_email, _pass);
        }

        public Task<(string name, string value)> GetMfaChoice(MfaConfig mfaConfig)
        {
            throw new NotImplementedException();
        }

        public void ShowApprovalNeeded()
        {
            throw new NotImplementedException();
        }
    }
}
