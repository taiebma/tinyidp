using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tinyidp.infrastructure.bdd;

public partial class Credential: ICachable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Ident { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public int State { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? LastIdent { get; set; }

    public int RoleIdent { get; set; }

    public int NbMaxRenew { get; set; }

    public long TokenMaxMinuteValidity { get; set; }

    public long RefreshMaxMinuteValidity { get; set; }

    public bool MustChangePwd { get; set; }

    public string? Audiences { get; set; } = null!;

    public string? AllowedScopes { get; set; } = null!;

    public string? AuthorizationCode { get; set; } = null!;

    public string? RedirectUri { get; set; } = null!;

    public string? CodeChallenge { get; set; } = null!;

    public string? CodeChallengeMethod { get; set; } = null!;

    public string? RefreshToken { get; set; } = null!;

    public DateTime? CreationDateRefreshToken { get; set; }

    public int KeyType { get; set; }

    public ICollection<Certificate> Certificates{ get; set; } = null!;
    public string? Nonce { get; set; } = null!;
    public string? Scoped { get; set; } = null!;

    public DateTime? Expiration 
    { 
        get
        {
            if (CreationDateRefreshToken.HasValue)
            {
                return CreationDateRefreshToken.Value.AddMinutes(RefreshMaxMinuteValidity);
            }
            return CreationDate.AddMinutes(TokenMaxMinuteValidity);
        } 
    }

    public Credential Clone()
    {
        return new Credential
        {
            Id = this.Id,
            Ident = this.Ident,
            Pass = this.Pass,
            State = this.State,
            CreationDate = this.CreationDate,
            LastIdent = this.LastIdent,
            RoleIdent = this.RoleIdent,
            NbMaxRenew = this.NbMaxRenew,
            TokenMaxMinuteValidity = this.TokenMaxMinuteValidity,
            RefreshMaxMinuteValidity = this.RefreshMaxMinuteValidity,
            MustChangePwd = this.MustChangePwd,
            Audiences = this.Audiences,
            AllowedScopes = this.AllowedScopes,
            AuthorizationCode = this.AuthorizationCode,
            RedirectUri = this.RedirectUri,
            CodeChallenge = this.CodeChallenge,
            CodeChallengeMethod = this.CodeChallengeMethod,
            RefreshToken = this.RefreshToken,
            CreationDateRefreshToken = this.CreationDateRefreshToken,
            KeyType = this.KeyType,
            Nonce = this.Nonce,
            Scoped = this.Scoped
        };
    }
}
