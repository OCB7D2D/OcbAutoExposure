@echo off

call MC7D2D AutoExposure.dll Harmony\*.cs ^
/reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" && ^
echo Successfully compiled AutoExposure.dll

pause