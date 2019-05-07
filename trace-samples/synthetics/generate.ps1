function Trace-Script-File {
    param([string]$file)
    Write-Output "Generating trace for $file"
    $outDir = Join-Path (Get-Location) ".."
    if ([System.IO.Path]::IsPathRooted($file)) {
        $phpFile = $file
    }
    else {
        $phpFile = Join-Path (Get-Location) $file
    }
    $traceFile = (Split-Path $file -leaf).Replace(".php", "")
    php -d xdebug.auto_trace=1 -d xdebug.trace_output_dir=$outDir -d xdebug.trace_output_name=$traceFile $phpFile
}
if ($args.Length -eq 0) {
    Write-Error "You have to either specify a list of files or 'all' as an argument"
}
if ($args[0] -eq "all") {
    foreach ($file in (Get-ChildItem *.php)) {
        Trace-Script-File $file
    }
}
else {
    foreach ($file in $args) {
        Trace-Script-File $file
    }
}