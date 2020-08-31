

var map;
function initMap() {
    map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: 32.818944, lng: 13.050596 },
        zoom: 1
    });
}


var jsondrop = function (elem, options) {
    this.element = document.getElementById(elem);
    this.options = options || {};
    this.name = 'jsondrop';
    this.files = [];
    this._addEventHandlers();
}
jsondrop.prototype._readFiles = function (files) {
    var _this = this;
    for (i = 0; i < files.length; i++) {
        (function (file) {
            var fr = new FileReader();
            fr.readAsText(file, 'UTF-8');
            fr.onload = function () {
                var json = JSON.parse(fr.result);
                $.ajax({
                    type: "POST",
                    url: "api/FlightPlan",
                    data: JSON.stringify(json),
                    contentType: "application/json",
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        $("#head").empty();
                        $("#head").append("the dropped flight plan is not valid");
                    }
                });
            };
        })(files[i]);
    }
}



jsondrop.prototype._addEventHandlers = function () {

    // bind jsondrop to _this for use in 'ondrop'
    var _this = this;

    this.element.addEventListener('dragover', ondragover, false);
    this.element.addEventListener('dragleave', ondragleave, false);
    this.element.addEventListener('drop', ondrop, false);


    function ondragover(e) {
        $("#ulFlights").hide();
        e = e || event;
        e.preventDefault();
        this.className = 'dragging';
    }

    function ondragleave(e) {   //not working
        e = e || event;
        e.preventDefault();
        this.className = 'pageRightPanel';
        $("#ulFlights").show();
    }

    function ondrop(e) {
        e = e || event;
        e.preventDefault();
        this.className = 'pageRightPanel';
        files = e.dataTransfer.files;
        _this._readFiles(files);
        visualdrop();
    }
}


let AllMarkersFromTheBeggingOfTheExecutionOfTheApp = new Map();
let AllLinesFromTheBeggingOfTheExecutionOfTheApp = new Map();
var flights = [];
let map_id_marker = new Map();
let map_id_line = new Map();
var lines = [];
var externalClicked = false;
var external;
var infoWindows = [];

var flightsFromLastRound = [];

var lastclicked = "";

var infowindowglobal = new google.maps.InfoWindow();



function visualdrop() {
    const sleep = (milliseconds) => {
        return new Promise(resolve => setTimeout(resolve, milliseconds))
    }
    sleep(100).then(() => {
        removeallpossiblemarkers();

        deleteRefToMarkers();
        flightsFromLastRound = flights;
        flights = [];
        $("#flightdata").empty();
        map_id_marker = new Map();
        var currentDate = new Date().toISOString();
        $("#ulFlights").empty();
        var flightUrl = "api/Flights?relative_to=" + currentDate + "&sync_all";
        $.getJSON(flightUrl, function (data) {
            data.forEach(function (flight) {
                if (flight.is_external) {
                    $("#ulFlights").append('<li onclick="liclick(this)" class="External" id="' + flight.flight_id + '"' + ">" + flight.flight_id + "&nbsp;&nbsp;&nbsp;" + flight.company_name + "&nbsp;&nbsp;&nbsp;External" + "</li>");
                } else {
                    $("#ulFlights").append('<li onclick="liclick(this)" class="" id="' + flight.flight_id + '"' + ">" + flight.flight_id + "&nbsp;&nbsp;&nbsp;" + flight.company_name + '<span class="close" onclick="deleteclick(this)">&times;</span>' + "</li>");
                }

                var coords = {
                    lat: flight.current_location.latitude,
                    lng: flight.current_location.longitude
                };
                if (!checklastRound(flight.flight_id)) {
                    var Line = generatePath(flight.pathForLine, flight.latitude, flight.longitude);
                    AllLinesFromTheBeggingOfTheExecutionOfTheApp.set(flight.flight_id, Line);
                    map_id_line.set(flight.flight_id, Line);
                }
                var marker = new google.maps.Marker({
                    position: coords,
                    map: map
                });
                marker.addListener('click', function () {
                    closeMarkersInfo();
                    var li = document.getElementById(flight.flight_id);
                    liclick(li);
                    map.setZoom(5);
                    map.setCenter(marker.getPosition());
                });



                flights.push(flight.flight_id);


                AllMarkersFromTheBeggingOfTheExecutionOfTheApp.set(flight.flight_id, marker);
                map_id_marker.set(flight.flight_id, marker);
                if (lastclicked == flight.flight_id) {
                    var li = document.getElementById(flight.flight_id);
                    liclick(li);
                }
            });
            checkForMarkersThatSouldnotBeOnTheMap();
            checkForLinesThatSouldnotBeOnTheMap();


            flightsFromLastRound = [];
        });
        $("#ulFlights").show();
    })
    $("#flightdata").empty();
}

