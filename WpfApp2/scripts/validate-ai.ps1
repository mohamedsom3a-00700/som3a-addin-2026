param(
    [string]$BuildRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")),
    [switch]$Quiet
)

$script:results = @{ passed = 0; failed = 0; details = @() }

function Write-Result {
    param([string]$Check, [bool]$Passed, [string]$Detail)
    if ($Passed) { $script:results.passed++; $msg = "  PASS: $Check" } else { $script:results.failed++; $msg = "  FAIL: $Check" }
    if (-not $Quiet) { Write-Host $msg -ForegroundColor $(if ($Passed) { "Green" } else { "Red" }) }
    $script:results.details += @{ check = $Check; passed = $Passed; detail = $Detail }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "AI Validation Suite" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/3] Provider Prompt Outputs..." -ForegroundColor Cyan
$aiRoot = Join-Path $BuildRoot "..\Som3a.AI"
$aiExists = Test-Path $aiRoot
Write-Result "Som3a.AI project exists" $aiExists ""
$abstractionExists = $aiExists -and (Test-Path (Join-Path $aiRoot "Orchestration\AIOrchestrator.cs"))
Write-Result "AI provider abstraction exists" $abstractionExists ""

Write-Host "`n[2/3] Retry Handling..." -ForegroundColor Cyan
$retryExists = $aiExists -and ((Test-Path (Join-Path $aiRoot "RetryHandler.cs")) -or (Test-Path (Join-Path $aiRoot "Orchestration\RetryHandler.cs")))
Write-Result "Retry mechanism present" $retryExists ""

Write-Host "`n[3/3] Structured JSON Parsing..." -ForegroundColor Cyan
$jsonExists = $aiExists -and (Test-Path (Join-Path $aiRoot "Parsing"))
Write-Result "JSON parsing infrastructure exists" $jsonExists ""

Write-Host "`n========================================" -ForegroundColor Cyan
$total = $script:results.passed + $script:results.failed
$allPassed = ($script:results.failed -eq 0)
Write-Host "AI Validation: $($script:results.passed)/$total passed" -ForegroundColor $(if ($allPassed) { "Green" } else { "Red" })
Write-Host "========================================" -ForegroundColor Cyan

$report = @{
    suite = "AI"
    passed = $script:results.passed
    failed = $script:results.failed
    total = $total
    overall = if ($allPassed) { "pass" } else { "fail" }
    details = $script:results.details
}
$report | ConvertTo-Json -Depth 3

if (-not $allPassed) { exit 1 }
