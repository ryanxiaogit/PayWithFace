using Amazon.Rekognition.Model;
using AWSRekonition.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AWSRekonition
{
    public interface IAWSRekonitionFunction
    {
        Task<DeleteCollectionResponse> DeleteCollection(string cardholderName);
        Task<IndexFacesResponse> AddFacesToCollection(string collectionId, MemoryStream imageStream);
        Task<SearchFacesByImageResponse> SearchFaceByImage(MemoryStream image, string collectionId);
        Task<CreateCollectionResponse> CreateFacesCollection(string collectionID);
    }
}
