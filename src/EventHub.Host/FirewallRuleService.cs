using System.ComponentModel;
using System.Diagnostics;

namespace EventHub.Host;

internal sealed class FirewallRuleService
{
    public async Task EnsureInboundRuleAsync(string publicBaseUrl, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttp ||
            uri.Port is <= 0 or > 65535)
        {
            throw new InvalidOperationException("無法從 LAN 網址判斷 Firewall Port。");
        }

        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Tools",
            "Configure-EventHubFirewall.ps1");
        if (!File.Exists(scriptPath))
        {
            throw new InvalidOperationException($"找不到 Firewall 設定工具：{scriptPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("-Port");
        startInfo.ArgumentList.Add(uri.Port.ToString());

        try
        {
            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("無法啟動 Firewall 設定工具。");
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Firewall 設定未完成（Exit Code {process.ExitCode}）。");
            }
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            throw new InvalidOperationException("已取消 Windows 系統管理員授權。", exception);
        }
    }
}
