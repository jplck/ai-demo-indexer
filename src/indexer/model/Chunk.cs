using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Company.Function {
    public class Chunk {
        public string Content { get; set;}

        public Collection<float>? Embeddings { get; set;}

        public string DocumentUri { get; set;}

        public string DocumentId { get;}

        public Chunk(string content, string documentUri) {
            Content = content;
            DocumentUri = documentUri;
            var hash = MD5.Create(); //Find a better way. SHA does not work as it is longer than 16bit.
            DocumentId = new Guid(hash.ComputeHash(Encoding.ASCII.GetBytes(documentUri))).ToString();
        }

        public Chunk(string content) {
            Content = content;
            DocumentUri = string.Empty;
            DocumentId = string.Empty;
        }
    }
}