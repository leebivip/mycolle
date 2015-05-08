for /r "C:\ProgramData\SogouPY"                       %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "C:\Program Files\SogouInput"                  %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "C:\Users\Public\SogouInput"                   %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "%userprofile%\AppData\LocalLow\SogouPY"       %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "%userprofile%\AppData\LocalLow\SogouPY.users" %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
                                                 netsh advfirewall firewall add rule program="C:\Windows\System32\SogouPy.ime" action=block name="block-ime" dir=out

for /r "C:\Program Files\Baidu"                       %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "C:\ProgramData\baidu"                         %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "C:\Users\rw\AppData\LocalLow\Baidu"           %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
for /r "C:\Users\rw\AppData\Local\Temp\baidu_jp_ime"  %%i in (*.exe) do netsh advfirewall firewall add rule program="%%~dpnxi" action=block name="block-ime" dir=out
                                               netsh advfirewall firewall add rule program="C:\Windows\System32\BaiduJP20.ime" action=block name="block-ime" dir=out
