echo Hello World
set PATH=%PATH%;%WINDIR%\Microsoft.Net\Framework64\v4.0.30319

rem go to current folder
cd %~dp0

msbuild Build.proj