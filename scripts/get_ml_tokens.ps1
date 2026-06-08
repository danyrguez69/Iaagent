# ============================================================
#  IaAgent - Obtener tokens de MercadoLibre Chile
#  Corre este script en PowerShell como Administrador
# ============================================================

$APP_ID       = "4667015521659025"
$APP_SECRET   = "SCZ99k2p0SZDcn75GcLzEMGEpxhgAqdr"
$REDIRECT_URI = "https://taskingweb.cl"

# Valores PKCE generados (no cambiar)
$CODE_VERIFIER  = "iQD1mNb0GzfHSVf21pFy5vpNYk3k5eJ8xLgZwpWzLYU"
$CODE_CHALLENGE = "wYFDTG8897O-ER5TOX-iWO2zI_1FmTu59herZcGyKrs"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IaAgent - Autenticacion MercadoLibre" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Abrir navegador para autorizar
$AUTH_URL = "https://auth.mercadolibre.cl/authorization?response_type=code&client_id=$APP_ID&redirect_uri=$([Uri]::EscapeDataString($REDIRECT_URI))&code_challenge=$CODE_CHALLENGE&code_challenge_method=S256"

Write-Host "PASO 1: Abriendo navegador para autorizar..." -ForegroundColor Yellow
Write-Host "URL: $AUTH_URL" -ForegroundColor DarkGray
Start-Process $AUTH_URL
Write-Host ""

# Paso 2: Pedir el codigo
Write-Host "PASO 2: Cuando el navegador te redirija a taskingweb.cl" -ForegroundColor Yellow
Write-Host "        copia el valor 'code=TG-XXXX' de la URL y pegalo aqui:" -ForegroundColor Yellow
Write-Host ""
$CODE = Read-Host "  Pega el code (TG-...)"
$CODE = $CODE.Trim()

if (-not $CODE.StartsWith("TG-")) {
    Write-Host "ERROR: El codigo debe comenzar con TG-" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "PASO 3: Intercambiando code por tokens..." -ForegroundColor Yellow

# Paso 3: Exchange code por tokens
$BODY = "grant_type=authorization_code" +
        "&client_id=$APP_ID" +
        "&client_secret=$APP_SECRET" +
        "&code=$CODE" +
        "&redirect_uri=$([Uri]::EscapeDataString($REDIRECT_URI))" +
        "&code_verifier=$CODE_VERIFIER"

try {
    $RESPONSE = Invoke-RestMethod `
        -Uri "https://api.mercadolibre.com/oauth/token" `
        -Method POST `
        -ContentType "application/x-www-form-urlencoded" `
        -Body $BODY

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  TOKENS OBTENIDOS EXITOSAMENTE" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Copia estos valores en tu appsettings.json o .env:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  AccessToken  : $($RESPONSE.access_token)" -ForegroundColor White
    Write-Host "  RefreshToken : $($RESPONSE.refresh_token)" -ForegroundColor White
    Write-Host "  UserId       : $($RESPONSE.user_id)" -ForegroundColor White
    Write-Host "  Expira en    : $($RESPONSE.expires_in / 3600) horas" -ForegroundColor DarkGray
    Write-Host ""

    # Guardar en archivo para copiar facil
    $OUTPUT = @"
MercadoLibre__AppId=$APP_ID
MercadoLibre__AppSecret=$APP_SECRET
MercadoLibre__AccessToken=$($RESPONSE.access_token)
MercadoLibre__RefreshToken=$($RESPONSE.refresh_token)
MercadoLibre__UserId=$($RESPONSE.user_id)
"@

    $OUTPUT | Out-File -FilePath "ml_tokens.txt" -Encoding UTF8
    Write-Host "  Tokens guardados en: ml_tokens.txt" -ForegroundColor Green
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "ERROR al obtener tokens:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red

    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorBody = $reader.ReadToEnd()
        Write-Host "Detalle: $errorBody" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Posibles causas:" -ForegroundColor Yellow
    Write-Host "  - El code ya expiro (tienes ~5 minutos desde que autorizas)" -ForegroundColor Yellow
    Write-Host "  - redirect_uri no coincide con la registrada en la app ML" -ForegroundColor Yellow
    Write-Host "  - Vuelve a correr el script para obtener un nuevo code" -ForegroundColor Yellow
}

Write-Host "Presiona Enter para cerrar..."
Read-Host
