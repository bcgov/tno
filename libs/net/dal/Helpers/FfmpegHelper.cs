namespace TNO.DAL.Helpers;
public class FfmpegHelper
{
    /// <summary>
    /// Get the process for the specified clip command.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="clipNbr"></param>
    /// <param name="prefix"></param>
    /// <returns>System.Diagnostics.Process</returns>
    public static System.Diagnostics.Process GetClipProcess(string fileName, string directory, int start, int end, int clipNbr, string prefix)
    {
        var cmd = GenerateClipCommand(fileName, directory, start, end, clipNbr, prefix);
        var process = new System.Diagnostics.Process();
        process = InitializeProcess(process, cmd, "clip");

        return process;
    }

    /// <summary>
    /// Generate the command for the clip process.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="clipNbr"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static string GenerateClipCommand(string fileName, string directory, int start, int end, int clipNbr, string prefix)
    {
        var format = Path.GetExtension(fileName).Replace(".", "");
        var duration = end - start;
        var input = directory + "/" + fileName;
        var output = directory + "/" + prefix + "_" + clipNbr + "." + format;
        var otherArgs = "-ab 64k -ar 22050 -vol 400";

        return $"ffmpeg -ss {start} -f {format} -t {duration} -i {input} {otherArgs} -y {output}";
    }

    /// <summary>
    /// Get the process for the specified clip command.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <param name="muxFile"></param>
    /// <param name="format"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static System.Diagnostics.Process GetJoinProcess(string directory, string fileName, string path, string muxFile, string format, string prefix)
    {
        var cmd = GenerateJoinCommand(directory, fileName, path, muxFile, format, prefix);
        var process = new System.Diagnostics.Process();
        process = InitializeProcess(process, cmd, "join");

        return process;
    }

    /// <summary>
    /// Get the process for the specified clip command.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cmd"></param>
    /// <param name="verb"></param>
    /// <returns></returns>
    public static System.Diagnostics.Process InitializeProcess(System.Diagnostics.Process process, string cmd, string verb)
    {
        process.StartInfo.Verb = verb;
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"{cmd}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.EnableRaisingEvents = true;

        return process;
    }

    /// <summary>
    /// Generate the command for the clip process.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <param name="muxFile"></param>
    /// <param name="format"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static string GenerateJoinCommand(string directory, string fileName, string path, string muxFile, string format, string prefix)
    {
        var input = directory + "/" + fileName;
        var output = path + "/" + prefix + "-snippet." + format;

        return $"ffmpeg -f concat -safe 0 -i {muxFile} {output}";
    }

    /// <summary>
    /// Create a muxfile that joins multiple clips into a snippet.
    /// </summary>
    /// <param name="listing"></param>
    /// <param name="fileName"></param>
    /// <param name="format"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static string GenerateMuxfile(string fileName, string safepath, string format, string prefix)
    {
        var clips = System.IO.Directory.GetFileSystemEntries(safepath, prefix + "_*." + format);
        var path = "/tmp/" + prefix + ".txt";
        Array.Sort(clips, string.CompareOrdinal);
        var sw = System.IO.File.CreateText(path);

        foreach (string file in clips)
        {
            sw.WriteLine("file " + file);
        }

        sw.Close();

        return path;
    }
}