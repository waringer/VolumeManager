using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VolumeManager
{
    public class C_BorderlessFormBase : Form
    {
        private bool _Drag = false;
        private Point _StartPoint = new Point(0, 0);
        private bool _Draggable = true;
        private List<string> _ExcludedComponentes;

        public C_BorderlessFormBase()
        {
            _ExcludedComponentes = new List<string>();

            MouseDown += new MouseEventHandler(Form_MouseDown);
            MouseUp += new MouseEventHandler(Form_MouseUp);
            MouseMove += new MouseEventHandler(Form_MouseMove);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (_Draggable && (!_ExcludedComponentes.Contains(e.Control.Name)))
            {
                e.Control.MouseDown += new MouseEventHandler(Form_MouseDown);
                e.Control.MouseUp += new MouseEventHandler(Form_MouseUp);
                e.Control.MouseMove += new MouseEventHandler(Form_MouseMove);
            }
            base.OnControlAdded(e);
        }

        #region Event Handlers
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (_Draggable && (!_ExcludedComponentes.Contains(((Control)sender).Name)))
            {
                _Drag = true;
                _StartPoint = new Point(e.X, e.Y);

                var _BorderWidth_ = (Width - ClientSize.Width) / 2;

                _StartPoint.Y += (Height - ClientSize.Height - 2 * _BorderWidth_) + _BorderWidth_;
                _StartPoint.X += _BorderWidth_;
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (_Draggable && (!_ExcludedComponentes.Contains(((Control)sender).Name)))
                _Drag = false;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (_Drag && (_Draggable && (!_ExcludedComponentes.Contains(((Control)sender).Name))))
            {
                var _p2_ = PointToScreen(new Point(e.X, e.Y));
                Location = new Point(_p2_.X - _StartPoint.X, _p2_.Y - _StartPoint.Y);
            }
        }
        #endregion

        #region Properties
        public string[] ExcludeList
        {
            get
            {
                return _ExcludedComponentes.ToArray();
            }
            set
            {
                _ExcludedComponentes.Clear();
                _ExcludedComponentes.AddRange(value);
            }
        }

        public void AddComponentName2ExcludeList(string ComponentName)
        {
            if (!_ExcludedComponentes.Contains(ComponentName))
                _ExcludedComponentes.Add(ComponentName);
        }

        public bool Draggable
        {
            set
            {
                _Draggable = value;
                if (!_Draggable)
                    _Drag = false;
            }
            get
            {
                return _Draggable;
            }
        }
        #endregion
    }
}
