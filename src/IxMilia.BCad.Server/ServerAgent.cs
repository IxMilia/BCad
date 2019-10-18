﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using StreamJsonRpc;

namespace IxMilia.BCad.Server
{
    public enum MouseButton
    {
        Left = 0,
        Middle = 1,
        Right = 2,
    }

    public class ServerAgent
    {
        private IWorkspace _workspace;
        private JsonRpc _rpc;
        private double _lastCursorX = 0.0;
        private double _lastCursorY = 0.0;
        private bool _isPanning = false;
        private double _lastWidth = 0.0;
        private double _lastHeight = 0.0;
        public bool IsRunning { get; private set; }

        public ServerAgent(IWorkspace workspace, JsonRpc rpc)
        {
            _workspace = workspace;
            _rpc = rpc;
            IsRunning = true;

            _workspace.WorkspaceChanged += _workspace_WorkspaceChanged;
        }

        private void _workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            var doUpdate = false;
            var clientUpdate = new ClientUpdate();
            if (e.IsActiveViewPortChange)
            {
                doUpdate = true;
                clientUpdate.Transform = GetTransform();
            }

            if (e.IsDrawingChange)
            {
                doUpdate = true;
                clientUpdate.Drawing = GetDrawing();
            }

            if (doUpdate)
            {
                _rpc.NotifyAsync("ClientUpdate", clientUpdate);
            }
        }

        public void ServerUpdate(ServerUpdate serverUpdate)
        {
            double[] transform = null;
            if (serverUpdate.DisplayUpdate.HasValue)
            {
                switch (serverUpdate.DisplayUpdate.GetValueOrDefault())
                {
                    case DisplayUpdate.ZoomIn:
                    case DisplayUpdate.ZoomOut:
                        break;
                }
            }

            var clientUpdate = new ClientUpdate();
            clientUpdate.Transform = transform;
            _rpc.NotifyAsync("ClientUpdate", clientUpdate);
        }

        public void Ready(double width, double height)
        {
            _lastWidth = width;
            _lastHeight = height;
            var clientUpdate = new ClientUpdate()
            {
                Drawing = GetDrawing(),
                Transform = GetTransform(),
            };
            _rpc.NotifyAsync("ClientUpdate", clientUpdate);
        }

        public void MouseDown(MouseButton button, double cursorX, double cursorY)
        {
            if (button == MouseButton.Middle)
            {
                _isPanning = true;
            }
        }

        public void MouseUp(MouseButton button, double cursorX, double cursorY)
        {
            if (button == MouseButton.Middle)
            {
                _isPanning = false;
            }
        }

        public void MouseMove(double cursorX, double cursorY)
        {
            var dx = cursorX - _lastCursorX;
            var dy = cursorY - _lastCursorY;
            _lastCursorX = cursorX;
            _lastCursorY = cursorY;
            if (_isPanning)
            {
                Pan(_lastWidth, _lastHeight, -dx, -dy);
            }
        }

        public Task<bool> ExecuteCommand(string command)
        {
            return _workspace.ExecuteCommand(command);
        }

        private double[] GetTransform()
        {
            var t = _workspace.ActiveViewPort.GetTransformationMatrixDirect3DStyle(_lastWidth, _lastHeight);
            // transpose
            return new[]
            {
                t.M11, t.M21, t.M31, t.M41,
                t.M12, t.M22, t.M32, t.M42,
                t.M13, t.M23, t.M33, t.M43,
                t.M14, t.M24, t.M34, t.M44,
            };
        }

        private ClientDrawing GetDrawing()
        {
            var lines = new List<ClientLine>();
            var autoColor = CadColor.White;
            foreach (var layer in _workspace.Drawing.GetLayers())
            {
                var layerColor = layer.Color ?? autoColor;
                foreach (var line in layer.GetEntities().OfType<Line>())
                {
                    var lineColor = line.Color ?? layerColor;
                    lines.Add(new ClientLine(ClientPoint.FromPoint(line.P1), ClientPoint.FromPoint(line.P2)));
                }
            }
            return new ClientDrawing()
            {
                Lines = lines,
            };
        }

        public ViewPort GetViewPort()
        {
            return _workspace.ActiveViewPort;
        }

        public void Pan(double width, double height, double dx, double dy)
        {
            var vp = _workspace.ActiveViewPort;
            var scale = vp.ViewHeight / height;
            dx = vp.BottomLeft.X + dx * scale;
            dy = vp.BottomLeft.Y - dy * scale;
            _workspace.Update(activeViewPort: vp.Update(bottomLeft: new Point(dx, dy, vp.BottomLeft.Z)));
        }

        public void Zoom(int cursorX, int cursorY, double width, double height, double delta)
        {
            // scale everything
            var scale = 1.25;
            if (delta > 0) scale = 0.8; // 1.0 / 1.25

            // center zoom operation on mouse
            var cursorPoint = new Point(cursorX, cursorY, 0.0);
            var vp = _workspace.ActiveViewPort;
            var oldHeight = vp.ViewHeight;
            var oldWidth = width * oldHeight / height;
            var newHeight = oldHeight * scale;
            var newWidth = oldWidth * scale;
            var heightDelta = newHeight - oldHeight;
            var widthDelta = newWidth - oldWidth;

            var relHoriz = cursorPoint.X / width;
            var relVert = (height - cursorPoint.Y) / height;
            var botLeftDelta = new Vector(relHoriz * widthDelta, relVert * heightDelta, 0.0);
            var newVp = vp.Update(
                bottomLeft: (Point)(vp.BottomLeft - botLeftDelta),
                viewHeight: vp.ViewHeight * scale);
            _workspace.Update(activeViewPort: newVp);
        }

        //public void MouseDown(MouseDownArgs args, int garbage)
        //{
        //    switch (args.Button)
        //    {
        //        case MouseButton.Left:
        //            var matrix = _workspace.ActiveViewPort.GetTransformationMatrixWindowsStyle(args.Width, args.Height);
        //            var inv = matrix.Inverse();
        //            var modelPoint = inv.Transform(args.Cursor);
        //            _workspace.InputService.PushPoint(modelPoint);
        //            break;
        //        case MouseButton.Middle:
        //            break;
        //        case MouseButton.Right:
        //            _workspace.InputService.PushNone();
        //            break;
        //    }
        //}
    }
}
