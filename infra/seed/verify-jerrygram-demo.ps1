param(
    [string]$ApiBaseUrl = "http://localhost:5018/api",
    [string]$RecommendHealthUrl = "http://localhost:13001/health",
    [string]$ElasticsearchUrl = "http://localhost:19200",
    [string]$SeedDataPath = (Join-Path $PSScriptRoot "seed-data.json"),
    [string]$CollisionSuffix = "_seed",
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

function Join-ApiPath {
    param([string]$Path)
    return "$($ApiBaseUrl.TrimEnd('/'))/$($Path.TrimStart('/'))"
}

function Invoke-Json {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body,
        [string]$Token
    )

    $headers = @{}
    if ($Token) {
        $headers.Authorization = "Bearer $Token"
    }

    $params = @{
        Method = $Method
        Uri = $Uri
        Headers = $headers
    }

    if ($null -ne $Body) {
        $params.ContentType = "application/json"
        $params.Body = ($Body | ConvertTo-Json -Depth 20)
    }

    return Invoke-RestMethod @params
}

function Get-KafkaTopicCount {
    param([string]$Topic)

    try {
        $output = docker exec jg-kafka kafka-run-class kafka.tools.GetOffsetShell --broker-list kafka:29092 --topic $Topic --time -1 2>$null
        $total = 0
        foreach ($line in $output) {
            $parts = [string]$line -split ":"
            if ($parts.Length -ge 3) {
                $total += [int64]$parts[2]
            }
        }
        return $total
    }
    catch {
        return $null
    }
}

function New-Credentials {
    param(
        [object]$SourceUser,
        [string]$Suffix
    )

    $username = [string]$SourceUser.username
    $email = [string]$SourceUser.email

    if (-not [string]::IsNullOrWhiteSpace($Suffix)) {
        $username = "$username$Suffix"
        $tag = $Suffix -replace '^[_.-]+', ''
        if ([string]::IsNullOrWhiteSpace($tag)) {
            $tag = "seed"
        }

        $at = $email.IndexOf("@")
        if ($at -gt 0) {
            $email = "$($email.Substring(0, $at))+$tag$($email.Substring($at))"
        }
        else {
            $email = "$email+$tag"
        }
    }

    return @{
        email = $email
        username = $username
        password = $SourceUser.password
    }
}

function Try-SeedLogin {
    param([hashtable]$Credentials)

    try {
        $login = Invoke-Json -Method Post -Uri (Join-ApiPath "auth/login") -Body @{
            email = $Credentials.email
            password = $Credentials.password
        }

        if ($login.token) {
            Write-Host "logged in seed user: $($Credentials.username)"
            return @{
                login = $login
                credentials = $Credentials
            }
        }
    }
    catch {
        return $null
    }

    return $null
}

if (-not (Test-Path -LiteralPath $SeedDataPath)) {
    throw "Seed data file not found: $SeedDataPath"
}

$seed = Get-Content -Raw -LiteralPath $SeedDataPath | ConvertFrom-Json
$primaryUser = $seed.users | Where-Object { $_.username -eq "jerry" } | Select-Object -First 1

if (-not $primaryUser) {
    throw "Seed data must include a user named 'jerry'."
}

$loginCandidate = Try-SeedLogin -Credentials (New-Credentials -SourceUser $primaryUser -Suffix "")

if (-not $loginCandidate -and -not [string]::IsNullOrWhiteSpace($CollisionSuffix)) {
    $loginCandidate = Try-SeedLogin -Credentials (New-Credentials -SourceUser $primaryUser -Suffix $CollisionSuffix)
}

$login = $loginCandidate.login
$loggedInSeedUser = $loginCandidate.credentials

$token = $login.token
if (-not $token) {
    throw "Could not login with the seed primary user. Run seed-jerrygram.ps1 first."
}

$recommendHealth = $null
try {
    $recommendHealth = Invoke-RestMethod -Uri $RecommendHealthUrl
}
catch {
    $recommendHealth = @{ status = "unavailable"; error = $_.Exception.Message }
}

$explore = Invoke-Json -Method Get -Uri (Join-ApiPath "explore") -Token $token
$popular = Invoke-Json -Method Get -Uri (Join-ApiPath "search/popular?limit=10&hours=24")
$trending = Invoke-Json -Method Get -Uri (Join-ApiPath "search/popular/trending?limit=5")

$eventIndices = @()
try {
    $eventIndices = Invoke-RestMethod "$($ElasticsearchUrl.TrimEnd('/'))/_cat/indices/jerrygram-events-*?format=json&h=index,docs.count,health,status"
}
catch {
    $eventIndices = @(@{ error = $_.Exception.Message })
}

$kafkaCounts = [ordered]@{
    "search-events" = Get-KafkaTopicCount -Topic "search-events"
    "post-events" = Get-KafkaTopicCount -Topic "post-events"
    "user-events" = Get-KafkaTopicCount -Topic "user-events"
}

$summary = [ordered]@{
    generatedAt = (Get-Date).ToUniversalTime().ToString("o")
    apiBaseUrl = $ApiBaseUrl
    seedUser = @{
        username = $loggedInSeedUser.username
        email = $loggedInSeedUser.email
    }
    recommendService = $recommendHealth
    explore = @{
        count = @($explore).Count
        top = @($explore | Select-Object -First 5 | ForEach-Object {
            [ordered]@{
                id = $_.id
                caption = $_.caption
                score = $_.score
                username = $_.user.username
            }
        })
    }
    popularSearches = @($popular | Select-Object -First 10)
    trendingSearches = @($trending | Select-Object -First 5)
    kafkaTopicOffsets = $kafkaCounts
    elasticsearchEventIndices = $eventIndices
}

$json = $summary | ConvertTo-Json -Depth 20

if ($OutputPath) {
    $directory = Split-Path -Parent $OutputPath
    if ($directory) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    Set-Content -LiteralPath $OutputPath -Value $json -Encoding UTF8
    Write-Host "Demo evidence saved to $OutputPath"
}

$json
