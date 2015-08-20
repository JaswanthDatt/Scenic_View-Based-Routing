# Scenic_View-Based-Routing
Scenic Routing between source and destination through the <i>most scenic places</i>.

<b>Introduction</b></br>
We all use google maps on a daily basis.The path that is shown by Google is always the shortest path between source and destination.
This project, we have planned to implement the path using <b>Scenic Places</b> as criterion.We have implemented the scenic route from any <i>source</i> to <i>The University of Akron</i>.

In deciding a place to be scenic we have taken factors like Natural Beauty,Safety,Weather,Traffic as primary.Places with natural beauty includes parks,reservations,wilf-life area,events etc.
Since google allows only 8 waypoints between source and destination for a free api user,we have chosen  8 points such that each point is at a distance of 1/8(distance between source and destination).
Now each of the eight markers are reverse geocoded,to get the address.This addresss is fed into the site to get the list of scenic places around it.
From the list of all places we get,we chose a single point as the scenic place around that marker.Factors like weather,Safety etc have been taken into consideration while deciding it.

All this process have been automated using Selenium.Using Selenium, we fed the marker address to get natural places,to get weather information,to get safety information without the user interference.
This process takes a bit of time since every time a marker place has to be fed,a new browser window has to be opened <i>sequentially</i>.

Later,this data is sent to the database.We have not opened the SQL 2012 db connection till the moment all markers have been decoded.For performance efficiency,we have used <i>SQLBULKCopy</i> to transfer this dataset to database,where we have done further querying to eliminate few places.
The outcome is list of 8 waypoints which are scenic,through which the path between source and <i>the University of Akron</i> has been plotted.

<img src="https://cloud.githubusercontent.com/assets/9910374/9393075/0f7080c4-474e-11e5-985e-8c7b4f33a688.png"/>

