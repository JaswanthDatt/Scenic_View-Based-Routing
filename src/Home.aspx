%@ Page Language="C#" AutoEventWireup="true" CodeFile="Direction.aspx.cs" Inherits="Direction" %>

<%@ Import Namespace="HtmlAgilityPack %>
<%@ Import Namespace="OpenQA.Selenium" %>
<%@ Import Namespace="OpenQA.Selenium.Firefox" %>
<%@ Import Namespace="OpenQA.Selenium.Support.UI" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="System.Web.Services" %>


<!doctype html>
<html>
<head id="Head1" runat="server">
    <title></title>
    <script src="https://maps.googleapis.com/maps/api/js?v=3.exp&signed_in=true&libraries=places"></script>
    <script src="http://www.geocodezip.com/scripts/v3_epoly.js" type="text/javascript"></script>
    <link href="HomeCSS.css" rel="stylesheet" />

   <script type="text/javascript">
       var mArray = Array();
       var map;
       /*      var destination = document.getElementById("Destination");
                var dst_lat = destination.geometry.lat();
                var dst_long = destination.geometry.lng();
                console.log(src_lat + "" + src_long);
    
                var source = document.getElementById("Source");
                var src_lat = source.geometry.lat();
                var src_long = source.geometry.lng();
    
       */
       var src_lat = "41.076366";
       var src_long = "-81.510253";
       var dst_lat = "40.1693362";
       var dst_long = "-90.6768425";
       var directionsService = new google.maps.DirectionsService();
       var directionsDisplay;
       var points = new Array();
       var landmarks = [];
       var destination = document.getElementById("Destination");
       var source = document.getElementById("Source");
       var scenic_places = null;
       var map = null;
       var infowindow = new google.maps.InfoWindow(
         {
             size: new google.maps.Size(150, 50)
         });

       var polyline = new google.maps.Polyline({
           path: [],
           strokeColor: '#0000FF',
           strokeWeight: 5
       });


       function load() {          
           loaddata();
       }

       function loadmarkers() {
           var distance_src_end = computedistance();
           var map_lat_lng = new Map();
           console.log("Distance is " + distance_src_end);
           var points_at_distance = (distance_src_end / 7) * 1000;    // 88888888888
           if (landmarks.length && (landmarks.length > 0)) {
               for (var i = 0; i < landmarks.length; i++)
                   landmarks[i].setMap(map);
           } else {
               var km_points = polyline.GetPointsAtDistance(points_at_distance);
               console.log("km_points:" + km_points);
               km_points.push(new google.maps.LatLng(src_lat, src_long));
               km_points.push(new google.maps.LatLng(dst_lat, dst_long));

               for (var i = 0; i < km_points.length; i++) {
                   var landmark = createMarker(km_points[i]);
                   landmarks.push(landmark);
                   // map_lat_lng.set(km_points[i].lat, km_points.lng);
                   console.log(km_points[i].lat);
               }
           }

           markers_address(km_points);

       }
       function marker_source_destination() {
           console.log("Entered here");
           var marker = new google.maps.Marker({
               position: destination,
               map: map,
               title: name,
               icon: 'Map-Marker-Bubble-Chartreuse-icon.png'
           });

           google.maps.event.addListener(marker, 'click', function () {
               infowindow.open(map, marker);
           });

           return;
       }
       function doLoad() {

           directionsDisplay = new google.maps.DirectionsRenderer({
               polylineOptions: {
                   strokeColor: "red"
               }
           });
           var myOptions = {
               zoom: 7,
               center: new google.maps.LatLng(41.076366, -81.510253),
               mapTypeId: google.maps.MapTypeId.ROADMAP
           }
           map = new google.maps.Map(document.getElementById("map_canvas"),
                                         myOptions);

           google.maps.event.addListener(map, 'click', function () {
               infowindow.close();
           });
           directionsDisplay.setMap(map);
           var trafficLayer = new google.maps.TrafficLayer();
           trafficLayer.setMap(map);
           //  bounds = new google.maps.LatLngBounds();
           directionsDisplay.setPanel(document.getElementById("directionpanel"));

       }
       function computedistance() {
           var start = new google.maps.LatLng(src_lat, src_long);
           var end = new google.maps.LatLng(dst_lat, dst_long);

           return (google.maps.geometry.spherical.computeDistanceBetween(start, end) / 1000).toFixed(2);

       }
       function drawCircle(lat_lng) {
           //console.log(lat_lng);
           var radius_req = computedistance();
           var marker = new google.maps.Marker({
               map: map,
               position: new google.maps.LatLng(lat_lng[0], lat_lng[1]),
               title: 'My marker loc'
           });

           // Add circle overlay and bind to marker
           var circle = new google.maps.Circle({
               map: map,
               radius: (radius_req / 7) * 1000,    //  miles in metres  /8
               fillColor: '#808080'
           });

           circle.bindTo('center', marker, 'position');

       }

       function createMarker(point) {
           var marker = new google.maps.Marker({
               position: point,
               map: map,
               title: name,
               zIndex: Math.round(point.lat() * -100000) << 5,
               stylers: [
               {
                   visibility: "off"
               }
               ]
           });

           google.maps.event.addListener(marker, 'click', function () {
               infowindow.open(map, marker);
           });
           var lat_lng = [marker.position.lat(), marker.position.lng()];
           drawCircle(lat_lng);
           //  codeLatLng(lat_lng);
           return marker;
       }


       function loaddata() {

           var request = {
               origin: new google.maps.LatLng(src_lat, src_long),
               destination: new google.maps.LatLng(dst_lat, dst_long),
               travelMode: google.maps.TravelMode.DRIVING
           };

           directionsService.route(request, function (result, status) {
               if (status == google.maps.DirectionsStatus.OK) {
                   var bounds = new google.maps.LatLngBounds();
                   var path = result.routes[0].overview_path;
                   var legs = result.routes[0].legs;
                   for (i = 0; i < legs.length; i++) {
                       var steps = legs[i].steps;
                       for (j = 0; j < steps.length; j++) {
                           var nextSegment = steps[j].path;
                           for (k = 0; k < nextSegment.length; k++) {
                               polyline.getPath().push(nextSegment[k]);
                               bounds.extend(nextSegment[k]);
                           }
                       }
                   }

                   polyline.setMap(map);
                   console.log("polyiline");
                   map.fitBounds(bounds);
                   loadmarkers();

               }
           });
       }


       function codeLatLng(lat_lng) {
           var latlngStr = Array.prototype.slice.call(lat_lng);
           var lat = parseFloat(latlngStr[0]);
           var lng = parseFloat(latlngStr[1]);
           var latlng = new google.maps.LatLng(lat, lng);
           var geocoder = new google.maps.Geocoder();

           setTimeout(function () {
               geocoder.geocode({ 'latLng': latlng }, function (results, status) {
                   if (status == google.maps.GeocoderStatus.OK) {
                       if (results[0]) {
                           map.setZoom(7);
                           marker = new google.maps.Marker(
                           {
                               position: latlng,
                               map: map
                           });
                           infowindow.setContent(results[0].formatted_address);
                           infowindow.open(map, marker);
                       }
                       else if (status == google.maps.GeocoderStatus.OVER_QUERY_LIMIT) {
                           setTimeout(codeLatLng(), 2000);
                       }
                   }
                   else {
                       alert("Geocoder failed due to: " + status);
                   }
               });
           }, 2000);


           return;
       }

       function markers_address(km_points, map) {
           console.log("kmpoints inside function" + km_points);
           PageMethods.markers_address(km_points);


       }

       function scenic_route() {
           console.log("enetered scenic route rgghehn waypoints ");
           alert("enetere scenic route finction");
           var start = new google.maps.LatLng(src_lat, src_long);
           var end = new google.maps.LatLng(dst_lat, dst_long);
           alert(start + "******" + end);

           for (i = 0; i < scenic_places.length; i++) {
               alert(scenic_places[i]);
           }
           var waypoints = [];
           // alert string to match waypoints
           for (i = 0; i < scenic_places.length; i++) {
               waypoints.push({
                   location: scenic_places[i],
                   stopover: true
               });
           }

           var request = {
               origin: start,
               destination: end,
               waypoints: waypoints,
               optimizeWaypoints: true,
               travelMode: google.maps.TravelMode.DRIVING
           };




           directionsService.route(request, function (result, status) {
               if (status == google.maps.DirectionsStatus.OK) {
                   directionsDisplay.setDirections(result);
               }
           });
           alert("way points are " + scenic_places);
       }

    </script>
    <style type="text/css">
        #Text1 {
            width: 145px;
        }
    </style>
</head>
<body onload=" doLoad()">
    <form id="form2" runat="server">
        <div id="wrapper">
            <div id="Head">
                <h1>The University of Akron</h1>
                <h3>Scenic routing</h3>
            </div>
            <div id="Body">
                <div id="leftMap">
                    &nbsp;&nbsp;&nbsp;&nbsp;
                    &nbsp;&nbsp;
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    &nbsp;&nbsp;&nbsp;&nbsp;
                    <asp:Button ID="Button4" runat="server" Text="GO ZIPS!!" OnClick="Button4_Click" ForeColor="Lime" Font-Italic="True" Font-Bold="True" Font-Size="Larger" />
                    <asp:Button ID="Button1" runat="server" Text="Google Path" OnClientClick="load()" />
                    <div id="map_canvas">
                        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="True">
                        </asp:ScriptManager>
                    </div>
                    <div id="directionpanel">
                       DIRECTIONS
                    </div>
                </div>
                <div id="footer">
                    <h2>
                        <marquee behavior="alternate"> Data Integration @ Jaswanth/Katta/Chris </marquee>
                    </h2>
                </div>
            </div>
            </form>


</body>
</html>



