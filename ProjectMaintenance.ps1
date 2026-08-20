#requires -Version 5.1

[CmdletBinding()]
param(
    [switch]$Audit,
    [switch]$Clean,
    [switch]$InstallScheduledTask,
    [switch]$UninstallScheduledTask
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ProjectRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$TaskName = 'Puffies Project Maintenance'
$GiB = [int64]1GB
$MiB = [int64]1MB
$LibraryThreshold = 10 * $GiB
$BeeThreshold = 4 * $GiB
$TemporaryThreshold = 500 * $MiB
$MinimumFreeSpace = 25 * $GiB
$GitLooseCountThreshold = 500
$GitLooseSizeThreshold = 256 * $MiB

$UnityCacheTargets = @(
    'Library\Bee',
    'Library\BurstCache',
    'Library\ShaderCache',
    'Library\ScriptAssemblies',
    'Library\PlayerDataCache',
    'Library\BuildPlayerData',
    'Library\TempArtifacts'
)

$TemporaryTargets = @(
    'Temp',
    'Logs',
    'obj',
    '.vs'
)

$AllowedTargets = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase
)
foreach ($relativePath in $UnityCacheTargets + $TemporaryTargets + @('debug.log')) {
    [void]$AllowedTargets.Add($relativePath)
}

function Assert-ProjectRoot {
    if (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot 'Assets') -PathType Container) -or
        -not (Test-Path -LiteralPath (Join-Path $ProjectRoot 'ProjectSettings') -PathType Container) -or
        -not (Test-Path -LiteralPath (Join-Path $ProjectRoot '.git') -PathType Container)) {
        throw "ProjectMaintenance.ps1 must run from the Puffies repository root."
    }
}

function Format-ByteSize([int64]$Bytes) {
    if ($Bytes -ge 1GB) {
        return ('{0:N2} GiB' -f ($Bytes / 1GB))
    }

    if ($Bytes -ge 1MB) {
        return ('{0:N2} MiB' -f ($Bytes / 1MB))
    }

    if ($Bytes -ge 1KB) {
        return ('{0:N2} KiB' -f ($Bytes / 1KB))
    }

    return "$Bytes bytes"
}

function Get-DirectorySize([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return [int64]0
    }

    [int64]$total = 0
    try {
        foreach ($filePath in [System.IO.Directory]::EnumerateFiles(
            $Path,
            '*',
            [System.IO.SearchOption]::AllDirectories
        )) {
            try {
                $total += [System.IO.FileInfo]::new($filePath).Length
            }
            catch {
                Write-Verbose "Could not inspect ${filePath}: $($_.Exception.Message)"
            }
        }
    }
    catch {
        Write-Verbose "Could not fully enumerate ${Path}: $($_.Exception.Message)"
    }

    return $total
}

function Get-TargetSize([string]$RelativePath) {
    $fullPath = Join-Path $ProjectRoot $RelativePath
    if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
        return [System.IO.FileInfo]::new($fullPath).Length
    }

    return Get-DirectorySize $fullPath
}

function Get-FreeDiskSpace {
    $driveRoot = [System.IO.Path]::GetPathRoot($ProjectRoot)
    $drive = [System.IO.DriveInfo]::new($driveRoot)
    return [int64]$drive.AvailableFreeSpace
}

function Get-GitMetrics {
    $safeRoot = $ProjectRoot.Replace('\', '/')
    $output = & git -c "safe.directory=$safeRoot" count-objects -v 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw 'git count-objects failed.'
    }

    $values = @{}
    foreach ($line in $output) {
        $parts = $line -split ':', 2
        if ($parts.Count -eq 2) {
            $values[$parts[0].Trim()] = $parts[1].Trim()
        }
    }

    return [pscustomobject]@{
        LooseCount = [int]$values['count']
        LooseBytes = [int64]$values['size'] * 1KB
        PackBytes = [int64]$values['size-pack'] * 1KB
    }
}

