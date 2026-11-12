# DOCUMENTO DE DISEÑO DE VIDEOJUEGO (GDD)
## Kenshi: El Camino del Samurai y la Sagrada Yaroa
### Documentación Oficial de Entrega de Proyecto de Videojuegos

---

**Desarrollador:** Alhann Jimenez  
**Matrícula:** 2-21-0018  
**Motor de Desarrollo:** Unity 6 (6000.6.4f1) - Universal Render Pipeline (URP 2D)  
**Lenguaje de Programación:** C# (.NET)  
**Fecha de Entrega:** Noviembre 2026  

---

### TABLA DE CONTENIDOS
1. [Ficha Técnica y Clasificación](#1-ficha-técnica-y-clasificación)
2. [Historia, Lore y Premisa Narrativa](#2-historia-lore-y-premisa-narrativa)
3. [Guion Narrativo y Diálogos por Escena](#3-guion-narrativo-y-diálogos-por-escena)
4. [Tipo de Juego y Perspectiva](#4-tipo-de-juego-y-perspectiva)
5. [Estructura y Diseño de Niveles](#5-estructura-y-diseño-de-niveles)
6. [Arquitectura de Escenas, Flujo y Menús](#6-arquitectura-de-escenas-flujo-y-menús)
7. [Mecánicas de Juego, Controles y Física](#7-mecánicas-de-juego-controles-y-física)
8. [Catálogo de Personajes, Enemigos y Elementos](#8-catálogo-de-personajes-enemigos-y-elementos)
9. [Audio, Banda Sonora y Efectos de Sonido](#9-audio-banda-sonora-y-efectos-de-sonido)
10. [Dirección de Arte y Fondos Panorámicos](#10-dirección-de-arte-y-fondos-panorámicos)
11. [Enlaces de Entrega (GitHub & Itch.io)](#11-enlaces-de-entrega-github--itchio)

---

### 1. FICHA TÉCNICA Y CLASIFICACIÓN

* **Título Oficial:** Kenshi: El Camino del Samurai y la Sagrada Yaroa
* **Género:** Plataformas de Acción 2D / Aventura Retro (Side-scroller)
* **Plataformas de Lanzamiento:** PC (Windows Standalone x64) y WebGL (Navegador)
* **Resolución Nativa / Aspect Ratio:** 1920x1080 (16:9 Full HD)
* **Público Objetivo:** Todo público aficionado a los videojuegos de plataformas clásicos (estilo retro 16-bit), retos de precisión, plataformeo ágil y combate dinámico con katana.
* **Clasificación por Edades Oficial:**
  * **ESRB (Norteamérica):** **Everyone 10+ (E10+)**
    * *Descriptores de contenido:* Violencia de fantasía en estilo Pixel Art (derrota de esqueletos, murciélagos y zombis con ataques de espada y pisotones de salto, sin desmembramientos ni sangre explícita).
  * **PEGI (Europa):** **PEGI 7**
    * *Descriptores de contenido:* Violencia fantástica animada no realista de tono ligero y elementos humorísticos en el desenlace.

---

### 2. HISTORIA, LORE Y PREMISA NARRATIVA

#### **El Lore de la Sagrada Yaroa y el Valle del Bambú:**
En el místico archipiélago oriental del Japón feudal, la prosperidad de la comarca y la fuerza espiritual de sus guerreros dependían de un tesoro ancestral: **La Sagrada Yaroa**, una reliquia culinaria dorada nacida de la combinación perfecta de plátano majado, carne tierna, salsas secretas y queso fundido sagrado, capaz de restaurar el vigor de cualquier ejército y brindar felicidad eterna.

Sin embargo, una noche de luna escarlata, el temible **Rey Esqueleto** y su legión de ultratumba (zombis errantes y murciélagos de la penumbra) asaltaron el santuario mayor. El monarca cadavérico robó la Yaroa para alimentarse de su poder calórico imperecedero, marchitando los campos, corrompiendo la fauna y condenando al reino a una hambruna perpetua.

#### **El Héroe - Kenshi el Samurai:**
Kenshi, el último maestro de la Orden del Honor y del Buen Comer, empuña su katana legendaria y toma un juramento inquebrantable: cruzar las tres regiones prohibidas, vencer a las hordas oscuras contrarreloj y recuperar la Sagrada Yaroa antes de que su magia sea consumida por el Rey Esqueleto.

---

### 3. GUION NARRATIVO Y DIÁLOGOS POR ESCENA

#### **Acto 0: Prólogo (Menú Principal)**
*(Viento soplando sobre las colinas. Kenshi medita contemplando la silueta distante de la Yaroa flotante).*  
* **Voz en off / Texto:**  
  > *"El honor de la espada se demuestra en el campo de batalla, pero la fuerza del alma nace en el buen comer. La Yaroa ha sido arrebatada. El tiempo corre. Desenvaina tu katana, Samurai."*

#### **Acto 1: Despertar en el Bosque Maldito (Nivel 1)**
*(Kenshi desciende al bosque donde la tierra tiembla ante los primeros no-muertos).*  
* **Kenshi (Monólogo interno):**  
  > *"Los muertos se levantan de la tierra... El Rey Esqueleto ya ha comenzado a corromper el valle. Debo avanzar rápido, recoger las monedas sagradas y no detenerme."*
* **Evento en juego:** Al cruzar la Puerta Torii final:  
  > *"¡Primera barrera superada! El sendero hacia las alturas del templo ha sido desbloqueado."*

#### **Acto 2: El Ascenso a las Fortalezas Flotantes (Nivel 2)**
*(El terreno se vuelve traicionero; abismos profundos y espinas afiladas amenazan cada salto mientras murciélagos oscuros acechan desde el cielo).*  
* **Kenshi:**  
  > *"Un paso en falso y caeré al vacío. Estos esqueletos están mejor armados, pero mi salto y mi acero son más rápidos."*
* **Evento en juego:** Al alcanzar la puerta de la ciudadela:  
  > *"El castillo del Rey Esqueleto está frente a mí. El aroma inconfundible del queso gratinado me guía a la batalla final."*

#### **Acto 3: La Batalla Final en la Arena Oscura (Nivel 3 - Boss)**
*(La niebla se disipa en una sala de piedra. El Rey Esqueleto desenvaina su pesada espada y los suelos crujen a su paso).*  
* **Rey Esqueleto (Jefe):**  
  > *"¡Necio mortal! ¿Has atravesado mi bosque y mis trampas solo por este manjar dorado? ¡La Yaroa es mía, y tus huesos adornarán mi trono!"*  
* **Kenshi:**  
  > *"¡La Yaroa pertenece al pueblo! Por el honor de mi katana, hoy volverás a la tumba."*  
*(Durante el combate, el Rey Esqueleto convoca oleadas de esbirros zombis y embiste con fuerza).*

#### **Acto 4: Victoria Épica y Epílogo (Desenlace)**
*(El Rey Esqueleto cae derrotado desvaneciéndose en humo carmesí. La legendaria Yaroa es expulsada al aire y aterriza frente a Kenshi. El Samurai la recoge alzándola al cielo).*  
* **Pantalla de Victoria en Pantalla Dividida (2 Columnas):**  
  * **Columna Izquierda (Narrativa y Honor):**
    > **¡VICTORIA ÉPICA, SAMURAI!**  
    > **Has derrotado al Rey Esqueleto y conseguido la legendaria YAROA.**  
    > **¡La orden del honor y del buen comer está a salvo!**  
  * **Columna Derecha (Resumen de Monedas):**
    > **RESUMEN DE RECOMPENSAS**  
    > **Monedas conseguidas:** **[X] / 6**  
    > *(Indica si el jugador completó el 100% de la recolección o cuántas monedas le faltaron al finalizar).*  

#### **Mensaje de Muerte / Game Over:**
* **Texto en Pantalla:**  
  > *"¡HAS MUERTO! La oscuridad reclama tu alma... Regresando al Nivel 1 para reiniciar tu entrenamiento."*

---

### 4. TIPO DE JUEGO Y PERSPECTIVA

* **Tipo de Juego:** Videojuego de plataformas en dos dimensiones (2D Side-Scrolling Platformer) con mecánicas híbridas de combate hack & slash ligero y plataformeo de precisión.
* **Perspectiva de Cámara:** Vista lateral ortográfica 2D (Side-View) con seguimiento suave mediante `CameraController2D`, límites dinámicos (`minX`, `maxX`) y fondos escénicos en profundidad con seguimiento horizontal continuo.
* **Inspiraciones y Referencias:**
  * *Super Mario Bros:* Mecánica de salto sobre enemigos (rebound stomp), cabeceo de bloques de ladrillo/pregunta que liberan monedas de oro, y temporizador regresivo de nivel.
  * *Ninja Gaiden / Castlevania Clásico:* Estética de samurai en pixel art, combate cuerpo a cuerpo con katana y ambientación fantástica con no-muertos, esqueletos y bestias de la noche.

---

### 5. ESTRUCTURA Y DISEÑO DE NIVELES

El juego está dividido en 3 niveles lineales de dificultad progresiva, además del Menú Principal interactivo:

```
+---------------+      +-----------+      +-----------+      +-------------------+
|   MainMenu    | ---> |  Level 1  | ---> |  Level 2  | ---> |   Level 3 (Boss)  |
|  (Diorama 2D) |      | (Bosque)  |      | (Ruinas)  |      | (Arena & Yaroa)   |
+---------------+      +-----------+      +-----------+      +-------------------+
```

#### **Nivel 1: El Bosque Maldito (Tutorial y Fundamentos)**
* **Objetivo:** Llegar a la Puerta Torii antes de 100 segundos.
* **Enemigos:** 
  * *Zombis Errantes:* Patrullan de izquierda a derecha. Enseñan la mecánica de eliminar enemigos pisándoles la cabeza o con el corte de katana.
  * *Murciélagos:* Vuelos oscilatorios que exigen medir la altura del salto.
* **Elementos Interactivos:** Bloques de monedas, recolección de monedas de oro, plataformas de piedra y pozos superficiales.
* **Fondo Escénico:** `background1.png` (Bosque crepuscular y templos lejanos).

#### **Nivel 2: Las Ruinas y Fortalezas Flotantes (Plataformeo de Precisión)**
* **Objetivo:** Cruzar plataformas suspendidas en el aire sin caer a los abismos.
* **Amenazas y Obstáculos:**
  * *Abismos sin Fondo:* Caer por un vacío reinicia la vida instantáneamente.
  * *Trampas de Espinas (Spikes):* Trampas terrestres letales que impiden aterrizar en falso.
  * *Esqueletos Guerreros:* Enemigos más ágiles armados con espadas.
  * *Murciélagos Aéreos:* Bloquean las rutas óptimas de salto forzando a atacar en el aire o esquivar.
* **Fondo Escénico:** `background2.png` (Atardecer dramático sobre montañas escarpadas).

#### **Nivel 3: La Cripta del Shogun / Arena del Rey Esqueleto (Batalla Final)**
* **Objetivo:** Enfrentar al Rey Esqueleto en una arena cerrada, reducir su barra de vida a 0 y reclamar la Sagrada Yaroa.
* **Fases del Combate contra el Jefe:**
  * *Fase 1 (Patrulla y Espadazo):* El jefe camina hacia el jugador reproduciendo el sonido de sus pasos de armadura (`walkSound`). Si el jugador se acerca demasiado, realiza un corte pesado.
  * *Fase 2 (Embestida / Dash):* Tras recibir golpes, el jefe acelera su velocidad y embiste a Kenshi, requiriendo esquivas con salto preciso o Dash.
  * *Fase 3 (Invocación de Refuerzos):* El jefe invoca zombis adicionales para desviar la atención del jugador.
* **Recompensa Final (Goal):**
  * Al caer derrotado el jefe, la **Yaroa física** aparece expulsada en parábola en la dirección en la que mira el jugador (`YaroaDropController.cs`).
  * Al tocar la Yaroa, se despliega la pantalla de victoria gloriosa y se retorna al Menú Principal con honor.
* **Fondo Escénico:** `background3.jpg` (Cripta de piedra ancestral y arena de combate).

---

### 6. ARQUITECTURA DE ESCENAS, FLUJO Y MENÚS

#### **1. Escena `MainMenu.unity`:**
* **Diorama Artístico Vivo:** Presenta a Kenshi en reposo junto a la flotante y legendaria Yaroa sobre el terreno, con `background4.png` de fondo en la capa `-100`.
* **Interfaz de Usuario (Canvas):**
  * Botón **"JUGAR"**: Inicia la partida cargando el Nivel 1.
  * Botón **"SALIR"**: Cierra la aplicación (o desactiva la ejecución en el editor).
  * Panel de Controles: Guía gráfica con las teclas de movimiento, salto y ataque.
* **Protección Anti-Doble Clic:** Sistema de cooldown de 0.5s (`loadInputCooldown`) para evitar que el clic de volver al menú desde la pausa pulse accidentalmente el botón "JUGAR".

#### **2. Escenas de Juego (`Level1`, `Level2`, `Level3`):**
* **HUD (Head-Up Display) en Pantalla:**
  * **Vidas:** Indicador de vidas restantes (inicia en 3).
  * **Monedas:** Contador de monedas acumuladas.
  * **Tiempo:** Temporizador regresivo en segundos (100s).
  * **Nombre de Nivel:** Muestra en qué fase se encuentra el jugador (ej: *Nivel: 3 (BOSS)*).
  * **Barra de Vida del Jefe (Exclusiva de Nivel 3):** Barra roja deslizante con indicador numérico de HP del Rey Esqueleto.
  * **Botón de Pausa:** Botón táctil en la esquina superior derecha.

#### **3. Sistema de Pausa Completo (`PauseMenuController.cs`):**
* **Acceso Rápido:** Presionando la tecla **`ESC`**, la tecla **`P`** o haciendo clic sobre el botón de pausa.
* **Inmunidad de Clic:** Al pulsar el botón de pausa, el personaje no realiza el ataque con la katana (`EventSystem.current.IsPointerOverGameObject()`).
* **Menú Emergente:** Ofrece las opciones **"CONTINUAR"** y **"MENÚ PRINCIPAL"**.
* **Gestión de Audio:** Pausa instantáneamente todos los efectos de sonido de combate y pasos, y disminuye la música ambiental al 50% de volumen.

---

### 7. MECÁNICAS DE JUEGO, CONTROLES Y FÍSICA

| Acción | Teclado Primario | Teclado Alternativo / Ratón | Descripción |
| :--- | :--- | :--- | :--- |
| **Moverse a la Izquierda** | `A` | `Flecha Izquierda` | Desplazamiento horizontal con aceleración suave. |
| **Moverse a la Derecha** | `D` | `Flecha Derecha` | Desplazamiento horizontal e inversión de sprite (flip X). |
| **Salto con Peso Vertical** | `Espacio` | `W` / `Flecha Arriba` | Impulso inicial de salto con física de caída pesada. |
| **Ataque de Katana** | `J` | `Clic Izquierdo` (Ratón) | Tajo frontal a corta distancia que daña enemigos y rompe obstáculos. |
| **Dash / Esquiva Rápida** | `K` | `Shift Izquierdo` | Desplazamiento relámpago invulnerable (1.5s de cooldown). |
| **Pausar / Reanudar** | `Escape` | `P` / Botón UI | Detiene el tiempo (`Time.timeScale = 0`) y abre el menú de pausa. |

#### **Física del Salto con Peso Vertical Realista:**
A diferencia del salto flotante tradicional, se implementó en `PlayerController.cs` un algoritmo de gravedad vertical dinámica:
* **Multiplicador de Caída:** `fallGravityMultiplier = 2.6f` incrementa la atracción gravitatoria cuando la velocidad en Y es negativa, otorgando una caída rápida y contundente estilo plataformas clásico.
* **Control en el Aire:** `airControl = 0.85f` restringe ligeramente el viraje libre en el aire para premiar la precisión en el despegue.

---

### 8. CATÁLOGO DE PERSONAJES, ENEMIGOS Y ELEMENTOS

#### **1. Kenshi (El Protagonista)**
* Samurai diestro en artes marciales y esgrima.
* Animaciones integradas en Mecanim: *Idle* (reposo), *Run* (carrera), *Jump* (salto), *Attack* (corte de espada), *Hurt* (impacto) y *Dead* (muerte).

#### **2. Zombi Errante (Enemigo Terrestre - Nivel 1)**
* Camina en patrulla constante rebotando contra paredes y bordes.
* Daño: 1 punto de vida al contacto. Se elimina de 1 corte o 1 pisotón.
* Audio: Sonido ambiental gutural aleatorio desincronizado (intervalo de 15 segundos).

#### **3. Murciélago Oscuro (Enemigo Volador - Niveles 1 y 2)**
* Se desplaza en trayectorias flotantes senoidales.
* Dificulta los saltos entre plataformas en los Niveles 1 y 2.
* Audio: Sonido ambiental de aleteo aleatorio cada 15 segundos.

#### **4. Esqueleto Guerrero (Enemigo Terrestre de Élite - Nivel 2)**
* Portador de armadura ligera y espada de hierro.
* Más veloz que el zombi común; patrulla zonas clave del Nivel 2.

#### **5. Rey Esqueleto (Jefe Final - Boss del Nivel 3)**
* Líder de la legión oscura con 5 puntos de golpe (HP).
* Cuenta con comportamiento en máquina de estados: patrulla pesada, sonido de pasos (`walkSound`), invocación de esbirros zombis y ataques de embestida acelerada.

#### **6. Objetos Interactivos y Coleccionables**
* **Monedas de Oro (Coins):** Monedas giratorias doradas que suman puntuación al marcador global.
* **Bloques de Piedra / Ladrillos:** Se pueden cabecear desde abajo para soltar recompensas.
* **Puerta Torii (Meta Niveles 1 y 2):** Portal tradicional japonés que teletransporta al siguiente nivel.
* **La Sagrada Yaroa (Meta Nivel 3):** Ítem legendario exclusivo que se desprende del jefe al morir, con física de rebote hacia la dirección del jugador.

---

### 9. AUDIO, BANDA SONORA Y EFECTOS DE SONIDO

#### **Banda Sonora (Música de Fondo - BGM):**
* **Menú Principal (`MenuMusic`):** Pista tradicional japonesa con instrumentos de viento (shakuhachi) y cuerdas (koto) en estilo retro chiptune.
* **Niveles 1 y 2 (`LevelMusic`):** Melodía de acción rítmica que impulsa el avance del jugador.
* **Nivel 3 (`BossMusic`):** Orquestación dramática de tambores taiko y ritmo acelerado para la batalla final.

#### **Efectos de Sonido (SFX):**
* `SwordAttack.wav`: Silbido del corte de la katana con cooldown acústico.
* `Jump.wav`: Efecto de impulso elástico al despegar del suelo.
* `Stomp.wav`: Sonido percusivo al aplastar un enemigo desde el aire.
* `Coin.wav`: Campanilla brillante al recoger monedas doradas.
* `BossWalk.wav`: Pasos metálicos y pesados que acompañan al Rey Esqueleto mientras camina (se silencia al pausar).
* `BossHurt.wav` y `BossDeath.wav`: Gemidos espectrales al golpear y vencer al jefe final.
* `AmbientZombie.wav` y `AmbientBat.wav`: Voces y aleteos aleatorios en intervalos desincronizados.

---

### 10. DIRECCIÓN DE ARTE Y FONDOS PANORÁMICOS

Cada nivel cuenta con su propio fondo panorámico posicionado como GameObject en la escena (`Background`), lo que permite manipular su posición (X, Y, Z) y escala libremente desde el editor de Unity, acompañado horizontalmente por la cámara:

* **Menú Principal (`background4.png`):** Fondo místico en capa `-100` que realza el diorama con el samurai y la Yaroa al frente.
* **Nivel 1 (`background1.png`):** Bosque crepuscular japonés con tonos verdosos y templos tradicionales.
* **Nivel 2 (`background2.png`):** Puesta de sol sobre cordilleras y plataformas suspendidas en el cielo.
* **Nivel 3 (`background3.jpg`):** Sala del trono sombría con columnas de piedra y antorchas.

---

### 11. ENLACES DE ENTREGA (GITHUB & ITCH.IO)

> [!IMPORTANT]
> Los siguientes enlaces corresponden a la entrega final del proyecto exigida en la rúbrica de evaluación.

#### **1. Repositorio de Código Fuente en GitHub:**
* **Enlace al Repositorio:** `https://github.com/AlhannJ/Kenshi-Juego2D`
*(Reemplaza este enlace con la URL exacta de tu repositorio de GitHub público o privado con acceso al profesor).*

#### **2. Publicación Jugable en Itch.io:**
* **Enlace al Videojuego en Itch.io:** `https://alhannj.itch.io/kenshi-el-camino-del-samurai`
*(Reemplaza este enlace con la URL pública donde se encuentra publicado tu juego en Itch.io).*

---

### GUÍA RÁPIDA PARA EXPORTAR A PDF
1. **Opción Recomendada:** Abre el archivo complementario **`DOCUMENTACION_VIDEOJUEGO.html`** generado en la carpeta del proyecto con cualquier navegador (Google Chrome, Microsoft Edge).
2. Presiona la combinación de teclas **`Ctrl + P`** (Imprimir).
3. En el campo "Destino", selecciona **"Guardar como PDF"**.
4. En "Márgenes", selecciona **"Predeterminados"** o **"Mínimos"** y asegúrate de marcar la casilla **"Gráficos de fondo"**.
5. Haz clic en **Guardar**. ¡Tendrás un documento PDF con maquetación profesional y tipografía impecable listo para entrega!
