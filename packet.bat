@echo off
cd Gsof.System.Token
call nuget pack -properties Configuration=Release -Verbosity detailed -Exclude dmidecode.exe -OutputDirectory ..\build

cd ..