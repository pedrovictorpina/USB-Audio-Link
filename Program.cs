using System.Diagnostics;
using System.IO.Compression;

namespace AudioCelularPC;

internal static class Program
{
    private const string ScrcpyVersion = "v3.3.1"; // ajustar automaticamente em futuro update
    private static readonly string WorkDir = Path.Combine(AppContext.BaseDirectory, "tools");
    private static readonly string ScrcpyZip = Path.Combine(WorkDir, $"scrcpy-win64-{ScrcpyVersion}.zip");
    private static readonly string ScrcpyDir = Path.Combine(WorkDir, $"scrcpy-win64-{ScrcpyVersion}");
    private static readonly string ScrcpyExe = Path.Combine(ScrcpyDir, "scrcpy.exe");

    private static async Task<int> Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==== Audio do Celular no PC (USB) ====");
        Console.WriteLine("Este utilitário baixa e executa o scrcpy para encaminhar áudio e vídeo.");
        Console.WriteLine();

        try
        {
            Directory.CreateDirectory(WorkDir);
            await EnsureScrcpyAsync();
            await EnsureAdbDeviceAsync();

            while (true)
            {
                var option = Menu();
                switch (option)
                {
                    case "1":
                        await StartAudioOnlyAsync();
                        break;
                    case "2":
                        await StartAudioAndMirrorAsync();
                        break;
                    case "3":
                        await StartMirrorNoAudioAsync();
                        break;
                    case "R":
                        Console.WriteLine("Recarregando detecção do dispositivo...");
                        await EnsureAdbDeviceAsync();
                        break;
                    case "S":
                        Console.WriteLine("Saindo...");
                        return 0;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
            return 1;
        }
    }

    private static string Menu()
    {
        Console.WriteLine();
        Console.WriteLine("Escolha o modo:");
        Console.WriteLine("  [1] Somente áudio (sem janela)");
        Console.WriteLine("  [2] Áudio + espelhamento de tela");
        Console.WriteLine("  [3] Somente espelhamento (sem áudio)");
        Console.WriteLine("  [R] Re-detectar dispositivo");
        Console.WriteLine("  [S] Sair");
        Console.Write("> ");
        return Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";
    }

    private static async Task EnsureScrcpyAsync()
    {
        if (File.Exists(ScrcpyExe))
        {
            Console.WriteLine($"scrcpy encontrado: {ScrcpyExe}");
            return;
        }

        var url = $"https://github.com/Genymobile/scrcpy/releases/download/{ScrcpyVersion}/scrcpy-win64-{ScrcpyVersion}.zip";
        Console.WriteLine($"Baixando scrcpy {ScrcpyVersion}...");
        await DownloadFileAsync(url, ScrcpyZip);

        Console.WriteLine("Extraindo...");
        ZipFile.ExtractToDirectory(ScrcpyZip, WorkDir, overwriteFiles: true);
        Console.WriteLine("OK!");
    }

    private static async Task EnsureAdbDeviceAsync()
    {
        Console.WriteLine("Verificando dispositivo via ADB...");
        var result = await RunProcessAsync(ScrcpyExe, "--version"); // warm-up

        // scrcpy traz adb embutido; vamos rodar 'adb devices'
        var adb = Path.Combine(ScrcpyDir, "adb.exe");
        var (exit, output, error) = await RunProcessAsync(adb, "devices");

        if (exit != 0)
            throw new Exception("ADB não pôde ser executado. Verifique permissões.");

        Console.WriteLine(output);
        if (!output.Split('\n').Any(l => l.Contains("\tdevice")))
        {
            Console.WriteLine("Nenhum dispositivo em modo 'device'. Dicas:");
            Console.WriteLine(" - Ative Opções do desenvolvedor > Depuração USB");
            Console.WriteLine(" - No telefone, aceite a impressão digital do PC quando solicitado");
            Console.WriteLine(" - Use cabo USB de dados e selecione MTP/Transf. Arquivos");
        }
    }

    private static async Task StartAudioOnlyAsync()
    {
        Console.WriteLine("Iniciando modo SOMENTE ÁUDIO...");
        // --no-video garante sem janela; --audio=alsa|... é auto; deixar padrão
        var args = "--no-video --no-control";
        await LaunchScrcpyInteractive(args);
    }

    private static async Task StartAudioAndMirrorAsync()
    {
        Console.WriteLine("Iniciando ÁUDIO + ESPELHAMENTO...");
        // janela padrão, com controle; áudio habilitado por padrão em Android 11+
        var args = ""; // usar defaults
        await LaunchScrcpyInteractive(args);
    }

    private static async Task StartMirrorNoAudioAsync()
    {
        Console.WriteLine("Iniciando SOMENTE ESPELHAMENTO (sem áudio)...");
        var args = "--no-audio";
        await LaunchScrcpyInteractive(args);
    }

    private static async Task LaunchScrcpyInteractive(string args)
    {
        Console.WriteLine("Pressione Ctrl+C para encerrar o scrcpy e voltar ao menu.");
        using var proc = new Process();
        proc.StartInfo.FileName = ScrcpyExe;
        proc.StartInfo.Arguments = args;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.CreateNoWindow = false;
        proc.OutputDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) Console.WriteLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) Console.Error.WriteLine(e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        await proc.WaitForExitAsync();
    }

    private static async Task DownloadFileAsync(string url, string destination)
    {
        using var http = new HttpClient();
        using var resp = await http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        await using var fs = File.Create(destination);
        await resp.Content.CopyToAsync(fs);
    }

    private static async Task<(int exitCode, string stdout, string stderr)> RunProcessAsync(string fileName, string arguments)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = fileName;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.CreateNoWindow = true;
        proc.Start();
        var stdOutTask = proc.StandardOutput.ReadToEndAsync();
        var stdErrTask = proc.StandardError.ReadToEndAsync();
        await Task.WhenAll(stdOutTask, stdErrTask, proc.WaitForExitAsync());
        return (proc.ExitCode, await stdOutTask, await stdErrTask);
    }
}
