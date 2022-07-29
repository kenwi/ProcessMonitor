public class WMIObject
{
    public string? Name { get; set; }
    public int? IDProcess { get; set; }
    public long? IODataBytesPersec { get; set; }
    public long? IOOtherBytesPersec { get; set; }
    public long? IOReadBytesPersec { get; set; }
    public long? IOWriteBytesPersec { get; set; }
    public long? ElapsedTime { get; set; }
    public long? PercentProcessorTime { get; set; }
    public long? ThreadCount { get; set; }
}