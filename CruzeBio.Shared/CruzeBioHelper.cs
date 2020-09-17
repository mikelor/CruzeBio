
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Android.Net.Wifi.Aware;
using CruzeBio.Data;
using CruzeBio.Models;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using IdentifyRequest = CruzeBio.Models.IdentifyRequest;

namespace CruzeBio.Shared
{
    public static class CruzeBioHelper
    {
        const string FACE_ENDPOINT = @"";
        const string FACE_SUBSCRIPTION_KEY = @"";

        static string sourcePersonGroup = null;
        static IFaceClient faceClient;
        public static CruzeServices CruzeApi { get; private set; }

        public static bool IsFaceRegistered { get; set; }

        public static bool IsInitialized { get; set; }

        public static string WorkspaceKey
        {
            get;
            set;
        }

      
        public static Action<int> AuthenticationStatusImageCallback { get => authenticationStatusImageCallback; set => authenticationStatusImageCallback = value; }

        private static Action<int>authenticationStatusImageCallback;

        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
        public static void Init(Action throttled = null)
        {
            // Authenticate.
            CruzeApi = new CruzeServices(new RestService());
            faceClient = Authenticate(FACE_ENDPOINT, FACE_SUBSCRIPTION_KEY);
            WorkspaceKey = Guid.NewGuid().ToString();
            IsInitialized = true;
        }

        public static async Task RegisterFaces()
        {
            try
            {
                IsFaceRegistered = false;

                // Create a PersonGroup to hold our images
                // As of this writing Recogniton Model 3 is the most accurate model, so let's use that.
                // https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-api-how-to-topics/specify-recognition-model#detect-faces-with-specified-model
                string personGroupId = Guid.NewGuid().ToString();
                await faceClient.PersonGroup.CreateAsync(personGroupId, "Xamarin", recognitionModel: RecognitionModel.Recognition03);
                Person p = await faceClient.PersonGroupPerson.CreateAsync(personGroupId, "Albert Einstein");

                // Add a person with face detected by "detection_02" model
                await faceClient.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, p.PersonId, "https://upload.wikimedia.org/wikipedia/commons/d/d3/Albert_Einstein_Head.jpg", DetectionModel.Detection02);

                await faceClient.PersonGroup.TrainAsync(personGroupId);

                IsFaceRegistered = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static async Task ProcessCameraCapture(byte[] data)
        {
            MainActivity.StatusText.Text = "Calling Traveler Verification Service";
            MainActivity.StatusImage.SetImageResource(Resource.Drawable.Refresh);

            string base64 = System.Convert.ToBase64String(data);

            IdentifyRequest identifyRequest = new IdentifyRequest()
            {
                CarrierCode = "AS",
                FlightNumber = "1276",
                ScheduledEncounterPort = "LAS",
                ScheduledEncounterDate = "20200716",
                PhotoDate = "20200716",
                DeviceId = "Device1",
                DepartureTerminal = "3",
                DepartureGate = "E8",
                Photo = base64,
                Token = "MyToken"
            };
            IdentifyResponse identifyResponse = await CruzeApi.IdentifyAsync(identifyRequest);
            string statusText = "";
            int resourceId = 0;
            if (!String.IsNullOrEmpty(identifyResponse.Result))
            {
                statusText = string.Format($"{identifyResponse.Result} - UID={identifyResponse.UID}");
                
                if (identifyResponse.Result.Equals("Match"))
                {
                    resourceId = Resource.Drawable.Pass;
                }
                else
                {
                    resourceId = Resource.Drawable.Fail;
                }
            }
            else
            {
                resourceId = Resource.Drawable.Fail;
                statusText = string.Format($"No Face Detected or Poor Image");
            }
            MainActivity.StatusText.Text = statusText;
            authenticationStatusImageCallback?.Invoke(resourceId);

        }

    }
}
