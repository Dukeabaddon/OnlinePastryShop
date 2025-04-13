@echo off
echo Cleaning project...
if exist bin rd /s /q bin
if exist obj rd /s /q obj

echo Creating temporary directories...
mkdir bin
mkdir obj

echo Building project...
dotnet build OnlinePastryShop.csproj /p:TargetFramework=net48 /p:Configuration=Debug /p:Platform="Any CPU"

echo Done!
pause 