<template>
    <div>
         <img :src="`/store.png`" class="store-image" />
        <div class="heading">3 Closest Branches</div>
        <div>
            <InputText
                placeholder="enter current address"
                id="address"
                v-model="state.address"
            />
            <br />
        </div>
        <div style="margin: 3vh">
            <Button
                label="Find 3 Closest"
                @click="genMap"
                class="p-button-outlined"
                style="width: 30vw"
            />
        </div>
        <div id="map" ref="mapRef" v-show="state.showmap === true"></div>
    </div>
</template>
<script>
import { ref, reactive } from "vue";
import { fetcher } from "../util/apiutil";
export default {
    name: "Map",
    setup() {
        const mapRef = ref(null);
        let state = reactive({
            status: "",
            address: "",
            showmap: false,
            threeBranches: [],
        });
        const genMap = async () => {
            try {
                state.showmap = true;
                const tt = window.tt;
                let url = `https://api.tomtom.com/search/2/geocode/${state.address}.json?key=GlvA6lJASABDqmxsnHODgVYf1VJ5NpVP`;
                let response = await fetch(url);
                let payload = await response.json();
                let lat = payload.results[0].position.lat;
                let lon = payload.results[0].position.lon;
                let map = tt.map({
                    key: "GlvA6lJASABDqmxsnHODgVYf1VJ5NpVP",
                    container: mapRef.value,
                    source: "vector/1/basic-main",
                    center: [lon, lat],
                    zoom: 8,
                });
                map.addControl(new tt.FullscreenControl());
                map.addControl(new tt.NavigationControl());

                let threeBranches = await fetcher(`branch/${lat}/${lon}`);
                threeBranches.map((branch) => {
                    let marker = new tt.Marker()
                        .setLngLat([branch.longitude, branch.latitude])
                        .addTo(map);
                    let popupOffset = 25;
                    let popup = new tt.Popup({ offset: popupOffset });

                    popup.setHTML(
                        `<div id="popup1" >
                            <p>
                                <strong>Branch# ${branch.id}</strong><br>${branch.street}, ${branch.city}<br>${branch.distance.toFixed(2)} km
                            </p>
                        </div>`
                    );
                    marker.setPopup(popup);
                });
            } catch (err) {
                state.status = err.message;
            }
        };
        return {
            mapRef,
            state,
            genMap,
        };
    },
};
</script>
<style>
#map {
    height: 50vh;
    width: 90vw;
    margin-left: 3vw;
    border: solid;
}
#popup {
    font-weight: bold;
}
</style>