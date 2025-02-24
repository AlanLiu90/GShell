rmdir /S /Q publish

dotnet publish GShell\GShell.csproj -r win-x64 -c Release -o publish

pause
