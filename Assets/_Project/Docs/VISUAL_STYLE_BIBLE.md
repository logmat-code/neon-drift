# NEON DRIFT — VISUAL STYLE BIBLE

## 1. Perspectiva
- **Formato**: 2D top-down 3/4.
- **Ángulo Aparente**: ~70° cenital oblicuo. Se observa claramente el capó, parabrisas frontal, techo completo, luneta trasera, parte superior de puertas laterales y sombras proyectadas.
- **Regla Estricta**: Consistencia total entre jugador, tráfico, edificios y props. Sin vistas laterales ni elementos 3D aislados.

## 2. Escala & Pixel Density (No Mixels)
- **Pixels Per Unit (PPU)**: **16** (1 Unity Unit = 16 píxeles = 1 metro).
- **Proporciones de Vehículos**:
  - **Player: KITSUNE S13**: 36 px ancho × 68 px largo (lienzo 48 × 80 px).
  - **Traffic: Compact**: 32 px ancho × 56 px largo (lienzo 40 × 64 px).
  - **Traffic: Sedan**: 36 px ancho × 68 px largo (lienzo 48 × 80 px).
  - **Traffic: Van**: 40 px ancho × 82 px largo (lienzo 48 × 96 px).
- **Proporción Carretera**:
  - Autopista Wangan / Neon Loop: 4 carriles de circulación (ancho total ~224 px, ~56 px por carril).
  - Permite que el vehículo de 36 px tenga margen de maniobra, espacio de derrape angular y zona precisa de Near Miss (5-12 px de margen lateral).

## 3. Paleta Cromática (Japanese Night Racing)
### Base Nocturna & Asfalto
- `#06080f` — Void / Cielo nocturno profundo
- `#10131e` — Asfalto húmedo base
- `#1a1e2c` — Carril de rodadura / asfalto iluminado
- `#2b3145` — Hormigón / guardrails / aceras
- `#444c66` — Bordillos y estructuras metálicas

### Acentos de Neón & Luces
- `#00e5ff` — **Cyber Cyan**: Flow multiplier, barras de boost, neones de tiendas, partículas de derrape avanzado.
- `#ff007f` — **Electric Magenta**: Rótulos comerciales de Neon District, combo popups, flashes de Perfect Exit.
- `#8a2be2` — **Metro Violet**: Iluminación ambiental secundaria, reflejos en charcos.
- `#ffaa00` — **Sodium Amber**: Farolas urbanas de vapor de sodio, intermitentes, Near Miss popup.
- `#ff2244` — **Laser Red**: Luces de posición traseras (Kitsune / Tráfico), aviso de colisión, medidor de peligro.
- `#ffffff` / `#d8f6ff` — **Crisp White / Ice**: Faros de xenón delanteros, chispas de roce, hit-flashes.

## 4. Delineado (Outline) & Iluminación
- **Contorno**: 1 px de borde oscuro tintado en azul noche (`#080a14`). Evitar negro `#000000` absoluto para conservar atmósfera.
- **Sombra Proyectada**: Desplazamiento oblicuo hacia abajo (Y: -3 px), semitransparente (`#05070e`, ~50% opacidad).
- **Reflejos Nocturnos**: Franja de 1 px de brillo especular tenue a lo largo del techo y parabrisas para reflejar las luces de la metrópolis.

## 5. Arquitectura Urbana & Neon District Props
- Edificios con fachadas modulares oscuras y letreros verticales en kanji/katakana (ficticios: ドリフト, ネオン, 速度, 峠).
- Máquinas expendedoras (*jidouhanbaiki*) con brillo cyan/magenta en la acera.
- Postes eléctricos con tendido de cables aéreos oscuros.
- Barreras de hormigón y guardrails bicolores con reflectores amarillos.

## 6. VFX & Juice
- **Drift Smoke**: Puffs volumétricos pixelados (3x3 a 8x8 px), degradado de gris azulado a blanco y cyan.
- **Skid Marks**: Trazas dobles sobre el asfalto generadas bajo las ruedas posteriores en tono asfalto quemado (`#0a0c14`).
- **Speed Streaks**: Líneas de 1 px blancas y cyan durante Boost.
- **Hit-Stop & Shake**: 2-3 frames de micro congelamiento y vibración de cámara sutil (amplitud máxima 0.25 unidades) al colisionar.
