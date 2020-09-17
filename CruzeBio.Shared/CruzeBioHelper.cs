
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace CruzeBio.Shared
{
    public static class CruzeBioHelper
    {
        const string FACE_ENDPOINT = @"https://cruze-cognitive-face-test.cognitiveservices.azure.com/";
        const string FACE_SUBSCRIPTION_KEY = @"2aaf3f27e36d44e28ff432b5a5852ac7";

        static string sourcePersonGroup = null;
        static IFaceClient faceClient;
        public static bool IsFaceRegistered { get; set; }

        public static bool IsInitialized { get; set; }

        public static string WorkspaceKey
        {
            get;
            set;
        }
        public static Action<string> GreetingsCallback { get => greetingsCallback; set => greetingsCallback = value; }

        private static Action<string> greetingsCallback;

        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
        public static void Init(Action throttled = null)
        {
            // Authenticate.
            faceClient = Authenticate(FACE_ENDPOINT, FACE_SUBSCRIPTION_KEY);

            /*
            FaceServiceHelper.ApiKey = "b1843365b41247538cffb304d36609b3";
            if(throttled!=null)
            FaceServiceHelper.Throttled += throttled;
            */

            WorkspaceKey = Guid.NewGuid().ToString();

            /*
            ImageAnalyzer.PeopleGroupsUserDataFilter = WorkspaceKey;
            FaceListManager.FaceListsUserDataFilter = WorkspaceKey;
            */
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
        /*
        public static async Task ProcessCameraCapture(ImageAnalyzer e)
        {

            DateTime start = DateTime.Now;

            await e.DetectFacesAsync();

            if (e.DetectedFaces.Any())
            {
                await e.IdentifyFacesAsync();
                string greetingsText = GetGreettingFromFaces(e);

                if (e.IdentifiedPersons.Any())
                {

                    if (greetingsCallback != null)
                    {
                        DisplayMessage(greetingsText);
                    }

                    Console.WriteLine(greetingsText);
                }
                else
                {
                    DisplayMessage("No Idea, who you're.. Register your face.");

                    Console.WriteLine("No Idea");

                }
            }
            else
            {
               // DisplayMessage("No face detected.");

                Console.WriteLine("No Face ");

            }

            TimeSpan latency = DateTime.Now - start;
            var latencyString = string.Format("Face API latency: {0}ms", (int)latency.TotalMilliseconds);
            Console.WriteLine(latencyString);
        }

        private static string GetGreettingFromFaces(ImageAnalyzer img)
        {
            if (img.IdentifiedPersons.Any())
            {
                string names = img.IdentifiedPersons.Count() > 1 ? string.Join(", ", img.IdentifiedPersons.Select(p => p.Person.Name)) : img.IdentifiedPersons.First().Person.Name;

                if (img.DetectedFaces.Count() > img.IdentifiedPersons.Count())
                {
                    return string.Format("Welcome back, {0} and company!", names);
                }
                else
                {
                    return string.Format("Welcome back, {0}!", names);
                }
            }
            else
            {
                if (img.DetectedFaces.Count() > 1)
                {
                    return "Hi everyone! If I knew any of you by name I would say it...";
                }
                else
                {
                    return "Hi there! If I knew you by name I would say it...";
                }
            }
        }
        */
        static void DisplayMessage(string greetingsText)
        {
            greetingsCallback?.Invoke(greetingsText);
        }
    }
}
