using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedQuizQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @F int,@G int,@E int,@M int,@H int;
SELECT @F=Id FROM Franchises WHERE Name=N'GameSense Quiz'; IF @F IS NULL BEGIN INSERT Franchises(Name) VALUES(N'GameSense Quiz'); SET @F=SCOPE_IDENTITY(); END;
SELECT @G=Id FROM Games WHERE Name=N'Video Game Knowledge Quiz' AND FranchiseId=@F; IF @G IS NULL BEGIN INSERT Games(Name,ReleaseYear,FranchiseId) VALUES(N'Video Game Knowledge Quiz',2026,@F); SET @G=SCOPE_IDENTITY(); END;
SELECT @E=Id FROM QuestionCategories WHERE Name=N'Easy'; IF @E IS NULL BEGIN INSERT QuestionCategories(Name) VALUES(N'Easy'); SET @E=SCOPE_IDENTITY(); END;
SELECT @M=Id FROM QuestionCategories WHERE Name=N'Medium'; IF @M IS NULL BEGIN INSERT QuestionCategories(Name) VALUES(N'Medium'); SET @M=SCOPE_IDENTITY(); END;
SELECT @H=Id FROM QuestionCategories WHERE Name=N'Hard'; IF @H IS NULL BEGIN INSERT QuestionCategories(Name) VALUES(N'Hard'); SET @H=SCOPE_IDENTITY(); END;
INSERT Questions(GameId,CategoryId,Difficulty,QuestionText,ExpectedAnswer,EvaluationCriteria,IsActive,CreatedAt,QuestionWeight)
SELECT @G,C,D,Q,A,N'Accept the expected answer or a semantically equivalent answer.',1,GETUTCDATE(),W FROM (VALUES
(@E,1,N'¿Cómo se llama el reino que explora Link en la mayoría de los juegos de The Legend of Zelda?',N'Hyrule.',1),
(@E,1,N'¿Qué famosa franquicia de RPG de Square Enix introdujo el sistema de combate por turnos ATB (Active Time Battle)?',N'Final Fantasy.',1),
(@E,1,N'¿Cuál es el arma inicial e icónica de Gordon Freeman en la saga de FPS Adventure Half-Life?',N'Una palanca (pata de cabra).',1),
(@E,1,N'¿Qué juego de acción y aventura ganó el premio al Juego del Año (GOTY) en The Game Awards 2018?',N'God of War.',1),
(@E,1,N'¿Qué botón debes mantener presionado en los juegos clásicos de Pokémon para correr tras conseguir las zapatillas?',N'Botón B.',1),
(@E,1,N'¿Qué clase de personaje en los RPG se especializa tradicionalmente en magia curativa y soporte?',N'Clérigo / Mago Blanco.',1),
(@E,1,N'¿Cómo se llama la inteligencia artificial enemiga que controla las instalaciones en el FPS System Shock?',N'SHODAN.',1),
(@E,1,N'¿Qué juego de disparos táctico en primera persona desarrollado por Riot Games enfrenta a dos equipos de 5 contra 5?',N'Valorant.',1),
(@E,1,N'¿Cuál es el nombre del planeta prisión donde se desarrolla la acción del clásico FPS Unreal?',N'Na Pali.',1),
(@E,1,N'¿Qué entrega de la saga Uncharted subtitulada El desenlace del ladrón fue nominada al GOTY en 2016?',N'Uncharted 4.',1),
(@E,1,N'¿Qué mecánica de movimiento icónica define al juego de parkour en primera persona Mirror''s Edge?',N'Parkour.',1),
(@E,1,N'¿Qué barra de energía clásica determina cuántas habilidades mágicas puedes usar en un RPG?',N'Barra de Maná (MP).',1),
(@E,1,N'¿Cuál es el nombre de la corporación farmacéutica responsable del desastre biológico en Resident Evil?',N'Umbrella Corporation.',1),
(@E,1,N'¿Qué juego cooperativo de acción y plataformas ganó el premio GOTY en 2021?',N'It Takes Two.',1),
(@E,1,N'¿Cómo se llama el contenedor que aumenta la salud máxima de Samus en la saga Metroid?',N'Tanque de Energía (Energy Tank).',1),
(@M,2,N'¿Qué objeto o habilidad necesitas activar en Super Metroid para romper el tubo de vidrio y entrar formalmente a la zona de Maridia?',N'Una Power Bomb (Bomba de Energía).',2),
(@M,2,N'¿Qué mecánica de diseño de niveles utiliza Dark Souls para conectar zonas lejanas con la hoguera principal sin usar pantallas de carga?',N'Atajos (Shortcuts) mediante puertas cerradas, ascensores o escaleras.',2),
(@M,2,N'¿Qué juego de rol de mundo abierto de CD Projekt Red ganó el GOTY en el año 2015?',N'The Witcher 3: Wild Hunt.',2),
(@M,2,N'¿Cómo se llama el efecto de diseño de audio en los FPS donde los sonidos cambian según el material de la habitación (eco, amortiguación)?',N'Reverberación / Oclusión acústica.',2),
(@M,2,N'¿Qué dos juegos de la saga Dishonored mezclan el género FPS con mecánicas de sigilo inmersivo y poderes sobrenaturales?',N'Dishonored y Dishonored 2.',2),
(@M,2,N'¿Qué juego de aventura y acción de FromSoftware centrado en la cultura shinobi ganó el GOTY en 2019?',N'Sekiro: Shadows Die Twice.',2),
(@M,2,N'¿Cuál es el nombre técnico del truco de diseño que limita la visión del jugador con niebla para ocultar la carga de texturas (famoso en Silent Hill)?',N'Fogging (Niebla de renderizado).',2),
(@M,2,N'¿Qué RPG de acción y ciencia ficción de BioWare permite importar tus decisiones a lo largo de una trilogía completa?',N'Mass Effect.',2),
(@M,2,N'¿Qué shooter en primera persona de 2023 ambientado en el universo de Borderlands fracasó en críticas tras cambiar su fórmula a un RPG de cartas?',N'Trineverse / Neo-Borderlands títulos móviles (o New Tales from the Borderlands / Borderlands Echoes).',2),
(@M,2,N'¿Qué dos juegos independientes de aventura y acción fueron nominados a GOTY en 2018 y 2020 respectivamente?',N'Celeste (2018) y Hades (2020).',2),
(@M,2,N'¿Qué nombre recibe la técnica de diseño que guía al jugador usando luces, colores llamativos o líneas naturales en el mapa?',N'Guiado visual (o líneas de guía / Breadcrumbs).',2),
(@M,2,N'¿Qué juego de la saga Fallout (FPS Adventure/RPG) fue desarrollado por Obsidian Entertainment y es considerado de culto por sus misiones?',N'Fallout: New Vegas.',2),
(@M,2,N'¿Qué elemento de la interfaz (HUD) en los FPS tradicionales te indica cuánta munición te queda en el cargador actual?',N'El contador de munición de la interfaz.',2),
(@M,2,N'¿Qué RPG táctico basado en el universo de Dungeons & Dragons se coronó como el indiscutible GOTY en 2023?',N'Baldur''s Gate 3.',2),
(@M,2,N'¿Qué clásico FPS Adventure de Nintendo GameCube implementó por primera vez una vista en primera persona dentro del casco de Samus Aran?',N'Metroid Prime.',2),
(@M,2,N'¿Qué juego de acción y aventura espacial de mundo abierto nominado al GOTY en 2019 te atrapa en un bucle temporal de 22 minutos?',N'Outer Wilds.',2),
(@M,2,N'¿Cómo se llama la barra o estadística en los RPG que define el orden de actuación de los personajes en un combate por turnos?',N'Velocidad / Iniciativa (o barra de ATB).',2),
(@M,2,N'¿Qué FPS clásico introdujo el concepto de Rocket Jumping como mecánica avanzada de movimiento?',N'Quake.',2),
(@H,3,N'Además de usar una Power Bomb en el tubo de vidrio de Brinstar, ¿qué otra ruta alternativa y oculta existe en Super Metroid para ingresar a Maridia desde la zona superior?',N'Entrando a través de la zona de ascensores de Red Brinstar, usando el rayo de enganche (Grapple Beam) en el techo roto tras derrotar a Draygon o mediante zonas de agua profunda verticales.',3),
(@H,3,N'¿Qué término de diseño de niveles acuñado por Valve describe una habitación segura donde el jugador aprende una nueva mecánica sin peligro de morir antes de aplicarla en el nivel?',N'La habitación verde o Zona de Entrenamiento Seguro (Safe Training Area / Sandboxes pedagógicos).',3),
(@H,3,N'¿Qué tres videojuegos compitieron y fueron nominados junto a Elden Ring y God of War Ragnarök para el GOTY de 2022?',N'A Plague Tale: Requiem, Horizon Forbidden West y Stray.',3),
(@H,3,N'¿Qué patrón geométrico específico utilizan los diseñadores de niveles en juegos como Doom para crear zonas de combate circulares que evitan que el jugador se arrincone?',N'El diseño en arena o Círculos de la muerte (Kiting arenas).',3),
(@H,3,N'¿Cuál es el nombre de la técnica de optimización gráfica que renderiza únicamente los objetos que se encuentran dentro del campo de visión directo de la cámara del jugador?',N'Frustum Culling.',3),
(@H,3,N'En la trilogía original de Mass Effect, ¿qué motor de físicas provocaba el famoso error de colisión que hacía flotar los vehículos Mako por el mapa?',N'PhysX / Motor Unreal Engine 3 modificado.',3),
(@H,3,N'¿Qué FPS Adventure de 2021 desarrollado por Arkane Studios basa toda su progresión en romper un bucle temporal asesinando a 8 objetivos en un solo día?',N'Deathloop.',3),
(@H,3,N'¿Qué videojuego de rol y acción ostenta el récord de ser el primer título de un estudio polaco en ser nominado a más de 5 categorías en The Game Awards?',N'Cyberpunk 2077 (o The Witcher 3).',3),
(@H,3,N'¿Qué limitación técnica de la consola Nintendo 64 obligó a los diseñadores de The Legend of Zelda: Ocarina of Time a dividir el Mercado de Hyrule en pantallas estáticas pre-renderizadas?',N'El límite de memoria de la textura del cartucho y la falta de potencia de la GPU para renderizar multitudes en 3D real.',3),
(@H,3,N'¿Qué juego de disparos y aventura en primera persona de 2012 introdujo el aclamado sistema de fuego dinámico que se propaga por la vegetación según la dirección del viento?',N'Far Cry 3.',3),
(@H,3,N'¿Qué nombre recibe la prueba de diseño donde se crea un nivel usando solo bloques grises o geométricos simples para testear la diversión antes de añadir arte final?',N'Greyboxing (o Whiteboxing).',3),
(@H,3,N'¿Qué título de rol postapocalíptico de 1997 utilizaba el sistema estadístico S.P.E.C.I.A.L. antes de convertirse en un FPS en su tercera entrega?',N'Fallout.',3),
(@H,3,N'En el diseño de sistemas de recompensas de RPG, ¿cómo se denomina al algoritmo que ajusta la probabilidad de obtener un objeto raro para evitar que el jugador se frustre tras muchos intentos fallidos?',N'Pity Timer (o Sistema de Pseudo-aleatoriedad).',3),
(@H,3,N'¿Qué FPS clásico de PC del año 2000 mezclaba elementos de simulación inmersiva, hackeo, RPG y conspiraciones globales bajo la dirección de Warren Spector?',N'Deus Ex.',3),
(@H,3,N'¿Qué juego de acción y aventura de Remedy Entertainment nominado al GOTY en 2019 comparte el mismo universo conectado que Alan Wake?',N'Control.',3),
(@H,3,N'¿Qué regla de diseño en los FPS de terror (como Alien: Isolation) hace que la inteligencia artificial del enemigo tenga dos cerebros: uno que sabe dónde está el jugador y otro que lo busca físicamente?',N'El sistema de dos hilos (Director AI y Xenomorph AI).',3),
(@H,3,N'¿Qué juego de rol y estrategia de Square Enix para PS1 utilizaba un sistema de diseño de niveles basado en cuadrículas de altura variable (eje Z) y peso de armas para determinar el alcance del daño?',N'Final Fantasy Tactics (o Vagrant Story).',3)
) S(C,D,Q,A,W) WHERE NOT EXISTS(SELECT 1 FROM Questions X WHERE X.QuestionText=S.Q);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DECLARE @G int; SELECT @G=Id FROM Games WHERE Name=N'Video Game Knowledge Quiz'; IF @G IS NOT NULL DELETE FROM Questions WHERE GameId=@G;");
        }
    }
}
