@echo off
setlocal enabledelayedexpansion

:: Archivo de salida
set JSON_FILE=repository-data.json

:: Inicializamos JSON
echo [> %JSON_FILE%

:: Recorremos cada lenguaje en storage
for /d %%L in (storage\*) do (
    set "LANG_NAME=%%~nxL"
    set "LANG_ICON=icons/%%~nxL.png"
    echo     {>> %JSON_FILE%
    echo         "name": "!LANG_NAME!",>> %JSON_FILE%
    echo         "icon": "!LANG_ICON!",>> %JSON_FILE%
    echo         "files": [>> %JSON_FILE%

    set FIRST_FILE=1

    :: Recorremos cada archivo en la carpeta del lenguaje
    for %%F in ("%%L\*.*") do (
        :: Ignoramos archivos .txt
        if /i not "%%~xF"==".txt" (
            :: Construimos ruta del .txt
            set TXT_FILE=%%F.txt

            :: Si no existe el txt, pedimos al usuario
            if not exist "!TXT_FILE!" (
                set /p DESC="No existe descripcion para %%~nxF. Escribe la descripcion: "
                echo !DESC!> "!TXT_FILE!"
            )

            :: Agregamos la coma si no es el primer archivo
            if !FIRST_FILE! equ 1 (
                set FIRST_FILE=0
            ) else (
                echo ,>> %JSON_FILE%
            )

            :: Escapamos comillas en la descripción
            set "DESC_TEXT="
            for /f "usebackq delims=" %%A in ("!TXT_FILE!") do (
                set "LINE=%%A"
                set "LINE=!LINE:"=\"!"
                set "DESC_TEXT=!DESC_TEXT!!LINE! "
            )

            set "FILE_PATH=%%F"
            set "FILE_PATH=!FILE_PATH:\=/!" 

            :: Escribimos el archivo en JSON
            echo             {>> %JSON_FILE%
            echo                 "name": "%%~nF",>> %JSON_FILE%
            echo                 "path": "!FILE_PATH!",>> %JSON_FILE%
            echo                 "description": "!DESC_TEXT:~0,-1!" >> %JSON_FILE%
            echo             }>> %JSON_FILE%
        )
    )

    echo         ]>> %JSON_FILE%
    echo     },>> %JSON_FILE%
)

:: Quitamos la última coma (opcional, para JSON válido)
powershell -Command "(Get-Content %JSON_FILE%) -replace ',\r?\n\s*\]',']' | Set-Content %JSON_FILE%"

:: Cerramos array principal
echo ]>> %JSON_FILE%

echo.
echo repository-data.json generado correctamente!
pause
