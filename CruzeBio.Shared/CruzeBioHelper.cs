
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

        // Used in the Detect Faces and Verify examples.
// Recognition model 3 is used for feature extraction, use 1 to simply recognize/detect a face. 
// However, the API calls to Detection that are used with Verify, Find Similar, or Identify must share the same recognition model.
const string RECOGNITION_MODEL3 = RecognitionModel.Recognition03;
const string RECOGNITION_MODEL1 = RecognitionModel.Recognition01;

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

            FaceServiceHelper.ApiKey = "b1843365b41247538cffb304d36609b3";
            if(throttled!=null)
            FaceServiceHelper.Throttled += throttled;

            WorkspaceKey = Guid.NewGuid().ToString();
            ImageAnalyzer.PeopleGroupsUserDataFilter = WorkspaceKey;
            FaceListManager.FaceListsUserDataFilter = WorkspaceKey;

            IsInitialized = true;
        }

        public static async Task RegisterFaces()
        {


            try
            {
                // Create a person group. 
                string personGroupId = Guid.NewGuid().ToString();
                sourcePersonGroup = personGroupId; // This is solely for the snapshot operations example
                Console.WriteLine($"Create a person group ({personGroupId}).");
                await faceClient.PersonGroup.CreateAsync(personGroupId, personGroupId, recognitionModel: recognitionModel);
                // The similar faces will be grouped into a single person group person.
                foreach (var groupedFace in personDictionary.Keys)
                {
                    // Limit TPS
                    await Task.Delay(250);
                    Person person = await client.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: groupedFace);
                    Console.WriteLine($"Create a person group person '{groupedFace}'.");

                    // Add face to the person group person.
                    foreach (var similarImage in personDictionary[groupedFace])
                    {
                        Console.WriteLine($"Add face to the person group person({groupedFace}) from image `{similarImage}`");
                        PersistedFace face = await client.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, person.PersonId,
                            $"{url}{similarImage}", similarImage);
                    }
                }



                var persongroupId = Guid.NewGuid().ToString();
                await FaceServiceHelper.CreatePersonGroupAsync(persongroupId,
                                                        "Xamarin",
                                                     WorkspaceKey);
                await FaceServiceHelper.CreatePersonAsync(persongroupId, "Albert Einstein");

                var personsInGroup = await FaceServiceHelper.GetPersonsAsync(persongroupId);

                await FaceServiceHelper.AddPersonFaceAsync(persongroupId, personsInGroup[0].PersonId,
                                                           "https://upload.wikimedia.org/wikipedia/commons/d/d3/Albert_Einstein_Head.jpg", null, null);

                await FaceServiceHelper.TrainPersonGroupAsync(persongroupId);


                IsFaceRegistered = true;


            }
            catch (FaceAPIException ex)

            {
                Console.WriteLine(ex.Message);
                IsFaceRegistered = false;

            }

        }

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

        static void DisplayMessage(string greetingsText)
        {
            greetingsCallback?.Invoke(greetingsText);
        }
    }
}
