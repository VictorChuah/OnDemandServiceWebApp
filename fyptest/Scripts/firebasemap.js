function initMap() {
  var map = new google.maps.Map(document.getElementById('map'), {
    center: { lat: 0, lng: 0 },
    zoom: 3,
    styles: [{
      featureType: 'poi',
      stylers: [{ visibility: 'off' }]  // Turn off points of interest.
    }, {
      featureType: 'transit.station',
      stylers: [{ visibility: 'off' }]  // Turn off bus stations, train stations, etc.
    }],
    disableDoubleClickZoom: true,
    streetViewControl: false
  });

  // Create the DIV to hold the control and call the makeInfoBox() constructor
  // passing in this DIV.
  var infoBoxDiv = document.createElement('div');
  var infoBox = new makeInfoBox(infoBoxDiv, map);
  infoBoxDiv.index = 1;
  map.controls[google.maps.ControlPosition.TOP_CENTER].push(infoBoxDiv);

  // Listen for clicks and add the location of the click to firebase.
  map.addListener('click', function (e) {
    data.lat = e.latLng.lat();
    data.lng = e.latLng.lng();
    addToFirebase(data);
  });

  // Create a heatmap.
  var heatmap = new google.maps.visualization.HeatmapLayer({
    data: [],
    map: map,
    radius: 16
  });

  initAuthentication(initFirebase.bind(undefined, heatmap));
}


