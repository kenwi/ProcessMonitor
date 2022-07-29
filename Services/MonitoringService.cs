using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

class MonitoringService : IHostedService
{
    readonly IMessageWriter messageWriter;
    readonly ProcessService processService;

    int tick = 0;
    readonly int tickInterval = 5, ticksPerUpdate = 15, warningLevels = 5;
    readonly Dictionary<int, long> writtenBytesData = new();
    readonly Dictionary<int, long> previousData = new();
    readonly Dictionary<int, long> ageData = new();
    readonly List<int> warnings = new();
    readonly StringBuilder output = new();

    public MonitoringService(
        IMessageWriter messageWriter,
        ProcessService processService)
    {
        this.messageWriter = messageWriter;
        this.processService = processService;
        messageWriter.Write("MonitoringService Started");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(tickInterval));
        await Task.Delay(0, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
    }

    public void DoWork(object? state)
    {
        using var process = processService.CreateWMIProcess();
        if (process is null)
        {
            messageWriter.Write("Unable to start WMI process");
            return;
        }

        process.OutputDataReceived += OutputDataReceived;
        process.ErrorDataReceived += (s, e) =>
        {

        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        messageWriter.Write($"Tick: {tick++} TicksPerUpdate: {ticksPerUpdate}");
    }

    private void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
        {
            output.AppendLine(e.Data);
            return;
        }

        Console.ResetColor();
        Console.Clear();

        var json = output.ToString();
        var processes = new List<WMIObject>();
        try
        {
            var elements = JsonConvert.DeserializeObject<List<WMIObject>>(json);
            if (elements is not null)
                processes = elements.Where(p => p.Name is not null && p.Name.Contains("python#"))
                    .ToList();
        }
        catch (Exception)
        {
            messageWriter.Write("Unable to parse WMI output");
            output.Clear();
            return;
        }

        ListProcesses(processes);
        UpdateProcessData();

        output.Clear();
    }

    private void UpdateProcessData()
    {
        if (tick <= ticksPerUpdate)
            return;

        foreach (var pid in writtenBytesData.Keys)
        {
            if (writtenBytesData[pid] == 0)
            {
                warnings.Add(pid);
            }
        }
        tick = 0;
        writtenBytesData.Clear();
    }

    private void ListProcesses(List<WMIObject>? processes)
    {
        if (processes is null)
        {
            messageWriter.Write("Unable to find processes");
            return;
        }

        foreach (var process in processes)
        {
            var pid = process.IDProcess ?? 0;
            var writtenBytes = process.IOWriteBytesPersec ?? 0;
            var age = process.ElapsedTime ?? 0;

            writtenBytesData.TryAdd(pid, writtenBytes);
            writtenBytesData.TryGetValue(pid, out long totalWrittenBytes);

            var line = new StringBuilder($"{process.Name} " +
                $"PID:{process.IDProcess} " +
                $"Age:{process.ElapsedTime}s " +
                $"CPU:{process.PercentProcessorTime}% " +
                $"Threads:{process.ThreadCount}] " +
                $"[{writtenBytes}/{totalWrittenBytes}]");

            if (warnings.Contains(pid))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (totalWrittenBytes > 0)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.Black;
                    writtenBytesData[pid] = 0;
                    warnings.Remove(pid);
                }

                var warningCount = warnings.Count(p => p == pid);
                for (int labels = 0; labels < warningCount; line.Append('!'), labels++) ;

                if (warningCount >= warningLevels - 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                if ((warningCount >= warningLevels && process.ThreadCount <= 2) || process.ElapsedTime > 10000)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.Black;
                    processService.KillPid(pid);
                    warnings.RemoveAll(p => p == pid);
                }
            }
            messageWriter.Write(line.ToString());
            writtenBytesData[pid] += writtenBytes;
            Console.ResetColor();
        }
    }
}
