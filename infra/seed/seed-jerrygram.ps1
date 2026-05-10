param(
    [string]$ApiBaseUrl = "http://localhost:5018/api",
    [string]$SeedDataPath = (Join-Path $PSScriptRoot "seed-data.json"),
    [string]$CollisionSuffix = "_seed",
    [switch]$SkipPosts,
    [switch]$SkipInteractions
)

$ErrorActionPreference = "Stop"

function Join-ApiPath {
    param([string]$Path)
    return "$($ApiBaseUrl.TrimEnd('/'))/$($Path.TrimStart('/'))"
}

function Invoke-Json {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body,
        [string]$Token
    )

    $headers = @{}
    if ($Token) {
        $headers.Authorization = "Bearer $Token"
    }

    $params = @{
        Method = $Method
        Uri = (Join-ApiPath $Path)
        Headers = $headers
    }

    if ($null -ne $Body) {
        $params.ContentType = "application/json"
        $params.Body = ($Body | ConvertTo-Json -Depth 20)
    }

    return Invoke-RestMethod @params
}

function Invoke-OptionalJson {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body,
        [string]$Token
    )

    try {
        return Invoke-Json -Method $Method -Path $Path -Body $Body -Token $Token
    }
    catch {
        Write-Host "skip: $Method $Path ($($_.Exception.Message))"
        return $null
    }
}