function checklastRound(id) {
    for (let flightid of flightsFromLastRound) {
        if (flightid == id) {
            return true;
        }
    }
    return false;
}



function deleteclick(el) {
    var parent = el.parentNode
    var id = parent.id;
    $.ajax({
        type: "DELETE",
        url: "api/Flights/" + id
    });
    DeleteRefToMarker(id);
    DeleteRefToLine(id);
    parent.remove();
    $("#flightdata").empty();
}

function DeleteRefToMarker(id) {
    var marker = map_id_marker.get(id);
    marker.setMap(null);
}

function DeleteRefToLine(id) {
    var line = map_id_line.get(id);
    line.setMap(null);
}



function liclick(e) {
    if (e.className == "External") {
        externalClicked = true;
        external = e;
    }
    $("li.clicked").removeClass("clicked");
    $(e).attr('class', 'clicked');
    showContentOnMarkerWhenLiClicked(e.id);
    var id = e.id;


    var flightUrl = "api/Flights/" + id;
    $.getJSON(flightUrl, function (flight) {
        $("#flightdata").empty();
        if (flight.flight_id != null) {
            $("#flightdata").append("<pre>" + "Flight ID: " + flight.flight_id + "      start_Longitude: " + flight.longitude + "      start_Latitude: " + flight.latitude
                + "      Passengers: " + flight.passengers + "<pre>" + "Company: " + flight.company_name + "      startTime: " + flight.date_time
                + "      finishTime: " + flight.finish_time + "</pre>" + "<pre>" + "      end_Longitude: " + flight.finish_location.longitude + "      end_Latitude: " + flight.finish_location.latitude + "      is_external: " + flight.is_external + "</pre>");
        } else {
            $("#flightdata").empty();
        }
    });
    $("#flightdata").show();
}

function getError() {
    $.get("api/Error", function (error) {
        $("#head").empty();
        $("#head").append(error);
    });
}

function load1sec() {
    flightsFromLastRound = flights;
    flights = [];
    map_id_marker = new Map();
    var currentDate = new Date().toISOString();
    var flightUrl = "api/Flights?relative_to=" + currentDate + "&sync_all";
    const sleep = (milliseconds) => {
        return new Promise(resolve => setTimeout(resolve, milliseconds))
    }
    sleep(50).then(() => {
        $.getJSON(flightUrl, function (data) {
            removeallpossiblemarkers();
            deleteRefToMarkers();
            $("#ulFlights").empty();
            getError();
            data.forEach(function (flight) {

                if (flight.is_external) {
                    $("#ulFlights").append('<li onclick="liclick(this)" class="External" id="' + flight.flight_id + '"' + ">" + flight.flight_id + "&nbsp;&nbsp;&nbsp;" + flight.company_name + "&nbsp;&nbsp;&nbsp;External" + "</li>");
                } else {
                    $("#ulFlights").append('<li onclick="liclick(this)" class="" id="' + flight.flight_id + '"' + ">" + flight.flight_id + "&nbsp;&nbsp;&nbsp;" + flight.company_name + '<span class="close" onclick="deleteclick(this)">&times;</span>' + "</li>");
                }


                var coords = {
                    lat: flight.current_location.latitude,
                    lng: flight.current_location.longitude
                };
                if (!checklastRound(flight.flight_id)) {
                    var Line = generatePath(flight.pathForLine, flight.latitude, flight.longitude);
                    AllLinesFromTheBeggingOfTheExecutionOfTheApp.set(flight.flight_id, Line);
                    map_id_line.set(flight.flight_id, Line);
                }

                var marker = new google.maps.Marker({
                    position: coords,
                    map: map
                });
                marker.addListener('click', function () {
                    closeMarkersInfo();
                    var li = document.getElementById(flight.flight_id);
                    liclick(li);
                    map.setZoom(5);
                    map.setCenter(marker.getPosition());
                });

                flights.push(flight.flight_id);


                AllMarkersFromTheBeggingOfTheExecutionOfTheApp.set(flight.flight_id, marker);
                map_id_marker.set(flight.flight_id, marker);

                if (lastclicked == flight.flight_id) {
                    var li = document.getElementById(flight.flight_id);
                    liclick(li);
                }
            });
            checkForMarkersThatSouldnotBeOnTheMap();
            checkForLinesThatSouldnotBeOnTheMap();

            flightsFromLastRound = [];

        });

    })
}


