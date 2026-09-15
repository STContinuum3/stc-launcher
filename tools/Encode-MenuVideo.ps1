<#
.SYNOPSIS
    Encodes the main menu background video in a format the game's video player can play.

.DESCRIPTION
    Space Engineers plays menu videos through DirectShow: the WM ASF Reader feeding the WMVideo
    Decoder DMO. It needs a WMV file written by Windows' own ASF writer. WMV files muxed by ffmpeg
    load without an error but stall on the first frame when they have an audio track.

    This script uses the Windows Media Foundation transcoder (WinRT MediaTranscoder), so it needs
    no extra software. It writes WMV3 (Main profile) video and standard WMA audio, the same codecs
    as the game's own menu videos.

    Run it in Windows PowerShell 5.1 (powershell.exe), not PowerShell 7, which lacks the WinRT
    projection.

.EXAMPLE
    powershell.exe -ExecutionPolicy Bypass -File tools\Encode-MenuVideo.ps1 -Source master.mp4 -Destination star_trek_background.wmv -StopSeconds 118.5
#>
param(
    [Parameter(Mandatory)] [string] $Source,
    [Parameter(Mandatory)] [string] $Destination,
    [int] $Width = 1280,
    [int] $Height = 720,
    [int] $FramesPerSecond = 24,
    [int] $VideoBitrate = 900000,
    # Standard WMA at 48 kHz stereo accepts 160, 192 and higher kbps, but not 128 kbps
    [int] $AudioBitrate = 160000,
    # Where to end the video; 0 keeps the whole source
    [double] $StopSeconds = 0
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Run this script in Windows PowerShell 5.1 (powershell.exe).' }

Add-Type -AssemblyName System.Runtime.WindowsRuntime
$null = [Windows.Storage.StorageFile, Windows.Storage, ContentType = WindowsRuntime]
$null = [Windows.Media.Transcoding.MediaTranscoder, Windows.Media.Transcoding, ContentType = WindowsRuntime]
$null = [Windows.Media.MediaProperties.MediaEncodingProfile, Windows.Media.MediaProperties, ContentType = WindowsRuntime]

$extensions = [System.WindowsRuntimeSystemExtensions].GetMethods()
$asTaskOperation = $extensions | Where-Object {
    $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1'
} | Select-Object -First 1
$asTaskActionWithProgress = $extensions | Where-Object {
    $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncActionWithProgress`1'
} | Select-Object -First 1

function Wait-Operation($operation, [Type] $resultType) {
    $task = $asTaskOperation.MakeGenericMethod($resultType).Invoke($null, @($operation))
    $task.Wait(-1) | Out-Null
    $task.Result
}

$sourcePath = (Resolve-Path $Source).Path
$destinationPath = [IO.Path]::GetFullPath($Destination)

$sourceFile = Wait-Operation ([Windows.Storage.StorageFile]::GetFileFromPathAsync($sourcePath)) ([Windows.Storage.StorageFile])
$folder = Wait-Operation ([Windows.Storage.StorageFolder]::GetFolderFromPathAsync((Split-Path $destinationPath))) ([Windows.Storage.StorageFolder])
$destinationFile = Wait-Operation ($folder.CreateFileAsync((Split-Path $destinationPath -Leaf), [Windows.Storage.CreationCollisionOption]::ReplaceExisting)) ([Windows.Storage.StorageFile])

$profile = [Windows.Media.MediaProperties.MediaEncodingProfile]::CreateWmv([Windows.Media.MediaProperties.VideoEncodingQuality]::HD720p)
$profile.Video.Subtype = 'WMV3'
$profile.Video.Width = $Width
$profile.Video.Height = $Height
$profile.Video.Bitrate = $VideoBitrate
$profile.Video.FrameRate.Numerator = $FramesPerSecond
$profile.Video.FrameRate.Denominator = 1
$profile.Audio.Subtype = 'WMA8'
$profile.Audio.Bitrate = $AudioBitrate
$profile.Audio.SampleRate = 48000
$profile.Audio.ChannelCount = 2

$transcoder = New-Object Windows.Media.Transcoding.MediaTranscoder
$transcoder.HardwareAccelerationEnabled = $false
if ($StopSeconds -gt 0) { $transcoder.TrimStopTime = [TimeSpan]::FromSeconds($StopSeconds) }

$prepared = Wait-Operation ($transcoder.PrepareFileTranscodeAsync($sourceFile, $destinationFile, $profile)) ([Windows.Media.Transcoding.PrepareTranscodeResult])
if (-not $prepared.CanTranscode) {
    throw "Windows cannot transcode with these settings ($($prepared.FailureReason)). Check the audio bitrate and the source format."
}

$task = $asTaskActionWithProgress.MakeGenericMethod([double]).Invoke($null, @($prepared.TranscodeAsync()))
$task.Wait(-1) | Out-Null

'{0}: {1:N2} MB' -f $destinationPath, ((Get-Item $destinationPath).Length / 1MB)
