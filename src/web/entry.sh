#!/bin/sh
sed -i 's|window.ENV = {}|window.ENV = { BFF_BASE_URI: "'$BFF_BASE_URI'" }|g' ./index.html
cat ./index.html
nginx -g 'daemon off;'