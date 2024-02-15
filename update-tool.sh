#!/bin/sh

# Fail on error
set -e

YELLOW="\033[1;33m"
CLEAR="\033[0m"

printf "${YELLOW}Packing tool${CLEAR}\n"

dotnet pack

printf "${YELLOW}Updating/installing tool${CLEAR}\n"

dotnet tool update noa -g --add-source package/

exit 0
