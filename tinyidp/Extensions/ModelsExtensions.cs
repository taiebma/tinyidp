
using System.ComponentModel;
using tinyidp.Business.BusinessEntities;
using Microsoft.AspNetCore.Mvc.Rendering;
using tinyidp.Pages.Models;
using tinyidp.Controllers.Models;

namespace tinyidp.Extensions;

public static class ModelsExtensions
{
    public static CredentialBusinessEntity ToBusiness(this CredentialCreateModel entity)
    {
        return new CredentialBusinessEntity() {
            Ident = entity.Ident, 
            Pass = entity.Pass,
            PassNew = entity.Pass, 
            State = entity.State, 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = entity.RoleIdent, 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity,
            AllowedScopes = entity.AllowedScopes?.Split(' ')??new string[0],
            Audiences = entity.Audiences?.Split(' ')??new string[0],
            AuthorizationCode = entity.AuthorizationCode,
            RedirectUri = entity.RedirectUri, 
            KeyType = entity.KeyType??AlgoKeyType.None
        };
    }

    public static CredentialBusinessEntity ToBusiness(this CredentialEditModel entity)
    {
        return new CredentialBusinessEntity() {
            Id = entity.Id,
            Ident = entity.Ident, 
            Pass = entity.Pass,
            PassNew  = entity.PassNew,
            State = entity.State, 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = entity.RoleIdent, 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity, 
            CreationDate = entity.CreationDate,
            LastIdent = entity.LastIdent,
            AllowedScopes = entity.AllowedScopes?.Split(' ')??new string[0],
            Audiences = entity.Audiences?.Split(' ')??new string[0],
            AuthorizationCode = entity.AuthorizationCode,
            RedirectUri = entity.RedirectUri, 
            KeyType = entity.KeyType??AlgoKeyType.None
        };
    }

    public static CredentialCreateModel ToModelCreate(this CredentialBusinessEntity entity)
    {
        return new CredentialCreateModel() {
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            State = (StateCredential)Enum.Parse(typeof(StateCredential),entity.State.ToString()), 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = (RoleCredential)Enum.Parse(typeof(RoleCredential),entity.RoleIdent.ToString()), 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity,
            AllowedScopes = string.Join(' ', entity.AllowedScopes??new List<string>()),
            Audiences = string.Join(' ', entity.Audiences??new List<string>()),
            AuthorizationCode = entity.AuthorizationCode,
            RedirectUri = entity.RedirectUri, 
            KeyType = entity.KeyType
        };
    }

    public static CredentialEditModel ToModelEdit(this CredentialBusinessEntity entity)
    {
        return new CredentialEditModel() {
            Id = entity.Id,
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            PassNew = entity.Pass, 
            State = (StateCredential)Enum.Parse(typeof(StateCredential),entity.State.ToString()), 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = (RoleCredential)Enum.Parse(typeof(RoleCredential),entity.RoleIdent.ToString()), 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity,
            CreationDate = entity.CreationDate,
            LastIdent = entity.LastIdent,
            AllowedScopes = string.Join(' ', entity.AllowedScopes??new List<string>()),
            Audiences = string.Join(' ', entity.Audiences??new List<string>()),
            AuthorizationCode = entity.AuthorizationCode,
            RedirectUri = entity.RedirectUri, 
            KeyType = entity.KeyType,
            Certificates = entity.CertificateBusinessEntities?.Select(x => x.ToModelEdit()).ToList()??new List<CertificateEditModel>()
        };
    }

    public static ChangePwdModel ToModelChPwd(this CredentialBusinessEntity entity)
    {
        return new ChangePwdModel() {
            Id = entity.Id,
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            PassNew = entity.Pass
        };
    }
    public static CredentialBusinessEntity ToBusiness(this ChangePwdModel entity)
    {
        return new CredentialBusinessEntity() {
            Id = entity.Id,
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            PassNew = entity.Pass
        };
    }

    public static CredentialView ToModelView(this CredentialBusinessEntity entity)
    {
        return new CredentialView() {
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            State = Enum.Parse<StateCredential>(entity.State.ToString()), 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = Enum.Parse<RoleCredential>(entity.RoleIdent.ToString()), 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity, 
            Id = entity.Id, 
            CreationDate = entity.CreationDate, 
            LastIdent = entity.LastIdent, 
            KeyType = entity.KeyType
        };
    }

    public static string GetEnumDescription(this Enum en)
    {
        if (en == null) return String.Empty;

        var type = en.GetType();

        var memberInfo = type.GetMember(en.ToString());
        var description = (memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
            false).FirstOrDefault() as DescriptionAttribute)?.Description??String.Empty;

