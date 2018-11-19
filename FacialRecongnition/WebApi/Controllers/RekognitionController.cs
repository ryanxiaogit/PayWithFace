using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Rekognition.Model;
using AWSRekonition;
using AWSRekonition.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public class RekognitionController : ControllerBase
    {
        private readonly IAWSRekonitionFunction _aWSRekonitionFunction = null;
        static readonly Dictionary<string, string> dicUserToCard = new Dictionary<string, string>();

        public RekognitionController(IAWSRekonitionFunction aWSRekonitionFunction)
        {
            _aWSRekonitionFunction = aWSRekonitionFunction;
        }

        [HttpPost]
        [Route("api/compareface/{cardholderName}")]
        public async Task<MessageResponse> CompareFace(string cardholderName)
        {
            var message = new MessageResponse
            {
                IsMatched = false,
                Message = "system error"
            };

            try
            {
                var ms = new MemoryStream();
                Request.Body.CopyTo(ms);
                var response = await _aWSRekonitionFunction.SearchFaceByImage(ms, cardholderName);
                message.IsMatched = response.FaceMatches.Count >= 1;
                message.Confidence = response.SearchedFaceConfidence;
                message.Message = response.FaceMatches.Count == 1 ? $"Matched:{response.SearchedFaceConfidence}%" : "NotMatched";
                if (dicUserToCard.TryGetValue(cardholderName, out var cardnumber))
                {
                    message.CardNumber = cardnumber.Substring(0, 6) + "****" + cardnumber.Substring(cardnumber.Length - 4, 4);
                }
                else
                {
                    message.CardNumber = "**************";
                }
                message.MessageCode = (int)response.HttpStatusCode;
            }
            catch (Exception ex)
            {
                message.Message = ex.Message;
            }
            return message;
        }

        [HttpGet]
        [Route("api/create/{cardholderName}")]
        public async Task<CreateFaceCollectionResponse> CreateFacesCollection(string cardholderName)
        {
            var response = new CreateFaceCollectionResponse();
            try
            {
                var result = await _aWSRekonitionFunction.CreateFacesCollection(cardholderName);
                response.MessageCode = result.StatusCode;
                response.Message = result.HttpStatusCode == System.Net.HttpStatusCode.OK ? "Face data profile has been created" : "Profile building faild";
            }
            catch (Exception ex)
            {
                response.Message = "Profile building failed";
            }
            return response;
        }

        [HttpPost]
        [Route("api/addfaces/{cardholderName}-{cardNumber}")]
        public async Task<AddFaceResponse> AddFaceToCollection(string cardholderName, string cardNumber)
        {
            var response = new AddFaceResponse();

            var ms = new MemoryStream();
            Request.Body.CopyTo(ms);
            try
            {
                var result = await _aWSRekonitionFunction.AddFacesToCollection(cardholderName, ms);
                response.MessageCode = (int)result.HttpStatusCode;
                response.IsAdded = result.HttpStatusCode == System.Net.HttpStatusCode.OK;
                response.Message = $"there are {result.FaceRecords.Count} faces data saved";
                dicUserToCard[cardholderName] = cardNumber;
            }
            catch (Exception ex)
            {


            }
            return response;
        }

        [HttpGet]
        [Route("api/delete/{cardholderName}")]
        public async Task<DeleteFaceCollectionResponse> DeleteFaceCollection(string cardholderName)
        {
            var result = await _aWSRekonitionFunction.DeleteCollection(cardholderName);
            var response = new DeleteFaceCollectionResponse
            {
                Message = result.HttpStatusCode == System.Net.HttpStatusCode.OK ? "Profile has been deleted" : "Error",
                MessageCode = result.StatusCode
            };

            return response;
        }
    }
}