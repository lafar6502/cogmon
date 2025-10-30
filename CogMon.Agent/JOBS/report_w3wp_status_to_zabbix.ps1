param(
    [switch]$dbg = $false,
    [string]$Server = "127.0.0.1",
    [string]$ProcName = "w3wp"
)
$pses = Get-Process -Name $ProcName -IncludeUserName | Select-Object -Property Name, Cpu, UserName, WorkingSet64
$machine = [Environment]::MachineName


$zabfile = ""
$lines = 0

foreach($p in $pses) {
    $uname = $p.UserName
    if ([string]::IsNullOrEmpty($uname)) {
        continue;
    }
    $ix = $uname.lastIndexOf('\')
    
    if ($ix -gt  0) {
        $uname = $uname.Substring($ix+1)
    }
    $uname = $uname.Replace(' ', '_')
    $mem = ($p.WorkingSet64 / (1024*1024))
	Write-Host -NoNewline "mem ", $p.WorkingSet64, " - ", $mem, "`r`n"
    $zabfile += "$machine  cpu_$($p.Name)_$uname  $($p.CPU)`r`n" 
    $zabfile += "$machine  workset_$($p.Name)_$uname  $mem`r`n" 
    $lines++
}
Write-Output $machine

$tpfile = New-TemporaryFile

$zabfile | Out-File -FilePath $tpfile.FullName -Encoding ascii -NoNewLine

#$pcontent = Get-Content -Path $tpfile.FullName
#Write-Host $tpfile.FullName
#Write-Host $pcontent
Write-Host "Collected $lines records"
Write-Host -NoNewline "Starting $PSScriptRoot\zabbix_sender.exe", $Server, "input file is $($tpfile.FullName)`r`n"

Start-Process -FilePath "$PSScriptRoot\zabbix_sender.exe" -Wait -RedirectStandardOutput "$PSScriptRoot\out1.txt"  -ArgumentList "-z", $Server, "-v", "-i", $tpfile.FullName
 #ConvertTo-Json $pses | Write-Output 

if($dbg) {
    Write-Host "--- zabbix_sender process out ---"
    Get-Content -Path "$PSScriptRoot\out1.txt" | Write-Host
    Write-Host "--- data for zabbix ----"
    Get-Content -Path $tpfile.FullName | Write-Host
}

$tpfile.Delete()  
