# SpotTrains
Periodically fetches train status records and stores them in a local database

## To launch from the Windows commandline: 
	SpotTrains

## Launching without arguments is equivalent to calling:
	SpotTrains --url "http://www3.septa.org/api/TrainView" --dbpath "%userprofile%\appdata\local\SpotTrains\trains.db" --period 30000

## To launch with help:
	SpotTrains --help

## To setup SpotTrains as an automatic task, see here:
https://www.windowscentral.com/how-create-automated-task-using-task-scheduler-windows-10
