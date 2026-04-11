{
  description = "OW3N";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    {
      self,
      nixpkgs,
      flake-utils,
      ...
    }:
    let
      # NixOS module is system-independent
      nixosModule =
        {
          config,
          lib,
          pkgs,
          ...
        }:
        let
          cfg = config.services.ordis;
        in
        {
          options.services.ordis = {
            enable = lib.mkEnableOption "Ordis Blazor server app";

            package = lib.mkOption {
              type = lib.types.package;
              default = self.packages.${pkgs.system}.default;
              defaultText = lib.literalExpression "self.packages.\${pkgs.system}.default";
              description = "The Ordis package to run.";
            };

            port = lib.mkOption {
              type = lib.types.port;
              default = 5000;
              description = "HTTP port for Kestrel.";
            };

            httpsPort = lib.mkOption {
              type = lib.types.nullOr lib.types.port;
              default = null;
              description = "HTTPS port for Kestrel. If null, HTTPS is disabled.";
            };

            httpsCertPath = lib.mkOption {
              type = lib.types.str;
              default = "/etc/ssl/tailscale-certs/cert.pfx";
              description = "Path to the PFX certificate for HTTPS.";
            };

            httpsCertPassword = lib.mkOption {
              type = lib.types.str;
              default = "";
              description = "Password for the PFX certificate.";
            };

            environmentFile = lib.mkOption {
              type = lib.types.nullOr lib.types.path;
              default = null;
              description = ''
                Path to an EnvironmentFile loaded by systemd (for secrets like
                connection strings). Format: KEY=VALUE per line, no `export`.
              '';
            };

            environment = lib.mkOption {
              type = lib.types.attrsOf lib.types.str;
              default = { };
              description = "Extra environment variables passed to the service.";
            };

            user = lib.mkOption {
              type = lib.types.str;
              default = "ordis";
              description = "User account under which Ordis runs.";
            };

            group = lib.mkOption {
              type = lib.types.str;
              default = "ordis";
              description = "Group under which Ordis runs.";
            };

            dataDir = lib.mkOption {
              type = lib.types.path;
              default = "/var/lib/ordis";
              description = "Working directory / state directory for the service.";
            };
          };

          config = lib.mkIf cfg.enable {
            users.users.${cfg.user} = {
              isSystemUser = true;
              group = cfg.group;
              home = cfg.dataDir;
              createHome = true;
            };

            users.groups.${cfg.group} = { };

            systemd.services.ordis = {
              description = "Ordis Blazor Server";
              after = [
                "network.target"
                "postgresql.service"
              ];
              wantedBy = [ "multi-user.target" ];

              environment =
                let
                  urls =
                    let
                      http = "http://0.0.0.0:${toString cfg.port}";
                      https = lib.optionalString (cfg.httpsPort != null) ";https://0.0.0.0:${toString cfg.httpsPort}";
                    in
                    "${http}${https}";
                in
                {
                  ASPNETCORE_URLS = urls;
                  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1";
                }
                // lib.optionalAttrs (cfg.httpsPort != null) {
                  ASPNETCORE_Kestrel__Endpoints__Https__Certificate__Path = cfg.httpsCertPath;
                  ASPNETCORE_Kestrel__Endpoints__Https__Certificate__Password = cfg.httpsCertPassword;
                }
                // cfg.environment;

              serviceConfig = {
                Type = "notify";
                ExecStart = "${cfg.package}/bin/Ordis";
                WorkingDirectory = cfg.dataDir;
                User = cfg.user;
                Group = cfg.group;
                Restart = "on-failure";
                RestartSec = 5;

                # Hardening
                ProtectSystem = "strict";
                ProtectHome = true;
                PrivateTmp = true;
                NoNewPrivileges = true;
                ReadWritePaths = [ cfg.dataDir ];
              }
              // lib.optionalAttrs (cfg.environmentFile != null) {
                EnvironmentFile = cfg.environmentFile;
              };
            };
          };
        };
    in
    {
      nixosModules.default = nixosModule;
    }
    // flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };
        dotnetPkg = pkgs.dotnetCorePackages.sdk_9_0;
        dotnetRuntime = pkgs.dotnetCorePackages.aspnetcore_9_0;
      in
      {

        packages.default = pkgs.buildDotnetModule {
          pname = "ordis";
          version = "0.1.0";
          src = ./.;

          projectFile = "Ordis.csproj";
          nugetDeps = ./deps.nix;

          dotnet-sdk = dotnetPkg;
          dotnet-runtime = dotnetRuntime;

          nativeBuildInputs = with pkgs; [
            nodejs
            dart-sass
            npmHooks.npmConfigHook # wires up the vendored node_modules
          ];

          npmDeps = pkgs.fetchNpmDeps {
            src = ./.;
            hash = "sha256-li3r+X48FvufAd1gFKECHQlfMkLORhHVAJ38On01JJg=";
          };

          preBuild = ''
            export HOME=$(mktemp -d)
            npm ci --ignore-scripts || npm install --ignore-scripts
            sass --load-path=node_modules sass/app.scss wwwroot/app.css
          '';

          postFixup = ''
            substituteInPlace $out/bin/Ordis \
              --replace-fail 'exec' 'cd ${placeholder "out"}/lib/ordis && exec'
          '';

          # npmConfigHook handles node_modules, so no manual npm install needed.
          # sass + MSBuild targets just work since node_modules is in place.

          meta = {
            description = "Ordis - Blazor Server application";
            mainProgram = "Ordis";
          };
        };

        devShells.default = pkgs.mkShell {
          ASPNETCORE_Kestrel__Endpoints__Https__Certificate__Path = "/etc/ssl/tailscale-certs/cert.pfx";
          ASPNETCORE_Kestrel__Endpoints__Https__Certificate__Password = "";

          buildInputs = with pkgs; [
            zlib
            zlib.dev
            openssl
            dotnetPkg
            (pkgs.writeShellScriptBin "compileSass" "sass sass/app.scss wwwroot/app.css")
            (pkgs.writeShellScriptBin "watchSass" "sass --watch sass/app.scss:wwwroot/app.css")
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
                if ! command -v psql >/dev/null 2>&1; then
                  echo "psql not found. Need PostgreSQL."
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
            nodejs
            sqlite
          ];

          shellHook = ''
            export DOTNET_ROOT="${dotnetPkg}"
            export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT="1"
            export DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=1
          '';
        };
      }
    );
}