        return description;
    }

    public static KidView ToModelView(this KidBusinessEntity entity)
    {
        return new KidView() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid = entity.Kid1, 
             State = entity.State,
             Valid = entity.Valid,
             KeyError = entity.KeyError
        };
    }

    public static KidEditModel ToModelEdit(this KidBusinessEntity entity)
    {
        return new KidEditModel() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid = entity.Kid1, 
             State = entity.State
        };
    }

    public static KidBusinessEntity ToBusiness(this KidEditModel entity)
    {
        return new KidBusinessEntity() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid1 = entity.Kid, 
             State = entity.State
        };
    }

    public static KidDeleteModel ToModelDelete(this KidBusinessEntity entity)
    {
        return new KidDeleteModel() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid = entity.Kid1, 
             State = entity.State
        };
    }

    public static KidBusinessEntity ToBusiness(this KidDeleteModel entity)
    {
        return new KidBusinessEntity() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid1 = entity.Kid, 
             State = entity.State
        };
    }

    public static TokenResponse ToModel(this TokenResponseBusiness token)
    {
        return new TokenResponse() 
        {
            access_token = token.access_token, 
            id_token = token.id_token,
            code = token.code, 
            refresh_token = token.refresh_token,
            token_type = token.token_type
        };
    }

    public static TokenRequestBusiness ToBusiness(this TokenRequest tokenRequest)
    {
        return new TokenRequestBusiness() 
        {
            client_id = tokenRequest.client_id, 
            client_secret = tokenRequest.client_secret, 
            code = tokenRequest.code, 
            code_verifier = tokenRequest.code_verifier, 
            device_code = tokenRequest.device_code, 
            grant_type = tokenRequest.grant_type, 
            redirect_uri = tokenRequest.redirect_uri, 
            scope = tokenRequest.scope,
            refresh_token = tokenRequest.refresh_token
        };
    }

    public static CertificateBusinessEntity ToBusiness(this CertificateCreateModel certificate)
    {
        return new CertificateBusinessEntity() {
            ValidityDate = certificate.ValidityDate, 
            Dn = certificate.Dn, 
            Issuer = certificate.Issuer, 
            Serial = certificate.Serial, 
            State = (int)certificate.State, 
            IdClient = certificate.IdClient
        };
    }

    public static CertificateBusinessEntity ToBusiness(this CertificateEditModel certificate)
    {
        return new CertificateBusinessEntity() {
            Id = certificate.Id, 
            ValidityDate = certificate.ValidityDate, 
            LastIdent = certificate.LastIdent, 
            Dn = certificate.Dn, 
            Issuer = certificate.Issuer, 
            Serial = certificate.Serial, 
            State = (int)certificate.State, 
            IdClient = certificate.IdClient
        };
    }

    public static CertificateCreateModel ToModelCreate(this CertificateBusinessEntity certificate)
    {
        return new CertificateCreateModel() {
            ValidityDate = certificate.ValidityDate, 
            Dn = certificate.Dn, 
            Issuer = certificate.Issuer, 
            Serial = certificate.Serial, 
            State = Enum.Parse<StateCredential>(certificate.State.ToString()), 
            IdClient = certificate.IdClient
        };
    }

    public static CertificateEditModel ToModelEdit(this CertificateBusinessEntity certificate)
    {
        return new CertificateEditModel() {
            Id = certificate.Id, 
            ValidityDate = certificate.ValidityDate, 
            LastIdent = certificate.LastIdent, 
            Dn = certificate.Dn, 
            Issuer = certificate.Issuer, 
            Serial = certificate.Serial, 
            State = Enum.Parse<StateCredential>(certificate.State.ToString()), 
            IdClient = certificate.IdClient
        };
    }

    public static ThrustStoreViewModel ToModelView( this ThrustStoreBusiness thrustStore)
    {
        return new ThrustStoreViewModel() {
            Id = thrustStore.Id, 
            Dn = thrustStore.Dn, 
            Issuer = thrustStore.Issuer, 
            ValidityDate = thrustStore.ValidityDate
        };
    }
    public static ThrustStoreEditModel ToModelEdit( this ThrustStoreBusiness thrustStore)
    {
        return new ThrustStoreEditModel() {
            Id = thrustStore.Id, 
            Dn = thrustStore.Dn, 
            Issuer = thrustStore.Issuer, 
            ValidityDate = thrustStore.ValidityDate
        };
    }
    public static ThrustStoreBusiness ToBusiness( this ThrustStoreEditModel thrustStore)
    {
        return new ThrustStoreBusiness() {
            Id = thrustStore.Id, 
            Dn = thrustStore.Dn, 
            Issuer = thrustStore.Issuer, 
            ValidityDate = thrustStore.ValidityDate
        };
    }
}