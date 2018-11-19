using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Kinesis;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;


using Amazon.Rekognition;
using Amazon.Rekognition.Model;

using Amazon.S3;
using Amazon.S3.Model;
using AWSRekonition.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSRekonition
{
    public class Function : IAWSRekonitionFunction
    {
        readonly IAmazonRekognition _rekognitionClient;
        static readonly Dictionary<string, string> dicPersonToCollectionId = new Dictionary<string, string>();

        /// <summary>
        /// The default minimum confidence used for detecting labels.
        /// </summary>
        public const float DEFAULT_MIN_CONFIDENCE = 70f;
        const string image = "ryan.jpg";
        const string bucket = "ryan-rekognition-collection";
        const string kdStreamArn = "arn:aws:kinesis:ap-northeast-1:198104553544:stream/RekonitionDataStream";
        const string kvStreamArn = "arn:aws:kinesisvideo:ap-northeast-1:198104553544:stream/RekognitionStream/1542164028894";
        const string iamRoleArn = "";
        const float matchThreshold = 50f;

        /// <summary>
        /// The name of the environment variable to set which will override the default minimum confidence level.
        /// </summary>
        public const string MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME = "MinConfidence";

        float MinConfidence { get; set; } = DEFAULT_MIN_CONFIDENCE;

        HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };


        public Function()
        {
            _rekognitionClient = new AmazonRekognitionClient();
        }


        public Function(IAmazonRekognition rekognitionClient)
        {
            _rekognitionClient = rekognitionClient;
        }

        public async Task FunctionHandler(ILambdaContext context, MemoryStream image, string cardHolderName)
        {
            #region
            //foreach (var record in input.Records)
            //{
            //    if (!SupportedImageTypes.Contains(Path.GetExtension(record.S3.Object.Key)))
            //    {
            //        Console.WriteLine($"Object {record.S3.Bucket.Name}:{record.S3.Object.Key} is not a supported image type");
            //        continue;
            //    }

            //    Console.WriteLine($"Looking for labels in image {record.S3.Bucket.Name}:{record.S3.Object.Key}");
            //    var detectResponses = await this._rekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
            //    {
            //        MinConfidence = MinConfidence,
            //        Image = new Image
            //        {
            //            S3Object = new Amazon.Rekognition.Model.S3Object
            //            {
            //                Bucket = record.S3.Bucket.Name,
            //                Name = record.S3.Object.Key
            //            }
            //        }
            //    });



            //    var tags = new List<Tag>();
            //    foreach (var label in detectResponses.Labels)
            //    {
            //        if (tags.Count < 10)
            //        {
            //            Console.WriteLine($"\tFound Label {label.Name} with confidence {label.Confidence}");
            //            tags.Add(new Tag { Key = label.Name, Value = label.Confidence.ToString() });
            //        }
            //        else
            //        {
            //            Console.WriteLine($"\tSkipped label {label.Name} with confidence {label.Confidence} because the maximum number of tags has been reached");
            //        }
            //    }

            //    await this._s3Client.PutObjectTaggingAsync(new PutObjectTaggingRequest
            //    {
            //        BucketName = record.S3.Bucket.Name,
            //        Key = record.S3.Object.Key,
            //        Tagging = new Tagging
            //        {
            //            TagSet = tags
            //        }
            //    });
            //}
            //return;
            #endregion

            await SearchFaceByImage(image, cardHolderName);
        }

        public async Task<CreateCollectionResponse> CreateFacesCollection(string cardHolderName)
        {
            var request = new CreateCollectionRequest
            {
                CollectionId = cardHolderName
            };
            var response = await _rekognitionClient.CreateCollectionAsync(request);

            return response;
        }
        public async Task<IndexFacesResponse> AddFacesToCollection(string collectionId, string bucket, string photo)
        {

            var request = new IndexFacesRequest
            {
                Image = new Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = bucket,
                        Name = photo
                    }

                },
                CollectionId = collectionId,
                ExternalImageId = photo,
                DetectionAttributes = { "ALL" }
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);

            return response;
        }
        public async Task<IndexFacesResponse> AddFacesToCollection(string collectionId, MemoryStream imageStream)
        {
            var request = new IndexFacesRequest
            {
                Image = new Image
                {
                    Bytes = imageStream
                },
                CollectionId = collectionId,
                DetectionAttributes = { "ALL" }
            };

            var response = await _rekognitionClient.IndexFacesAsync(request);

            return response;
        }
        public async Task<SearchFacesByImageResponse> SearchFaceByImage(MemoryStream image, string cardHolderName)
        {
            var response = await _rekognitionClient.SearchFacesByImageAsync(new SearchFacesByImageRequest
            {
                Image = new Image
                {
                    Bytes = image
                },
                CollectionId = cardHolderName,
                FaceMatchThreshold = matchThreshold,
                MaxFaces = 3
            });


            return response;
        }

        public async Task<DetectFacesResponse> DetectImage(MemoryStream image)
        {
            var response = await _rekognitionClient.DetectFacesAsync(new DetectFacesRequest
            {
                Attributes = new List<string> { "ALL" },
                Image = new Image
                {
                    Bytes = image
                }
            });


            return response;
        }

        public async Task<DeleteCollectionResponse> DeleteCollection(string cardholderName)
        {
            var response = await _rekognitionClient.DeleteCollectionAsync(new DeleteCollectionRequest
            {
                CollectionId = cardholderName
            });

            return response;
        }

        //private async Task<CreateStreamProcessorResponse> CreateStreamProcessor(string streamProcessorName, string collectionId)
        //{
        //    var streamProcessorSetting = new StreamProcessorSettings
        //    {
        //        FaceSearch = { CollectionId = collectionId, FaceMatchThreshold = matchThreshold }
        //    };

        //    var createStreamProcessorResult =
        //     await _rekognitionClient.CreateStreamProcessorAsync(new CreateStreamProcessorRequest
        //     {
        //         Input = { KinesisVideoStream = { Arn = kvStreamArn } },
        //         Output = { KinesisDataStream = { Arn = kdStreamArn } },
        //         Settings = { FaceSearch = { CollectionId = collectionId, FaceMatchThreshold = matchThreshold } },
        //         RoleArn = iamRoleArn,
        //         Name = streamProcessorName
        //     });
        //    return createStreamProcessorResult;
        //}

        //private async Task<StartStreamProcessorResponse> StartStreamProcessor(string streamProcessorName)
        //{
        //    var result = await _rekognitionClient.StartStreamProcessorAsync(new StartStreamProcessorRequest
        //    {
        //        Name = streamProcessorName
        //    });

        //    return result;
        //}

        //private async Task<StopStreamProcessorResponse> StopStreamProcessor(string name)
        //{
        //    var result = await _rekognitionClient.StopStreamProcessorAsync(new StopStreamProcessorRequest
        //    {
        //        Name = name
        //    });
        //    return result;
        //}

        //private async Task<DeleteStreamProcessorResponse> DeleteStreamProcessor(string name)
        //{
        //    var result = await _rekognitionClient.DeleteStreamProcessorAsync(new DeleteStreamProcessorRequest
        //    {
        //        Name = name
        //    });
        //    return result;
        //}
    }
}
