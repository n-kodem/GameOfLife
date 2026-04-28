# Sprawozdanie z laboratorium:

## "Gra w Życie" w środowisku WPF

**Wykonawca:** Nikodem Reszka

**Środowisko wykonawcze:** Linux, .NET (WPF przez VM )

---

## Uwaga wstępna

Zadanie polegało na implementacji automatu komórkowego „Gra w Życie” Conwaya. Projekt został zrealizowany w technologii WPF z wykorzystaniem języka C#. Aplikacja rozszerza klasyczny model o dodatkowe topologie siatki oraz mechanizmy kolorowania komórek, co pozwala na bardziej zaawansowane eksperymenty symulacyjne.

---

## 1. Wybór technologii i architektura

Aplikacja została zaprojektowana zgodnie ze wzorcem **MVVM (Model-View-ViewModel)**, co pozwoliło na pełne oddzielenie logiki symulacji od warstwy interfejsu użytkownika.

Projekt wykorzystuje:

* XAML do definicji interfejsu
* Bindingi i konwertery do komunikacji między warstwami
* Style, szablony oraz animacje Storyboard

Struktura projektu obejmuje:

* `Models/` – logika gry i definicje siatek
* `ViewModels/` – logika aplikacji i sterowanie UI
* `Views/` – komponenty wizualne (np. `GridCanvas`)
* `Helpers/` – klasy pomocnicze i konwertery

---

## 2. Optymalizacja wydajności

W celu optymalizacji:

* zastosowano `DrawingVisual` zamiast ciężkich elementów UI
* wprowadzono **dwuwarstwowy rendering**:

  * warstwa statyczna (siatka)
  * warstwa dynamiczna (żywe komórki)

Dzięki temu aplikacja obsługuje płynnie siatki przekraczające rozmiar 1000×1000 komórek.

---

## 3. Implementacja funkcjonalności

Zaimplementowano następujące elementy:

### Topologie siatki

* siatka kwadratowa
* siatka sześciokątna
* siatka trójkątna

### Modele kolorowania (Inheritance Models)

* Standard – jednokolorowa symulacja
* Immigration – dwa kolory, dziedziczenie większości
* QuadLife – cztery kolory i rozszerzone zasady mieszania

### Panel sterowania

* start / pauza symulacji
* regulacja prędkości w czasie rzeczywistym
* pojedynczy krok symulacji

### Interakcja użytkownika

* rysowanie myszą z interpolacją
* automatyczna pauza podczas edycji
* zoom oraz przesuwanie widoku (pan)

### Reguły symulacji

* konfigurowalne reguły w formacie `B/S` (np. `B3/S23`)
* możliwość eksperymentowania z różnymi zestawami reguł

---

## 4. Zoom i personalizacja widoku

Zaimplementowano mechanizm powiększenia oraz dostosowania wyglądu:

* płynny zoom (suwak / scroll)
* możliwość zmiany kształtu komórek (kwadraty / koła)
* eksport widoku do pliku PNG

---

## 5. Obsługa danych i stan aplikacji

Aplikacja umożliwia:

* zapis stanu planszy do pliku JSON
* odczyt zapisanych konfiguracji

Dzięki separacji logiki (MVVM) zarządzanie stanem aplikacji jest niezależne od warstwy wizualnej.

---

## Wykorzystane pojęcia i technologie

* **WPF (Windows Presentation Foundation)** – framework do tworzenia aplikacji desktopowych
* **MVVM** – wzorzec projektowy oddzielający logikę od UI
* **DrawingVisual** – wydajny mechanizm renderowania grafiki
* **Binding / MultiBinding** – mechanizmy wiązania danych
* **Storyboard** – system animacji w WPF


