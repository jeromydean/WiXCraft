param(
  [string]$UiProjectName = "",
  [Parameter(Mandatory = $true)]
  [string]$CancellationProjectName,
  [Parameter(Mandatory = $true)]
  [string]$OutputPath,
  [switch]$IncludeCancellationActions,
  [switch]$IncludeEmbeddedUi
)

$lines = @(
  '<?xml version="1.0" encoding="utf-8"?>',
  '<Include xmlns="http://wixtoolset.org/schemas/v4/wxs">'
)

if ($IncludeCancellationActions) {
  $lines += "  <Binary Id=`"WiXCraft_CustomActionsBinary`" SourceFile=`"`$(var.$CancellationProjectName.TargetDir)$CancellationProjectName.CA.dll`" />"
  $lines += ""
  $lines += "  <CustomAction Id=`"WiXCraft_SetCheckEmbeddedUICancellationData`" Property=`"WiXCraft_CheckEmbeddedUICancellation`" Value=`"EMBEDDEDUICANCELLATIONMUTEXNAME=[EMBEDDEDUICANCELLATIONMUTEXNAME]`" />"
  $lines += ""
  $lines += "  <CustomAction Id=`"WiXCraft_CheckEmbeddedUICancellation`" BinaryRef=`"WiXCraft_CustomActionsBinary`" DllEntry=`"CheckEmbeddedUICancellation`" Execute=`"deferred`" Return=`"check`" Impersonate=`"no`" />"
  $lines += ""
  $lines += "  <InstallExecuteSequence>"
  $lines += "    <Custom Action=`"WiXCraft_SetCheckEmbeddedUICancellationData`" Before=`"WiXCraft_CheckEmbeddedUICancellation`" />"
  $lines += "    <Custom Action=`"WiXCraft_CheckEmbeddedUICancellation`" Before=`"InstallFinalize`" />"
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
