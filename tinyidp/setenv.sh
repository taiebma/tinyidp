#!/bin/sh
export ASPNETCORE_ENVIRONMENT=Development
export TINYIDP_BDDCONFIG__SERVERNAME=nas2.taieb.fr:49154
export TINYIDP_BDDCONFIG__BDDNAME=tinyidp
export TINYIDP_BDDCONFIG__USERNAME=tinyidp
export TINYIDP_SECU__PATH=/Users/taiebma/dev/tinyidp/tinyidp
export TINYIDP_IDP__BASE_URL_IDP=https://localhost:8083/oauth
export TINYIDP_IDP__TOKEN_LIFETIME=5