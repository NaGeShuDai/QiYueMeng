$ErrorActionPreference = 'Stop'
try {
    $runtime = (Get-Command pythonw.exe -ErrorAction SilentlyContinue).Source
    if (-not $runtime) { $runtime = (Get-Command python.exe -ErrorAction Stop).Source }
    $port = $null
    $available = $null
    foreach ($candidate in 18777..18797) {
        $listener = $null
        $free = $false
        try { $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,$candidate); $listener.Start(); $free = $true } catch {} finally { if ($listener) { $listener.Stop() } }
        if ($free) { if ($null -eq $available) { $available = $candidate }; continue }
        try {
            $health = Invoke-RestMethod "http://127.0.0.1:$candidate/api/health" -TimeoutSec 1
            if ($health.app -eq 'SeventhPier' -and [IO.Path]::GetFullPath($health.root) -eq [IO.Path]::GetFullPath($PSScriptRoot)) { $port = $candidate; break }
        } catch {}
    }
    if ($null -eq $port) {
        if ($null -eq $available) { throw 'No available local website port.' }
        $port = $available
        Start-Process -FilePath $runtime -ArgumentList @(('"' + (Join-Path $PSScriptRoot 'server.py') + '"'), '--port', "$port") -WorkingDirectory $PSScriptRoot -WindowStyle Hidden
        $healthy = $false
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            Start-Sleep -Milliseconds 300
            try { $health = Invoke-RestMethod "http://127.0.0.1:$port/api/health" -TimeoutSec 1; $healthy = $health.app -eq 'SeventhPier' -and [IO.Path]::GetFullPath($health.root) -eq [IO.Path]::GetFullPath($PSScriptRoot) } catch {}
            if ($healthy) { break }
        }
        if (-not $healthy) { throw 'Website startup failed. Check Python installation.' }
    }
    Start-Process "http://127.0.0.1:$port/index.html"
} catch {
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show($_.Exception.Message,'Website startup error') | Out-Null
    exit 1
}
