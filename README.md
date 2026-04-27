# Game of Life - WPF Edition

Zaawansowana aplikacja zrealizowana w technologii WPF, implementująca automat komórkowy "Gra w życie" (Conway's Game of Life) z obsługą wielu topologii, modeli kolorowania oraz optymalizacją dla dużych plansz.

## 🚀 Główne Funkcje

-   **Wsparcie dla wielu topologii:**
    -   Siatka kwadratowa (8 sąsiadów - Moore neighborhood).
    -   Siatka sześciokątna (6 sąsiadów).
    -   Siatka trójkątna (12 sąsiadów).
-   **Modele kolorowania (Inheritance Models):**
    -   **Standard:** Klasyczna, jednokolorowa symulacja.
    -   **Immigration:** Dwa kolory; nowa komórka przyjmuje kolor większości z trzech rodziców.
    -   **QuadLife:** Cztery kolory; specyficzne zasady mieszania barw przy narodzinach.
-   **Interaktywny interfejs:**
    -   Płynne rysowanie myszą z interpolacją (brak przerw przy szybkich ruchach).
    -   Wielopoziomowy zoom i panowanie (przesuwanie widoku).
    -   Możliwość zmiany kształtu komórek (wypełnione kształty lub kółka).
-   **Zarządzanie symulacją:**
    -   Konfigurowalne reguły w formacie `B/S` (np. domyślne `B3/S23`).
    -   Regulacja prędkości symulacji w czasie rzeczywistym.
    -   Zapis i odczyt stanu planszy do plików JSON.
    -   Eksport aktualnego widoku planszy do formatu PNG.
-   **Przykładowe wzorce:** Szybkie wstawianie gotowych struktur (Szybowiec, Gosper Glider Gun, Pulsar).

## 🛠 Technologia i Architektura

Aplikacja została zaprojektowana zgodnie z najlepszymi praktykami programowania w C# i WPF:

-   **MVVM (Model-View-ViewModel):** Pełna separacja logiki biznesowej od warstwy prezentacji.
-   **Wysoka Wydajność:** Wykorzystanie `DrawingVisual` zamiast ciężkich obiektów `Shape`, co pozwala na płynną obsługę siatek przekraczających 1000x1000 komórek.
-   **Dwuwarstwowy Rendering:** Oddzielenie warstwy statycznej siatki od dynamicznej warstwy żywych komórek w celu minimalizacji narzutu procesora.
-   **WPF Concepts:** Wykorzystanie stylów, szablonów, wyzwalaczy (triggers), animacji Storyboard oraz konwerterów (MultiBinding).

## 📖 Instrukcja Obsługi

1.  **Start/Pauza:** Użyj przycisków na górnym pasku, aby kontrolować bieg czasu.
2.  **Rysowanie:** Kliknij i przeciągnij lewym przyciskiem myszy na planszy, aby ożywić komórki. Symulacja pauzuje się automatycznie podczas rysowania.
3.  **Ustawienia:** W panelu bocznym możesz zmienić wymiary planszy (wymaga kliknięcia "Stwórz nową planszę" lub zmiany topologii).
4.  **Zoom:** Użyj suwaka lub kółka myszy (jeśli zaimplementowano), aby przybliżyć widok.
5.  **Reguły:** Możesz eksperymentować z regułami, np. dla siatki Hex polecane jest `B2/S34`.

## 📂 Struktura Projektu

-   `Models/`: Silnik logiczny gry, definicje siatek i zasady sąsiedztwa.
-   `ViewModels/`: Logika aplikacji i powiązania z UI.
-   `Views/`: Dedykowane kontrolki renderujące (`GridCanvas`).
-   `Helpers/`: Konwertery i klasy pomocnicze.

---
Projekt zrealizowany w ramach laboratorium programowania wizualnego.
