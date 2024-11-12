$folder = "C:\Src\EnergyWorldnet\platform\sql\EWN"

$sqlOrderExcludeArray = (
    "**/*.test*",
    "Assemblies/**",
    "**/Roles/**",
    "**/Security/Users/EWN*",
    "**/Service Broker/**"
)

$sqlOrderExcludes = $sqlOrderExcludeArray -join ","

dotnet run -- --Folder $folder -i $sqlOrderExcludes
