using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        private const int Rows = 10;
        private const int Cols = 10;
        private const int MineCount = 15;

        private Button[,] _buttons;
        private bool[,] _mines;
        private int[,] _adjacentMines;
        private bool[,] _revealed;
        private bool[,] _flagged;
        private int _revealedCount;
        private bool _gameOver;
        private DispatcherTimer _timer;
        private int _secondsElapsed;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            GameGrid.Rows = Rows;
            GameGrid.Columns = Cols;
            GameGrid.Children.Clear();

            _buttons = new Button[Rows, Cols];
            _mines = new bool[Rows, Cols];
            _adjacentMines = new int[Rows, Cols];
            _revealed = new bool[Rows, Cols];
            _flagged = new bool[Rows, Cols];
            _revealedCount = 0;
            _gameOver = false;
            _secondsElapsed = 0;

            MinesCounter.Text = MineCount.ToString();
            TimerText.Text = "0";

            SetupTimer();
            GenerateMines();
            CalculateAdjacentMines();
            CreateButtons();
        }

        private void SetupTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _secondsElapsed++;
            TimerText.Text = _secondsElapsed.ToString();
        }

        private void GenerateMines()
        {
            var random = new Random();
            int minesPlaced = 0;

            while (minesPlaced < MineCount)
            {
                int row = random.Next(Rows);
                int col = random.Next(Cols);

                if (!_mines[row, col])
                {
                    _mines[row, col] = true;
                    minesPlaced++;
                }
            }
        }

        private void CalculateAdjacentMines()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (_mines[row, col])
                    {
                        _adjacentMines[row, col] = -1;
                        continue;
                    }

                    int count = 0;

                    for (int r = Math.Max(0, row - 1); r <= Math.Min(Rows - 1, row + 1); r++)
                    {
                        for (int c = Math.Max(0, col - 1); c <= Math.Min(Cols - 1, col + 1); c++)
                        {
                            if (_mines[r, c]) count++;
                        }
                    }

                    _adjacentMines[row, col] = count;
                }
            }
        }

        private void CreateButtons()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    Button button = new Button();
                    button.FontWeight = FontWeights.Bold;
                    button.Margin = new Thickness(1);

                    button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
                    button.PreviewMouseRightButtonDown += Button_PreviewMouseRightButtonDown;

                    _buttons[row, col] = button;
                    GameGrid.Children.Add(button);
                }
            }
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_gameOver) return;

            Button button = (Button)sender;
            int index = GameGrid.Children.IndexOf(button);
            int row = index / Cols;
            int col = index % Cols;

            if (_flagged[row, col]) return;

            RevealCell(row, col);

            if (_mines[row, col])
            {
                GameOver(false);
            }
            else if (_revealedCount == Rows * Cols - MineCount)
            {
                GameOver(true);
            }
        }

        private void Button_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_gameOver) return;

            Button button = (Button)sender;
            int index = GameGrid.Children.IndexOf(button);
            int row = index / Cols;
            int col = index % Cols;

            if (!_revealed[row, col])
            {
                _flagged[row, col] = !_flagged[row, col];
                UpdateButtonAppearance(row, col);

                // Обновляем счетчик мин
                int flaggedCount = 0;
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Cols; c++)
                    {
                        if (_flagged[r, c]) flaggedCount++;
                    }
                }

                MinesCounter.Text = (MineCount - flaggedCount).ToString();
            }
        }

        private void RevealCell(int row, int col)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Cols ||
                _revealed[row, col] || _flagged[row, col])
            {
                return;
            }

            _revealed[row, col] = true;
            _revealedCount++;
            UpdateButtonAppearance(row, col);

            if (_adjacentMines[row, col] == 0)
            {
               
                for (int r = row - 1; r <= row + 1; r++)
                {
                    for (int c = col - 1; c <= col + 1; c++)
                    {
                        if (r == row && c == col) continue;
                        RevealCell(r, c);
                    }
                }
            }
        }

        private void UpdateButtonAppearance(int row, int col)
        {
            Button button = _buttons[row, col];

            if (_flagged[row, col])
            {
                button.Content = "🚩";
                button.Background = Brushes.LightGray;
            }
            else if (!_revealed[row, col])
            {
                button.Content = "";
                button.Background = Brushes.LightGray;
            }
            else if (_mines[row, col])
            {
                button.Content = "💣";
                button.Background = Brushes.Red;
            }
            else
            {
                int count = _adjacentMines[row, col];
                button.Content = count > 0 ? count.ToString() : "";
                button.Background = Brushes.White;

                // Разные цвета для цифр
                switch (count)
                {
                    case 1: button.Foreground = Brushes.Blue; break;
                    case 2: button.Foreground = Brushes.Green; break;
                    case 3: button.Foreground = Brushes.Red; break;
                    case 4: button.Foreground = Brushes.DarkBlue; break;
                    case 5: button.Foreground = Brushes.DarkRed; break;
                    case 6: button.Foreground = Brushes.Teal; break;
                    case 7: button.Foreground = Brushes.Black; break;
                    case 8: button.Foreground = Brushes.Gray; break;
                    default: button.Foreground = Brushes.Black; break;
                }
            }
        }

        private void GameOver(bool isWin)
        {
            _gameOver = true;
            _timer.Stop();

            // Показать все мины
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (_mines[row, col])
                    {
                        _revealed[row, col] = true;
                        UpdateButtonAppearance(row, col);
                    }
                }
            }

            MessageBox.Show(isWin ? "Поздравляем! Вы победили!" : "Игра окончена! Вы проиграли!",
                          "Игра окончена");
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }
    }
}
