# Keeps the PC awake (display may turn off) until this process ends or the time limit passes.
param([double]$Hours = 8)

Add-Type -Namespace Win32 -Name Power -MemberDefinition @'
[DllImport("kernel32.dll")]
public static extern uint SetThreadExecutionState(uint esFlags);
'@

$ES_CONTINUOUS = [uint32]'0x80000000'
$ES_SYSTEM_REQUIRED = [uint32]'0x00000001'

[Win32.Power]::SetThreadExecutionState($ES_CONTINUOUS -bor $ES_SYSTEM_REQUIRED) | Out-Null
try
{
    Start-Sleep -Seconds ([int]($Hours * 3600))
}
finally
{
    [Win32.Power]::SetThreadExecutionState($ES_CONTINUOUS) | Out-Null
}
