param(
    [switch]$start = $false,
    [switch]$stop = $false,
    [switch]$restart = $false
)
$pidfile = 'pidfile.txt'
$processfile = 'processes.txt'
$processes = Get-Content $processfile

If($processes[0].StartsWith("#EXECUTABLES"))
{
    If(Test-Path $pidfile)
    {
        If($stop -Or $restart)
        {
            $pids = Get-Content $pidfile
            foreach($id in $pids)
            {
                Stop-Process $id
            }
            Remove-Item $pidfile
        }
    }
    
    If($start -Or $restart)
    {
        $pids = @()
        foreach($process in $processes)
        {
            If(-Not $process.StartsWith("#"))
            {
                $p = Start-Process -FilePath $process -PassThru
                $pids += $p.Id
            }
        }
        $pids | out-file $pidfile
    }
}
ElseIf($processes[0].StartsWith("#SERVICES"))
{
    If($stop -Or $restart)
    {
        foreach($process in $processes)
        {
            If(-Not $process.StartsWith("#"))
            {
                net stop $process
            }
        }
    }
    
    If($start -Or $restart)
    {
        foreach($process in $processes)
        {
            If(-Not $process.StartsWith("#"))
            {
                net start $process
            }
        }
    }
}
Else 
{
    Write-Host "processes.txt is corrupt" -ForegroundColor "Red"
}

$host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")