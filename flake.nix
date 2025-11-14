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

            (pkgs.writeShellScriptBin "compileSass" (''sass sass/app.scss wwwroot/app.css''))
            (pkgs.writeShellScriptBin "watchSass" (''sass --watch sass/app.scss:wwwroot/app.css ''))

            (pkgs.writeShellScriptBin "run" (''
              export $(grep -v '^#' .env | xargs) && (watchSass &) && dotnet watch run

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
