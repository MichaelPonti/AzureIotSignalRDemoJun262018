
// const url = 'http://localhost:7071/api';
const url = 'https://plantdashboardiotfuncs20190526042133.azurewebsites.net/api';

const deviceEventConnection = new signalR.HubConnectionBuilder()
	.withUrl(url)
	.configureLogging(signalR.LogLevel.Information)
	.build();




deviceEventConnection.on("notify", function (message) {
	console.log(message);
	let container = document.getElementById("eventcontainer");
	let ptag = document.createElement("P");
	ptag.appendChild(document.createTextNode(JSON.stringify(message)));
	container.appendChild(ptag);
});


deviceEventConnection.start();
