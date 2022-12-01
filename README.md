# Gsof.System.Token
获取 smbios uuid 作为系统唯一 ID 的库

0.2.0 支持 `net40`、`net45`、`net46`、`net5.0`、`net6.0`。

> 从 `0.2.0` 版本开始，移除 dmidecode.exe 作为获取 uuid ，改为直接通过 `GetSystemFirmwareTable` 读取 SMBIOS 信息。
