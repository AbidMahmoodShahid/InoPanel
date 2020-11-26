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
        #region dependancy properties

        public static readonly DependencyProperty ColumnsProperty = 
            DependencyProperty.Register(nameof(Columns), typeof(int),
                typeof(InoPanel), new FrameworkPropertyMetadata(2));
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }


        public static readonly DependencyProperty ElementMarginProperty =
            DependencyProperty.Register(nameof(ElementMargin), typeof(int),
                typeof(InoPanel), new FrameworkPropertyMetadata(0));
        public int ElementMargin
        {
            get { return (int)GetValue(ElementMarginProperty); }
            set { SetValue(ElementMarginProperty, value); }
        }

        #endregion

        #region fields

        private Size _panelSize = new Size(0, 0);
        private double[] _columnWidthList;
        private List<double> _rowHeightList = new List<double>();
        private double _currentRowHeight = 0;

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            int currentColumn = 0;
            bool newRow = true;
            _columnWidthList = new double[Columns];
            _rowHeightList = new List<double>();
            _panelSize = new Size(0, 0);

            if (Columns < 1)
                return new Size(0, 0);   

            foreach (UIElement element in Children)
            {
                element.Measure(availableSize);

                _panelSize = UpdatePanelSize(_panelSize, element, currentColumn, newRow, _columnWidthList, _rowHeightList, ElementMargin);

                // set current column/newrow
                currentColumn++;
                if (currentColumn >= Columns)
                {
                    newRow = true;
                    currentColumn = 0;
                }
                else
                {
                    newRow = false;
                }
            }
            return _panelSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int currentRow = 0;
            int currentColumn = 0;
            double currentHorizontalOffset = 0;
            double currentVerticalOffset = 0;

            if (Columns < 1)
                return new Size(0, 0);

            foreach (UIElement element in Children)
            {
                Rect arrangeRect = SetElementSizeAndPosition(currentHorizontalOffset, currentVerticalOffset, _rowHeightList[currentRow], _columnWidthList[currentColumn], element);
                element.Arrange(arrangeRect);

                // set current horizontal offset
                currentHorizontalOffset += _columnWidthList[currentColumn];

                // reset variables for new row
                currentColumn++;
                if (currentColumn >= Columns)
                {
                    currentColumn = 0;
                    currentHorizontalOffset = 0;
                    currentVerticalOffset += _rowHeightList[currentRow];
                    currentRow++;
                }
            }
            return finalSize;
        }


        #region measure private methods

        private Size UpdatePanelSize(Size panelSize, UIElement element, int currentColumn, bool newRow, double[] columnWidthList, List<double> rowHeightList, int elementMargin)
        {
            Size adjustedPanelSize = panelSize;

            if (currentColumn < 1)
                _currentRowHeight = 0;

            // adjust current column width if necessary
            adjustedPanelSize.Width = AdjustPanelWidth(columnWidthList, currentColumn, element.DesiredSize.Width, elementMargin);

            // adjust current row height if necessary
            adjustedPanelSize.Height = AdjustPanelHeight(rowHeightList, element.DesiredSize.Height, newRow, elementMargin);

            return adjustedPanelSize;
        }

        private double AdjustPanelWidth(double[] columnWidthList, int currentColumn, double elementDesiredWidth, int elementMargin)
        {
            columnWidthList[currentColumn] = Math.Max(columnWidthList[currentColumn], elementDesiredWidth + 2 * elementMargin);
            return columnWidthList.Sum();
        }

        private double AdjustPanelHeight(List<double> rowHeightList, double elementDesiredHeight, bool newRow, int elementMargin)
        {
            if(newRow)
            {
                _currentRowHeight = elementDesiredHeight + 2 * elementMargin;
                rowHeightList.Add(_currentRowHeight);
                return rowHeightList.Sum();
            }
            else
            {
                if(_currentRowHeight < elementDesiredHeight + 2 * elementMargin)
                {
                    _currentRowHeight = elementDesiredHeight + 2 * elementMargin;
                    rowHeightList.RemoveAt(rowHeightList.Count - 1);
                    rowHeightList.Add(_currentRowHeight);
                }
                return rowHeightList.Sum();
            }
        }

        #endregion

        #region arrange private methods

        private Rect SetElementSizeAndPosition(double currentHorizontalOffset, double currentVerticalOffset, double rowHeight, double columnWidth, UIElement element)
        {
            HorizontalAlignment horizontalAlignment = ((Control)element).HorizontalAlignment;
            VerticalAlignment verticalAlignment = ((Control)element).VerticalAlignment;

            // set horizontal offset
            double x = PositionElementHorizontally(currentHorizontalOffset, columnWidth, element.DesiredSize.Width, horizontalAlignment);

            // set vertical offset
            double y = PositionElementVertically(currentVerticalOffset, rowHeight, element.DesiredSize.Height, verticalAlignment);

            // set element width
            double elementWidth = SetElementWidth(element, horizontalAlignment, columnWidth);

            // set element height
            double elementHeight = SetElementHeight(element, verticalAlignment, rowHeight);

            Rect arrangeRect = new Rect(x, y, elementWidth, elementHeight);
            return arrangeRect;
        }

        private double PositionElementHorizontally(double Xo, double columnWidth, double elementWidth, HorizontalAlignment horizontalAlignment)
        {
            switch(horizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    return Xo;
                case HorizontalAlignment.Center:
                    return Xo + ((columnWidth - elementWidth) / 2);
                case HorizontalAlignment.Right:
                    return Xo + (columnWidth - elementWidth);
                case HorizontalAlignment.Stretch:
                    return Xo + ElementMargin;
                default:
                    return Xo + ((columnWidth - elementWidth) / 2);
            }
        }

        private double PositionElementVertically(double Yo, double rowHeight, double elementHeight, VerticalAlignment verticalAlignment)
        {
            switch(verticalAlignment)
            {
                case VerticalAlignment.Top:
                    return Yo;
                case VerticalAlignment.Center:
                    return Yo + ((rowHeight - elementHeight) / 2);
                case VerticalAlignment.Bottom:
                    return Yo + (rowHeight - elementHeight);
                case VerticalAlignment.Stretch:
                    return Yo + ElementMargin;
                default:
                    return Yo + ((rowHeight - elementHeight) / 2);
            }
        }

        private double SetElementWidth(UIElement element, HorizontalAlignment horizontalAlignment, double columnWidth)
        {
            switch(horizontalAlignment)
            {
                case HorizontalAlignment.Stretch:
                    return columnWidth - 2 * ElementMargin;
                default:
                    return element.DesiredSize.Width;
            }
        }

        private double SetElementHeight(UIElement element, VerticalAlignment verticalAlignment, double rowHeight)
        {
            switch (verticalAlignment)
            {
                case VerticalAlignment.Stretch:
                    return rowHeight - 2 * ElementMargin;
                default:
                    return element.DesiredSize.Height;
            }
        }

        #endregion

    }
}