function Get-MaintenanceProcesses {
    $allProcesses = @(Get-Process -ErrorAction SilentlyContinue)
    $unityProcesses = @($allProcesses | Where-Object {
        $_.ProcessName -in @('Unity', 'UnityCrashHandler64', 'bee_backend')
    })
    $gitProcesses = @($allProcesses | Where-Object {
        $_.ProcessName -eq 'git' -or $_.ProcessName -like 'git-*'
    })
    $buildProcesses = @($allProcesses | Where-Object {
        $_.ProcessName -in @('MSBuild', 'VBCSCompiler')
    })

    try {
        $dotnetBuilds = @(Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction Stop |
            Where-Object {
                $_.CommandLine -match '(?i)(\sbuild\s|MSBuild|VBCSCompiler)'
            })
        $buildProcesses += $dotnetBuilds
    }
    catch {
        Write-Verbose "Could not inspect dotnet command lines: $($_.Exception.Message)"
    }

    return [pscustomobject]@{
        Unity = $unityProcesses
        Git = $gitProcesses
        Build = $buildProcesses
    }
}

function Get-MaintenanceSnapshot {
    $libraryBytes = Get-DirectorySize (Join-Path $ProjectRoot 'Library')
    $beeBytes = Get-DirectorySize (Join-Path $ProjectRoot 'Library\Bee')
    [int64]$temporaryBytes = 0
    foreach ($relativePath in $TemporaryTargets) {
        $temporaryBytes += Get-TargetSize $relativePath
    }

    return [pscustomobject]@{
        LibraryBytes = $libraryBytes
        BeeBytes = $beeBytes
        TemporaryBytes = $temporaryBytes
        FreeDiskBytes = Get-FreeDiskSpace
        Git = Get-GitMetrics
        Processes = Get-MaintenanceProcesses
    }
}

function Get-ThresholdReasons($Snapshot) {
    $reasons = [System.Collections.Generic.List[string]]::new()
    if ($Snapshot.LibraryBytes -ge $LibraryThreshold) {
        $reasons.Add("Library reached $(Format-ByteSize $Snapshot.LibraryBytes).")
    }
    if ($Snapshot.BeeBytes -ge $BeeThreshold) {
        $reasons.Add("Library/Bee reached $(Format-ByteSize $Snapshot.BeeBytes).")
    }
    if ($Snapshot.TemporaryBytes -ge $TemporaryThreshold) {
        $reasons.Add("Temporary directories reached $(Format-ByteSize $Snapshot.TemporaryBytes).")
    }
    if ($Snapshot.FreeDiskBytes -lt $MinimumFreeSpace) {
        $reasons.Add("Free disk space fell to $(Format-ByteSize $Snapshot.FreeDiskBytes).")
    }
    if ($Snapshot.Git.LooseCount -ge $GitLooseCountThreshold) {
        $reasons.Add("Git loose object count reached $($Snapshot.Git.LooseCount).")
    }
    if ($Snapshot.Git.LooseBytes -ge $GitLooseSizeThreshold) {
        $reasons.Add("Git loose objects reached $(Format-ByteSize $Snapshot.Git.LooseBytes).")
    }

    return $reasons
}

function Write-AuditReport($Snapshot) {
    $reasons = @(Get-ThresholdReasons $Snapshot)
    Write-Output 'Puffies project maintenance audit'
    Write-Output "  Project: $ProjectRoot"
    Write-Output "  Library: $(Format-ByteSize $Snapshot.LibraryBytes) / threshold $(Format-ByteSize $LibraryThreshold)"
    Write-Output "  Library/Bee: $(Format-ByteSize $Snapshot.BeeBytes) / threshold $(Format-ByteSize $BeeThreshold)"
    Write-Output "  Temporary: $(Format-ByteSize $Snapshot.TemporaryBytes) / threshold $(Format-ByteSize $TemporaryThreshold)"
    Write-Output "  Free disk: $(Format-ByteSize $Snapshot.FreeDiskBytes) / minimum $(Format-ByteSize $MinimumFreeSpace)"
    Write-Output "  Git loose objects: $($Snapshot.Git.LooseCount), $(Format-ByteSize $Snapshot.Git.LooseBytes)"

    if ($reasons.Count -eq 0) {
        Write-Output '  Result: no cleanup threshold reached.'
    }
    else {
        Write-Output '  Result: cleanup threshold reached.'
        foreach ($reason in $reasons) {
            Write-Output "    - $reason"
        }
    }

    $active = @($Snapshot.Processes.Unity) + @($Snapshot.Processes.Build) + @($Snapshot.Processes.Git)
    if ($active.Count -gt 0) {
        $names = $active | ForEach-Object {
            if ($_.PSObject.Properties['ProcessName']) { $_.ProcessName } else { $_.Name }
        } | Sort-Object -Unique
        Write-Output "  Active maintenance conflicts: $($names -join ', ')"
    }
}

