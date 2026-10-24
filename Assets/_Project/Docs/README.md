# NEON DRIFT — MANUAL TÉCNICO Y DE DESARROLLO

## 1. Visión General
**NEON DRIFT** es un arcade de drift 2D top-down 3/4 en pixel art ambientado en una metrópolis japonesa nocturna ficticia.
Diseñado para sesiones móviles rápidas (runs de ~3 minutos en landscape) con soporte completo para teclado en Editor y controles táctiles en pantalla.

---

## 2. Controles
| Acción | Teclado (Editor / PC) | Mobile (Touch en Pantalla) |
|---|---|---|
| Girar Izquierda | `A` / `Flecha Izquierda` | Botón táctil D-Pad Izquierdo |
| Girar Derecha | `D` / `Flecha Derecha` | Botón táctil D-Pad Derecho |
| Iniciar / Mantener Drift | `Espacio` / `Left Shift` | Botón táctil DRIFT (Pulsación continua) |
| Activar Boost | `B` / `W` / `Flecha Arriba` | Botón táctil BOOST |

El vehículo cuenta con **aceleración automática** constante orientada a la experiencia arcade.

---

## 3. Arquitectura del Proyecto
El código sigue una estricta separación de responsabilidades:

- **Core/**:
  - `GameManager`: Máquina de estados principal (`Home`, `Garage`, `Playing`, `Results`). Coordina el reinicio rápido sin recargas pesadas.
- **Data/**:
  - `CarConfig`: ScriptableObject con tuning físico (velocidad, aceleración, fricciones, ángulos de derrape y spinout).
  - `FlowConfig`: ScriptableObject con multiplicadores (x1 a x8), tasas de caída y recompensas de maniobras.
  - `UpgradeConfig`: Niveles de mejoras y multiplicadores de atributos para el Garage.
- **Vehicle/**:
  - `VehicleController`: Controlador arcade con máquina de estados (`Grip`, `DriftEntry`, `Drifting`, `DriftExit`, `Spinout`), contravolante, ventana de Perfect Exit y encadenamiento (Drift Chain).
- **Gameplay/**:
  - `FlowManager`: Medidor de Flow, multiplicadores automáticos (x1, x2, x3, x5, x8) y generación de energía de Boost.
  - `ScoreManager`: Cálculo `BaseScore × FlowMultiplier`, tracking de récord de derrape más largo, combos y pasadas arriesgadas.
  - `NearMissDetector`: Detección geométrica de proximidad peligrosa sin colisión (`Near Miss` y `Drift Near Miss`) y cruce entre dos vehículos (`Traffic Gap` y `Drift Gap`). Anti-exploits por cooldown y pasada única por vehículo.
  - `CollisionHandler`: Categorización de impactos (`WallTouch`, `TrafficHit`, `HeavyCrash`).
- **World/**:
  - `TrackManager`: Construcción del circuito **NEON DISTRICT — NIGHT LOOP** (11 secciones con rectas, curvas suaves, chicanes, túnel Wangan y recta de meta).
  - `TrafficManager`: Sistema de Object Pooling para tráfico (`Compact`, `Sedan`, `Van`) en 4 carriles con garantía de hueco transitable.
  - `TrafficCar`: Comportamiento de tráfico cinemático.
- **Presentation/**:
  - `CameraController`: Seguimiento suave 2D, anticipación de derrape (lookahead), pullback en Boost y trauma shake amortiguado.
  - `VehicleVFX`: Humo pixelado de neumáticos, marcas de skidmarks en el asfalto, conos de faros y estelas de velocidad en Boost.
  - `AudioManager`: Audio arquitectónico con clips sintetizados nativos procedurales (motor variable por velocidad, chirrido de ruedas, boost, impactos, chimes de Perfect Exit y clicks de UI).
  - `GameFeelManager`: Hit-stop (congelamiento de 0.05s-0.08s en choques fuertes).
- **UI / Views**:
  - `HomeView`: Portada urbana nocturna, coche estacionado y acceso directo a Drive/Garage/Settings.
  - `GarageView`: Taller clandestino, barras de stats (Speed, Accel, Grip, Drift, Boost) y compra de mejoras.
  - `HUDView`: Puntuación, barra de Flow, badge de multiplicador, medidor de Boost y botones táctiles.
  - `ResultsView`: Resumen de run (Score, Récord, Derrape más largo, Combo Máximo, Near Misses, Créditos obtenidos) y botón de Retry instantáneo.
- **Progression/**:
  - `SaveSystem`: Almacenamiento local JSON persistente de créditos, récords y niveles de mejoras.
  - `ProgressionManager`: Gestión de compras y fórmula ponderada de créditos ganados.

---

## 4. Guía de Extensión

### Cómo Agregar Nuevos Vehículos de Jugador
1. Crear el sprite pixel-art respetando el canon (PPU = 16, paleta oficial, vista 2D top-down 3/4).
2. Crear un nuevo `CarConfig.asset` en `Assets/_Project/Data/` duplicando el existente y ajustando velocidad, peso y agarre.
3. Crear un Prefab en `Assets/_Project/Prefabs/Vehicles/` con `VehicleController`, `VehicleVFX`, `CollisionHandler` y `Rigidbody2D`.

### Cómo Agregar Nuevos Vehículos de Tráfico
1. Crear el sprite en `Assets/_Project/Art/Traffic/` (PPU = 16).
2. Crear un Prefab en `Assets/_Project/Prefabs/Traffic/` con `TrafficCar`, `BoxCollider2D` y `SpriteRenderer`.
3. Asignar el sprite y prefab en `TrafficManager`.

### Cómo Diseñar o Modificar Circuitos
1. Abrir `TrackManager.cs`.
2. Modificar el arreglo `sections` agregando o editando longitudes (`lengthMeters`), curvaturas (`curvature`), flags de túnel (`isTunnel`) y densidades de tráfico (`trafficDensity`).
3. Ejecutar en Unity el menú `NeonDrift -> Build Everything` para regenerar el circuito.

### Cómo Modificar el Balance de Juego
- **Handling / Físicas**: Seleccionar `Assets/_Project/Data/CarConfig.asset` en el Inspector y tunear `baseTopSpeed`, `steeringSpeed`, `gripFriction`, `driftFriction`, etc.
- **Puntuaciones & Flow**: Seleccionar `Assets/_Project/Data/FlowConfig.asset` para modificar umbrales de multiplicador, tiempo de caída y puntos por maniobra.
- **Costos de Mejoras**: Seleccionar `Assets/_Project/Data/UpgradeConfig.asset` para ajustar la curva económica de Engine, Tires y Turbo.
