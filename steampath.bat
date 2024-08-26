
:: GET steam path in registry 

setlocal ENABLEEXTENSIONS
set KEY_NAME="HKEY_CURRENT_USER\Software\Valve\Steam"
set VALUE_NAME=SteamPath

FOR /F "usebackq tokens=1-3" %%A IN (`REG QUERY %KEY_NAME% /v %VALUE_NAME% 2^>nul`) DO (
    set ValueName=%%A
    set ValueType=%%B
    set ValueValue=%%C
)

if defined ValueName (
    @echo Value Name = %ValueName%
    @echo Value Type = %ValueType%
    @echo Value Value = %ValueValue%
) else (
    @echo %KEY_NAME% %VALUE_NAME% not found.
)

:: save it in user variable env

setx STEAM_PATH %ValueValue%

pause