function Write-MaintenanceLog([string]$Message) {
    $settingsPath = Join-Path $ProjectRoot 'UserSettings'
    if (-not (Test-Path -LiteralPath $settingsPath -PathType Container)) {
        New-Item -ItemType Directory -Path $settingsPath | Out-Null
    }

    $line = '[{0}] {1}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $Message
    Add-Content -LiteralPath (Join-Path $settingsPath 'ProjectMaintenance.log') -Value $line -Encoding UTF8
}

function Remove-SafeTarget([string]$RelativePath) {
    if (-not $AllowedTargets.Contains($RelativePath)) {
        throw "Refusing to remove a non-allowlisted target: $RelativePath"
    }

    $fullPath = [System.IO.Path]::GetFullPath((Join-Path $ProjectRoot $RelativePath))
    $rootPrefix = $ProjectRoot.TrimEnd('\') + '\'
    if (-not $fullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a path outside the repository: $fullPath"
    }

    if (-not (Test-Path -LiteralPath $fullPath)) {
        return [int64]0
    }

    $item = Get-Item -LiteralPath $fullPath -Force
    if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Refusing to remove a reparse point: $fullPath"
    }

    $bytes = Get-TargetSize $RelativePath
    Remove-Item -LiteralPath $fullPath -Recurse -Force
    return $bytes
}

