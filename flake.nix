{
  description = "Ordis discord bot";

  inputs = {
    nixpkgs.url = "nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";

  };

  outputs =
    {
      self,
      nixpkgs,
      flake-utils,
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = nixpkgs.legacyPackages.${system};

        run = pkgs.writeShellApplication {
          name = "run";

          text = ''

            (cd "$(git rev-parse --show-toplevel)/ordis-webui/" && bun start)

          '';

        };

      in
      {
        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            bun
            nodejs
            run
          ];

        };

      }
    );
}
