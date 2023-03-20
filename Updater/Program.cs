
using System.Diagnostics;
using System.IO.Compression;

try
{
    int proc_id = int.Parse(args[0]);

    if (proc_id > 0)
    {
        var process = Process.GetProcessById(proc_id);

        process.WaitForExit();
    }

    using (ZipArchive archive = ZipFile.Open(args[1], ZipArchiveMode.Update))
    {
        var entries = archive.Entries.Where(entry => entry.Name != AppDomain.CurrentDomain.FriendlyName);

        archive.GetEntry("Updater.exe")?.Delete();

        archive.ExtractToDirectory("./", true);
    }

    File.Delete(args[1]);

    Process.Start("Broadifyer");
}
catch(Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadLine();

    throw;
}