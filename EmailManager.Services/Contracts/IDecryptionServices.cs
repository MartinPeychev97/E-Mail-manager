using System.Collections.Generic;
using System.Threading.Tasks;
using EmailManager.Data.Implementation;

namespace EmailManager.Services.Contracts
{
    public interface IDecryptionServices
    {
        string Decrypt(string cipherText);
        IEnumerable<Client> DecryptClientList(IEnumerable<Client> client);
        string Base64Decrypt(string base64EncodedData);
        Task<Client> DecryptClientInfo(Client clientId);
    }
}