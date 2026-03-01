# Test Rate Limiting Script
# This script sends two rapid requests to trigger the 429 error

$email = "test@example.com"
$baseUrl = "http://localhost:5000/api/email"

Write-Host "=== Testing Rate Limiting (429 Error) ===" -ForegroundColor Blue
Write-Host "Sending two requests with the same email within 3 seconds...`n"

# First request - should succeed (200)
Write-Host "Request 1: Sending email '$email'..." -ForegroundColor Green
$body = @{ email = $email } | ConvertTo-Json

try {
    $response1 = Invoke-WebRequest -Uri $baseUrl -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    Write-Host "✓ Status: $($response1.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response1.Content)" -ForegroundColor Green
} catch {
    Write-Host "✗ No response - server might not be running on port 5000" -ForegroundColor Red
    exit
}

# Wait 1 second (still within the 3-second window)
Write-Host "`nWaiting 1 second..." -ForegroundColor Yellow
Start-Sleep -Seconds 1

# Second request - should fail with 429
Write-Host "Request 2: Sending email '$email' again (within 3-second window)..." -ForegroundColor Yellow
try {
    $response2 = Invoke-WebRequest -Uri $baseUrl -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    Write-Host "Response: $($response2.Content)" -ForegroundColor Green
} catch {
    $statusCode = $_.Exception.Response.StatusCode.Value__
    if ($statusCode -eq 429) {
        Write-Host "✓ Got expected 429 error!" -ForegroundColor Green
        Write-Host "Status Code: $statusCode (Too Many Requests)" -ForegroundColor Green
        $errorContent = $_.ErrorDetails.Message
        if ($errorContent) {
            Write-Host "Error Response: $errorContent" -ForegroundColor Cyan
        }
    } else {
        Write-Host "Status Code: $statusCode" -ForegroundColor Yellow
    }
}

# Wait 3+ seconds and try again - should succeed
Write-Host "`nWaiting 3+ seconds (to reset the rate limit window)..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

Write-Host "Request 3: Sending email '$email' again (after 3-second window)..." -ForegroundColor Green
try {
    $response3 = Invoke-WebRequest -Uri $baseUrl -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    Write-Host "✓ Status: $($response3.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response3.Content)" -ForegroundColor Green
} catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Blue
