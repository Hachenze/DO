using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using GameTheoryAlgorithm.Methods;

namespace GameTheoryAlgorithm;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _a;
    private int _b;

    public MainWindow()
    {
        _a = 3;
        _b = 3;

        InitializeComponent();
        GenerateTransportGrid();
        PreDefine();
    }

    private void AComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _a = int.Parse((AComboBox.SelectedItem as ComboBoxItem)!.Content.ToString()!);
        GenerateTransportGrid();
    }

    private void BComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _b = int.Parse((BComboBox.SelectedItem as ComboBoxItem)!.Content!.ToString()!);
        GenerateTransportGrid();
    }

    private void GenerateTransportGrid()
    {
        TransportGrid ??= new Grid();
        TransportGrid.RowDefinitions.Clear();
        TransportGrid.ColumnDefinitions.Clear();
        TransportGrid.Children.Clear();

        for (int i = 0; i <= _a; i++)
        {
            TransportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        for (int j = 0; j <= _b; j++)
        {
            TransportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // Create headers (top row and left column)
        for (int i = 1; i <= _a; i++)
        {
            CreateStyledCell(TransportGrid, i, 0, "A" + (i - 1), Brushes.CornflowerBlue, true);
        }

        for (int j = 1; j <= _b; j++)
        {
            CreateStyledCell(TransportGrid, 0, j, "B" + (j - 1), Brushes.CornflowerBlue, true);
        }

        // Fill grid cells with TextBoxes for input (except the first row/column for headers)
        for (int i = 1; i <= _a; i++)
        {
            for (int j = 1; j <= _b; j++)
            {
                CreateStyledCell(TransportGrid, i, j, 0.ToString(),  Brushes.White, false, true);
            }
        }
    }

    // Event handler for TextBox validation: allows only numeric input
    private void Cell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+"); // Only digits are allowed
        e.Handled = regex.IsMatch(e.Text);
    }

    private void SolveButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Read game matrix from the grid
        decimal[,] gameMatrix = new decimal[_a, _b];
        for (int i = 1; i <= _a; i++)
        {
            for (int j = 1; j <= _b; j++)
            {
                var cell = (TextBox)((Border)GetGridElement(TransportGrid, i, j)).Child;
                gameMatrix[i - 1, j - 1] = decimal.Parse(cell.Text);
            }
        }

        // Solve using GameTheory algorithm
        var gameTheory = new GameTheory(gameMatrix, this);
        (List<decimal> aStrategy, List<decimal> bStrategy, decimal gamePrice) = gameTheory.Solve();
        aStrategy = aStrategy.Select(x => Math.Round(x, 2)).ToList();
        bStrategy = bStrategy.Select(x => Math.Round(x, 2)).ToList();
        gamePrice = Math.Round(gamePrice, 2);

        // Display result (total cost)
        ShowResultInNewTab(aStrategy, bStrategy, gamePrice);
    }
    public void ShowResultInNewTab(List<decimal> aStrategy, List<decimal> bStrategy, decimal gamePrice)
    {
        // Create a new tab for the result
        var resultTab = new TabItem { Header = "Result" };

        // Create a TextBlock to display the result
        var resultTextBlock = new TextBlock
        {
            Text = $"Player A strategy is [{string.Join("; ", aStrategy)}]" +
                   $"\nPlayer B strategy is [{string.Join("; ", bStrategy)}]" +
                   $"\nPrice of the game is {gamePrice}",
            FontSize = 16,
            Margin = new Thickness(10),
            TextWrapping = TextWrapping.Wrap
        };

        // Add the TextBlock to the TabItem
        resultTab.Content = new ScrollViewer
        {
            Content = resultTextBlock,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        // Add the new tab to the TabControl
        GridContainer.Items.Add(resultTab);
    }


    public void CreateAndAddDynamicGridSimplex(decimal[,] constraints, decimal[] cb, Dictionary<int, decimal> plan,
        decimal[] deriv, decimal[] delta, int[] myBase, decimal f)
    {
        // Create a new tab for the simplex grid
        var simplexTab = new TabItem { Header = $"Simplex Grid {GridContainer.Items.Count}" };

        // Create the grid to display the simplex table
        var dynamicGrid = new Grid
        {
            Margin = new Thickness(10),
            Background = Brushes.White
        };

        int rows = constraints.GetLength(0) + 2;
        int cols = constraints.GetLength(1) + 4;

        // Define rows and columns for the grid
        for (int i = 0; i < rows; i++)
            dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int j = 0; j < cols; j++)
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Add Headers
        CreateStyledCell(dynamicGrid, 0, 0, "Base", Brushes.CornflowerBlue, true);
        CreateStyledCell(dynamicGrid, 0, 1, "Cb", Brushes.CornflowerBlue, true);
        CreateStyledCell(dynamicGrid, 0, 2, "Plan", Brushes.CornflowerBlue, true);

        for (int i = 3; i < cols - 1; i++)
            CreateStyledCell(dynamicGrid, 0, i, $"x{i - 2}", Brushes.CornflowerBlue, true);

        CreateStyledCell(dynamicGrid, 0, cols - 1, "der", Brushes.CornflowerBlue, true);

        // Fill Data
        for (int i = 1; i < deriv.Length + 1; i++)
            CreateStyledCell(dynamicGrid, i, cols - 1, deriv[i - 1].ToString("F2"), Brushes.White);

        for (int i = 1; i < myBase.Length + 1; i++)
            CreateStyledCell(dynamicGrid, i, 0, $"x{myBase[i - 1] + 1}", Brushes.White);

        for (int i = 1; i < myBase.Length + 1; i++)
            CreateStyledCell(dynamicGrid, i, 1, cb[i - 1].ToString("F2"), Brushes.White);

        for (int i = 1; i < myBase.Length + 1; i++)
            CreateStyledCell(dynamicGrid, i, 2, plan[myBase[i - 1]].ToString("F2"), Brushes.White);

        for (int i = 1; i < constraints.GetLength(0) + 1; i++)
        {
            for (int j = 3; j < constraints.GetLength(1) + 3; j++)
            {
                CreateStyledCell(dynamicGrid, i, j, constraints[i - 1, j - 3].ToString("F2"), Brushes.White);
            }
        }

        // Final Row (Delta)
        CreateStyledCell(dynamicGrid, rows - 1, 0, "F*", Brushes.CornflowerBlue, true);
        CreateStyledCell(dynamicGrid, rows - 1, 1, " ", Brushes.White);
        CreateStyledCell(dynamicGrid, rows - 1, 2, f.ToString("F2"), Brushes.White);
        CreateStyledCell(dynamicGrid, rows - 1, cols - 1, " ", Brushes.White);
        for (int i = 3; i < delta.Length + 3; i++)
            CreateStyledCell(dynamicGrid, rows - 1, i, delta[i - 3].ToString("F2"), Brushes.White);

        // Add the grid to the tab
        simplexTab.Content = dynamicGrid;
        GridContainer.Items.Add(simplexTab);
    }
    
        public void ShowReducedMatrix(decimal[,] matrix, string reduceDimension)
    {
        // Create a new tab for the simplex grid
        var simplexTab = new TabItem { Header = $"Reduced Matrix by {reduceDimension}" };

        // Create the grid to display the simplex table
        var dynamicGrid = new Grid
        {
            Margin = new Thickness(10),
            Background = Brushes.White
        };

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        // Define rows and columns for the grid
        for (int i = 0; i < rows; i++)
            dynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int j = 0; j < cols; j++)
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                CreateStyledCell(dynamicGrid, i, j, matrix[i, j].ToString("F2"), Brushes.White);
            }
        }

        // Add the grid to the tab
        simplexTab.Content = dynamicGrid;
        GridContainer.Items.Add(simplexTab);
    }

    private void CreateStyledCell(Grid grid, int row, int col, string text, Brush background, bool isHeader = false,
        bool isEditable = false)
    {
        Border cellBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Background = background
        };

        FrameworkElement cellText;
        if (!isEditable)
        {
            cellText = new TextBlock
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                Foreground = isHeader ? Brushes.White : Brushes.DarkSlateGray,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 70,
                Height = 70,
                Padding = new Thickness(5)
            };
        }
        else
        {
            cellText = new TextBox
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                Foreground = isHeader ? Brushes.White : Brushes.DarkSlateGray,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 70,
                Height = 70,
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


    private void PreDefine()
    {
        // decimal[,] gameMatrix = {
        //     {1M, 4, 6, 3, 7},
        //     { 3, 1, 2, 4, 3},
        //     { 2, 3, 4, 3, 5},
        //     { 0, 1, 5, 2, 6}
        // };
        //     
        // AComboBox.SelectedValue = "4";
        // BComboBox.SelectedValue = "5";

        decimal[,] gameMatrix =
        {
            { 3, 2, 6, 5, 2, 5 },
            { 8, 7, 8, 6, 7, 9 },
            { 2, 7, 9, 8, 2, 1 },
            { 5, 2, 4, 1, 0, 3 },
            { 2, 3, 6, 1, 4, 7 },
            { 8, 4, 3, 2, 5, 6 }
        };


        AComboBox.SelectedValue = "6";
        BComboBox.SelectedValue = "6";


        for (int i = 1; i <= _a; i++)
        {
            for (int j = 1; j <= _b; j++)
            {
                ((TextBox)((Border)GetGridElement(TransportGrid, i, j)).Child).Text = gameMatrix[i - 1, j - 1].ToString();
            }
        }

    }

    private UIElement GetGridElement(Grid grid, int row, int column)
    {
        foreach (UIElement element in grid.Children)
        {
            if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
            {
                return element;
            }
        }

        return null;
    }
}
