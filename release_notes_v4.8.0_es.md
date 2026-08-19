\# pk3DS Progressive Randomizer v4.8.0



\## Smart Trainer Overhaul



Esta versión agrega una mejora grande al sistema de entrenadores, enfocada en crear equipos más coherentes, mejores movesets, objetos equipados más inteligentes y una curva de dificultad más configurable.



\---



\## Cambios principales



\### Better Movesets



Se agregó un sistema nuevo de \*\*Better Movesets\*\* para entrenadores.



Ahora los movimientos pueden construirse usando varias fuentes:



\* Movimientos por nivel.

\* TMs / HMs.

\* Movimientos de tutor.

\* Egg moves.

\* Movimientos de pre-evoluciones.

\* Movimientos actuales.

\* Fallback de movimientos fuertes cuando hace falta.



El objetivo es evitar sets sin lógica y mejorar la calidad general de los combates.



\---



\### Template global de Better Movesets



La opción global de \*\*Better Movesets\*\* ahora puede usar una plantilla externa:



```txt

Templates/trainer\_better\_movesets.txt

```



La plantilla permite controlar la probabilidad de aplicar Better Movesets según:



\* Tipo de entrenador.

\* Grupo.

\* ID específico.

\* Nivel mínimo.

\* Nivel máximo.

\* Porcentaje de activación.



Ejemplo:



```txt

Any       | 1  | 15  | 20  | Better

Any       | 16 | 30  | 35  | Better

Any       | 31 | 45  | 55  | Better

Any       | 46 | 100 | 70  | Better

Important | 1  | 30  | 80  | Better

Important | 31 | 100 | 100 | Better

```



\---



\### Better Movesets por entrenador



Se agregó control por entrenador desde la ventana de reglas.



Ahora se puede forzar Better Movesets en entrenadores seleccionados, de forma similar a las reglas de movimientos, Smart Items y EVs.



\---



\### Movesets conscientes de habilidades



Better Movesets ahora toma en cuenta habilidades para priorizar movimientos que tengan sinergia.



Ejemplos:



\* \*\*Contrary / Respondón\*\* prioriza movimientos que normalmente bajan stats, como Leaf Storm, Draco Meteor, Overheat, Superpower, Close Combat, Fleur Cannon, entre otros.

\* \*\*Technician\*\* prioriza movimientos de baja potencia que se benefician de la habilidad.

\* \*\*Skill Link\*\* prioriza movimientos multi-hit.

\* \*\*Sheer Force\*\* prioriza movimientos con efectos secundarios útiles.

\* \*\*Prankster\*\* prioriza movimientos de estado.

\* \*\*Regenerator\*\* prioriza movimientos de rotación como Teleport, U-turn, Volt Switch, Flip Turn o Parting Shot.



También se agregaron reglas para habilidades como Adaptability, No Guard, Compound Eyes, Rock Head, Reckless, Iron Fist, Strong Jaw, Mega Launcher, Tough Claws, Triage y otras.



\---



\### Mejor lógica para movimientos de estado



El sistema ya no mete movimientos de estado solo porque están permitidos.



Ahora se toma en cuenta el perfil del Pokémon:



\* Pokémon ofensivos y rápidos tienden a usar más movimientos de daño.

\* Pokémon bulky pueden recibir más movimientos de soporte.

\* Pokémon con Prankster pueden recibir varios movimientos de estado.

\* Pokémon con pocas defensas priorizan ataques en lugar de soporte innecesario.



Esto permite que vuelvan a aparecer sets de 4 ataques y que objetos Choice tengan más sentido.



\---



\### Mejor lógica de STAB y cobertura



Better Movesets ahora intenta priorizar STAB de forma más útil.



Para Pokémon con dos tipos, intenta incluir:



\* Un movimiento STAB del primer tipo.

\* Un movimiento STAB del segundo tipo.

\* Luego cobertura o soporte.



También intenta evitar repetir tipos ofensivos dentro del mismo moveset. Por ejemplo, evita llenar un set con varios movimientos de daño tipo Normal si hay mejores alternativas.



\---



\### Cobertura contra debilidades



Se agregó lógica para priorizar cobertura que ayude contra debilidades importantes.



Ejemplo:



\* Si un Pokémon es débil a Eléctrico y puede aprender un movimiento tipo Tierra, ese movimiento gana prioridad como cobertura.



La prioridad de STAB se mantiene, pero ahora el sistema valora mejor los movimientos que ayudan a cubrir amenazas.



\---



\### Movimientos dependientes de sinergia



Se corrigieron movimientos que no deberían aparecer sin soporte adecuado.



Ejemplos:



\* \*\*Stored Power / Poder Reserva\*\* y \*\*Power Trip / Chulería\*\* requieren movimientos que suban stats.

\* \*\*Last Resort / Última Baza\*\* se bloqueó por ser poco compatible con entrenadores IA.

\* \*\*Dream Eater / Come Sueños\*\* se bloquea si no hay forma de dormir al rival.

\* Movimientos de boost físico, como Swords Dance, ya no se priorizan en Pokémon claramente especiales.

\* Movimientos de boost especial, como Nasty Plot, ya no se priorizan en Pokémon claramente físicos.



\---



\### Lógica de clima



Se mejoró la lógica relacionada con clima.



Ahora movimientos como:



\* Solar Beam.

\* Solar Blade.

\* Hurricane.

\* Thunder.

\* Blizzard.

\* Weather Ball.



solo se priorizan cuando existe un clima compatible confirmado.



