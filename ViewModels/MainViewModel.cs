using GameOfLife.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace GameOfLife.ViewModels
{
    /// <summary>
    /// Główny ViewModel aplikacji, zarządzający stanem symulacji, komendami i interakcją z widokiem.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private GridBase _grid;
        private DispatcherTimer _timer;
        private bool _isRunning;
        private int _delay = 100;
        private int _boardWidth = 100;
        private int _boardHeight = 100;
        private string _ruleString = "B3/S23";
        private Topology _selectedTopology = Topology.Square;
        private ColoringModel _selectedColoring = ColoringModel.Standard;
        private double _zoom = 10.0;
        private bool _useCircles = false;
        private long _refreshCounter = 0;

        /// <summary> Obiekt siatki przechowujący stan komórek. </summary>
        public GridBase Grid
        {
            get => _grid;
            set => SetProperty(ref _grid, value);
        }

        /// <summary> Licznik wymuszający odświeżenie widoku (GridCanvas). </summary>
        public long RefreshCounter
        {
            get => _refreshCounter;
            set => SetProperty(ref _refreshCounter, value);
        }

        /// <summary> Określa, czy animacja jest w toku. </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    if (_isRunning) _timer.Start();
                    else _timer.Stop();
                }
            }
        }

        /// <summary> Opóźnienie między krokami animacji w milisekundach. </summary>
        public int Delay
        {
            get => _delay;
            set
            {
                if (SetProperty(ref _delay, value))
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(_delay);
                }
            }
        }

        public int BoardWidth
        {
            get => _boardWidth;
            set => SetProperty(ref _boardWidth, value);
        }

        public int BoardHeight
        {
            get => _boardHeight;
            set => SetProperty(ref _boardHeight, value);
        }

        /// <summary> Ciąg znaków definiujący reguły (np. B3/S23). </summary>
        public string RuleString
        {
            get => _ruleString;
            set
            {
                if (SetProperty(ref _ruleString, value))
                {
                    Grid.SetRules(_ruleString);
                }
            }
        }

        /// <summary> Poziom powiększenia planszy. </summary>
        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        /// <summary> Czy rysować komórki jako koła (zamiast domyślnych kształtów topologii). </summary>
        public bool UseCircles
        {
            get => _useCircles;
            set => SetProperty(ref _useCircles, value);
        }

        /// <summary> Wybrana topologia siatki. </summary>
        public Topology SelectedTopology
        {
            get => _selectedTopology;
            set
            {
                if (SetProperty(ref _selectedTopology, value))
                {
                    CreateNewBoard();
                }
            }
        }

        /// <summary> Wybrany model kolorowania. </summary>
        public ColoringModel SelectedColoring
        {
            get => _selectedColoring;
            set
            {
                if (SetProperty(ref _selectedColoring, value))
                {
                    if (Grid != null)
                    {
                        Grid.Coloring = value;
                        RefreshCounter++;
                    }
                }
            }
        }

        public ObservableCollection<Topology> Topologies { get; } = new(Enum.GetValues<Topology>().Cast<Topology>());
        public ObservableCollection<ColoringModel> ColoringModels { get; } = new(Enum.GetValues<ColoringModel>().Cast<ColoringModel>());

        /// <summary> Statystyki udostępniane dla widoku. </summary>
        public Statistics Stats => Grid.Stats;

        // Komendy UI
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand StepCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand RandomizeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ExportImageCommand { get; }
        public ICommand NewBoardCommand { get; }
        public ICommand LoadPatternCommand { get; }

        public MainViewModel()
        {
            _grid = new SquareGrid(_boardWidth, _boardHeight);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_delay) };
            _timer.Tick += (s, e) => Step();

            StartCommand = new RelayCommand(_ => IsRunning = true);
            StopCommand = new RelayCommand(_ => IsRunning = false);
            StepCommand = new RelayCommand(_ => Step(), _ => !IsRunning);
            ClearCommand = new RelayCommand(_ => { Grid.Clear(); RefreshCounter++; OnPropertyChanged(nameof(Stats)); });
            RandomizeCommand = new RelayCommand(_ => 
            { 
                int colors = _selectedColoring switch {
                    ColoringModel.Immigration => 2,
                    ColoringModel.QuadLife => 4,
                    _ => 1
                };
                Grid.Randomize(0.2, colors); 
                RefreshCounter++;
                OnPropertyChanged(nameof(Stats));
            });
            SaveCommand = new RelayCommand(_ => SaveToFile());
            LoadCommand = new RelayCommand(_ => LoadFromFile());
            ExportImageCommand = new RelayCommand(ExportImage);
            NewBoardCommand = new RelayCommand(_ => CreateNewBoard());
            LoadPatternCommand = new RelayCommand(p => LoadPattern(p?.ToString()));
        }

        /// <summary> Zdarzenie żądania eksportu obrazu, obsługiwane w code-behind MainWindow. </summary>
        public event Action? RequestExportImage;

        private void ExportImage(object? obj)
        {
            RequestExportImage?.Invoke();
        }

        /// <summary> Wykonuje krok symulacji i odświeża statystyki/widok. </summary>
        private void Step()
        {
            Grid.Step();
            RefreshCounter++;
            OnPropertyChanged(nameof(Stats));
        }

        /// <summary> Tworzy nową instancję siatki na podstawie wybranych ustawień. </summary>
        private void CreateNewBoard()
        {
            IsRunning = false;
            Grid = _selectedTopology switch
            {
                Topology.Square => new SquareGrid(BoardWidth, BoardHeight),
                Topology.Hexagonal => new HexagonalGrid(BoardWidth, BoardHeight),
                Topology.Triangular => new TriangularGrid(BoardWidth, BoardHeight),
                _ => new SquareGrid(BoardWidth, BoardHeight)
            };
            Grid.Coloring = _selectedColoring;
            Grid.SetRules(RuleString);
            RefreshCounter++;
            OnPropertyChanged(nameof(Stats));
        }

        /// <summary> Wstawia predefiniowany wzorzec w centrum planszy. </summary>
        private void LoadPattern(string? patternName)
        {
            if (string.IsNullOrEmpty(patternName) || patternName == "Wybierz wzorzec...") return;

            int midX = Grid.Width / 2;
            int midY = Grid.Height / 2;

            if (patternName == "Szybowiec (Glider)")
            {
                Grid.SetCell(midX, midY - 1, 1);
                Grid.SetCell(midX + 1, midY, 1);
                Grid.SetCell(midX - 1, midY + 1, 1);
                Grid.SetCell(midX, midY + 1, 1);
                Grid.SetCell(midX + 1, midY + 1, 1);
            }
            else if (patternName == "Działo (Gosper Glider Gun)")
            {
                int[][] gun = new int[][] {
                    new int[] {25, 1}, new int[] {23, 2}, new int[] {25, 2}, new int[] {13, 3}, new int[] {14, 3}, new int[] {21, 3}, new int[] {22, 3}, new int[] {35, 3}, new int[] {36, 3},
                    new int[] {12, 4}, new int[] {16, 4}, new int[] {21, 4}, new int[] {22, 4}, new int[] {35, 4}, new int[] {36, 4}, new int[] {1, 5}, new int[] {2, 5}, new int[] {11, 5},
                    new int[] {17, 5}, new int[] {21, 5}, new int[] {22, 5}, new int[] {1, 6}, new int[] {2, 6}, new int[] {11, 6}, new int[] {15, 6}, new int[] {17, 6}, new int[] {18, 6},
                    new int[] {23, 6}, new int[] {25, 6}, new int[] {11, 7}, new int[] {17, 7}, new int[] {25, 7}, new int[] {12, 8}, new int[] {16, 8}, new int[] {13, 9}, new int[] {14, 9}
                };
                foreach (var p in gun) Grid.SetCell(p[0], p[1], 1);
            }
            else if (patternName == "Pulsar")
            {
                int[] offsets = { -4, -3, -2, 2, 3, 4 };
                foreach (int i in offsets) {
                    foreach (int j in new[] { -1, 1 }) {
                        Grid.SetCell(midX + i, midY + j * 6, 1);
                        Grid.SetCell(midX + j * 6, midY + i, 1);
                        Grid.SetCell(midX + i, midY + j * 1, 1);
                        Grid.SetCell(midX + j * 1, midY + i, 1);
                    }
                }
            }
            else if (patternName == "Blok (Niezmienny)")
            {
                Grid.SetCell(midX, midY, 1);
                Grid.SetCell(midX + 1, midY, 1);
                Grid.SetCell(midX, midY + 1, 1);
                Grid.SetCell(midX + 1, midY + 1, 1);
            }

            RefreshCounter++;
            OnPropertyChanged(nameof(Stats));
        }

        private void SaveToFile()
        {
            var sfd = new SaveFileDialog { Filter = "Pliki JSON (*.json)|*.json|Wszystkie pliki (*.*)|*.*" };
            if (sfd.ShowDialog() == true)
            {
                var data = new {
                    Width = Grid.Width,
                    Height = Grid.Height,
                    Cells = Grid.Cells,
                    Rules = RuleString,
                    Topology = SelectedTopology,
                    Coloring = SelectedColoring
                };
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(data));
            }
        }

        private void LoadFromFile()
        {
            var ofd = new OpenFileDialog { Filter = "Pliki JSON (*.json)|*.json|Wszystkie pliki (*.*)|*.*" };
            if (ofd.ShowDialog() == true)
            {
                var json = File.ReadAllText(ofd.FileName);
                dynamic data = JsonConvert.DeserializeObject(json);

                BoardWidth = data.Width;
                BoardHeight = data.Height;
                RuleString = data.Rules;
                SelectedTopology = (Topology)data.Topology;
                SelectedColoring = (ColoringModel)data.Coloring;

                CreateNewBoard();
                int[] cells = data.Cells.ToObject<int[]>();
                Array.Copy(cells, Grid.Cells, cells.Length);
                RefreshCounter++;
                OnPropertyChanged(nameof(Stats));
            }
        }
    }
}
