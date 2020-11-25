using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InoPanel
{
    public class InoPanel : Panel
    {
        public static readonly DependencyProperty ColumnsProperty = 
            DependencyProperty.Register(nameof(Columns), typeof(int),
                typeof(InoPanel), new FrameworkPropertyMetadata(2));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }


        private Size _panelSize = new Size(0, 0);
        private double[] _columnWidthList;
        private List<double> _rowHeightList = new List<double>();
        private double _currentRowHeight = 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            int currentColumn = 0; 
            _columnWidthList = new double[Columns];

            if (Columns < 1)
                return new Size(0, 0);
            

            foreach (UIElement element in Children)
            {
                element.Measure(availableSize);

                _panelSize = SetPanelSize(availableSize, _panelSize, element, Columns, currentColumn, _columnWidthList, _rowHeightList);

                // set current column
                currentColumn++;
                if (currentColumn >= Columns)
                {
                    currentColumn = 0;
                }
            }

            return _panelSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int currentRow = 0;
            int currentColumn = 0;

            if (Columns < 1)
                return new Size(0, 0);

            foreach (UIElement element in Children)
            {
                Rect arrangeRect = SetElementOffset(currentRow, currentColumn, element);
                element.Arrange(arrangeRect);

                // set current row and column
                currentColumn++;
                if (currentColumn >= Columns)
                {
                    currentColumn = 0;
                    currentRow++;
                }

            }

            return finalSize;
        }


        private Size SetPanelSize(Size availableSize, Size panelSize, UIElement element, int columns, int currentColumn, double[] columnWidthList, List<double> rowHeightList)
        {
            Size adjustedPanelSize = new Size(0,0);
            if(columns < 1)
                return adjustedPanelSize;

            if (currentColumn < 1)
                _currentRowHeight = 0;


            element.Measure(availableSize);

            // adjust current column width if necessary
            columnWidthList[currentColumn] = Math.Max(columnWidthList[currentColumn], element.DesiredSize.Width);

            // adjust current row height if necessary
            _currentRowHeight = Math.Max(_currentRowHeight, element.DesiredSize.Height);

            // adjust panel height
            if(currentColumn == columns - 1)
            {
                panelSize.Height += _currentRowHeight;
                rowHeightList.Add(_currentRowHeight);
            }

            // adjust panel width
            panelSize.Width = columnWidthList.Sum();

            adjustedPanelSize = new Size(panelSize.Width, panelSize.Height);

            return adjustedPanelSize;
        }

        private Rect SetElementOffset(int currentRow, int currentColumn, UIElement element)
        {
            #region Horizontal offset
            // set horizontal offset
            int columnCount = 0;
            double horizontalOffset = 0;
            foreach (double columnWidth in _columnWidthList)
            {
                if (currentColumn > columnCount)
                    horizontalOffset += columnWidth;
                else
                    break;

                columnCount++;
            }
            #endregion

            #region Vertical offset
            // set vertical offset
            int rowCount = 0;
            double verticalOffset = 0;
            foreach (double rowHeight in _rowHeightList)
            {
                if (currentRow > rowCount)
                    verticalOffset += rowHeight;
                else
                    break;

                rowCount++;
            }
            #endregion

            Rect arrangeRect = new Rect(horizontalOffset, verticalOffset, element.DesiredSize.Width, element.DesiredSize.Height);
            return arrangeRect;
        }
    }
}
