﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BCad.UI.Controls
{
    public enum PlotType
    {
        File,
        Print
    }

    public enum ViewportType
    {
        Extents,
        Window
    }

    public enum PageSize
    {
        Letter,
        Legal,
        Landscape
    }

    public enum ScalingType
    {
        Absolute,
        ToFit
    }

    public class PlotDialogViewModel : INotifyPropertyChanged
    {
        public IEnumerable<PlotType> AvailablePlotTypes
        {
            get { return new[] { Controls.PlotType.File, Controls.PlotType.Print }; }
        }

        private Drawing drawing;
        private PlotType plotType;
        private string fileName;
        private ViewportType viewportType;
        private ScalingType scalingType;
        private Point bottomLeft;
        private Point topRight;
        private double scaleA;
        private double scaleB;
        private PageSize pageSize;
        private Visibility printOptVis;
        private Visibility fileOptVis;
        private int pixelWidth;
        private int pixelHeight;
        private double previewWidth;
        private double previewHeight;
        private ViewPort activeViewPort;

        public PlotType PlotType
        {
            get { return this.plotType; }
            set
            {
                if (this.plotType == value)
                    return;
                this.plotType = value;
                OnPropertyChanged();
                switch (this.plotType)
                {
                    case PlotType.File:
                        FileOptionsVisibility = Visibility.Visible;
                        PrintOptionsVisibility = Visibility.Hidden;
                        break;
                    case PlotType.Print:
                        FileOptionsVisibility = Visibility.Hidden;
                        PrintOptionsVisibility = Visibility.Visible;
                        break;
                    default:
                        throw new InvalidOperationException("unexpected plot type");
                }

                UpdatePreviewSize();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public Drawing Drawing
        {
            get { return drawing; }
            set
            {
                if (drawing == value)
                    return;
                drawing = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get { return this.fileName; }
            set
            {
                if (this.fileName == value)
                    return;
                this.fileName = value;
                OnPropertyChanged();
            }
        }

        public ViewportType ViewportType
        {
            get { return this.viewportType; }
            set
            {
                if (this.viewportType == value)
                    return;
                this.viewportType = value;
                OnPropertyChanged();
            }
        }

        public ScalingType ScalingType
        {
            get { return this.scalingType; }
            set
            {
                if (this.scalingType == value)
                    return;
                this.scalingType = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public Point BottomLeft
        {
            get { return this.bottomLeft; }
            set
            {
                if (this.bottomLeft == value)
                    return;
                this.bottomLeft = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public Point TopRight
        {
            get { return this.topRight; }
            set
            {
                if (this.topRight == value)
                    return;
                this.topRight = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public double ScaleA
        {
            get { return this.scaleA; }
            set
            {
                if (this.scaleA == value)
                    return;
                this.scaleA = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public double ScaleB
        {
            get { return this.scaleB; }
            set
            {
                if (this.scaleB == value)
                    return;
                this.scaleB = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public PageSize PageSize
        {
            get { return this.pageSize; }
            set
            {
                if (this.pageSize == value)
                    return;
                this.pageSize = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
                UpdatePreviewSize();
            }
        }

        public Visibility PrintOptionsVisibility
        {
            get { return this.printOptVis; }
            private set
            {
                if (this.printOptVis == value)
                    return;
                this.printOptVis = value;
                OnPropertyChanged();
                UpdatePreviewSize();
            }
        }

        public Visibility FileOptionsVisibility
        {
            get { return this.fileOptVis; }
            private set
            {
                if (this.fileOptVis == value)
                    return;
                this.fileOptVis = value;
                OnPropertyChanged();
                UpdatePreviewSize();
            }
        }

        public int PixelWidth
        {
            get { return this.pixelWidth; }
            set
            {
                if (this.pixelWidth == value)
                    return;
                this.pixelWidth = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
                UpdatePreviewSize();
            }
        }

        public int PixelHeight
        {
            get { return this.pixelHeight; }
            set
            {
                if (this.pixelHeight == value)
                    return;
                this.pixelHeight = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
                UpdatePreviewSize();
            }
        }

        public ViewPort ViewPort
        {
            get
            {
                ViewPort vp;
                switch (ViewportType)
                {
                    case ViewportType.Extents:
                        vp = Drawing.ShowAllViewPort(
                            ActiveViewPort.Sight,
                            ActiveViewPort.Up,
                            850,
                            1100,
                            pixelBuffer: 0);
                        break;
                    case ViewportType.Window:
                        vp = new ViewPort(BottomLeft, ActiveViewPort.Sight, ActiveViewPort.Up, TopRight.Y - BottomLeft.Y);
                        break;
                    default:
                        throw new InvalidOperationException("unsupported viewport type");
                }

                if (PlotType == PlotType.Print)
                {
                    var desiredHeight = PlotDialog.GetHeight(PageSize);
                    switch (ScalingType)
                    {
                        case ScalingType.Absolute:
                            vp = vp.Update(viewHeight: desiredHeight * ScaleB / ScaleA);
                            break;
                        case ScalingType.ToFit:
                            break;
                        default:
                            throw new InvalidOperationException("unsupported scaling type");
                    }
                }

                return vp;
            }
        }

        public double PreviewWidth
        {
            get { return previewWidth; }
            set
            {
                if (previewWidth == value)
                    return;
                previewWidth = value;
                OnPropertyChanged();
            }
        }

        public double PreviewHeight
        {
            get { return previewHeight; }
            set
            {
                if (previewHeight == value)
                    return;
                previewHeight = value;
                OnPropertyChanged();
            }
        }

        public ViewPort ActiveViewPort
        {
            get { return activeViewPort; }
            set
            {
                if (activeViewPort == value)
                    return;
                activeViewPort = value;
                OnPropertyChanged();
                OnPropertyChangedDirect("ViewPort");
            }
        }

        public PageSize[] AvailablePageSizes
        {
            get { return new[] { PageSize.Letter, PageSize.Landscape, PageSize.Legal }; }
        }

        public PlotDialogViewModel()
        {
            Drawing = new Drawing();
            PlotType = AvailablePlotTypes.First();
            FileName = string.Empty;
            ViewportType = ViewportType.Extents;
            ScalingType = ScalingType.ToFit;
            BottomLeft = Point.Origin;
            TopRight = Point.Origin;
            ScaleA = 1.0;
            ScaleB = 1.0;
            PageSize = PageSize.Letter;
            PixelWidth = 800;
            PixelHeight = 600;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string property = "")
        {
            OnPropertyChangedDirect(property);
        }

        protected void OnPropertyChangedDirect(string property)
        {
            var changed = PropertyChanged;
            if (changed != null)
                changed(this, new PropertyChangedEventArgs(property));
        }

        private void UpdatePreviewSize()
        {
            var maxWidth = 300;
            var maxHeight = 300;
            double width, height;
            if (PlotType == PlotType.Print)
            {
                width = PlotDialog.GetWidth(PageSize);
                height = PlotDialog.GetHeight(PageSize);
            }
            else
            {
                width = PixelWidth;
                height = PixelHeight;
            }

            if (width > height)
            {
                PreviewWidth = maxWidth;
                PreviewHeight = (height / width) * maxHeight;
            }
            else
            {
                PreviewHeight = maxHeight;
                PreviewWidth = (width / height) * maxWidth;
            }
        }
    }
}
