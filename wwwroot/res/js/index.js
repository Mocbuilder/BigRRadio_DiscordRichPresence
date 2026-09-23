//variable
let isPlaying = true;
const PlayButton = document.getElementById("PlayButton");
const VolumeSlider = document.getElementById("VolumeSlider");

const playIcon = document.getElementById('PlayIcon');
const pauseIco = '<path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>';
const playIco = '<path d="M8 5v14l11-7z"/>';

//save|load
if (window.localStorage.getItem("volume")) {
    let volume = window.localStorage.getItem("volume");
    VolumeSlider.value = volume;
    setVolume(volume);
}
else {
    VolumeSlider.value = 75;
    setVolume(75);
}

//event
PlayButton.addEventListener("click", () => {
    isPlaying = !isPlaying;
    playIcon.innerHTML = isPlaying ? pauseIco : playIco;
    post('toggle');
})

VolumeSlider.addEventListener("input", () => {
    let volume = VolumeSlider.value;
    setVolume(volume);
    window.localStorage.setItem("volume", volume)
})

//class
class RadioListManager {
    constructor(){
        this.radioList = document.getElementById("RadioList");
        this.buildOption();
        this.restoreRadio();
        this.radioList.addEventListener("input", ()=>{
            this.goToRadio(this.radioList.value);
        });
    }

    async getData() {
        const data = (await fetch("./res/data/stations.json")).json();
        return data;
    }

    createOption(id,name){
        const newElement = document.createElement("option");
        newElement.value = id;
        newElement.textContent = name;
        this.radioList.appendChild(newElement);
    }

    async buildOption(){
        const Data = await this.getData();
        const DataLenght = Data.length;
        for(let i = 0; i < DataLenght; i++){
            const singleData = Data[i];
            this.createOption(singleData.id,singleData.name);
        }
        this.radioList.value = "";
    }

    restoreRadio(){
        if (window.localStorage.getItem("id")) {
            let id = window.localStorage.getItem("id");
            this.goToRadio(id);
        }
        else {
            this.goToRadio("a55004");
        }
    }

    async chanceHeadline(id){
        const Headline = document.getElementById("Headline");
        try{
            let img = `./res/img/icon_${id}.png`
            await fetch(img);
            Headline.src = img;
        }
        catch{
            Headline.src = `./res/img/icon_error.png`
        }
    }

    goToRadio(id){
        post('channel:' + id);
        this.chanceHeadline(id);
        this.radioList.value = "";
        window.localStorage.setItem("id", id);
    }
}

const radioListManager = new RadioListManager;

//function
function setVolume(volume) {
    post('vol:' + volume);
}

function post(data) {
    window.external.sendMessage(data);
}

//get data
window.external.receiveMessage(rawJson => {
    const data = JSON.parse(rawJson)["current-track"];
    document.getElementById("Art").src = data.art;
    document.getElementById("Title").innerText = data.title;
    document.getElementById("Artist").innerText = "by " + data.artist;
});