{
  description = "Motur flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixos-unstable";
  };

  outputs = { nixpkgs, ... } @ inputs:
  let
      systems = [
        "aarch64-darwin"
        "x86_64-linux"
      ];
      forAllSystems = f:
        nixpkgs.lib.genAttrs systems (system:
          f {
            pkgs = import nixpkgs {
              inherit system;

              config.allowUnfree = true;
            };
          });
 in {
    devShells = forAllSystems (
      {pkgs}: {
        default = pkgs.mkShell {
          packages = builtins.attrValues {
            inherit
              (pkgs)
              ngrok
              go-task
              pre-commit
              act;
            combinedSdk = pkgs.buildEnv {
                  name = "combinedSdk";
                  paths = [
                    (with pkgs.dotnetCorePackages;
                      combinePackages [
                        sdk_9_0
                      ])
                  ];
            };
          };
        };
      }
    );
  };
}
