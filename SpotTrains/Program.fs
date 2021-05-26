(*
    SpotTrains : Periodically fetches train status records and stores them in a local database

    Debugging Note #1: Configure commandline as follows...
        --url "http://www3.septa.org/api/TrainView"
        --dbpath "%userprofile%\appdata\local\SpotTrains\trains.db"
        --period 30000

    Debugging Note #2: Use "DB Browser for SQLite" to view the database. https://sqlitebrowser.org/dl/
*)

open System
open System.Threading
open System.Data.SQLite
open FSharp.Data
open CommandLine
open Options

let toUtc datetime = DateTime.Parse(datetime).ToUniversalTime()
let toIso8601 (datetime:DateTime) = datetime.ToString("yyyy-MM-dd HH:mm:ss.fff")
let toUtcIso = toUtc >> toIso8601

let connect path =
    let conn = new SQLiteConnection("Data Source=" + path)
    conn.Open()
    let command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS trains (
            id INTEGER PRIMARY KEY,
            utc TEXT NOT NULL,
            lat TEXT,
            lon TEXT,
            trainno TEXT,
            service TEXT,
            dest TEXT,
            currentstop TEXT,
            nextstop TEXT,
            line TEXT,
            consist TEXT,
            heading FLOAT,
            late INTEGER,
            SOURCE TEXT,
            TRACK TEXT,
            TRACK_CHANGE TEXT)", conn)
    command.ExecuteNonQuery() |> ignore
    conn

let toText httpResponseBody =
    match httpResponseBody with
    | Text txt -> txt
    | _ -> ""

let toCommand (datetime:string) (conn:SQLiteConnection) (record:JsonValue) =
    let props = ("utc", JsonValue.String datetime) :: (record |> JsonExtensions.Properties |> Array.toList)
    let names = props 
                |> List.map (fun prop -> fst prop)
                |> List.reduce (fun x y -> x + ",@" + y)
    let valueText = " (@" + names + ") "
    let nameText = valueText.Replace("@", "")

    let command = conn.CreateCommand()
    props |> List.iter (fun prop -> command.Parameters.AddWithValue("@" + fst prop, JsonExtensions.AsString (snd prop)) |> ignore)
    command.CommandText <- "INSERT INTO trains" + nameText + "VALUES" + valueText
    command

let fetch url db  =
    let response = Http.Request(url)
    let utc = response.Headers.Item "Date" |> toUtcIso 
    let toInsert = toCommand utc db
    response.Body
        |> toText
        |> JsonValue.Parse
        |> JsonExtensions.AsArray
        |> Array.map toInsert
        |> Array.map (fun command -> command.ExecuteNonQuery())
        |> Array.sum
        |> fun n -> eprintfn $"{utc}: inserted {n} records \"{url}\" --> \"{db.ConnectionString}\""

let loop url db period once =
    let event = new AutoResetEvent(false)
    let timer = new Timer(new TimerCallback (fun event -> 
                            fetch url db 
                            if once then (event :?> AutoResetEvent).Set() |> ignore),
                    event,
                    0,
                    period)

    event.WaitOne() |> ignore
    timer.Dispose()
    db.Close()
    0

let run (opts : options)  =
    let dbpath = Environment.ExpandEnvironmentVariables(opts.dbpath)
    let db = connect dbpath
    let period = if opts.period < 0 then 0 else opts.period |> int // period in milliseconds
    let once = period = 0
    loop opts.url db period once

[<EntryPoint>]
let main argv =
    let result = Parser.Default.ParseArguments<options>(argv)
    match result with
    | :? Parsed<options> as parsed -> run parsed.Value
    | _ -> 1

    
