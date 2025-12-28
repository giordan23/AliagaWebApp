Set WshShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

' Obtener la ruta del script
strScriptPath = objFSO.GetParentFolderName(WScript.ScriptFullName)

' Iniciar Backend y Frontend en paralelo (sin mostrar ventanas)
WshShell.Run "cmd /c cd /d """ & strScriptPath & "\Backend"" && dotnet run", 0, False
WshShell.Run "cmd /c cd /d """ & strScriptPath & "\Frontend"" && npm start", 0, False

' Esperar 5 segundos para que ambos inicien
WScript.Sleep 5000

' Abrir el navegador en el dashboard
WshShell.Run "http://localhost:4200", 1, False
