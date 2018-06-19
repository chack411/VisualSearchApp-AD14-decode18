using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Microsoft.AppCenter.Push;

namespace VisualSearchApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //Applicable OCR server locations(at time of writing) are: westus, eastus2, westcentralus, westeurope, southeastasia
            AppConstants.SetOcrLocation("westus");

            //
            // Add your API keys of Cognitive Services.
            //
            //AppConstants.ComputerVisionApiKey = "{Your API Key here}";
            //AppConstants.BingWebSearchApiKey = "{Your API Key here}";

            //AppConstants.CustomVisionApiKey = "{Your Custom Vision API Key here}";
            //AppConstants.CustomVisionApiUrl = "{Your Csutom Vision Prediction URL for image file here}";
            // ex. https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/xxxxxx/image?iterationId=yyyyyy

            MainPage = new NavigationPage(new OcrSelectPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts

            //
            // Add your App Secret, if you use App Center. ( https://appcenter.ms )
            //
            //AppCenter.Start("ios=ba44a5f8-a131-4933-b285-59da14129699;" +
            //                "uwp=5074d29b-97a6-4bf1-aba9-f3c90edb017c;" +
            //                "android=814450af-c5b0-43d6-8ae9-419af7881119",
            //                typeof(Analytics), typeof(Crashes), typeof(Distribute), typeof(Push));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}