function Invoke-MultipartPost {
    param(
        [string]$Path,
        [hashtable]$Fields,
        [string]$FileField,
        [string]$FilePath,
        [string]$Token
    )

    Add-Type -AssemblyName System.Net.Http

    $client = [System.Net.Http.HttpClient]::new()
    $content = [System.Net.Http.MultipartFormDataContent]::new()
    $stream = $null

    try {
        if ($Token) {
            $client.DefaultRequestHeaders.Authorization =
                [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
        }

        foreach ($key in $Fields.Keys) {
            $content.Add([System.Net.Http.StringContent]::new([string]$Fields[$key]), $key)
        }

        if ($FilePath -and (Test-Path -LiteralPath $FilePath)) {
            $stream = [System.IO.File]::OpenRead($FilePath)
            $fileContent = [System.Net.Http.StreamContent]::new($stream)
            $fileContent.Headers.ContentType =
                [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/png")
            $content.Add($fileContent, $FileField, [System.IO.Path]::GetFileName($FilePath))
        }

        $response = $client.PostAsync((Join-ApiPath $Path), $content).GetAwaiter().GetResult()
        $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

        if (-not $response.IsSuccessStatusCode) {
            throw "POST $Path returned $([int]$response.StatusCode): $responseBody"
        }

        if ([string]::IsNullOrWhiteSpace($responseBody)) {
            return $null
        }

        return $responseBody | ConvertFrom-Json
    }
    finally {
        if ($stream) { $stream.Dispose() }
        $content.Dispose()
        $client.Dispose()
    }
}

function Get-PagedItems {
    param([string]$Path)

    $response = Invoke-Json -Method Get -Path $Path -Body $null
    if ($response.items) { return @($response.items) }
    if ($response.Items) { return @($response.Items) }
    if ($response -is [array]) { return @($response) }
    return @()
}

function Get-PropertyValue {
    param(
        [object]$Object,
        [string[]]$Names
    )

    foreach ($name in $Names) {
        if ($Object.PSObject.Properties.Name -contains $name) {
            return $Object.$name
        }
    }

    return $null
}

function Get-VisibilityValue {
    param([string]$Visibility)

    switch ($Visibility) {
        "FollowersOnly" { return 1 }
        "Private" { return 2 }
        default { return 0 }
    }
}

function Ensure-User {
    param([object]$User)

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

    function Use-Credentials {
        param([hashtable]$Credentials)

        try {
            $result = Invoke-Json -Method Post -Path "auth/register" -Body $Credentials
            Write-Host "registered user: $($Credentials.username)"
            return $result
        }
        catch {
            $registerError = $_.Exception.Message
            try {
                $result = Invoke-Json -Method Post -Path "auth/login" -Body @{
                    email = $Credentials.email
                    password = $Credentials.password
                }
                Write-Host "logged in existing user: $($Credentials.username)"
                return $result
            }
            catch {
                throw "Could not register or login user '$($Credentials.username)'. Register error: $registerError Login error: $($_.Exception.Message)"
            }
        }
    }

    $credentials = New-Credentials -SourceUser $User -Suffix ""
    try {
        $result = Use-Credentials -Credentials $credentials
    }
    catch {
        if ([string]::IsNullOrWhiteSpace($CollisionSuffix)) {
            throw
        }

        $credentials = New-Credentials -SourceUser $User -Suffix $CollisionSuffix
        Write-Host "using collision-safe seed user: $($credentials.username)"
        $result = Use-Credentials -Credentials $credentials
    }

    $profile = Invoke-Json -Method Get -Path "users/$($credentials.username)" -Body $null -Token $result.token
    return @{
        username = $credentials.username
        email = $credentials.email
        token = $result.token
        id = [string](Get-PropertyValue -Object $profile -Names @("id", "Id"))
    }
}

if (-not (Test-Path -LiteralPath $SeedDataPath)) {
    throw "Seed data file not found: $SeedDataPath"
}

$seed = Get-Content -Raw -LiteralPath $SeedDataPath | ConvertFrom-Json
$users = @{}
$posts = @{}

foreach ($user in $seed.users) {
    $session = Ensure-User -User $user
    $users[$user.username] = $session
}

$existingPosts = Get-PagedItems -Path "posts?page=1&pageSize=200"

if (-not $SkipPosts) {
    foreach ($post in $seed.posts) {
        $existing = $existingPosts | Where-Object { $_.caption -eq $post.caption } | Select-Object -First 1
        if ($existing) {
            Write-Host "post exists: $($post.caption)"
            $posts[$post.caption] = $existing
            continue
        }

        $author = $users[$post.author]
        if (-not $author) {
            Write-Host "skip post, missing author: $($post.author)"
            continue
        }

        $imagePath = Join-Path $PSScriptRoot $post.image
        $created = Invoke-MultipartPost -Path "posts" -Fields @{
            Caption = $post.caption
            Visibility = (Get-VisibilityValue -Visibility $post.visibility)
        } -FileField "Image" -FilePath $imagePath -Token $author.token

        Write-Host "created post: $($post.caption)"
        $posts[$post.caption] = $created
    }
}

if (-not $SkipInteractions) {
    foreach ($relationship in $seed.relationships) {
        $follower = $users[$relationship.follower]
        $following = $users[$relationship.following]
        if ($follower -and $following) {
            Invoke-OptionalJson -Method Post -Path "users/$($following.id)/follow" -Body $null -Token $follower.token | Out-Null
        }
    }

    $allPosts = Get-PagedItems -Path "posts?page=1&pageSize=200"

    foreach ($interaction in $seed.interactions) {
        $actor = $users[$interaction.user]
        $target = $allPosts | Where-Object {
            $_.caption -like "*$($interaction.postCaptionContains)*"
        } | Select-Object -First 1

        if (-not $actor -or -not $target) {
            Write-Host "skip interaction: $($interaction.user) $($interaction.type)"
            continue
        }

        if ($interaction.type -eq "like") {
            Invoke-OptionalJson -Method Post -Path "posts/$($target.id)/like" -Body $null -Token $actor.token | Out-Null
        }
        elseif ($interaction.type -eq "save") {
            Invoke-OptionalJson -Method Post -Path "posts/$($target.id)/save" -Body $null -Token $actor.token | Out-Null
        }
    }

    $searchToken = $users["jerry"].token
    foreach ($term in $seed.searches) {
        $encoded = [System.Uri]::EscapeDataString([string]$term)
        Invoke-OptionalJson -Method Get -Path "search?query=$encoded" -Body $null -Token $searchToken | Out-Null
    }
}

Write-Host ""
Write-Host "Seed complete."
Write-Host "Users: $($users.Count)"
Write-Host "Post definitions: $($seed.posts.Count)"
Write-Host "Search events requested: $($seed.searches.Count)"
