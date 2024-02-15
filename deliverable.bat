del /s /q "deliverable\Mount & Blade II Bannerlord\Modules\SandBoxCoreMP"
del /s /q "deliverable\Mount & Blade II Dedicated Server\Modules\SandBoxCoreMP"
xcopy /y /e /k /h /i SandBoxCoreMP "deliverable\Mount & Blade II Bannerlord\Modules\SandBoxCoreMP"
xcopy /y /e /k /h /i SandBoxCoreMP "deliverable\Mount & Blade II Dedicated Server\Modules\SandBoxCoreMP"


:: Serveur
mkdir "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\Battles\Finished"
mkdir "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\Battles\Pending"
del /s /q "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\bin\Win64_Shipping_Server\*"
xcopy /y BattleLink.Server\bin\x64\Release\BattleLink.Common.dll     "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\bin\Win64_Shipping_Server\"
xcopy /y BattleLink.Server\bin\x64\Release\BattleLink.CommonSvSp.dll "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\bin\Win64_Shipping_Server\"
xcopy /y BattleLink.Server\bin\x64\Release\BattleLink.CommonSvMp.dll "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\bin\Win64_Shipping_Server\"
xcopy /y BattleLink.Server\bin\x64\Release\BattleLink.Server.dll     "deliverable\Mount & Blade II Dedicated Server\Modules\BattleLink\bin\Win64_Shipping_Server\"

:: Multiplayer
del /s /q "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink\bin\Win64_Shipping_Client\*"
xcopy /y BattleLink.Common\bin\x64\Release\BattleLink.Common.dll                    "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink\bin\Win64_Shipping_Client\"
xcopy /y BattleLink.CommonSvMp\bin\Release\netstandard2.0\BattleLink.CommonSvMp.dll "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink\bin\Win64_Shipping_Client\"
xcopy /y BattleLink.Multiplayer\obj\Release\BattleLink.dll                          "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink\bin\Win64_Shipping_Client\"


:: Singleplayer
mkdir "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\Battles"
del /s /q "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\bin\Win64_Shipping_Client\*"
xcopy /y BattleLink.Common\bin\x64\Release\BattleLink.Common.dll                    "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\bin\Win64_Shipping_Client\"
xcopy /y BattleLink.CommonSvSp\bin\Release\netstandard2.0\BattleLink.CommonSvSp.dll "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\bin\Win64_Shipping_Client\"
xcopy /y BattleLink.Singleplayer\obj\Release\BattleLink.Singleplayer.dll            "deliverable\Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\bin\Win64_Shipping_Client\"

cd deliverable
C:\tools\7-Zip\7z.exe a "BattleLink.zip" "Mount & Blade II Dedicated Server" "Mount & Blade II Bannerlord"
cd ..


















