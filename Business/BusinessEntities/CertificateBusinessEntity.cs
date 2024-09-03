using System;
using System.Collections.Generic;

namespace tinyidp.Business.BusinessEntities;

public partial class CertificateBusinessEntity
{
    public int Id { get; set; }

    public string Dn { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Serial { get; set; } = null!;

    public int State { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public int IdClient { get; set; }

    public CredentialBusinessEntity ClientCredential { get; set; } = null!;
}
