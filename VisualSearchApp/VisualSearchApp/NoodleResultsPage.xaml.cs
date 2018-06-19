using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;

namespace VisualSearchApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoodleResultsPage : ContentPage
    {
        #region fields
        HttpClient visionApiClient;
        byte[] photo;
        ObservableCollection<string> values;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:VisualSearchApp.OcrResultsPage"/> class.
        /// </summary>
        /// <param name="photo">Photo to find text in</param>
        /// <param name="isHandwritten">Indicates whether the text in the photo is handwritten</param>
        public NoodleResultsPage(byte[] photo)
        {
            InitializeComponent();
            this.photo = photo;
            visionApiClient = new HttpClient();
            visionApiClient.DefaultRequestHeaders.Add("Prediction-Key", AppConstants.CustomVisionApiKey);
        }
        #endregion

        #region overrides
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (values == null)
            {
                await LoadData();
            }
        }
		#endregion

		#region methods
        async Task LoadData()
        {
			// Try loading the results, show error message if necessary.
			Boolean error = false;
			try
			{
				values = await FetchNoodleList();
			}
			catch
			{
				error = true;
			}

			// Hide the spinner, show the table
			LoadingIndicator.IsVisible = false;
			LoadingIndicator.IsRunning = false;

			if (error)
			{
				await ErrorAndPop("Error", "Error finding noodle", "OK");
			}
            else if (values.Count() > 0)
			{
                NoodleResult.Text = values.Distinct().First();
                NoodlePict.Source = ImageSource.FromStream(() => new MemoryStream(photo));
			}
			else
			{
				await ErrorAndPop("Error", "No noodle found", "OK");
			}
        }

		/// <summary>
		/// Handles the selection of an item in the list
		/// </summary>
		async void ListItemSelectionEventHandler(object sender, SelectedItemChangedEventArgs e)
        {
            // ItemSelected is called on both selection and deselection; if null 
            // (i.e. it's a deselect) do nothing
            if (e.SelectedItem == null) { return; }

            // Show the WebResultsPage for the selected item
            await Navigation.PushAsync(new WebResultsPage((string)e.SelectedItem));

            // Deselect the item
            ((ListView)sender).SelectedItem = null;
        }

		/// <summary>
		/// Uses the Computer Vision API to parse printed text from the photo 
        /// set in the constructor
		/// </summary>
		async Task<ObservableCollection<string>> FetchNoodleList()
        {
            ObservableCollection<string> wordList = new ObservableCollection<string>();
            if (photo != null)
            {
                HttpResponseMessage response = null;
                //using (var content = new StreamContent(photoStream))
                using (var content = new ByteArrayContent(photo))
                {
                    // The media type of the body sent to the API. 
                    // "application/octet-stream" defines an image represented 
                    // as a byte array
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await visionApiClient.PostAsync(AppConstants.CustomVisionApiUrl, content);
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string noodleResponseString = await response.Content.ReadAsStringAsync();
                    string result = ConvertToText(GetTopPredictionResult(noodleResponseString));
                    wordList.Add(result);
                }
            }
            return wordList;
        }

        public Prediction GetTopPredictionResult(string jsonData)
        {
            NoodlePrediction NoodleData = JsonConvert.DeserializeObject<NoodlePrediction>(jsonData);

            Prediction result = new Prediction
            {
                Tag = "",
                Probability = 0.0F
            };

            foreach (Prediction prediction in NoodleData.Predictions)
            {
                if (prediction.Probability > result.Probability)
                    result = prediction;
            }

            return result;
        }

        public string ConvertToText(Prediction result)
        {
            string msg;

            if (result.Tag == "Udon")
                msg = string.Format("うどん です (確度 {0})", result.Probability.ToString("P1"));
            else if (result.Tag == "OkinawaSoba")
                msg = string.Format("沖縄そば です (確度 {0})", result.Probability.ToString("P1"));
            else if (result.Tag == "Soba")
                msg = string.Format("蕎麦 です (確度 {0})", result.Probability.ToString("P1"));
            else if (result.Tag == "Ramen")
                msg = string.Format("ラーメン です (確度 {0})", result.Probability.ToString("P1"));
            else if (result.Tag == "HiyashiChuka")
                msg = string.Format("冷やし中華 です (確度 {0})", result.Probability.ToString("P1"));
            else if (result.Tag == "Jiro")
                msg = string.Format("二郎 です (確度 {0})", result.Probability.ToString("P1"));
            else
                return "判別できませんでした";

            if (result.Probability < 0.6F)
                msg = "たぶん " + msg;

            return msg;
        }

        /// <summary>
        /// Shows an error message, navigates back after it is dismissed.
        /// </summary>
		protected async Task ErrorAndPop(string title, string text, string button)
		{
			await DisplayAlert(title, text, button);
            Console.WriteLine($"ERROR: {text}");
			await Task.Delay(TimeSpan.FromSeconds(0.1d));
			await Navigation.PopAsync(true);
		}
        #endregion
    }

    public class NoodlePrediction
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public DateTime Created { get; set; }
        public Prediction[] Predictions { get; set; }
    }

    public class Prediction
    {
        public string TagId { get; set; }
        public string Tag { get; set; }
        public float Probability { get; set; }
    }
}