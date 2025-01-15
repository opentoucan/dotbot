with import <nixpkgs> {
  config.allowUnfree = true;
};

mkShell {
  name = "dotnet-env";
  packages = [
    ngrok
    (with dotnetCorePackages; combinePackages [
      sdk_8_0
      sdk_9_0
    ])
  ];
}
