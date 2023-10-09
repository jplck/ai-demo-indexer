using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace Company.Function {
    public class DocumentRef {
        public List<string> Chunks { get; set;}

        public string DocumentUri { get; set;}

        public string DocumentId { get;}

        public DocumentRef(List<string> chunks, string documentUri) {

            var hash = MD5.Create(); //Find a better way. SHA does not work as it is longer than 16bit.
            var docGuid = new Guid(hash.ComputeHash(Encoding.ASCII.GetBytes(documentUri)));
            Chunks = chunks;
            DocumentUri = documentUri;
            DocumentId = docGuid.ToString();
        }
    }
}