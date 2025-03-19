#!/bin/sh
# if [ -d "bin" ]; then
#   dotnet build
# fi
# dotnet run --no-build

rm -rf bin obj
dotnet build
dotnet run --no-build
