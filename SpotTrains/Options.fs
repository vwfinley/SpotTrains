module Options

open CommandLine
open CommandLine.Text

[<Literal>]
let defaultUrl = @"http://www3.septa.org/api/TrainView";
[<Literal>]
let defaultDbPath = @"%userprofile%\appdata\local\SpotTrains\trains.db";
[<Literal>]
let defaultPeriod = 30000;

// https://dotnetfiddle.net/3kKKS4
// https://github.com/commandlineparser/commandline
type options = {
    [<Option(Default = defaultUrl, HelpText = "Url of a train status RESTful api.")>]
        url : string;
    [<Option(Default = defaultDbPath, HelpText = "Path to local SQLite database. One will be created if it doesn't exist.")>]
        dbpath : string;
    [<Option(Default = defaultPeriod, HelpText = "Update period in milliseconds. If period is 0, only a single set of records will be stored.")>]
        period : int;
    } with
        [<Usage>]
        static member examples
            with get() = seq {
                yield Example("Periodically fetches train status records and stores them in a local database", {url = "<url>"; dbpath = "<dbpath>"; period = defaultPeriod });
                    yield Example("Periodically fetches train status records and stores them in a local database", {url = defaultUrl; dbpath = defaultDbPath; period = defaultPeriod })}
