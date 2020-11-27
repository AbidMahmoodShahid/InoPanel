using System;
using System.Collections.Generic;
using System.Linq;
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

        public Thickness ElementMargin
        {
            get { return (Thickness)GetValue(ElementMarginProperty); }
            set { SetValue(ElementMarginProperty, value); }
        }
        public static readonly DependencyProperty ElementMarginProperty =
            DependencyProperty.Register(nameof(ElementMargin), typeof(Thickness),
                typeof(InoPanel), new UIPropertyMetadata());

        #endregion

        #region fields

        private Size _panelSizeMeasurement = new Size(0, 0);
        private List<double> _columnWidthList;
        private List<double> _rowHeightList = new List<double>();
        private double _currentRowHeight = 0;

        #endregion

        #region measure

        protected override Size MeasureOverride(Size availableSize)
        {
            int currentColumn = 0;
            bool isNewRow;
            _columnWidthList = new List<double>();
            for (int i = 0; i < Columns; i++)
            {
                _columnWidthList.Add(0);
            }
            _rowHeightList = new List<double>();
            _panelSizeMeasurement = new Size(0, 0);

            if (Columns < 1)
                return new Size(0, 0);

            foreach (UIElement element in Children)
            {
                element.Measure(availableSize);
                isNewRow = false;

                if (currentColumn >= Columns)
                {
                    currentColumn = 0;
                }
                if (currentColumn < 1)
                {
                    isNewRow = true;
                }

                _panelSizeMeasurement = UpdatePanelSize(element, currentColumn, isNewRow);

                currentColumn++;
            }
            return _panelSizeMeasurement;
        }

        private Size UpdatePanelSize(UIElement element, int currentColumn, bool isNewRow)
        {
            // TODO AM implement later: if margin is set manually on element, take manually set margin in to conideration instead of DP(ElementMargin). AttachedProperty can be used for this
            Thickness manuallySetMargin = ((FrameworkElement)element).Margin;

            if (currentColumn < 1)
                _currentRowHeight = 0;

            // adjust current column width if necessary
            double newPanelWidth = AdjustPanelWidth(_columnWidthList, currentColumn, element.DesiredSize.Width, ElementMargin.Left, ElementMargin.Right);

            // adjust current row height if necessary
            double newPanelHeight = AdjustPanelHeight(_rowHeightList, element.DesiredSize.Height, isNewRow, ElementMargin.Top, ElementMargin.Bottom);

            return new Size(newPanelWidth, newPanelHeight);
        }

        private double AdjustPanelWidth(List<double> columnWidthList, int currentColumn, double elementDesiredWidth, double leftMargin, double rightMargin)
        {
            columnWidthList[currentColumn] = Math.Max(columnWidthList[currentColumn], elementDesiredWidth + leftMargin + rightMargin);
            return columnWidthList.Sum();
        }

        private double AdjustPanelHeight(List<double> rowHeightList, double elementDesiredHeight, bool newRow, double topMargin, double bottomMargin)
        {
            if (newRow)
            {
                _currentRowHeight = elementDesiredHeight + (topMargin + bottomMargin);
                rowHeightList.Add(_currentRowHeight);
                return rowHeightList.Sum();
            }
            else
            {
                if (_currentRowHeight < elementDesiredHeight + (topMargin + bottomMargin))
                {
                    _currentRowHeight = elementDesiredHeight + (topMargin + bottomMargin);
                    rowHeightList.RemoveAt(rowHeightList.Count - 1);
                    rowHeightList.Add(_currentRowHeight);
                }
                return rowHeightList.Sum();
            }
        }

        #endregion

        #region arrange 

        protected override Size ArrangeOverride(Size finalPanelSize)
        {
            int currentRow = 0;
            int currentColumn = 0;
            double currentHorizontalOffset = 0;
            double currentVerticalOffset = 0;

            if (Columns < 1)
                return new Size(0, 0);

            foreach (UIElement element in Children)
            {
                Rect arrangeRect = SetElementSizeAndPosition(finalPanelSize, currentHorizontalOffset, currentVerticalOffset, _rowHeightList[currentRow], _columnWidthList[currentColumn], element);
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
            return finalPanelSize;
        }

        private Rect SetElementSizeAndPosition(Size finalPanelSize, double currentHorizontalOffset, double currentVerticalOffset, double rowHeight, double columnWidth, UIElement element)
        {
            HorizontalAlignment horizontalAlignment;
            VerticalAlignment verticalAlignment;
            try
            {
                horizontalAlignment = ((FrameworkElement)element).HorizontalAlignment;
                verticalAlignment = ((FrameworkElement)element).VerticalAlignment;
            }
            catch
            {
                horizontalAlignment = HorizontalAlignment.Stretch;
                verticalAlignment = VerticalAlignment.Stretch;
            }

            // reset final column width based on horizontal alignment of panel
            _columnWidthList[Columns - 1] = ResetLastColumnWidth(finalPanelSize.Width, _columnWidthList);

            // reset final row height based on vertical alignment of panel
            _rowHeightList[_rowHeightList.Count - 1] = ResetLastRowHeight(finalPanelSize.Height, _rowHeightList);

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

        private double ResetLastColumnWidth(double finalPanelWidth, List<double> columnWidthList)
        {
            int lastColumnIndex = columnWidthList.Count() - 1;
            double currentLastColunnWidth = columnWidthList[lastColumnIndex];
            return finalPanelWidth - columnWidthList.Sum() + currentLastColunnWidth;
        }

        private double ResetLastRowHeight(double finalPanelHeight, List<double> rowHeightList)
        {
            int lastRowIndex = rowHeightList.Count() - 1;
            double currentLastRowHeight = rowHeightList[lastRowIndex];
            return finalPanelHeight - rowHeightList.Sum() + currentLastRowHeight;
        }

        private double PositionElementHorizontally(double Xo, double columnWidth, double elementWidth, HorizontalAlignment horizontalAlignment)
        {
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    return Xo + ElementMargin.Left;
                case HorizontalAlignment.Center:
                    return Xo + ((columnWidth - elementWidth) / 2);
                case HorizontalAlignment.Right:
                    return Xo + (columnWidth - elementWidth) - ElementMargin.Right;
                case HorizontalAlignment.Stretch:
                    return Xo + ElementMargin.Left;
                default:
                    return Xo + ((columnWidth - elementWidth) / 2);
            }
        }

        private double PositionElementVertically(double Yo, double rowHeight, double elementHeight, VerticalAlignment verticalAlignment)
        {
            switch (verticalAlignment)
            {
                case VerticalAlignment.Top:
                    return Yo + ElementMargin.Top;
                case VerticalAlignment.Center:
                    return Yo + ((rowHeight - elementHeight) / 2);
                case VerticalAlignment.Bottom:
                    return Yo + (rowHeight - elementHeight) - ElementMargin.Bottom;
                case VerticalAlignment.Stretch:
                    return Yo + ElementMargin.Top;
                default:
                    return Yo + ((rowHeight - elementHeight) / 2);
            }
        }

        private double SetElementWidth(UIElement element, HorizontalAlignment horizontalAlignment, double columnWidth)
        {
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Stretch:
                    return columnWidth - (ElementMargin.Left + ElementMargin.Right);
                default:
                    return element.DesiredSize.Width;
            }
        }

        private double SetElementHeight(UIElement element, VerticalAlignment verticalAlignment, double rowHeight)
        {
            switch (verticalAlignment)
            {
                case VerticalAlignment.Stretch:
                    return rowHeight - (ElementMargin.Top + ElementMargin.Bottom);
                default:
                    return element.DesiredSize.Height;
            }
        }

        #endregion
    }
}
