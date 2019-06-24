const urn = 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6bWlsbF9kYl9jdXN0b21lcl8xLzY1ODA4MDIwNzguemlw';

const options = {
	env: "AutodeskProduction",
	useADP: false,
	getAccessToken: function (callback) {
		const url = "https://azureforgeauth20190531062058.azurewebsites.net/api/ForgeAuth";
		fetch(url)
			.then(response => response.json())
			.then(json => callback(json.access_token, json.expires_in));
	}
};


const config3d = {
	extensions: [ ]
};


function onItemLoadSuccess(model) {
	console.log("onItemLoadSuccess");
}


function onItemLoadFailure(error) {
	console.log("onItemLoadFailure");
}

function onDocumentLoadSuccess(doc) {
	const viewables = viewerApp.bubble.search({ "type": "geometry" });
	if (viewables.length > 0) {
		viewerApp.selectItem(viewables[0], onItemLoadSuccess, onItemLoadFailure);
	}
}

function onDocumentLoadFailure() {
	console.log("document load failure");
}


let viewerApp = null;

Autodesk.Viewing.Initializer(options, () => {
	viewerApp = new Autodesk.Viewing.ViewingApplication("MyViewerDiv");
	viewerApp.registerViewer(viewerApp.k3D, Autodesk.Viewing.Private.GuiViewer3D, config3d);
	viewerApp.loadDocument(`urn:${urn}`, onDocumentLoadSuccess, onDocumentLoadFailure);
});


// ***************************************************************
// ***************************************************************
// ***************************************************************
// setup charts here

const powerReadingChart = new SmoothieChart({ responsive: true });
const powerReadingTimeSeries = new TimeSeries();
powerReadingChart.addTimeSeries(powerReadingTimeSeries, {
	strokeStyle: 'rgba(0, 255, 0, 1)',
	fillStyle: 'rgba(0, 255, 0, 0.25)',
	lineWidth: 2
});
powerReadingChart.streamTo(document.getElementById("chartPower"), 1000);

const defectsChart = new SmoothieChart({ responsive: true });
const defectsChartTimeSeries = new TimeSeries();
defectsChart.addTimeSeries(defectsChartTimeSeries, {
	strokeStyle: 'rgba(255, 0, 0, 1)',
	fillStyle: 'rgba(255, 0, 0, 0.25)',
	lineWidth: 2
});
defectsChart.streamTo(document.getElementById("chartDefects"), 2000);

const outputChart = new SmoothieChart({ responsive: true });
const outputChartTimeSeries = new TimeSeries();
outputChart.addTimeSeries(outputChartTimeSeries, {
	strokeStyle: 'rgba(0, 0, 255, 1)',
	fillStyle: 'rgba(0, 0, 255, 0.25)',
	lineWidth: 2
});
outputChart.streamTo(document.getElementById("chartOutput"), 3000);


const airQualityChart = new SmoothieChart({ responsive: true });
const airQualityChartTimeSeries = new TimeSeries();
airQualityChart.addTimeSeries(airQualityChartTimeSeries, {
	strokeStyle: 'rgba(255, 255, 0, 1)',
	fillStyle: 'rgba(255, 255, 0, 0.25)',
	lineWidth: 2
});
airQualityChart.streamTo(document.getElementById("chartAirQuality"), 4000);


// ***************************************************************
// ***************************************************************
// ***************************************************************
// do all of the graphs and their signalr here

//const signalRDevServerUrl = "https://localhost:44392/plantstatus";
const signalRDevServerUrl = "http://localhost:52757/plantstatus";


const plantStatusConnection = new signalR.HubConnectionBuilder()
	.withUrl(signalRDevServerUrl)
	.configureLogging(signalR.LogLevel.Information)
	.build();

plantStatusConnection.start()
	.catch(err => console.error(err.toString()));

plantStatusConnection.on("PowerReading", powerValue => powerReadingTimeSeries.append(new Date().getTime(), powerValue));
plantStatusConnection.on("DefectCount", defectCount => defectsChartTimeSeries.append(new Date().getTime(), defectCount));
plantStatusConnection.on("ProductOutput", prodOutput => outputChartTimeSeries.append(new Date().getTime(), prodOutput));
plantStatusConnection.on("AirQualityPpm", ppmCount => airQualityChartTimeSeries.append(new Date().getTime(), ppmCount));


// ***************************************************************
// ***************************************************************
// ***************************************************************

const urlIotData = 'https://plantdashboardiotfuncs20190526042133.azurewebsites.net/api';

const deviceEventConnection = new signalR.HubConnectionBuilder()
	.withUrl(urlIotData)
	.configureLogging(signalR.LogLevel.Information)
	.build();

deviceEventConnection.on("notify", function (message) {
	console.log(message);
	const messageData = JSON.parse(message);
	moveViewForAlarm(messageData.Camera, messageData.Target, messageData.CameraUpVector, 3000);
});


deviceEventConnection.start()
	.catch(err => console.error(err.toString()));


// ***************************************************************
// ***************************************************************
// ***************************************************************
// manipulating the viewer for alarms


function createTween(params) {
	return new Promise(resolve => {
		new TWEEN.Tween(params.start)
			.to(params.to, params.duration)
			.easing(params.easing)
			.onComplete(() => resolve())
			.onUpdate(params.onUpdate)
			.start();
	});
}


function moveViewForAlarm(cameraTo, positionTo, upVector, duration) {
	const nav = viewerApp.myCurrentViewer.navigation;
	const cameraFrom = nav.getPosition();
	const positionFrom = nav.getTarget();
	const currentUpVector = nav.getCameraUpVector();


	const cameraTween = createTween({
		start: { x: cameraFrom.x, y: cameraFrom.y, z: cameraFrom.z },
		to: { x: cameraTo[0], y: cameraTo[1], z: cameraTo[2] },
		duration: duration,
		easing: TWEEN.Easing.Sinusoidal.InOut,
		onUpdate: p => nav.setPosition(p)
	});

	const targetTween = createTween({
		start: { x: positionFrom.x, y: positionFrom.y, z: positionFrom.z },
		to: { x: positionTo[0], y: positionTo[1], z: positionTo[2] },
		duration: duration,
		easing: TWEEN.Easing.Sinusoidal.InOut,
		onUpdate: p => nav.setTarget(p)
	});

	let upTween = null;
	if (currentUpVector.x !== upVector.x || currentUpVector.y !== upVector.y || currentUpVector.z !== upVector.z) {
		console.log("creating tween for the camera up vector");
		upTween = createTween({
			start: { x: currentUpVector.x, y: currentUpVector.y, z: currentUpVector.z },
			to: { x: upVector[0], y: upVector[1], z: upVector[2] },
			duration: duration,
			easing: TWEEN.Easing.Sinusoidal.InOut,
			onUpdate: p => nav.setCameraUpVector(p)
		});
	}

	let animating = true;
	function animate(start) {
		if (start || animating) {
			requestAnimationFrame(animate);
			TWEEN.update();
		}
	}

	const myTweens = [ cameraTween, targetTween ];
	if (upTween) {
		myTweens.push(upTween);
	}
	Promise.all(myTweens).then(() => animating = false);
	animate(true);
}




