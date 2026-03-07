#!/bin/sh
set -e
JWT_DIR="/var/www/html/config/jwt"
if [ ! -f "${JWT_DIR}/private.pem" ]; then
  mkdir -p "${JWT_DIR}"
  if [ -n "${JWT_PASSPHRASE}" ]; then
    openssl genpkey -out "${JWT_DIR}/private.pem" -aes256 -algorithm rsa -pkeyopt rsa_keygen_bits:4096 -pass pass:"${JWT_PASSPHRASE}"
    openssl pkey -in "${JWT_DIR}/private.pem" -out "${JWT_DIR}/public.pem" -pubout -passin pass:"${JWT_PASSPHRASE}"
  else
    openssl genpkey -out "${JWT_DIR}/private.pem" -algorithm rsa -pkeyopt rsa_keygen_bits:4096
    openssl pkey -in "${JWT_DIR}/private.pem" -out "${JWT_DIR}/public.pem" -pubout
  fi
  chown -R www-data:www-data "${JWT_DIR}"
fi
exec apache2-foreground
