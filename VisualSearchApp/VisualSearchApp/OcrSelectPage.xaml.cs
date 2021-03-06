﻿using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;

namespace VisualSearchApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OcrSelectPage : TabbedPage
    {
        #region fields
        private bool isFirstPageLoad = true;
		#endregion

		#region constructors
		public OcrSelectPage()
		{
			InitializeComponent();
			CrossMedia.Current.Initialize();
		}
		#endregion

		#region overrides
		protected override async void OnAppearing()
		{
            base.OnAppearing();
            if (isFirstPageLoad)
			{
                if (String.IsNullOrEmpty(AppConstants.ComputerVisionApiKey)
                    || String.IsNullOrEmpty(AppConstants.BingWebSearchApiKey)
                    || String.IsNullOrEmpty(AppConstants.CustomVisionApiKey)
                    || String.IsNullOrEmpty(AppConstants.CustomVisionApiUrl)
                    || String.IsNullOrEmpty(AppConstants.ComputerVisionApiOcrUrl)
                    || String.IsNullOrEmpty(AppConstants.ComputerVisionApiHandwritingUrl))
                {
                    await DisplayAlert("Error", "One or more of your keys are invalid. Please update App.xaml.cs with your Cognitive Services API keys and try again", "OK");
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                this.isFirstPageLoad = false;
			}
        }
        #endregion

        #region methods
        /// <summary>
        /// Called when Take Photo is pressed from the standard and handwritten OCR page.
        /// </summary>
        async void TakePhotoButtonClickEventHandler(object sender, EventArgs e)
		{
			byte[] photoByteArray = null;

			try
			{
                photoByteArray = await TakePhoto();
			}
			catch(Exception exc)
			{
                Console.WriteLine(exc.Message);
			}

			if (photoByteArray != null)
			{
                if (sender == ButtonTakeHandwritten)
                {
                    await Navigation.PushAsync(new OcrResultsPage(photoByteArray, true));
                }
                else if (sender == ButtonTakePrinted)
                {
                    await Navigation.PushAsync(new OcrResultsPage(photoByteArray, false));
                }
                else
                {
                    await Navigation.PushAsync(new NoodleResultsPage(photoByteArray));
                }
            }
		}

		/// <summary>
		/// Uses the Xamarin Media Plugin to import photos from the native photo library
		/// </summary>
		async void ImportPhotoButtonClickEventHandler(object sender, EventArgs e)
		{
            Boolean error = false;
			MediaFile photoMediaFile = null;
			byte[] photoByteArray = null;

			try
			{
				photoMediaFile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
					PhotoSize = PhotoSize.Medium,
				});
				photoByteArray = MediaFileToByteArray(photoMediaFile);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"ERROR: {exception.Message}");
                error = true;
			}

            if (error) 
            {
				await DisplayAlert("Error", "Error taking photo", "OK");
            }
            else if(photoByteArray != null)
			{
                if (sender == ButtonImportHandwritten)
                {
                    await Navigation.PushAsync(new OcrResultsPage(photoByteArray, true));
                }
                else if (sender == ButtonImportPrinted)
                {
                    await Navigation.PushAsync(new OcrResultsPage(photoByteArray, false));
                }
                else
                {
                    await Navigation.PushAsync(new NoodleResultsPage(photoByteArray));
                }
            }
		}
         
		/// <summary>
		/// Uses the Xamarin Media Plugin to take photos using the native camera 
        /// application
		/// </summary>
		async Task<byte[]> TakePhoto()
		{
			MediaFile photoMediaFile = null;
			byte[] photoByteArray = null;

			if (CrossMedia.Current.IsCameraAvailable)
			{
				var mediaOptions = new StoreCameraMediaOptions
                {
					PhotoSize = PhotoSize.Medium,
					AllowCropping = false,
					SaveToAlbum = true,
                    RotateImage = true,
                    Name = $"{DateTime.UtcNow}.jpg"
				};
				photoMediaFile = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
				photoByteArray = MediaFileToByteArray(photoMediaFile);
            }
            else
			{
				await DisplayAlert("Error", "No camera found", "OK");
				Console.WriteLine($"ERROR: No camera found");
			}
            return photoByteArray;
        }

        /// <summary>
        /// Convert the media file to a byte array.
        /// </summary>
		byte[] MediaFileToByteArray(MediaFile photoMediaFile)
		{
			using (var memStream = new MemoryStream())
			{
				photoMediaFile.GetStream().CopyTo(memStream);
				return memStream.ToArray();
			}
		}
        #endregion    
    }
}