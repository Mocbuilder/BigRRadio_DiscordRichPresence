class RadioManager {
    constructor(){
        this.isPlaying = true;
        this.playButton = document.getElementById("PlayButton");
        this.volumeSlider = document.getElementById("VolumeSlider");
        this.playIcon = document.getElementById('PlayIcon');
        this.pauseIco = '<path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>';
        this.playIco = '<path d="M8 5v14l11-7z"/>';
        this.radioList = document.getElementById("RadioList");

        this.addEvent();
        this.buildOption();
        this.restoreRadio();

        window.external.receiveMessage(rawJson => {
            const data = JSON.parse(rawJson)["current-track"];
            document.getElementById("Art").src = data.art;
            document.getElementById("Title").innerText = data.title;
            document.getElementById("Artist").innerText = "by " + data.artist;
        });
    }

    addEvent(){
        this.playButton.addEventListener("click", () => {
            this.isPlaying = !this.isPlaying;
            this.playIcon.innerHTML = this.isPlaying ? this.pauseIco : this.playIco;
            this.post('toggle');
        });

        this.volumeSlider.addEventListener("input", () => {
            let volume = this.volumeSlider.value;
            this.setVolume(volume);
            window.localStorage.setItem("volume", volume)
        });

        this.radioList.addEventListener("input", ()=>{
            this.goToRadio(this.radioList.value);
        });
    }

    async buildOption(){
        const Data = await this.getData();
        const DataLenght = Data.length;
        for(let i = 0; i < DataLenght; i++){
            const singleData = Data[i];
            this.createOption(singleData.id,singleData.name);
        }
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

    goToRadio(id){
        this.post('channel:' + id);
        this.chanceHeadline(id);
        window.localStorage.setItem("id", id);
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

    restoreRadio(){
        if (window.localStorage.getItem("id")) {
            let id = window.localStorage.getItem("id");
            this.goToRadio(id);
            this.radioList.value = id;
        }
        else {
            this.goToRadio("a55004");
            this.radioList.value = "a55004";
        }

        if (window.localStorage.getItem("volume")) {
            let volume = window.localStorage.getItem("volume");
            this.volumeSlider.value = volume;
            this.setVolume(volume);
        }
        else {
            this.volumeSlider.value = 75;
            this.setVolume(75);
        }
    }

    setVolume(volume) {
        this.post('vol:' + volume);
    }

    post(data) {
        window.external.sendMessage(data);
    }
}

new RadioManager;