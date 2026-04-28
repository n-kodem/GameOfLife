using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GameOfLife.Models;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GameOfLife.ViewModels
{
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

        public GridBase Grid
        {
            get => _grid;
            set => SetProperty(ref _grid, value);
        }

        public long RefreshCounter
        {
            get => _refreshCounter;
            set => SetProperty(ref _refreshCounter, value);
        }

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

        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        public bool UseCircles
        {
            get => _useCircles;
            set => SetProperty(ref _useCircles, value);
        }

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
        }

        public event Action? RequestExportImage;

        private void ExportImage(object? obj)
        {
            RequestExportImage?.Invoke();
        }

        private void Step()
        {
            Grid.Step();
            RefreshCounter++;
            OnPropertyChanged(nameof(Stats));
        }

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
            
            RuleString = Grid.DefaultRules;
            
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
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                if (data == null) return;

                BoardWidth = (int)data.Width;
                BoardHeight = (int)data.Height;
                RuleString = (string)data.Rules;
                SelectedTopology = (Topology)(int)data.Topology;
                SelectedColoring = (ColoringModel)(int)data.Coloring;

                CreateNewBoard();
                int[] cells = ((Newtonsoft.Json.Linq.JArray)data.Cells).ToObject<int[]>() ?? Array.Empty<int>();
                Array.Copy(cells, Grid.Cells, Math.Min(cells.Length, Grid.Cells.Length));
                RefreshCounter++;
                OnPropertyChanged(nameof(Stats));
            }
        }
    }
}
