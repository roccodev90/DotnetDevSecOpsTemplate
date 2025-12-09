# Script per audit delle vulnerabilità nei pacchetti NuGet
# Fallisce il build se trova vulnerabilità HIGH o CRITICAL

Write-Host "Scanning for vulnerable packages..." -ForegroundColor Cyan

# Esegui il comando e cattura l'output
$output = dotnet list package --vulnerable --include-transitive 2>&1

# Controlla se ci sono vulnerabilità HIGH o CRITICAL
if ($output -match "(HIGH|CRITICAL)") {
    Write-Host ""
    Write-Host "VULNERABILITÀ TROVATE!" -ForegroundColor Red
    Write-Host "================================" -ForegroundColor Red
    Write-Host $output
    Write-Host "================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Build fallito: sono state trovate vulnerabilità HIGH o CRITICAL" -ForegroundColor Yellow
    Write-Host "Risolvi le vulnerabilità prima di procedere." -ForegroundColor Yellow
    exit 1
}

# Controlla se ci sono vulnerabilità di qualsiasi livello
if ($output -match "vulnerable") {
    Write-Host ""
    Write-Host "Vulnerabilità trovate (non HIGH/CRITICAL):" -ForegroundColor Yellow
    Write-Host $output
    Write-Host ""
    Write-Host "Build continuerà, ma si consiglia di risolvere le vulnerabilità." -ForegroundColor Cyan
    exit 0
}

Write-Host "Nessuna vulnerabilità trovata!" -ForegroundColor Green
exit 0