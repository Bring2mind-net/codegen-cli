

# Building

dotnet pack
dotnet pack -p:NuspecFile=./Templates/Default.nuspec

# Installing

dotnet tool install --global --add-source ./nupkg Bring2mind.CodeGen.Cli

dotnet tool uninstall -g Bring2mind.CodeGen.Cli
