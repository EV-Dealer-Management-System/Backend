using SWP391Web.Domain.Enums;
using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Domain.Entities
{
    public class SignatureRequest
    {
        public Guid Id { get; set; }
        public SignerRole Role { get; set; }
        public Party SignerParty { get; set; } = null!;
        public int Order { get; set; }
        public SignatureMethod Method { get; set; }
        public string? OneTimeToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public bool IsSigned => SignedAtUtc.HasValue;
        public DateTime? SignedAtUtc { get; set; }
        public string? CertificatePem { get; set; }
        public string? CertSerial { get; set; }
        public string? SignerIp { get; set; }

        public SignatureRequest(SignerRole role, Party signer, int order, SignatureMethod method)
        {
            Role = role;
            SignerParty = signer;
            Order = order;
            Method = method;
        }

        public void SetOneTimeToken(string token, DateTime expiry)
        {
            OneTimeToken = token;
            TokenExpiry = expiry;
        }

        public void RecordSigned(string certificatePem, string certSerial, string signerIp, DateTime nowUtc)
        {
            SignedAtUtc = nowUtc;
            CertificatePem = certificatePem;
            CertSerial = certSerial;
            SignerIp = signerIp;
        }
    }

}
