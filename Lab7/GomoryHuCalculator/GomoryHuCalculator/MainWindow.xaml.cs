using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GomoryHuCalculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public partial class MainWindow
{
    private int _nodeCount;

    private int NodeCount
    {
        get => _nodeCount;
        set => _nodeCount = value > 2 ? value : 2;
    }

    public MainWindow()
    {
        NodeCount = 9;

        InitializeComponent();
        GenerateTransportGrid();
        PreDefineGraph();
    }

    private void GenerateTransportGrid()
    {
        TransportGrid ??= new Grid();
        TransportGrid.RowDefinitions.Clear();
        TransportGrid.ColumnDefinitions.Clear();
        TransportGrid.Children.Clear();

        // Create Row Definitions
        for (var i = 0; i <= NodeCount; i++)
        {
            TransportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            TransportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // Create headers (top row and left column)
        for (var i = 0; i <= NodeCount; i++)
        {
            CreateStyledCell(TransportGrid, 0, i + 1, Convert.ToChar('A' + i).ToString(), Brushes.CornflowerBlue, true);
            CreateStyledCell(TransportGrid, i + 1, 0, Convert.ToChar('A' + i).ToString(), Brushes.CornflowerBlue, true);
        }

        // Fill grid cells with TextBoxes for input (except the first row/column for headers)
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                CreateStyledCell(TransportGrid, i, j, 0.ToString(),  Brushes.White, false, true);
            }
        }
    }

    private void PreDefineGraph()
    {
        // Wikipedia
        // double [,]weightMatrix =
        // {
        //     { 0,  1,  7, -1, -1, -1},
        //     { 1,  0,  1,  3,  2, -1},
        //     { 7,  1,  0, -1,  4, -1},
        //     {-1,  3, -1,  0,  1,  6},
        //     {-1,  2,  4,  1,  0,  2},
        //     {-1, -1, -1,  6,  2,  0},
        // };

        // GeekForGeeks
        // double [,]weightMatrix =
        // {
        //     { 0, 10, -1, -1, -1,  8},
        //     {10,  0,  4, -1,  2,  3},
        //     {-1,  4,  0,  5,  4,  2},
        //     {-1, -1,  5,  0,  7,  2},
        //     {-1,  2,  4,  7,  0,  3},
        //     { 8,  3,  2,  2,  3,  0},
        // };

        // DO example
        // double [,]weightMatrix =
        // {
        //     { 0,  8,  9,  7, -1, -1, -1},
        //     { 8,  0, -1,  5,  7, -1, -1},
        //     { 9, -1,  0,  4, -1,  9, -1},
        //     { 7,  5,  4,  0,  4,  6,  8},
        //     {-1,  7, -1,  4,  0, -1,  2},
        //     {-1, -1,  9,  6, -1,  0, 11},
        //     {-1, -1, -1,  8,  2, 11,  0}
        // };

        // My
        // double [,]weightMatrix =
        // {
        //     { 0,  5, -1,  5,  3,  8, -1},
        //     { 5,  0, -1, -1,  9, -1, -1},
        //     {-1, -1,  0,  6,  2, -1,  6},
        //     { 5, -1,  6,  0, -1,  7,  6},
        //     { 3,  9,  2, -1,  0,  4, -1},
        //     { 8, -1, -1,  7,  4,  0, -1},
        //     {-1, -1,  6,  6, -1, -1,  0}
        // };

        // Danik
        // int [,]weightMatrix =
        // {
        //     { 0,  2,  8, -1,  1,  4, -1, -1},
        //     { 2,  0, -1, -1,  4,  7, -1, -1},
        //     { 8, -1,  0,  7, -1,  3, -1, -1},
        //     {-1, -1,  7,  0, -1, -1,  6, -1},
        //     { 1,  4, -1, -1,  0, -1, -1,  2},
        //     { 4,  7,  3, -1, -1,  0, -1,  4},
        //     {-1, -1, -1,  6, -1, -1,  0,  6},
        //     {-1, -1, -1, -1,  2,  4,  6,  0}
        // };

        // 
        // double [,]weightMatrix =
        // {
        //     { 0,  8, -1,  4, -1, -1, -1},
        //     { 8,  0,  7, -1, -1, -1,  5},
        //     {-1,  7,  0,  6,  4, -1,  8},
        //     { 4, -1,  6,  0,  2, -1,  9},
        //     {-1, -1,  4,  2,  0,  3, -1},
        //     {-1, -1, -1, -1,  3,  0,  5},
        //     {-1,  5,  8,  9, -1,  5,  0}
        // };

        // // Vova
        // double [,]weightMatrix =
        // {
        //     { 0,  2, -1,  8,  3,  4, -1},
        //     { 2,  0, -1, -1,  4,  7, -1},
        //     {-1, -1,  0,  4,  2, -1,  6},
        //     { 8, -1,  4,  0, -1, -1,  6},
        //     { 3,  4,  2, -1,  0,  4, -1},
        //     { 4,  7, -1, -1,  4,  0, -1},
        //     {-1, -1,  6,  6, -1, -1,  0}
        // };

        // Max
        double[,] weightMatrix =
        {
            //   a   b   c   d   e   f   g   h   i
            { 0, 12, 8, -1, -1, 4, 1, -1, -1 },
            { 12, 0, -1, -1, 14, -1, -1, -1, -1 },
            { 8, -1, 0, 9, -1, 13, -1, -1, -1 },
            { -1, -1, 9, 0, -1, -1, -1, -1, 16 },
            { -1, 14, -1, -1, 0, -1, 9, 2, -1 },
            { 4, -1, 13, -1, -1, 0, -1, 4, -1 },
            { 1, -1, -1, -1, 9, -1, 0, -1, -1 },
            { -1, -1, -1, -1, 2, 4, -1, 0, 6 },
            { -1, -1, -1, 16, -1, -1, -1, 6, 0 }
        };

        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                ((TextBox)((Border)GetGridElement(TransportGrid, i, j)).Child).Text = weightMatrix[i - 1, j - 1].ToString();
            }
        }
    }

    private UIElement GetGridElement(Grid grid, int row, int column)
    {
        foreach (UIElement element in grid.Children)
        {
            if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
                return element;
        }

        return null;
    }

    // Event handler for TextBox validation: allows only numeric input
    private void Cell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^-?0-9]+"); // Only positive and negative digits are allowed
        e.Handled = regex.IsMatch(e.Text);
    }

    private void SolveButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Read weight matrix from the grid
        var weightMatrix = new double[NodeCount, NodeCount];
        for (var i = 1; i <= NodeCount; i++)
        {
            for (var j = 1; j <= NodeCount; j++)
            {
                var cell = (TextBox)((Border)GetGridElement(TransportGrid, i, j)).Child;
                weightMatrix[i - 1, j - 1] = double.Parse(cell?.Text ?? string.Empty);
            }
        }

        var gomoryHu = new GomoryHuMethod(weightMatrix);

        var (solutionMatrix, iterations, resultingString) = gomoryHu.Solve();

        ShowMatrix(solutionMatrix, iterations, resultingString);

    }

    private void ShowMatrix(double[,] solutionMatrix, string iterations, string resultingString)
    {
        var matrixTab = new TabItem { Header = $"Matrix {GridContainer.Items.Count}" };
        var tabGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition
                {
                    Width = new GridLength(GridContainer.ActualWidth / 2)
                },
                new ColumnDefinition
                {
                    Width = new GridLength(GridContainer.ActualWidth / 6)
                },
                new ColumnDefinition
                {
                    Width = new GridLength(GridContainer.ActualWidth / 3)
                }
            }
        };

        Grid dynamicGrid = new Grid
        {
            Margin = new Thickness(10),
            Background = Brushes.White
        };

        for (int i = 0; i <= NodeCount; i++)
        {
            dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        // Add Headers
        CreateStyledCell(dynamicGrid, 0, 0, " ", Brushes.WhiteSmoke, true);
        for (int i = 0; i < NodeCount; i++)
        {
            CreateStyledCell(dynamicGrid, 0, i + 1, Convert.ToChar('A' + i).ToString(), Brushes.CornflowerBlue, true);
            CreateStyledCell(dynamicGrid, i + 1, 0, Convert.ToChar('A' + i).ToString(), Brushes.CornflowerBlue, true);
        }

        // Fill Matrix Data
        for (int i = 0; i < NodeCount; i++)
        {
            for (int j = 0; j < NodeCount; j++)
            {
                CreateStyledCell(dynamicGrid, i + 1, j + 1, solutionMatrix[i, j].ToString(), Brushes.White);
            }
        }

        tabGrid.Children.Add(dynamicGrid);
        Grid.SetRow(dynamicGrid, 0);
        Grid.SetColumn(dynamicGrid, 0);
        
        var panel = new StackPanel
            {Children = 
                {new ScrollViewer
                    {Content = new TextBlock{Text = iterations}, Height = GridContainer.ActualHeight }
                }, Height = GridContainer.ActualHeight
            };
        tabGrid.Children.Add(panel);
        Grid.SetRow(panel, 0);
        Grid.SetColumn(panel, 1);
        
        panel = new StackPanel
            {Children = 
                {new ScrollViewer
                    {Content = new TextBlock{Text = resultingString}, Height = GridContainer.ActualHeight }
                }, Height = GridContainer.ActualHeight
            };
        tabGrid.Children.Add(panel);
        Grid.SetRow(panel, 0);
        Grid.SetColumn(panel, 2);
        
        matrixTab.Content = tabGrid;
        GridContainer.Items.Add(matrixTab);
    }

    private void CreateStyledCell(Grid grid, int row, int col, string text, Brush background, bool isHeader = false, bool isEditable = false)
    {
        Border cellBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Background = background
        };

        FrameworkElement cellText;
        if(!isEditable)
        {
            cellText = new TextBlock
            {
                Text = text,
                Foreground = isHeader ? Brushes.White : Brushes.DarkSlateGray,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 40,
                Height = 40,
                Padding = new Thickness(5)
            };
        }
        else
        {
            cellText = new TextBox
            {
                Text = text,
                Foreground = isHeader ? Brushes.White : Brushes.DarkSlateGray,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 40,
                Height = 40,
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0),
                
                
            };

            cellText.PreviewTextInput += Cell_PreviewTextInput;
        }

        cellBorder.Child = cellText;

        grid.Children.Add(cellBorder);
        Grid.SetRow(cellBorder, row);
        Grid.SetColumn(cellBorder, col);
    }


    private void MinusButton_OnClick(object sender, RoutedEventArgs e)
    {
        NodeCount--;
        GenerateTransportGrid();
    }

    private void PlusButton_OnClick(object sender, RoutedEventArgs e)
    {
        NodeCount++;
        GenerateTransportGrid();
    }
}