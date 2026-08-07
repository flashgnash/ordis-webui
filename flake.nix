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
      nixosModule =
        {
          config,
          lib,
          pkgs,
          ...
        }:
        let
          cfg = config.services.ow3n;
        in
        {
          options.services.ow3n = {
            enable = lib.mkEnableOption "OW3N Blazor server app";

            package = lib.mkOption {
              type = lib.types.package;
              default = self.packages.${pkgs.system}.default;
              defaultText = lib.literalExpression "self.packages.\${pkgs.system}.default";
              description = "The OW3N package to run.";
            };

            environmentFile = lib.mkOption {
              type = lib.types.path;
              default = "${cfg.dataDir}/.env";
              defaultText = lib.literalExpression ''"''${cfg.dataDir}/.env"'';
              description = ''
                Path to an EnvironmentFile loaded by systemd (for secrets like
                Discord credentials). Format: KEY=VALUE per line, no `export`.
                The file is created as empty if it does not exist.
              '';
            };

            environment = lib.mkOption {
              type = lib.types.attrsOf lib.types.str;
              default = { };
              description = "Extra environment variables passed to the service.";
            };

            user = lib.mkOption {
              type = lib.types.str;
              default = "ow3n";
              description = "User account under which OW3N runs.";
            };

            group = lib.mkOption {
              type = lib.types.str;
              default = "ow3n";
              description = "Group under which OW3N runs.";
            };

            extraGroups = lib.mkOption {
              type = lib.types.listOf lib.types.str;
              default = [ ];
              description = "Extra groups for the OW3N service user.";
            };

            dataDir = lib.mkOption {
              type = lib.types.path;
              default = "/var/lib/ow3n";
              description = "Working directory / state directory for the service.";
            };

            database = {
              enable = lib.mkEnableOption "PostgreSQL database provisioning for OW3N";

              name = lib.mkOption {
                type = lib.types.str;
                default = "ow3n";
                description = "PostgreSQL database name.";
              };

              extraUsers = lib.mkOption {
                type = lib.types.listOf lib.types.str;
                default = [ ];
                description = "Additional system users to grant access to the OW3N database.";
              };
            };
          };

          config = lib.mkIf cfg.enable (
            lib.mkMerge [
              {
                users.users.${cfg.user} = {
                  isSystemUser = true;
                  group = cfg.group;
                  home = cfg.dataDir;
                  createHome = true;
                  extraGroups = cfg.extraGroups;
                };

                users.groups.${cfg.group} = { };

                systemd.tmpfiles.rules = [
                  "f ${cfg.environmentFile} 0600 ${cfg.user} ${cfg.group} -"
                ];

                systemd.services.ow3n = {
                  description = "OW3N Blazor Server";
                  after = [ "network.target" ];
                  wantedBy = [ "multi-user.target" ];

                  environment = {
                    ASPNETCORE_ENVIRONMENT = "Production";
                    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1";
                  }
                  // cfg.environment;

                  serviceConfig = {
                    Type = "simple";
                    ExecStart = "${cfg.package}/bin/Ordis";
                    WorkingDirectory = cfg.dataDir;
                    User = cfg.user;
                    Group = cfg.group;
                    Restart = "on-failure";
                    RestartSec = 5;
                    EnvironmentFile = cfg.environmentFile;

                    ProtectSystem = "strict";
                    ProtectHome = true;
                    PrivateTmp = true;
                    NoNewPrivileges = true;
                    ReadWritePaths = [ cfg.dataDir ];
                  };
                };
              }

              (lib.mkIf cfg.database.enable {
                services.postgresql = {
                  enable = true;
                  ensureDatabases = [ cfg.database.name ];
                  ensureUsers = [
                    {
                      name = cfg.user;
                      ensureDBOwnership = true;
                    }
                  ]
                  ++ map (u: { name = u; }) cfg.database.extraUsers;
                  authentication = lib.mkAfter (
                    lib.concatStringsSep "\n" (
                      [ "local ${cfg.database.name} ${cfg.user} peer" ]
                      ++ map (u: "local ${cfg.database.name} ${u} peer") cfg.database.extraUsers
                    )
                  );
                };

                systemd.services.ow3n-db-grants = lib.mkIf (cfg.database.extraUsers != [ ]) {
                  description = "Grant extra users access to ${cfg.database.name}";
                  after = [ "postgresql.service" ];
                  requires = [ "postgresql.service" ];
                  wantedBy = [ "multi-user.target" ];

                  serviceConfig = {
                    Type = "oneshot";
                    RemainAfterExit = true;
                    User = "postgres";
                  };

                  script = lib.concatMapStringsSep "\n" (u: ''
                    ${config.services.postgresql.package}/bin/psql -d ${cfg.database.name} <<SQL
                      GRANT CONNECT ON DATABASE ${cfg.database.name} TO ${u};
                      GRANT USAGE ON SCHEMA public TO ${u};
                      GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO ${u};
                      ALTER DEFAULT PRIVILEGES FOR ROLE ${cfg.user} IN SCHEMA public
                        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO ${u};
                    SQL
                  '') cfg.database.extraUsers;
                };

                systemd.services.ow3n = {
                  after = [
                    "postgresql.service"
                  ]
                  ++ lib.optional (cfg.database.extraUsers != [ ]) "ow3n-db-grants.service";
                  requires = [ "postgresql.service" ];
                  environment = {
                    connectionstrings__CharacterDb = "host=/run/postgresql;username=${cfg.user};database=${cfg.database.name}";
                  };
                };
              })
            ]
          );
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

        # Implements `nix run .#build`: the repository-local, sandbox-safe build wrapper.
        buildCommand = pkgs.writeShellApplication {
          name = "ow3n-build";
          runtimeInputs = with pkgs; [
            dart-sass
            dotnetPkg
            nodejs
          ];
          text = ''
            if [[ ! -f Ordis.csproj ]]; then
              echo "Run this command from the OW3N repository root." >&2
              exit 1
            fi

            export XDG_CACHE_HOME="''${XDG_CACHE_HOME:-''${TMPDIR:-/tmp}/ow3n-build-cache}"
            mkdir -p "$XDG_CACHE_HOME"
            exec dotnet build --nologo -p:UseAppHost=false "$@"
          '';
        };

        # Implements `nix run .#test-serve`: an isolated PostgreSQL + stub + packaged app stack.
        testServeCommand = pkgs.writeShellApplication {
          name = "ow3n-test-serve";
          runtimeInputs = with pkgs; [
            coreutils
            curl
            postgresql
            python3
          ];
          text = ''
            export OW3N_APP="${self.packages.${system}.default}/bin/Ordis"
            export OW3N_INIT_DB="${./initDb.sql}"
            export OW3N_ROLL_STUB="${./test-support/roll-stub.py}"
            ${builtins.readFile ./test-support/test-serve.sh}
          '';
        };
      in
      {
        packages.default = pkgs.buildDotnetModule {
          pname = "ow3n";
          version = "0.1.0";
          src = ./.;

          projectFile = "Ordis.csproj";
          nugetDeps = ./deps.nix;

          dotnet-sdk = dotnetPkg;
          dotnet-runtime = dotnetRuntime;

          nativeBuildInputs = with pkgs; [
            nodejs
            dart-sass
            npmHooks.npmConfigHook
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
              --replace-fail 'exec' 'cd ${placeholder "out"}/lib/ow3n && exec'
          '';

          meta = {
            description = "OW3N - Blazor Server application";
            mainProgram = "Ordis";
          };
        };

        apps = {
          # Build OW3N without an apphost and with a writable XDG cache: `nix run .#build`.
          build = flake-utils.lib.mkApp { drv = buildCommand; };

          # Launch the disposable browser-test stack: `nix run .#test-serve`.
          test-serve = flake-utils.lib.mkApp { drv = testServeCommand; };
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
