using System.Diagnostics;
using TNO.Core.Extensions;

namespace TNO.DAL.Helpers;

/// <summary>
/// FfmpegHelper static class, provides helper methods to run ffmpeg commands.
/// </summary>
public static class FfmpegHelper
{
    #region methods
    /// <summary>
    /// Run the ffmpeg command and create a clip from the file at the specified 'path'.
    /// </summary>
    /// <param name="path">Full path to the source file.</param>
    /// <param name="start">Starting position in file.</param>
    /// <param name="end">Ending position in file.</param>
    /// <param name="outputName">The name of the file to create.</param>
    /// <returns>System.Diagnostics.Process</returns>
    public static async Task<string> CreateClipAsync(string path, int start, int end, string outputName)
    {
        var (Cmd, Output) = GenerateClipCommand(path, start, end, outputName);
        var process = CreateProcess(Cmd, "clip");
        var code = await RunProcessAsync(process);
        // TODO: propogate exception properly.
        if (code != 0) throw new Exception("An unexpected error occurred while joining clips");
        return Output;
    }

    /// <summary>
    /// Run the ffmpeg command and create a new clip by joining all clips matching the prefix and source file extension type.
    /// </summary>
    /// <param name="path">Full path to the source file.</param>
    /// <param name="prefix">Filename prefix to search for files to join.</param>
    /// <returns></returns>
    public static async Task<string> JoinClipsAsync(string path, string prefix)
    {
        var muxfile = GenerateMuxfile(path, prefix);
        var (Cmd, Output) = GenerateJoinCommand(path, muxfile, prefix);
        var process = CreateProcess(Cmd, "join");
        // TODO: A failure will not clean up the mux file.
        var code = await RunProcessAsync(process);
        File.Delete(muxfile);
        // TODO: propogate exception properly.
        if (code != 0) throw new Exception("An unexpected error occurred while joining clips");
        return Output;
    }

    /// <summary>
    /// Generate the command for the clip process.
    /// </summary>
    /// <param name="path">Full path to the source file.</param>
    /// <param name="start">Starting position in file.</param>
    /// <param name="end">Ending position in file.</param>
    /// <param name="outputName">The name of the file to create.</param>
    /// <returns></returns>
    private static (string Cmd, string Output) GenerateClipCommand(string path, int start, int end, string outputName)
    {
        var directory = Path.GetDirectoryName(path) ?? "";
        var ext = Path.GetExtension(path).Replace(".", "");
        var duration = end - start;

        // Ensures we keep creating new files and never overwrite.
        var count = Directory.GetFiles(directory, $"{outputName}_*.{ext}").Length;
        var output = $"{Path.Combine(directory, outputName)}_{count + 1}.{ext}";

        return ($"ffmpeg -ss {start} -t {duration} -i {path} -c:a copy {output}", output);
    }

    /// <summary>
    /// Generate the command for the clip process.
    /// </summary>
    /// <param name="path">Full path to the source file.</param>
    /// <param name="muxFile"></param>
    /// <param name="prefix">Filename prefix to search for files to join.</param>
    /// <returns></returns>
    private static (string Cmd, string Output) GenerateJoinCommand(string path, string muxFile, string prefix)
    {
        var ext = Path.GetExtension(path).Replace(".", "");
        var directory = Path.GetDirectoryName(path) ?? "";
        var output = Path.Combine(directory, $"{prefix}-final.{ext}");

        return ($"ffmpeg -f concat -safe 0 -i {muxFile} {output}", output);
    }

    /// <summary>
    /// Create a muxfile that joins multiple clips into a snippet.
    /// </summary>
    /// <param name="path">Full path to the source file.</param>
    /// <param name="prefix">Filename prefix to search for files to join.</param>
    /// <returns></returns>
    private static string GenerateMuxfile(string path, string prefix)
    {
        var ext = Path.GetExtension(path).Replace(".", "");
        var directory = Path.GetDirectoryName(path) ?? "";
        var clips = Directory.GetFileSystemEntries(directory, prefix + "_*." + ext);
        // TODO: Two users with same prefix will overwrite each other.
        var muxfile = Path.Combine(directory, "_tmp", $"{prefix}.txt");
        var muxpath = Path.GetDirectoryName(muxfile) ?? "";
        if (!muxpath.DirectoryExists()) Directory.CreateDirectory(muxpath);

        Array.Sort(clips, string.CompareOrdinal);
        using (var sw = File.CreateText(muxfile))
        {
            foreach (var filename in clips)
            {
                sw.WriteLine("file " + filename);
            }
            sw.Close();
        }
        return muxfile;
    }

    /// <summary>
    /// Create the process for the specified clip command.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="verb"></param>
    /// <returns></returns>
    private static Process CreateProcess(string cmd, string verb)
    {
        var process = new Process();
        process.StartInfo.Verb = verb;
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"{cmd}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.EnableRaisingEvents = true;
        return process;
    }

    /// <summary>
    /// Run the specified process asynchronously.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    private static Task<int> RunProcessAsync(Process process)
    {
        var tcs = new TaskCompletionSource<int>();

        process.Exited += (sender, args) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();

        return tcs.Task;
    }
    #endregion
}
