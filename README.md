# Gra 4 w rzędzie

Zespół: Adam 73834, Olgierd 73715

Nazwa projektu: Gra „4 w rzędzie”.

Założenia gry: Gra "4 w rzędzie" to klasyczna gra logiczna dla dwóch graczy, w której celem jest ułożenie czterech swoich pionków w rzędzie, kolumnie lub na skos na planszy o siatce 6x7 pól.

Zasady gry: Gra odbywa się na pionowej, sześciorzędowej i siedmiokolumnowej planszy. Gracze wykonują ruchy na przemian. Celem każdego gracza jest ułożenie czterech swoich pionków w jednym ciągu (poziomo, pionowo lub na skos) przed przeciwnikiem. Wygrywa gracz, który pierwszy ułoży cztery pionki w rzędzie: Gdy jeden z graczy ułoży cztery swoje pionki w jednym ciągu, ogłasza się zwycięstwo, a gra kończy się. Jeśli plansza zostanie całkowicie zapełniona, a żaden z graczy nie ułożył czterech swoich pionków w rzędzie, gra kończy się remisem.

Parametry programu: Język: C# Program typu klient-serwer: połączenie klient1-serwer i klient2-serwer.

Aby uruchomić i skompilować projekt "Gra 4 w rzędzie" na swoim komputerze, wykonaj następujące kroki:

1. Sklonuj repozytorium za pomocą Visual Studio lub innego programu do zarządzania repozytoriami Git. Użyj linku: https://github.com/ogerszynski/gra_4_w_rzedzie.git.

2. Otwórz Visual Studio i wybierz opcję "Otwórz projekt lub rozwiązanie". Znajdź i wybierz plik 4to4.sln z katalogu repozytorium.

3. Skompiluj projekt, wybierając z menu "Kompiluj" > "Kompiluj rozwiązanie".

4. Po zakończeniu kompilacji, przejdź do katalogu 4to4/bin/Debug/net8.0-windows/ i uruchom plik 4to4.exe, aby rozpocząć grę.

5. Do sprawdzenia działania na jednym komputerze plik .exe należy uruchomić 3 razy, w pierwszym oknie wybrać Start server, w dwóch kolejnych Connect to server, gdzie wpisujemy adres serwera IPv4 i możemy rozpocząć grę!

Program umożliwia przywrócenie stanu rozgrywki po rozłączeniu użytkownika z serwerem (przycisk updateboard).

Dodatkowy opis uruchamiania i rozgrywki znajduje się w załączonym tutorialu wideo.
