//fib bundle .....
using System.CommandLine;
var PossibleExtentions = new string[] { "cs", "css", "cpp", "h", "c", "py", "java", "js", "ts", "html", "sql" };
var excludedDirectories = new string[] { "bin", "debug", ".vs", "obj", "venv", ".idea" };

var bundleCommand = new Command("bundle", "Bundle code files to single files");
var bundleOption = new Option<string>("--output", "file path and name");
bundleOption.AddAlias("-o");
var languageOption = new Option<string>("--language", "List of programming languages or \"all\". Required.");
languageOption.AddAlias("-l");
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines.");
removeEmptyLinesOption.AddAlias("-r");
var noteOption = new Option<bool>("--note", "Include source note.");
noteOption.AddAlias("-n");
var sortOption = new Option<string>("--sort", "Sort order name or type. Default is by name.");
sortOption.AddAlias("-s");
var authorOption = new Option<string>("--author", "Author's name.");
authorOption.AddAlias("-a");



bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(authorOption);

var rootCommand = new RootCommand("Root command for file bundler CLI");
rootCommand.AddCommand(bundleCommand);

bundleCommand.SetHandler((output, language, sort, note, removeEmptyLines, author) =>
{
    try
    {
        //File.Create(output.FullName).Close();
        if (output == null)
        {
            Console.WriteLine("the output option is requiered!");
            return;
        }
        if (!output.Contains("\\"))
        {
            output = Directory.GetCurrentDirectory() + "\\" + output;
        }
        File.Create(output).Close();

        //נפלטר את הסיומות שהמשתמש הכניס או את האופציה all
        List<string> languages = language.Split(' ').ToList();

        if (languages == null)
        {
            Console.WriteLine("the language option is requiered!");
            return;
        }
        //משאיר רק שפות הקיימות במערך השפות
        languages = languages.Where(language => PossibleExtentions.Contains(language)).ToList();

        //מוצא את כל הקבצים שבתקיה 
        //אח"כ עובר עליהם ומשאיר רק קבצים שנבחרו ונמצאים במערך languages או את כולם במקרה של all ושומר
        string[] codeFiles = Directory.GetFiles(".", "*.*", SearchOption.AllDirectories)
        .Where(file => languages.Contains("all") || languages.Any(lang => file.EndsWith($".{lang}")))
        .ToArray();

        // bin, debugמוציא את כל קבצי הקוד בתיקיות
        codeFiles = codeFiles.Where(file =>
        !excludedDirectories.Any(dir => file.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar)))
         .ToArray();

        if (codeFiles.Length == 0)
        {
            Console.WriteLine("Error: No code files found for the specified language/s.");
            return;
        }
        codeFiles = codeFiles.OrderBy(file => Path.GetFileName(file)).ToArray();

        if (!string.IsNullOrEmpty(sort) && sort.ToLower() == "type")
        {
            codeFiles = codeFiles.OrderBy(file => Path.GetExtension(file)).ToArray();
        }

        string bundleContent = "";
        string codeFileHelp = "";
        foreach (string codeFile in codeFiles)
        {
            codeFileHelp = File.ReadAllText(codeFile);
            if (removeEmptyLines)
            {
                codeFileHelp = string.Join("", codeFileHelp.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line))); ;
            }

            bundleContent += codeFileHelp + Environment.NewLine;

            if (note)
            {
                bundleContent += Environment.NewLine + $"# Source: {codeFileHelp}" + Environment.NewLine;
            }

        }
        if (!string.IsNullOrEmpty(author))
        {
            bundleContent = $"Author: {author}" + Environment.NewLine + bundleContent;
        }
        try
        {

            using (StreamWriter writer = File.AppendText(output))
            {
                writer.WriteLine(bundleContent);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message} - failed to bundle");
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("files bundle succesfully!!!!");
        Console.ForegroundColor = ConsoleColor.White;
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: File path is invalid");
        Console.ForegroundColor = ConsoleColor.White;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message} failed to bundle");
        Console.ForegroundColor = ConsoleColor.White;

    }

    var inputLanguages = language.Split(',').Select(lang => lang.Trim().ToLower()).ToList();



}, bundleOption, languageOption, sortOption, noteOption, removeEmptyLinesOption, authorOption);

//************ create-rsp ******************
//fib create-rsp
//fib bundle name-rsp
var rsp = new Command("create-rsp", "create response file for bundle command");
rootCommand.AddCommand(rsp);
rsp.SetHandler(() =>
{
    Console.WriteLine("Enter name for the response file: (name.rsp)");
    string fullPath = Directory.GetCurrentDirectory() + "\\" + Console.ReadLine();
    try
    {
        File.Create(fullPath).Close();
        using (StreamWriter writer = new StreamWriter(fullPath))
        {
            Console.Write("--output " + " " + "File path and name: ");
            writer.WriteLine($"--output {Console.ReadLine()}");

            Console.Write("--language " + " " + "Programming languages to bundle: ");
            writer.WriteLine($"--language {Console.ReadLine()}");

            Console.Write("--sort " + " " + "Sort order (name or type): ");
            writer.WriteLine($"--sort {Console.ReadLine()}");

            Console.Write("--remove-empty-lines " + " " + "Remove empty lines (true or false): ");
            writer.WriteLine($"--remove-empty-lines {Console.ReadLine()}");

            Console.Write("--note " + " " + "Include source note (true or false): ");
            writer.WriteLine($"--note {Console.ReadLine()}");

            Console.Write("--author " + " " + "Author's name: ");
            writer.WriteLine($"--author {Console.ReadLine()}");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message} ");
        Console.ForegroundColor = ConsoleColor.White;
    }
});


rootCommand.InvokeAsync(args);


