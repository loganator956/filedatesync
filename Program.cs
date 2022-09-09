using System.Diagnostics;

namespace filedatesync
{
    internal class Program
    {
        public enum Mode
        {
            Unset, Earliest, Modify, Create
        }

        static Mode operatingMode = Mode.Unset;

        static void Main(string[] args)
        {
            Version version = new Version(1,1,1,0);
            Console.WriteLine($"filedatesync Version {version.ToString()}");
            try
            {
                if (args.Length == 0)
                {
                    throw new ArgumentException("Not enough arguments");
                }
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        default:
                            if (i != args.Length-1)
                            {
                                throw new ArgumentException($"Argument not recognised \"{args[i]}\"");
                            }
                            break;
                        case "-h":
                        case "--help":
                            ShowHelpPage();
                            return;
                        case "-e":
                        case "--earliest":
                            operatingMode = Mode.Earliest;
                            break;
                        case "-m":
                        case "--modify":
                            operatingMode = Mode.Modify;
                            break;
                        case "-c":
                        case "--create":
                            operatingMode = Mode.Create;
                            break;
                    }
                }
                if (operatingMode == Mode.Unset)
                {
                    Console.WriteLine("Operating mode not set, assuming \"earliest\"");
                    operatingMode = Mode.Earliest;
                }
                ProcessFilesRecursive(args[args.Length - 1]);
            }
            catch (Exception except)
            {
                ConsoleColor prevColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(except.GetType().ToString() + ": " + except.Message);
                Console.ForegroundColor = prevColour;
            }
        }

        static void ProcessFilesRecursive(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(directory);
            }

            string[] files = Directory.GetFiles(directory);
            foreach(string file in files)
            {
                ProcessFile(file);
            }
            string[] subDirs = Directory.GetDirectories(directory);
            foreach(string subdir in subDirs)
            {
                ProcessFilesRecursive(subdir);
            }
        }

        static void ProcessFile(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            DateTime? newDateTime = null;
            switch(operatingMode)
            {
                case Mode.Earliest:
                case Mode.Unset:
                    DateTime a = fileInfo.CreationTime;
                    if (fileInfo.LastWriteTime < a) { a = fileInfo.LastWriteTime;};
                    newDateTime = a;
                    break;
                case Mode.Modify:
                    newDateTime = fileInfo.LastWriteTime;
                    break;
                case Mode.Create:
                    newDateTime = fileInfo.CreationTime;
                    break;
            }
            if (newDateTime.HasValue)
            {
                Console.WriteLine($"{newDateTime.ToString()} {file}");
                fileInfo.CreationTime = fileInfo.LastWriteTime = newDateTime.Value;
            }
            else
            {
                throw new NullReferenceException("newDateTime is null");
            }
        }

        static void ShowHelpPage()
        {
            Console.WriteLine("filedatesync help" +
                "\n Usage: filedatesync [arguments] [directory]" +
                "\n\nSome arguments will ignore further arguments (eg help)" +
                "\n\nArguments" +
                "\n -h --help\tShows this help page" +
                "\n -e --earliest\tUse the earliest date time (out of create and modify) (DEFAULT)" +
                "\n -m --modify\tUse the last modifed/write time" +
                "\n -c --create\tUse the creation time");
        }
    }
}