using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDMTAIBotCognitive
{
    class Program
    {
        static FaceServiceClient faceServiceClient = new FaceServiceClient("e1dd2f28e8db47cb9fc1fd701de8869d", "https://southeastasia.api.cognitive.microsoft.com/face/v1.0");

        public static async void CreatePersonGroup(string personGroupId, string personGroupName) {
            try
            {
                await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);
                Console.WriteLine("Create Person Group succeed.");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public static async void AddPersonToGroup(string personGroupId, string personName, string imgPath) {
            try
            {
                await faceServiceClient.GetPersonGroupAsync(personGroupId);
                CreatePersonResult personResult = await faceServiceClient.CreatePersonAsync(personGroupId, personName);

                DetectFaceAndRegister(personGroupId, personResult, imgPath);
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
            }
        }

        private static async void DetectFaceAndRegister(string personGroupId, CreatePersonResult personResult, string imgPath)
        {
            foreach (var image in Directory.GetFiles(imgPath, "*.*")) {
                using (Stream s = File.OpenRead(image)) {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, personResult.PersonId, s);
                }
            }
            Console.WriteLine("Add images to " + personGroupId + " group succeed.");
        }

        private static async void TrainingAI(string personGroupId) {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
            TrainingStatus training = null;
            while (true) {
                training = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);
                if (training.Status != Status.Running) {
                    Console.WriteLine("Status: " + training.Status);
                    break;
                }
                Console.WriteLine("Waiting for training AI...");
                await Task.Delay(1000);
            }

            Console.WriteLine("Training AI completed.");
        }

        private static async void IdentifyFace(string personGroupId, string imgPath) {
            using (Stream s = File.OpenRead(imgPath)) {
                var faces = await faceServiceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();
                try
                {
                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine($"Result of face: {identifyResult.FaceId}");
                        if (identifyResult.Candidates.Length == 0)
                            Console.WriteLine("No one identified");
                        else
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            Console.WriteLine($"Identified as {person.Name}");
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static void Main(string[] args){

            //Step 1
            //CreatePersonGroup("class1", "class1");

            //Step 2
            //AddPersonToGroup("class1", "Syed", @"C:\Users\User\Downloads\Compressed\Syed");

            //Step 3
            //TrainingAI("class1");

            //Step 4
            IdentifyFace("class1", @"C:\Users\User\Downloads\_MG_2220.jpg");

            Console.ReadKey();
        }
    }
}
