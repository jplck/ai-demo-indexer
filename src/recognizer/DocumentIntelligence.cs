
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace Company.Function {
    public class DocumentIntelligence : IDocumentRecognizer
    {
        private DocumentAnalysisClient _diClient;

        public DocumentIntelligence(IConfiguration configuration) {
            var diEndpoint = configuration["DI_ENDPOINT"];

            if (string.IsNullOrEmpty(diEndpoint)) {
                throw new ArgumentNullException("Document Analysis endpoint must be provided.");
            }

            _diClient = new DocumentAnalysisClient(
                new Uri(diEndpoint),
                new DefaultAzureCredential()
            );
        }

        public async Task<string> RecognizeAsync(string documentUri)
        {

            if (string.IsNullOrEmpty(documentUri)) {
                throw new ArgumentNullException("Document URI must be provided.");
            }

            var blobUri = new Uri(documentUri);
            var blobClient = new BlobClient(blobUri, new DefaultAzureCredential());
                
            AnalyzeDocumentOperation operation = await _diClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", blobClient.Uri);

            if (operation.HasValue)
            {
                AnalyzeResult result = operation.Value;
                return result.Content.ToString();
            }

            return string.Empty;
        }
    }
}