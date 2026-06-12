param(
  [string]$UiProjectName = "",
  [Parameter(Mandatory = $true)]
  [string]$CancellationProjectName,
  [Parameter(Mandatory = $true)]
  [string]$OutputPath,
  [string]$SequenceHooksFile = "",
  [switch]$IncludeCancellationActions,
  [switch]$IncludeEmbeddedUi
)

function Get-SafeWixId {
  param([string]$Value)
  $safe = $Value -replace '[^A-Za-z0-9_\.]', '_'
  if ($safe -match '^[0-9]') {
    $safe = "_$safe"
  }
  return $safe
}

function Get-InstallOnlyCondition {
  param([string]$UserCondition)

  if ([string]::IsNullOrWhiteSpace($UserCondition)) {
    return 'NOT REMOVE'
  }

  return "(NOT REMOVE) AND ($UserCondition)"
}

$lines = @(
  '<?xml version="1.0" encoding="utf-8"?>',
  '<Include xmlns="http://wixtoolset.org/schemas/v4/wxs">'
)

$sequenceHookRows = @()

if ($IncludeCancellationActions) {
  $lines += "  <Binary Id=`"WiXCraft_CustomActionsBinary`" SourceFile=`"`$(var.$CancellationProjectName.TargetDir)$CancellationProjectName.CA.dll`" />"
  $lines += ""
  $lines += "  <CustomAction Id=`"WiXCraft_SetCheckEmbeddedUICancellationData`" Property=`"WiXCraft_CheckEmbeddedUICancellation`" Value=`"EMBEDDEDUICANCELLATIONMUTEXNAME=[EMBEDDEDUICANCELLATIONMUTEXNAME]`" />"
  $lines += "  <CustomAction Id=`"WiXCraft_CheckEmbeddedUICancellation`" BinaryRef=`"WiXCraft_CustomActionsBinary`" DllEntry=`"CheckEmbeddedUICancellation`" Execute=`"deferred`" Return=`"check`" Impersonate=`"no`" />"
  $lines += "  <CustomAction Id=`"WiXCraft_SetApplySessionPropertiesData`" Property=`"WiXCraft_ApplySessionProperties`" Value=`"WIXCRAFT_STUB=1`" />"
  $lines += "  <CustomAction Id=`"WiXCraft_ApplySessionProperties`" BinaryRef=`"WiXCraft_CustomActionsBinary`" DllEntry=`"ApplySessionProperties`" Execute=`"deferred`" Return=`"ignore`" Impersonate=`"no`" />"
  $lines += "  <CustomAction Id=`"WiXCraft_SequenceHook`" BinaryRef=`"WiXCraft_CustomActionsBinary`" DllEntry=`"SequenceHook`" Execute=`"immediate`" Return=`"check`" />"
  $lines += ""

  if ($SequenceHooksFile -and (Test-Path -LiteralPath $SequenceHooksFile)) {
    $hookLines = Get-Content -LiteralPath $SequenceHooksFile | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($hookLine in $hookLines) {
      $parts = $hookLine.Split('|')
      if ($parts.Count -lt 1) { continue }

      $hookId = $parts[0].Trim()
      if ([string]::IsNullOrWhiteSpace($hookId)) { continue }

      $before = if ($parts.Count -gt 1) { $parts[1].Trim() } else { "" }
      $after = if ($parts.Count -gt 2) { $parts[2].Trim() } else { "" }
      $condition = if ($parts.Count -gt 3) { $parts[3].Trim() } else { "" }
      $payload = if ($parts.Count -gt 4) { $parts[4].Trim() } else { "" }
      $buttons = if ($parts.Count -gt 5) { $parts[5].Trim() } else { "OKCancel" }
      $hookCondition = Get-InstallOnlyCondition $condition

      $safeId = Get-SafeWixId $hookId
      $setDataId = "WiXCraft_SetSequenceHookData_$safeId"

      $dataValue = "HOOKID=$hookId"
      if (-not [string]::IsNullOrWhiteSpace($payload)) {
        $dataValue += ";PAYLOAD=$payload"
      }
      if (-not [string]::IsNullOrWhiteSpace($buttons)) {
        $dataValue += ";BUTTONS=$buttons"
      }

      $lines += "  <CustomAction Id=`"$setDataId`" Property=`"WiXCraft_SequenceHook`" Value=`"$dataValue`" />"
      $lines += ""

      $setDataRow = "    <Custom Action=`"$setDataId`" Before=`"WiXCraft_SequenceHook`" Condition=`"$hookCondition`" />"
      $sequenceHookRows += $setDataRow

      $invokeRow = "    <Custom Action=`"WiXCraft_SequenceHook`""
      if (-not [string]::IsNullOrWhiteSpace($before)) {
        $invokeRow += " Before=`"$before`""
      }
      elseif (-not [string]::IsNullOrWhiteSpace($after)) {
        $invokeRow += " After=`"$after`""
      }
      else {
        $invokeRow += " Before=`"InstallFinalize`""
      }

      $invokeRow += " Condition=`"$hookCondition`" />"
      $sequenceHookRows += $invokeRow
    }
  }

  $lines += "  <InstallExecuteSequence>"
  $lines += "    <Custom Action=`"WiXCraft_SetCheckEmbeddedUICancellationData`" Before=`"WiXCraft_CheckEmbeddedUICancellation`" Condition=`"EMBEDDEDUICANCELLATIONMUTEXNAME&lt;&gt;&quot;&quot;`" />"
  $lines += "    <Custom Action=`"WiXCraft_CheckEmbeddedUICancellation`" Before=`"InstallFinalize`" Condition=`"EMBEDDEDUICANCELLATIONMUTEXNAME&lt;&gt;&quot;&quot;`" />"
  $lines += "    <Custom Action=`"WiXCraft_SetApplySessionPropertiesData`" Before=`"WiXCraft_ApplySessionProperties`" Condition=`"NOT REMOVE`" />"
  $lines += "    <Custom Action=`"WiXCraft_ApplySessionProperties`" Before=`"InstallFinalize`" Condition=`"NOT REMOVE`" />"

  foreach ($row in $sequenceHookRows) {
    $lines += $row
  }

  $lines += "  </InstallExecuteSequence>"
  $lines += ""
}

if ($IncludeEmbeddedUi) {
  $lines += "  <UI>"
  $lines += "    <EmbeddedUI Id=`"WiXCraft_EmbeddedUI`" SourceFile=`"`$(var.$UiProjectName.TargetDir)$UiProjectName.CA.dll`" />"
  $lines += "  </UI>"
}

$lines += "</Include>"

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllLines($OutputPath, $lines, $utf8NoBom)
