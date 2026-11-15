// Mapa de seguimiento de bolsines (Leaflet + OpenStreetMap).
// Mantiene el mismo formato de siempre: marcador por defecto de Leaflet con la
// etiqueta "Bolsín N" fija. Al hacer clic en un marcador se avisa a la pantalla
// (Blazor) qué bolsín se seleccionó.

let mapa = null;
let capaBolsines = null;
let referenciaPantalla = null;
let marcadores = {};

export function cargarMapa(nuevasLocalizaciones, pantalla) {
    referenciaPantalla = pantalla;

    if (mapa !== null) {
        mapa.remove();
    }

    mapa = L.map('map').setView({
        lon: -64.81754484681906,
        lat: -41.4547838657288
    }, 4);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 16
    }).addTo(mapa);

    capaBolsines = L.layerGroup().addTo(mapa);
    actualizarMarcadores(nuevasLocalizaciones);

    return "";
}

// Vuelve a dibujar los marcadores (por ejemplo, después de filtrar)
export function actualizarMarcadores(nuevasLocalizaciones) {
    if (mapa === null) {
        return;
    }

    capaBolsines.clearLayers();
    marcadores = {};

    var localizaciones = JSON.parse(String(nuevasLocalizaciones));

    // Carga directa de la capa GeoJSON agregando los tooltips automáticamente
    L.geoJSON(localizaciones, {
        onEachFeature: function (feature, layer) {
            if (feature.properties && feature.properties.name) {
                layer.bindTooltip(feature.properties.name, {
                    permanent: true,
                    offset: [0, 0]
                });
            }

            if (feature.properties && feature.properties.numeroBolsin !== undefined) {
                var numero = feature.properties.numeroBolsin;
                marcadores[numero] = layer;

                // Paso 7: el EB selecciona un bolsín del mapa
                layer.on('click', function () {
                    if (referenciaPantalla !== null) {
                        referenciaPantalla.invokeMethodAsync('SeleccionarBolsinDesdeMapa', numero);
                    }
                });
            }
        }
    }).addTo(capaBolsines);
}

// Resalta la etiqueta del bolsín seleccionado (0 = ninguno)
export function resaltarBolsin(numeroSeleccionado) {
    for (var numero in marcadores) {
        var tooltip = marcadores[numero].getTooltip();
        var elemento = tooltip ? tooltip.getElement() : null;

        if (elemento) {
            elemento.classList.toggle('tooltip-seleccionado', Number(numero) === numeroSeleccionado);
        }
    }
}
