@echo off
setlocal
pushd "%~dp0"
dotnet test SASD.Bewerbungsmanager.sln -c Release --no-build
set EXITCODE=%ERRORLEVEL%
popd
exit /b %EXITCODE%