function Invoke-SafeGitGc {
    $safeRoot = $ProjectRoot.Replace('\', '/')
    & git -c "safe.directory=$safeRoot" gc
    if ($LASTEXITCODE -ne 0) {
        throw 'git gc failed.'
    }
}

function Invoke-Cleanup($Snapshot) {
    $reasons = @(Get-ThresholdReasons $Snapshot)
    if ($reasons.Count -eq 0) {
        Write-Output 'No cleanup threshold reached. Nothing was removed.'
        Write-MaintenanceLog 'No cleanup threshold reached.'
        return
    }

    [int64]$releasedBytes = 0
    $lowDisk = $Snapshot.FreeDiskBytes -lt $MinimumFreeSpace
    $cleanUnityCaches = $lowDisk -or
        $Snapshot.LibraryBytes -ge $LibraryThreshold -or
        $Snapshot.BeeBytes -ge $BeeThreshold
    $cleanTemporary = $lowDisk -or $Snapshot.TemporaryBytes -ge $TemporaryThreshold
    $cleanGit = $Snapshot.Git.LooseCount -ge $GitLooseCountThreshold -or
        $Snapshot.Git.LooseBytes -ge $GitLooseSizeThreshold

    if ($cleanUnityCaches -or $cleanTemporary) {
        $blockingProcesses = @($Snapshot.Processes.Unity) + @($Snapshot.Processes.Build)
        if ($blockingProcesses.Count -gt 0) {
            $names = $blockingProcesses | ForEach-Object {
                if ($_.PSObject.Properties['ProcessName']) { $_.ProcessName } else { $_.Name }
            } | Sort-Object -Unique
            $message = "Skipped cache cleanup because these processes are active: $($names -join ', ')."
            Write-Output $message
            Write-MaintenanceLog $message
        }
        else {
            if ($cleanUnityCaches) {
                foreach ($relativePath in $UnityCacheTargets) {
                    [int64]$removedBytes = Remove-SafeTarget $relativePath
                    $releasedBytes += $removedBytes
                    if ($removedBytes -gt 0) {
                        Write-Output "Removed $relativePath ($(Format-ByteSize $removedBytes))."
                    }
                }
            }
            if ($cleanTemporary) {
                foreach ($relativePath in $TemporaryTargets) {
                    [int64]$removedBytes = Remove-SafeTarget $relativePath
                    $releasedBytes += $removedBytes
                    if ($removedBytes -gt 0) {
                        Write-Output "Removed $relativePath ($(Format-ByteSize $removedBytes))."
                    }
                }
            }
            if (($cleanUnityCaches -or $cleanTemporary) -and
                (Test-Path -LiteralPath (Join-Path $ProjectRoot 'debug.log'))) {
                [int64]$removedBytes = Remove-SafeTarget 'debug.log'
                $releasedBytes += $removedBytes
                if ($removedBytes -gt 0) {
                    Write-Output "Removed debug.log ($(Format-ByteSize $removedBytes))."
                }
            }
        }
    }

    if ($cleanGit) {
        if (@($Snapshot.Processes.Git).Count -gt 0) {
            $message = 'Skipped git gc because a Git process is active.'
            Write-Output $message
            Write-MaintenanceLog $message
        }
        else {
            Write-Output 'Running git gc with the default safety retention period.'
            Invoke-SafeGitGc
        }
    }

    $summary = "Maintenance completed. Estimated released space: $(Format-ByteSize $releasedBytes)."
    Write-Output $summary
    Write-MaintenanceLog $summary
}

function Install-MaintenanceTask {
    if ($env:OS -ne 'Windows_NT') {
        throw 'Scheduled task installation is only supported on Windows.'
    }

    Import-Module ScheduledTasks -ErrorAction Stop
    $powerShellPath = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
    $arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File `"$PSCommandPath`" -Clean"
    $action = New-ScheduledTaskAction -Execute $powerShellPath -Argument $arguments -WorkingDirectory $ProjectRoot
    $trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At 3:00AM
    $settings = New-ScheduledTaskSettingsSet `
        -StartWhenAvailable `
        -MultipleInstances IgnoreNew `
        -ExecutionTimeLimit (New-TimeSpan -Hours 2) `
        -AllowStartIfOnBatteries `
        -DontStopIfGoingOnBatteries
    $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
    $principal = New-ScheduledTaskPrincipal `
        -UserId $identity `
        -LogonType Interactive `
        -RunLevel Limited
    $task = New-ScheduledTask `
        -Action $action `
        -Trigger $trigger `
        -Settings $settings `
        -Principal $principal `
        -Description 'Audits Puffies every week and removes only allowlisted caches after thresholds are reached.'

    Register-ScheduledTask -TaskName $TaskName -InputObject $task -Force | Out-Null
    $registered = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
    $registeredAction = @($registered.Actions)[0]
    if ($registeredAction.Arguments -notlike "*$PSCommandPath*") {
        throw 'The scheduled task was registered with an unexpected script path.'
    }

    Write-Output "Installed scheduled task '$TaskName'."
    Write-Output "  Schedule: every Sunday at 03:00, start when available."
    Write-Output "  Script: $PSCommandPath"
    Write-MaintenanceLog "Installed scheduled task '$TaskName' for $PSCommandPath."
}

function Uninstall-MaintenanceTask {
    Import-Module ScheduledTasks -ErrorAction Stop
    $existing = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    if ($null -eq $existing) {
        Write-Output "Scheduled task '$TaskName' is not installed."
        return
    }

    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    Write-Output "Uninstalled scheduled task '$TaskName'."
    Write-MaintenanceLog "Uninstalled scheduled task '$TaskName'."
}

Assert-ProjectRoot

$operationCount = @($Audit, $Clean, $InstallScheduledTask, $UninstallScheduledTask |
    Where-Object { $_ }).Count
if ($operationCount -gt 1) {
    throw 'Choose only one operation.'
}

if ($InstallScheduledTask) {
    Install-MaintenanceTask
    exit 0
}

if ($UninstallScheduledTask) {
    Uninstall-MaintenanceTask
    exit 0
}

$snapshot = Get-MaintenanceSnapshot
Write-AuditReport $snapshot

if ($Clean) {
    Invoke-Cleanup $snapshot
}
