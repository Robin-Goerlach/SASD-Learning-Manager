@echo off
setlocal
pushd "%~dp0"
dotnet restore SASD.Bewerbungsmanager.sln || goto :error
dotnet build SASD.Bewerbungsmanager.sln -c Release --no-restore || goto :error
popd
exit /b 0
:error
set EXITCODE=%ERRORLEVEL%
popd
exit /b %EXITCODE%
