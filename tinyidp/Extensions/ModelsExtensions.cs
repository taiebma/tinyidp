
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

    public static CredentialBusinessEntity ToBusiness(this CredentialEditModel entity, CredentialBusinessEntity? existingEntity = null)
    {
        if (existingEntity == null)
        {
            return new CredentialBusinessEntity() {
                Id = entity.Id,
                Ident = entity.Ident, 
                Pass = entity.Pass,
                PassNew = entity.Pass, 
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
        else
        {
            existingEntity.Ident = entity.Ident;
            existingEntity.Pass = entity.Pass;
            existingEntity.PassNew = entity.Pass;
            existingEntity.State = entity.State;
            existingEntity.NbMaxRenew = entity.NbMaxRenew;
            existingEntity.RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity;
            existingEntity.RoleIdent = entity.RoleIdent;
            existingEntity.TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity;
            existingEntity.CreationDate = entity.CreationDate;
            existingEntity.LastIdent = entity.LastIdent;
            existingEntity.AllowedScopes = entity.AllowedScopes?.Split(' ')??new string[0];
            existingEntity.Audiences = entity.Audiences?.Split(' ')??new string[0];
            existingEntity.AuthorizationCode = entity.AuthorizationCode;
            existingEntity.RedirectUri = entity.RedirectUri; 
            existingEntity.KeyType = entity.KeyType??AlgoKeyType.None;
            return existingEntity;
        }

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

    public static tinyidp.Components.Pages.Account.ChangePwd.ChangePwdModel ToModelChPwd(this CredentialBusinessEntity entity)
    {
        return new tinyidp.Components.Pages.Account.ChangePwd.ChangePwdModel() {
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

        return en switch
        {
            StateCredential.Active => "Active",
            StateCredential.Inactive => "Inactive",
            RoleCredential.Admin => "Admin",
            RoleCredential.User => "User",
            _ => string.Empty
        };        
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

    public static KidBusinessEntity ToBusiness(this KidEditModel entity, KidBusinessEntity? existingEntity = null)
    {
        if (existingEntity == null)
        {
            return new KidBusinessEntity() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid1 = entity.Kid, 
             State = entity.State
            };
        }
        else        {
            existingEntity.Algo = entity.Algo;
            existingEntity.CreationDate = entity.CreationDate;
            existingEntity.Kid1 = entity.Kid;
            existingEntity.State = entity.State;
            return existingEntity;
        }
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

    public static KidBusinessEntity ToBusiness(this KidDeleteModel entity, KidBusinessEntity? existingEntity = null)
    {
        if (existingEntity == null)
        {
            return new KidBusinessEntity() {
             Algo = entity.Algo, 
             CreationDate = entity.CreationDate, 
             Id = entity.Id,
             Kid1 = entity.Kid, 
             State = entity.State
            };
        }
        else
        {
            existingEntity.Algo = entity.Algo;
            existingEntity.CreationDate = entity.CreationDate;
            existingEntity.Kid1 = entity.Kid;
            existingEntity.State = entity.State;
            return existingEntity;
        }
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

    public static CertificateBusinessEntity ToBusiness(this CertificateEditModel certificate, CertificateBusinessEntity? existingEntity = null)
    {
        if (existingEntity == null)
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
        else
        {
            existingEntity.Id = certificate.Id;
            existingEntity.ValidityDate = certificate.ValidityDate;
            existingEntity.LastIdent = certificate.LastIdent;
            existingEntity.Dn = certificate.Dn;
            existingEntity.Issuer = certificate.Issuer;
            existingEntity.Serial = certificate.Serial;
            existingEntity.State = (int)certificate.State;
            existingEntity.IdClient = certificate.IdClient;
            return existingEntity;
        }
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

    public static TrustStoreViewModel ToModelView( this TrustStoreBusiness thrustStore)
    {
        return new TrustStoreViewModel() {
            Id = thrustStore.Id, 
            Dn = thrustStore.Dn, 
            Issuer = thrustStore.Issuer, 
            ValidityDate = thrustStore.ValidityDate
        };
    }
    public static TrustStoreEditModel ToModelEdit( this TrustStoreBusiness thrustStore)
    {
        return new TrustStoreEditModel() {
            Id = thrustStore.Id, 
            Dn = thrustStore.Dn, 
            Issuer = thrustStore.Issuer, 
            ValidityDate = thrustStore.ValidityDate
        };
    }
    public static TrustStoreBusiness ToBusiness( this TrustStoreEditModel thrustStore)
    {
        return new TrustStoreBusiness() {
            Id = thrustStore.Id, 
            Dn = thrustStore.Dn, 
            Issuer = thrustStore.Issuer, 
            ValidityDate = thrustStore.ValidityDate
        };
    }
}