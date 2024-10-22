CREATE TABLE public.credentials (
	id bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
	ident varchar NOT NULL,
	pass varchar NOT NULL,
	state int NOT NULL,
	creation_date timestamp NOT NULL,
	last_ident timestamp NULL,
	role_ident int NOT NULL,
	nbmaxrenew int NOT NULL,
	tokenmaxminutevalidity int8 NOT NULL,
	refreshmaxminutevalidity int8 NOT NULL,
	must_change_pwd bool DEFAULT false NOT NULL,
	audiences varchar,
	allowed_scopes varchar,
	authorization_code varchar,
	redirect_uri varchar,
	code_challenge varchar,
	code_challenge_method varchar,
	refresh_token varchar,
	creation_date_rtoken timestamp NULL,
	key_type int4 DEFAULT 1 NOT NULL,
	nonce varchar,
	scoped varchar,
	CONSTRAINT credentials_pk PRIMARY KEY (id)
);
CREATE UNIQUE INDEX credentials_ident_idx ON public.credentials USING btree (ident);

CREATE TABLE public.certificates (
	id int8 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 9223372036854775807 START 1 CACHE 1 NO CYCLE) NOT NULL,
	dn varchar NOT NULL,
	issuer varchar NOT NULL,
	serial varchar NOT NULL,
	state int4 NOT NULL,
	validity_date timestamp NOT NULL,
	last_ident timestamp NULL,
	id_client int8 NOT NULL,
	CONSTRAINT certificates_pk PRIMARY KEY (id)
);
CREATE UNIQUE INDEX certificates_ident_idx ON public.certificates USING btree (dn);


-- public.certificates foreign keys

ALTER TABLE public.certificates ADD CONSTRAINT certificates_credentials_fk FOREIGN KEY (id_client) REFERENCES public.credentials(id);

CREATE TABLE kids (
    id bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    kid varchar NOT NULL,
    algo varchar NOT NULL,
    state int NOT NULL,
    publickey text NOT NULL,
    privatekey text NOT NULL,
	creation_date timestamp NOT NULL,
	CONSTRAINT kids_pk PRIMARY KEY (id)
);
CREATE UNIQUE INDEX kids_idx ON kids USING btree (kid);

CREATE TABLE tokens (
    id bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    refreshtoken text NOT NULL,
    type int NOT NULL,
    idcred int NOT NULL,
    nbrenew int NOT NULL,
    lastrenew timestamp,
	CONSTRAINT tokens_pk PRIMARY KEY (id)
);
CREATE UNIQUE INDEX tokens_idx ON tokens USING btree (refreshtoken);

CREATE TABLE public.thrust_store (
	id int8 GENERATED ALWAYS AS IDENTITY NOT NULL,
	dn varchar NOT NULL,
	issuer varchar NOT NULL,
	validity_date timestamp NOT NULL,
	certificate text NOT NULL,
	CONSTRAINT thrust_store_pk PRIMARY KEY (id)
);
CREATE UNIQUE INDEX thrust_store_id_idx ON public.thrust_store (id);
CREATE INDEX thrust_store_dn_idx ON public.thrust_store (dn,issuer,validity_date);