También se agregó sinergia de equipo: si un Pokémon del equipo pone clima con habilidad, otros miembros pueden aprovechar ese clima en sus movesets.



\---



\### Rocas de clima



Smart Items ahora prioriza rocas de clima cuando realmente aportan:



\* Rain Dance / Llovizna → Damp Rock.

\* Sunny Day / Sequía → Heat Rock.

\* Sandstorm / Chorro Arena → Smooth Rock.

\* Hail / Snow Warning → Icy Rock.



Pero se corrigió que climas permanentes o especiales no reciban rocas inútiles:



\* Primordial Sea / Mar del Albor ya no usa Damp Rock.

\* Desolate Land / Tierra del Desaliento ya no usa Heat Rock.

\* Delta Stream / Ráfaga Delta ya no usa roca de clima.



\---



\### Trainer Held Item Template



Se agregó una plantilla externa para controlar objetos equipados de Pokémon de entrenadores:



```txt

Templates/trainer\_held\_items.txt

```



La plantilla permite controlar objetos por:



\* Tipo de entrenador.

\* Grupo.

\* ID específico.

\* Nivel.

\* Porcentaje.

\* Modo de selección.



Ejemplo:



```txt

Any       | 1  | 15  | 25  | Random | POOL

Any       | 16 | 30  | 40  | Random | POOL

Any       | 31 | 45  | 60  | Smart  | POOL

Any       | 46 | 100 | 75  | Smart  | POOL

Important | 1  | 100 | 100 | Smart  | POOL

```



\---



\### Smart Items mejorados



Smart Items ahora evalúa mejor el moveset final, habilidad, rol del Pokémon y sinergia con objetos.



Mejoras destacadas:



\* Choice Band, Choice Specs y Choice Scarf ya no se asignan si el set tiene movimientos de estado incompatibles.

\* Choice items sí pueden aparecer con Trick o Switcheroo.

\* Pokémon bulky priorizan objetos defensivos como Leftovers, Sitrus Berry, Black Sludge, Rocky Helmet o Assault Vest.

\* Focus Sash baja prioridad en Pokémon bulky.

\* Power Herb y White Herb tienen más lógica de uso.

\* Se mejoró la selección de objetos según clima.

\* Se evita que objetos de clima salgan en habilidades donde no tienen efecto.



\---



\### Item Clause



Se agregó una opción de \*\*Item Clause\*\*.



Cuando está activa, evita repetir objetos dentro del mismo equipo de entrenador.



Aplica a:



\* Random Items.

\* Smart Items.

\* Held item template.



Esto reduce casos donde todo el equipo termina con el mismo objeto.



\---



\### Progressive BST



Se agregó y mejoró el sistema de \*\*Progressive BST\*\*.



Ahora los Pokémon pueden randomizarse según rangos de BST basados en nivel, permitiendo una progresión más natural:



\* Early game con más variedad.

\* Mid game más estable.

\* Late game con Pokémon más fuertes.

\* Menos posibilidades de que salgan Pokémon muy débiles al final del juego.



También se agregó configuración manual de rangos de BST.



\---



\### Corrección de Progressive BST + Type Theme



Se corrigió un problema donde Progressive BST podía forzar un tipo aunque Type Theme Trainers estuviera apagado.



Ahora:



\* Type Theme ON + Progressive BST ON respeta el tipo temático.

\* Type Theme OFF + Progressive BST ON randomiza por BST sin forzar tipo.



\---



\### Level Caps manuales



Se agregó soporte para configurar level caps manuales en entrenadores importantes.



Los entrenadores regulares pueden ajustarse para no superar el siguiente cap, evitando que queden demasiado fuertes o demasiado atrás en la curva.



\---



\### EVs por entrenador



Se mejoró el flujo para aplicar EVs a entrenadores seleccionados desde la ventana de reglas.



\---



\### MOs olvidables



Se agregó una opción para hacer que las MOs puedan olvidarse.



Esto mejora la calidad de vida en partidas randomizadas.



\---



\### Rare Candies en tiendas



Se agregó una opción para añadir Rare Candies a tiendas normales.



Precio configurado: 10.



\---



\### Zonas de captura y cambios de mapa



Se agregó un botón para habilitar zonas de captura y cambios en mapas, inspirado en mapas modificados estilo Maikiki/Folagor.



\---



\### Barra de progreso



Se agregó una ventana de progreso al randomizar entrenadores.



Esto permite ver en tiempo real que el proceso sigue trabajando, especialmente cuando Better Movesets está activo para muchos entrenadores.



\---



\## Correcciones destacadas



\* Se corrigieron Choice items con movimientos de estado incompatibles.

\* Se corrigió Power Herb / White Herb para reconocer más movimientos útiles.

\* Se corrigieron movimientos climáticos sin clima real.

\* Se corrigió Damp Rock en Primordial Sea.

\* Se corrigió Swords Dance en Pokémon especiales.

\* Se corrigió Stored Power sin boosts.

\* Se corrigió Last Resort en sets incompatibles.

\* Se corrigió Sandstorm en Pokémon que no lo aprovechan.

\* Se corrigieron repeticiones excesivas de items.

\* Se corrigió la aplicación de Smart Items para que ocurra después del moveset final.

\* Se limpió lógica duplicada en Gen6 y Gen7 relacionada con held items.



\---



\## Nota



Después de actualizar, se recomienda randomizar entrenadores de nuevo para que los cambios de Better Movesets, Smart Items, Item Clause y templates se reflejen correctamente.



