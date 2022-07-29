using System.Diagnostics;
using System.Text;

class ProcessService
{
    List<WMIObject> processes = new List<WMIObject>();
    StringBuilder stringBuilder = new StringBuilder();
    private readonly IMessageWriter messageWriter;

    public ProcessService(IMessageWriter messageWriter)
    {
        this.messageWriter = messageWriter;
    }
 
    public Process? CreateWMIProcess()
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = ".\\listprocessdata.ps1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });
    }

    public Process? KillPid(int pid)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "tskill.exe",
            Arguments = pid.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });
    }
}
