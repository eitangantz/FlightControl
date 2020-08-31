# FlightControl

web application - full stack app - Restful api - the app displays active flights known to our server on a google maps's map.
the user can add flights to our server by drag and drop flight plans as json's files,
or delete active flights, the server updates accordingly.
the client asks every second for active flights using ajax.
the server calculates for each known flight if its now active relative to UTC(Coordinated Universal Time),
and if it is then calculates the current location of the plane using linear interpolation.
the client displays all active flights: each plane is represented with icon on the map at its current location,
pressing the icon shows the full path of the flight and extra info on the flight.


example to flight plan(json):
{
  "passengers": 101,
  "company_name": "SwissAir",
  "initial_location": {
    "longitude": 20.0,
    "latitude": 30.2,
    "date_time": "2020-12-27T01:56:21Z"
  },
  "segments": [
    {
      "longitude": 33.23,
      "latitude": 31.56,
      "timespan_seconds": 850.0
    }
  ]
}

to use the app download all the code and then click on the exe file found here: out\FlightControlWeb.exe
also make some jsons as above and drop them to the drop area (note:the time should be relative to UTC.)
