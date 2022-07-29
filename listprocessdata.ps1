get-wmiobject Win32_PerfFormattedData_PerfProc_Process -Filter "Name LIKE 'python#%'"  | Select-Object -Property IDProcess,Name,IODataBytesPersec,IOOtherBytesPersec,IOReadBytesPersec,IOWriteBytesPersec,ElapsedTime,PercentProcessorTime,ThreadCount | ConvertTo-Json