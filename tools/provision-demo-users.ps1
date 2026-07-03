<#
Use only after deploying C010 to the intended environment.
This script does not contain passwords, JWTs, or secrets and never prints them.
It prompts for the administrator credentials and each initial user password securely.
Do not run this script from automated validation or against production unless explicitly approved for the deployment window.
#>
param(
  [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"
$api = $BaseUrl.TrimEnd('/')

function Convert-SecureStringToPlainText([securestring]$Value) {
  $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
  try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
  finally { if ($bstr -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) } }
}

function Read-MatchingSecret([string]$Prompt) {
  $first = Read-Host "$Prompt" -AsSecureString
  $second = Read-Host "$Prompt (confirmación)" -AsSecureString
  $plain = Convert-SecureStringToPlainText $first
  $confirm = Convert-SecureStringToPlainText $second
  if ($plain -cne $confirm) { throw "Las contraseñas ingresadas no coinciden." }
  return $plain
}

$adminEmail = Read-Host "Correo de Administrador"
$adminPasswordSecure = Read-Host "Contraseña de Administrador" -AsSecureString
$adminPassword = Convert-SecureStringToPlainText $adminPasswordSecure

try {
  $login = Invoke-RestMethod -Method Post -Uri "$api/api/auth/login" -ContentType "application/json" -Body (@{ email = $adminEmail; password = $adminPassword } | ConvertTo-Json)
  $token = $login.accessToken
  if (-not $token) { $token = $login.token }
  if (-not $token) { throw "La API no devolvió un token utilizable." }
} catch {
  Write-Host "No fue posible iniciar sesión con el Administrador indicado. Revise URL y credenciales."
  exit 1
} finally {
  $adminPassword = $null
}

$headers = @{ Authorization = "Bearer $token" }
$roles = Invoke-RestMethod -Method Get -Uri "$api/api/admin/roles" -Headers $headers
$profiles = @(
  @{ fullName = "Franz"; email = "franz@nuamexchange.cl"; role = "Administrador" },
  @{ fullName = "Nicolai"; email = "nicolai@nuamexchange.cl"; role = "Analista Tributario" },
  @{ fullName = "Rodrigo"; email = "rodrigo@nuamexchange.cl"; role = "Supervisor" }
)

foreach ($profile in $profiles) {
  try {
    $password = Read-MatchingSecret "Contraseña inicial para $($profile.fullName) <$($profile.email)>"
    $role = $roles | Where-Object { $_.name -eq $profile.role } | Select-Object -First 1
    if (-not $role) { throw "Rol no disponible: $($profile.role)" }
    $body = @{ fullName = $profile.fullName; email = $profile.email; roleId = $role.id; isActive = $true; password = $password } | ConvertTo-Json
    Invoke-RestMethod -Method Post -Uri "$api/api/admin/users" -Headers $headers -ContentType "application/json" -Body $body | Out-Null
    Write-Host "$($profile.fullName) | $($profile.email) | $($profile.role) | Creado"
  } catch {
    $response = $_.Exception.Response
    if ($response -and [int]$response.StatusCode -eq 409) { Write-Host "$($profile.fullName) | $($profile.email) | $($profile.role) | Ya existe; se continúa" }
    else { Write-Host "$($profile.fullName) | $($profile.email) | $($profile.role) | Error seguro: $($_.Exception.Message)" }
  } finally { $password = $null }
}

$token = $null
