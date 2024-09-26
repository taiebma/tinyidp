using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace tinyidp.infrastructure.bdd;

public partial class TinyidpContext : DbContext
{
    private readonly IConfiguration _config;

    public TinyidpContext(DbContextOptions<TinyidpContext> options, IConfiguration config)
        : base(options)
    {
        _config = config;
    }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Credential> Credentials { get; set; }

    public virtual DbSet<Kid> Kids { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<ThrustStore> ThrustStore { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            BddConfig? conf = _config?.GetSection("TINYIDP_BDDCONFIG").Get<BddConfig>();

            if (conf == null) 
                throw new Exception("No BDD configuration found");

            string connectString = string.Format("Host={0};Database={1};Username={2};Password={3}",
                conf.ServerName,
                conf.BddName,
                conf.UserName,
                conf.Password
                );
            optionsBuilder.UseNpgsql(connectString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ThrustStore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("thrust_store_pk");

            entity.ToTable("thrust_store");

            entity.HasIndex(e => new {e.Dn, e.Issuer, e.ValidityDate}, "thrust_store_idx").IsUnique();

            entity.Property(e => e.Id)
//                .ValueGeneratedNever()
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
            entity.Property(e => e.ValidityDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("validity_date");
            entity.Property(e => e.Dn)
                .HasColumnType("character varying")
                .HasColumnName("dn");
            entity.Property(e => e.Issuer)
                .HasColumnType("character varying")
                .HasColumnName("issuer");
            entity.Property(e => e.Certificate)
                .HasColumnType("text")
                .HasColumnName("certificate");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("certificates_pk");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.Dn, "certificates_ident_idx").IsUnique();

            entity.Property(e => e.Id)
//                .ValueGeneratedNever()
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
                //.HasDefaultValueSql("nextval('\"id_certificates\"')");
            entity.Property(e => e.ValidityDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("validity_date");
            entity.Property(e => e.Dn)
                .HasColumnType("character varying")
                .HasColumnName("dn");
            entity.Property(e => e.Issuer)
                .HasColumnType("character varying")
                .HasColumnName("issuer");
            entity.Property(e => e.LastIdent)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_ident");
            entity.Property(e => e.Serial)
                .HasColumnType("character varying")
                .HasColumnName("serial");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.IdClient).HasColumnName("id_client");
        });

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("credentials_pk");

            entity.ToTable("credentials");

            entity.HasIndex(e => e.Ident, "credentials_ident_idx").IsUnique();

            entity.Property(e => e.Id)
                //.ValueGeneratedNever()
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
                //.HasDefaultValueSql("nextval('\"id_credentials\"')");
            entity.Property(e => e.CreationDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_date");
            entity.Property(e => e.Ident)
                .HasColumnType("character varying")
                .HasColumnName("ident");
            entity.Property(e => e.LastIdent)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_ident");
            entity.Property(e => e.NbMaxRenew).HasColumnName("nbmaxrenew");
            entity.Property(e => e.Pass)
                .HasColumnType("character varying")
                .HasColumnName("pass");
            entity.Property(e => e.RefreshMaxMinuteValidity).HasColumnName("refreshmaxminutevalidity");
            entity.Property(e => e.RoleIdent).HasColumnName("role_ident");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.MustChangePwd).HasColumnName("must_change_pwd");
            entity.Property(e => e.TokenMaxMinuteValidity).HasColumnName("tokenmaxminutevalidity");
            entity.Property(e => e.Audiences)
                .HasColumnType("character varying")
                .HasColumnName("audiences");
            entity.Property(e => e.AllowedScopes)
                .HasColumnType("character varying")
                .HasColumnName("allowed_scopes");
            entity.Property(e => e.AuthorizationCode)
                .HasColumnType("character varying")
                .HasColumnName("authorization_code");
            entity.Property(e => e.RedirectUri)
                .HasColumnType("character varying")
                .HasColumnName("redirect_uri");
            entity.Property(e => e.CodeChallenge)
                .HasColumnType("character varying")
                .HasColumnName("code_challenge");
            entity.Property(e => e.CodeChallengeMethod)
                .HasColumnType("character varying")
                .HasColumnName("code_challenge_method");
            entity.Property(e => e.RefreshToken)
                .HasColumnType("character varying")
                .HasColumnName("refresh_token");
            entity.Property(e => e.CreationDateRefreshToken)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_date_rtoken");
            entity.Property(e => e.KeyType).HasColumnName("key_type");
            entity.HasMany(e => e.Certificates)
                .WithOne(e => e.ClientCredential)
                .HasForeignKey(e => e.IdClient)
                .HasPrincipalKey(e => e.Id);
        });

        modelBuilder.Entity<Kid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kids_pk");

            entity.ToTable("kids");

            entity.HasIndex(e => e.Kid1, "kids_idx").IsUnique();

            entity.Property(e => e.Id)
                //.ValueGeneratedNever()
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
                //.HasDefaultValueSql("nextval('\"id_kids\"')");
            entity.Property(e => e.Algo)
                .HasColumnType("character varying")
                .HasColumnName("algo");
            entity.Property(e => e.Kid1)
                .HasColumnType("character varying")
                .HasColumnName("kid");
            entity.Property(e => e.PrivateKey).HasColumnName("privatekey");
            entity.Property(e => e.PublicKey).HasColumnName("publickey");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.CreationDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creation_date");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tokens_pk");

            entity.ToTable("tokens");

            entity.HasIndex(e => e.RefreshToken, "tokens_idx").IsUnique();

            entity.Property(e => e.Id)
                //.ValueGeneratedNever()
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
                //.HasDefaultValueSql("nextval('\"id_tokens\"')");
            entity.Property(e => e.IdCred).HasColumnName("idcred");
            entity.Property(e => e.LastRenew)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastrenew");
            entity.Property(e => e.NbRenew).HasColumnName("nbnenew");
            entity.Property(e => e.RefreshToken).HasColumnName("refreshtoken");
            entity.Property(e => e.Type).HasColumnName("type");
        });
        modelBuilder.HasSequence("id_certificates");
        modelBuilder.HasSequence("id_credentials");
        modelBuilder.HasSequence("id_kids");
        modelBuilder.HasSequence("id_tokens");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
