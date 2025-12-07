@echo off
setlocal enabledelayedexpansion

:: Archivo de salida
set JSON_FILE=repository-data.json

:: Inicializamos JSON
echo [> %JSON_FILE%

:: Contar cuántos lenguajes hay
set LANG_COUNT=0
for /d %%L in (storage\*) do (
    set /a LANG_COUNT+=1
)

:: Inicializamos índice de lenguajes
set LANG_INDEX=0

:: Recorremos cada lenguaje en storage
for /d %%L in (storage\*) do (
    set /a LANG_INDEX+=1

    set "LANG_NAME=%%~nxL"
    set "LANG_ICON=icons/%%~nxL.png"
    echo     {>> %JSON_FILE%
    echo         "name": "!LANG_NAME!",>> %JSON_FILE%
    echo         "icon": "!LANG_ICON!",>> %JSON_FILE%
    echo         "files": [>> %JSON_FILE%

    :: Contar archivos válidos (no .txt)
    set COUNT=0
    for %%F in ("%%L\*.*") do (
        if /i not "%%~xF"==".txt" (
            set /a COUNT+=1
        )
    )


    :: Inicializamos índice
    set INDEX=0

    :: Recorremos archivos válidos
    for %%F in ("%%L\*.*") do (
        if /i not "%%~xF"==".txt" (
            set /a INDEX+=1

            :: Construir path para JSON
            set "FILE_PATH=%%F"
            set "FILE_PATH=!FILE_PATH:\=/!"

            :: Si no existe el .txt, pedirlo
            set TXT_FILE=%%F.txt
            if not exist "!TXT_FILE!" (
                set /p DESC="No existe descripcion para %%~nxF. Escribe la descripcion: "
                echo !DESC!> "!TXT_FILE!"
            )

            :: Leer descripción (escapando comillas)
            set "DESC_TEXT="
            for /f "usebackq delims=" %%A in ("!TXT_FILE!") do (
                set "LINE=%%A"
                set "LINE=!LINE:"=\"!"
                set "DESC_TEXT=!DESC_TEXT!!LINE! "
            )

            :: Escribir JSON
            echo             {>> %JSON_FILE%
            echo                 "name": "%%~nF",>> %JSON_FILE%
            echo                 "path": "!FILE_PATH!",>> %JSON_FILE%
            echo                 "description": "!DESC_TEXT:~0,-1!" >> %JSON_FILE%
            echo             }>> %JSON_FILE%

            :: Añadir coma si NO es el último archivo
            if !INDEX! lss !COUNT! (
                echo ,>> %JSON_FILE%
            )
        )
    )


    echo         ]>> %JSON_FILE%
    echo     }>> %JSON_FILE%

    :: Añadir coma si NO es el último lenguaje
    if !LANG_INDEX! lss !LANG_COUNT! (
        echo ,>> %JSON_FILE%
    )
)

:: Quitamos la última coma (opcional, para JSON válido)
powershell -Command "(Get-Content %JSON_FILE%) -replace ',\r?\n\s*\]',']' | Set-Content %JSON_FILE%"

:: Cerramos array principal
echo ]>> %JSON_FILE%

echo.
echo repository-data.json generado correctamente!
pause
