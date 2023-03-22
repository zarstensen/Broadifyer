
using System.Diagnostics;
using System.IO.Compression;

try
{
    // retrieve the prog id, of the Broadifyer process that called updater.
    // use it to wait for the process to exit, as this program cannot modify the broadifyer exe, if broadifyer is currently running.
    int proc_id = int.Parse(args[0]);

    if (proc_id > 0)
    {
        var process = Process.GetProcessById(proc_id);

        process.WaitForExit();
    }

    // the downloaded release is in a zip format, however the updater must not be extacted, as it is currently being used,
    // so this is removed from the archive.

    using (ZipArchive archive = ZipFile.Open(args[1], ZipArchiveMode.Update))
    {
        var entries = archive.Entries.Where(entry => entry.Name != AppDomain.CurrentDomain.FriendlyName);

        archive.GetEntry("Updater.exe")?.Delete();

        archive.ExtractToDirectory("./", true);
    }

    // delete the downloaded release (args[1])

    File.Delete(args[1]);

    Process.Start("Broadifyer");
}
catch(Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadLine();

    throw;
}