function checkForMarkersThatSouldnotBeOnTheMap() {
    var fits = 0;
    for (let flightid of AllMarkersFromTheBeggingOfTheExecutionOfTheApp.keys()) {
        fits = 0;
        for (let flightID of flights) {
            if (flightid == flightID) {
                fits = 1;
            }
        }
        if (fits == 0) {
            var marker = AllMarkersFromTheBeggingOfTheExecutionOfTheApp.get(flightid);
            marker.setMap(null);
        }
    }
}


function checkForLinesThatSouldnotBeOnTheMap() {
    var fits = 0;
    for (let flightid of AllLinesFromTheBeggingOfTheExecutionOfTheApp.keys()) {
        fits = 0;
        for (let flightID of flights) {
            if (flightid == flightID) {
                fits = 1;
            }
        }
        if (fits == 0) {
            var line = AllLinesFromTheBeggingOfTheExecutionOfTheApp.get(flightid);
            console.log(line);
            line.setVisible(false);
            line.setMap(null);
            var fdt = document.getElementById("flightdata").firstElementChild.innerHTML.substr(11, 6);
            if (fdt == flightid) {
                $("#flightdata").empty();
            }
        }
    }
}












function removeallpossiblemarkers() {

    for (let marker of AllMarkersFromTheBeggingOfTheExecutionOfTheApp.values()) {
        marker.setMap(null);
    }

}





function generatePath(segments, initialLAT, initialLNG) {

    Path = [];
    var initialpoint = new google.maps.LatLng(initialLAT, initialLNG);
    Path.push(initialpoint);

    for (let segment of segments) {
        var point = new google.maps.LatLng(segment.latitude, segment.longitude);
        Path.push(point);
    }

    var line = new google.maps.Polyline({
        path: Path,
        strokeColor: "black",
        strokeOpacity: 1.0,
        strokeWeight: 2,
        map: null
    });
    console.log(line);

    lines.push(line);
    return line;
}




function showMarkersOnMap() {
    for (let value of map_id_marker.values()) {
        value.setMap(map);
    }
}
function deleteRefToMarkers() {
    for (let value of map_id_marker.values()) {
        value.setMap(null);
    }
}

function showContentOnMarkerWhenLiClicked(id) {
    var marker = map_id_marker.get(id);
    var infowindow = new google.maps.InfoWindow({
        content: id
    });
    infowindowglobal = infowindow;
    infoWindows.push(infowindow);
    closeMarkersInfo();
    infowindow.open(map, marker);

    deleteRefToLines();/////////////////////////////////////////////////
    var line = map_id_line.get(id);/////////////////////////////////////////////////
    line.setMap(map);/////////////////////////////////////////////////
}

function deleteRefToLines() {/////////////////////////////////////////////////
    for (let value of lines) {/////////////////////////////////////////////////
        value.setMap(null);/////////////////////////////////////////////////
    }/////////////////////////////////////////////////
}/////////////////////////////////////////////////





function closeMarkersInfo() {
    infoWindows.forEach(myFunction);
}
function myFunction(value) {
    value.close();
}


function clickanywhere() {
    $(document).ready(function () {
        $(document).on("click", function (e) {
            if (($(e.target).attr('class') != "clicked") && (e.target.shape != "poly")) {
                $("li.clicked").removeClass("clicked");
                $("#flightdata").empty();
                closeMarkersInfo();
                deleteRefToLines();
                if (externalClicked) {
                    $(external).attr('class', 'External');
                    externalClicked = false;
                }
            }
            if (e.target.className == "clicked") {
                lastclicked = e.target.id;
            } else {
                lastclicked = "";
            }

            if (e.target.shape == "poly") {
                lastclicked = infowindowglobal.getContent();
            }
        });
    });
}

function draganddropVar() {
    var dropArea = new jsondrop('prpanddp');
}
