{
  description = "Fetchpet web UI";

  inputs = {
    nixpkgs = {
      url = "github:nixos/nixpkgs/nixos-unstable";
    };
    flake-utils = {
      url = "github:numtide/flake-utils";
    };
  };
  outputs =
    { nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };

        dotnetPkg = pkgs.dotnetCorePackages.sdk_9_0;
      in
      {
        devShell = pkgs.mkShell {
          buildInputs = with pkgs; [
            zlib

            zlib.dev
            openssl
            dotnetPkg

            (pkgs.writeShellScriptBin "compileSass" ("sass sass/app.scss wwwroot/app.css"))
            (pkgs.writeShellScriptBin "watchSass" ("sass --watch sass/app.scss:wwwroot/app.css "))

            (pkgs.writeShellScriptBin "publishAndRun" (
              # bash
              ''
                dotnet publish -c Release
                export $(grep -v '^#' .env | xargs)
                cp .env bin/Release/net9.0/publish/
                cd bin/Release/net9.0/publish/
                dotnet Ordis.dll
              ''))

            (pkgs.writeShellScriptBin "run" (
              # bash
              ''
                export $(grep -v '^#' .env | xargs)

                sass --watch sass/app.scss:wwwroot/app.css &
                p1=$!

                cleanup() {
                    kid=$(pgrep -P "$p1")
                    kill "$kid" "$p1" 2>/dev/null
                }
                trap cleanup EXIT
                trap cleanup INT

                dotnet watch run
              ''))

            (pkgs.writeShellScriptBin "updateDatabase" (
              # bash
              ''
                export $(grep -v '^#' .env | xargs)
                dotnet tool restore
                dotnet ef database update
              ''))

            (pkgs.writeShellScriptBin "initDatabase" (
              # bash
              ''
                #!/usr/bin/env bash

                if ! command -v psql >/dev/null 2>&1; then
                  echo "psql no here. Need PostgreSQL."
                  exit 1
                fi

                read -r -s -p "Enter password for ow3n: " PW
                echo

                sudo -u postgres psql -v pw="$PW" -f initDb.sql

                echo "connectionstrings__CharacterDb=\"host=localhost;username=ow3n;password=$PW;database=ow3n\"" >> .env

                updateDatabase

              ''))

            netcoredbg
            bruno
            omnisharp-roslyn
            dart-sass

            nodejs # Begrudgingly I need npm

            sqlite
          ];

          shellHook = ''
            DOTNET_ROOT="${dotnetPkg}"
            DOTNET_SYSTEM_GLOBALIZATION_INVARIANT="1"

            DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=1
          '';

        };
      }
    );
}
