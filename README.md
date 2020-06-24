# DAN_XXXVII_MilosPeric

Daily Task 37

Napraviti konzolnu aplikaciju koja simulira utovar i istovar 10 kamiona. Na
samom početku potrebno je upisati 1000 random izgnerisanih brojeva od 1 do 5000
koji predstavljaju oznake mogućih ruta do odredišta. Za to vreme menadžer čeka
prikaz mogućih ruta kako bi odabrao najbolju za vozače kamiona. Menadžer
najviše može da čeka 3 sekunde, nakon čega treba da javi vozačima da su rute
odabrane, koje su to rute i da mogu da pokrenu proces utovara, a nakon toga da
krenu. Sistem sam bira najbolje rute, a najbolje su one koje je u fajlu
predstavljaju najmanje brojeve deljive sa 3.

U svakom trenutku se 2 kamiona mogu puniti ispovremeno. Punjenje traje između
500ms i 5 sec. Čim se dva kamiona napune, pune se sledeća dva i tako redom.
Kada se svi kamioni napune, svaki posebno dobija gore odabranu rutu. Svaki
vozač treba da javi odredištu da je krenuo i da isporuku mogu da očekuju između
500ms i 5 sec. Odredište na kome će se desiti istovar čeka najviše 3000ms
sekundi da kamion dođe. Ukoliko kamion ne stigne, odredište otkazuje
porudžbinu, a kamion se vraća na početnu tačku. Da bi se kamion vratio na
početku tačku, treba mu onoliko vremena koliko je potrošio do tačke gde mu je
javljeno da se mora vratiti. Kada kamion stigne do odredišta, istovar traje 1.5
puta manje nego njegov utovar.

U konzoli je potrebno logovati sve akcije.
