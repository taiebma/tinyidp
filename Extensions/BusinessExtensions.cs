
using tinyidp.Business.BusinessEntities;
using tinyidp.Encryption;
using tinyidp.Exceptions;
using tinyidp.infrastructure.bdd;

namespace tinyidp.Extensions;

public static class BusinessExtensions
{
    public static Credential ToEntity(this CredentialBusinessEntity entity)
    {
        return new Credential() {
            Id = entity.Id,
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            State = (int)entity.State, 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = (int)entity.RoleIdent, 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity, 
            CreationDate = entity.CreationDate, 
            LastIdent = entity.LastIdent,
            MustChangePwd = entity.MustChangePwd,
            AllowedScopes = string.Join(' ', entity.AllowedScopes??new List<string>()),
            Audiences = string.Join(' ', entity.Audiences??new List<string>()),
            AuthorizationCode = entity.AuthorizationCode,
            RedirectUri = entity.RedirectUri,
            CodeChallenge = entity.CodeChallenge,
            CodeChallengeMethod = entity.CodeChallengeMethod,
            RefreshToken = entity.RefreshToken,
            CreationDateRefreshToken = entity.CreationDateRefreshToken
        };
    }

    public static CredentialBusinessEntity ToBusiness(this Credential entity)
    {
        return new CredentialBusinessEntity() {
            Id = entity.Id,
            Ident = entity.Ident, 
            Pass = entity.Pass, 
            PassNew = entity.Pass, 
            State = (StateCredential)Enum.Parse(typeof(StateCredential), entity.State.ToString()), 
            NbMaxRenew = entity.NbMaxRenew, 
            RefreshMaxMinuteValidity = entity.RefreshMaxMinuteValidity, 
            RoleIdent = (RoleCredential)Enum.Parse(typeof(RoleCredential), entity.RoleIdent.ToString()), 
            TokenMaxMinuteValidity = entity.TokenMaxMinuteValidity, 
            CreationDate = entity.CreationDate, 
            LastIdent = entity.LastIdent,
            MustChangePwd = entity.MustChangePwd,
            AllowedScopes = entity.AllowedScopes?.Split(' ')??new string[0],
            Audiences = entity.Audiences?.Split(' ')??new string[0],
            AuthorizationCode = entity.AuthorizationCode,
            RedirectUri = entity.RedirectUri,
            CodeChallenge = entity.CodeChallenge,
            CodeChallengeMethod = entity.CodeChallengeMethod,
            RefreshToken = entity.RefreshToken,
            CreationDateRefreshToken = entity.CreationDateRefreshToken
        };
    }

    public static KidBusinessEntity ToBusiness( this Kid kid, IEncryptionService encryptionService)
    {

        KidBusinessEntity newKid;
        newKid= new KidBusinessEntity();
        newKid.Algo = (AlgoType)Enum.Parse(typeof(AlgoType), kid.Algo);
        newKid.Id = kid.Id;
        newKid.Kid1 = kid.Kid1; 
        newKid.Valid = true;
        try
        {
            newKid.PrivateKey = encryptionService.Decrypt( kid.PrivateKey);
            if (string.IsNullOrEmpty(newKid.PrivateKey))
            {
                newKid.Valid = false;
                newKid.KeyError = "Bad empty key";
            }
        }
        catch (TinyidpKeyException ex)
        {
            newKid.Valid = false;
            newKid.KeyError = ex.Message;
        }
        newKid.PublicKey = kid.PublicKey;
        newKid.State = (KidState)Enum.Parse(typeof(KidState), kid.State.ToString());
        newKid.CreationDate = kid.CreationDate;

        return newKid;
    }

    public static Kid ToEntity( this KidBusinessEntity kid, IEncryptionService encryptionService)
    {
        return new Kid() {
             Algo = kid.Algo.ToString(), 
             Id = kid.Id, 
             Kid1 = kid.Kid1, 
             PrivateKey = encryptionService.Encrypt(kid.PrivateKey), 
             PublicKey = kid.PublicKey, 
             State = (int)kid.State,
             CreationDate = kid.CreationDate
        };
    }

    public static AuthorizationRequest ToBusiness( this tinyidp.Controllers.Models.AuthorizationRequest authorizationRequest)
    {
        return new AuthorizationRequest() {
            client_id = authorizationRequest.client_id, 
            code_challenge = authorizationRequest.code_challenge, 
            code_challenge_method = authorizationRequest.code_challenge_method, 
            nonce = authorizationRequest.nonce, 
            redirect_uri = authorizationRequest.redirect_uri, 
            response_type = authorizationRequest.response_type, 
            scope = authorizationRequest.scope, 
            state = authorizationRequest.state
            
        };
    }
}