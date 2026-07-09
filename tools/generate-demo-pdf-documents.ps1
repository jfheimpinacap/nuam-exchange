$ErrorActionPreference = 'Stop'
$desktop = [Environment]::GetFolderPath('Desktop')
if ([string]::IsNullOrWhiteSpace($desktop)) { $desktop = (Get-Location).Path }
$output = Join-Path $desktop 'NuamExchangePdfDemo'
New-Item -ItemType Directory -Force -Path $output | Out-Null

function Escape-PdfText([string]$Text) { return $Text.Replace('\','\\').Replace('(','\(').Replace(')','\)') }
function New-SimplePdf($Path, [string[]]$Lines) {
  $contentLines = @('BT','/F1 12 Tf','50 760 Td','16 TL')
  foreach ($line in $Lines) { $contentLines += "($(Escape-PdfText $line)) Tj"; $contentLines += 'T*' }
  $contentLines += 'ET'
  $stream = ($contentLines -join "`n")
  $objects = @(
    '1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj',
    '2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj',
    '3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj',
    '4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj',
    "5 0 obj << /Length $($stream.Length) >> stream`n$stream`nendstream endobj"
  )
  $pdf = "%PDF-1.4`n"
  $offsets = @(0)
  foreach ($obj in $objects) { $offsets += [Text.Encoding]::ASCII.GetByteCount($pdf); $pdf += "$obj`n" }
  $xref = [Text.Encoding]::ASCII.GetByteCount($pdf)
  $pdf += "xref`n0 $($objects.Count + 1)`n0000000000 65535 f `n"
  for ($i=1; $i -lt $offsets.Count; $i++) { $pdf += ('{0:D10} 00000 n ' -f $offsets[$i]) + "`n" }
  $pdf += "trailer << /Size $($objects.Count + 1) /Root 1 0 R >>`nstartxref`n$xref`n%%EOF"
  [IO.File]::WriteAllText($Path, $pdf, [Text.Encoding]::ASCII)
}

New-SimplePdf (Join-Path $output '01_pdf_valido_certificado_tributario.pdf') @('Tipo de documento: Certificado Tributario','Mercado: BOLSA','Instrumento: NUAM-ACC-001','Periodo tributario: 2026','Factor aplicado: 1.23456789','Monto de referencia: 1000000','Fecha de emisión: 04-07-2026')
New-SimplePdf (Join-Path $output '02_pdf_incompleto_sin_factor.pdf') @('Tipo de documento: Certificado Tributario','Mercado: BOLSA','Instrumento: NUAM-ACC-002','Periodo tributario: 2026','Monto de referencia: 850000','Fecha de emisión: 04-07-2026')
New-SimplePdf (Join-Path $output '03_pdf_no_compatible.pdf') @('Documento informativo genérico','Este archivo no contiene estructura tributaria esperada.')
New-SimplePdf (Join-Path $output '04_pdf_incompleto_sin_periodo.pdf') @('Tipo de documento: Certificado Tributario','Mercado: BOLSA','Instrumento: NUAM-ACC-004','Factor aplicado: 1.10000000','Monto de referencia: 1200000','Fecha de emisión: 04-07-2026')
New-SimplePdf (Join-Path $output '05_pdf_no_tributario_generico.pdf') @('Acta interna','Contenido administrativo sin campos tributarios.')
Write-Host "PDFs demo creados en: $output"
