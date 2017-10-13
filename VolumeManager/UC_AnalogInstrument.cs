using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VolumeManager
{
    public class UC_AnalogInstrument : Control
    {
        private Bitmap _Backgound;

        private int _BorderWidth = 2;
        private Color _BorderColor = Color.Black;
        private bool _BorderEnabled = true;

        private int _StartAngel = 0;
        private int _StopAngel = 360;
        private int _AngelOffset = 0;

        private float _ScaleMin = 0;
        private float _ScaleMax = 10;
        private bool _ScaleReverse = false;

        private float _ScaleMainStep = 1;
        private Color _ScaleMainColor = Color.Black;
        private int _ScaleMainLineWidth = 2;
        private int _ScaleMainLineLenght = 10;
        private bool _ScaleMainEnabled = true;

        private int _ScaleMainTextOffset = 10;
        private Color _ScaleMainTextColor = Color.Black;
        private Font _ScaleMainTextFont;
        private string _ScaleMainTextFormat = "{0}";
        private bool _ScaleMainTextEnabled = true;

        private float _ScaleSecundaryStep = 1;
        private Color _ScaleSecundaryColor;
        private int _ScaleSecundaryLineWidth = 1;
        private int _ScaleSecundaryLineLenght = 5;
        private bool _ScaleSecundaryEnabled = true;

        private Color _PointerColor = Color.Black;
        private int _PointerWidth = 2;

        private Color _BorderIndicatorColor = Color.Black;

        private Color _PointerBaseColor = Color.Black;
        private int _PointerBaseSize = 20;
        private bool _PointerBaseEnabled = true;

        private Color _StatMaxColor = Color.Black;
        private Color _StatMinColor = Color.Black;
        private bool _StatMaxVisible = false;
        private bool _StatMinVisible = false;

        private float _Value = 0;
        private float _DestValue = 0;
        private float _MaxUpdateValue = 5;

        private float _Value2 = 0;

        private PointF _BasePoint;
        private float _Radius;

        private Timer _UpdateSmother;
        private bool _UseSmother = true;

        private C_RingBuffer<float> _LastValues;

        public UC_AnalogInstrument()
        {
            InitStyle();

            _ScaleMainTextFont = Font;
            _BasePoint = new PointF(0, 0);

            InitBackground();
            InitUpdateSmother();

            _LastValues = new C_RingBuffer<float>(100);
        }

        private void InitStyle()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        private void InitBackground()
        {
            RecalcSize();
            MakeBackGround();
            Paint += new PaintEventHandler(UC_AnalogInstrument_Paint);
            Resize += new EventHandler(UC_AnalogInstrument_Resize);
        }

        private void InitUpdateSmother()
        {
            _UpdateSmother = new Timer();
            _UpdateSmother.Interval = 50;
            _UpdateSmother.Tick += new EventHandler(UpdateSmother_Tick);
            _UpdateSmother.Start();
        }

        #region eventhandler
        private void UC_AnalogInstrument_Resize(object sender, EventArgs e)
        {
            RecalcSize();
            MakeBackGround();
            Invalidate();
        }

        #region Paint
        private void UC_AnalogInstrument_Paint(object sender, PaintEventArgs e)
        {
            //paint bg
            e.Graphics.DrawImageUnscaled(_Backgound, new Point(0, 0));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //draw min & max
            var _Max_ = _ScaleMin;
            var _Min_ = _ScaleMax;

            foreach (var _val_ in _LastValues)
            {
                if (_Max_ < _val_)
                    _Max_ = _val_;

                if (_Min_ > _val_)
                    _Min_ = _val_;
            }

            PaintStatMax(e, _Max_);
            PaintStatMin(e, _Min_);

            //draw pointer
            PaintPointer(e);
            PaintPointerBase(e);

            var _From_ = new PointF();
            var _To_ = new PointF();

            if (_ScaleReverse)
            {
                var _stopAngel_ = _StopAngel - (_Value2 * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));
                var _AngelFrom_ = Grad2Bogen(_StopAngel + _AngelOffset);
                _From_.X = _BasePoint.X + (_Radius * (float)Math.Cos(_AngelFrom_));
                _From_.Y = _BasePoint.Y + (_Radius * (float)Math.Sin(_AngelFrom_));

                using (var _BorderIndicatorPen_ = new Pen(_BorderIndicatorColor, _BorderWidth))
                    for (var _Angel_ = _StopAngel; _Angel_ >= _stopAngel_; _Angel_--)
                    {
                        _To_ = GetPointOnRadius(_Angel_ + _AngelOffset, _Radius);
                        e.Graphics.DrawLine(_BorderIndicatorPen_, _From_, _To_);
                        _From_ = _To_;
                    }
            }
            else
            {
                var _stopAngel_ = _StartAngel + (_Value2 * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));
                var _AngelFrom_ = Grad2Bogen(_StartAngel + _AngelOffset);
                _From_.X = _BasePoint.X + (_Radius * (float)Math.Cos(_AngelFrom_));
                _From_.Y = _BasePoint.Y + (_Radius * (float)Math.Sin(_AngelFrom_));

                using (var _BorderIndicatorPen_ = new Pen(_BorderIndicatorColor, _BorderWidth))
                    for (var _Angel_ = _StartAngel + 1; _Angel_ <= _stopAngel_; _Angel_++)
                    {
                        _To_ = GetPointOnRadius(_Angel_ + _AngelOffset, _Radius);
                        e.Graphics.DrawLine(_BorderIndicatorPen_, _From_, _To_);
                        _From_ = _To_;
                    }
            }
        }

        private void PaintStatMax(PaintEventArgs e, float MaxScale)
        {
            if (_StatMaxVisible)
            {
                using (var _StatMaxBrush_ = new SolidBrush(_StatMaxColor))
                {
                    float _Angel_;

                    if (_ScaleReverse)
                        _Angel_ = _StopAngel - (MaxScale * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));
                    else
                        _Angel_ = _StartAngel + (MaxScale * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));

                    e.Graphics.FillPolygon(_StatMaxBrush_, GetTriangel(_Angel_));
                }
            }
        }

        private void PaintStatMin(PaintEventArgs e, float MinScale)
        {
            if (_StatMinVisible)
            {
                using (var _StatMinBrush_ = new SolidBrush(_StatMinColor))
                {
                    float _Angel_;

                    if (_ScaleReverse)
                        _Angel_ = _StopAngel - (MinScale * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));
                    else
                        _Angel_ = _StartAngel + (MinScale * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));

                    e.Graphics.FillPolygon(_StatMinBrush_, GetTriangel(_Angel_));
                }
            }
        }

        private void PaintPointer(PaintEventArgs e)
        {
            using (var _PointerPen_ = new Pen(_PointerColor, _PointerWidth))
            {
                float _Angel_;

                if (_ScaleReverse)
                    _Angel_ = _StopAngel - (_Value * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));
                else
                    _Angel_ = _StartAngel + (_Value * ((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)));

                e.Graphics.DrawLine(_PointerPen_, _BasePoint, GetPointOnRadius(_Angel_ + _AngelOffset, _Radius - _BorderWidth));
            }
        }

        private void PaintPointerBase(PaintEventArgs e)
        {
            if (_PointerBaseEnabled)
                using (var _BaseBrush_ = new SolidBrush(_PointerBaseColor))
                {
                    e.Graphics.FillEllipse(_BaseBrush_, new Rectangle((int)_BasePoint.X - (_PointerBaseSize / 2), (int)_BasePoint.Y - (_PointerBaseSize / 2), _PointerBaseSize, _PointerBaseSize));
                }
        }
        #endregion

        private void UpdateSmother_Tick(object sender, EventArgs e)
        {
            if (_Value != _DestValue)
            {
                var _Update_ = _MaxUpdateValue;
                if (Math.Abs(_DestValue - _Value) < _Update_)
                    _Update_ = Math.Abs(_DestValue - _Value);

                if (_DestValue > _Value)
                    _Value += _Update_;
                else
                    _Value -= _Update_;

                Invalidate();
            }
        }
        #endregion

        private PointF[] GetTriangel(float Angel)
        {
            var _bAngel_ = Grad2Bogen(Angel + _AngelOffset);
            var _Triangel_ = new PointF[3];
            var _To_ = new PointF();

            _To_.X = _BasePoint.X + ((_Radius - _BorderWidth) * (float)Math.Cos(_bAngel_));
            _To_.Y = _BasePoint.Y + ((_Radius - _BorderWidth) * (float)Math.Sin(_bAngel_));
            _Triangel_[0] = _To_;

            _bAngel_ = Grad2Bogen((Angel + 2.5F) + _AngelOffset);
            _To_.X = _BasePoint.X + (((_Radius - 5) - _BorderWidth) * (float)Math.Cos(_bAngel_));
            _To_.Y = _BasePoint.Y + (((_Radius - 5) - _BorderWidth) * (float)Math.Sin(_bAngel_));

            _Triangel_[1] = _To_;

            _bAngel_ = Grad2Bogen((Angel - 2.5F) + _AngelOffset);
            _To_.X = _BasePoint.X + (((_Radius - 5) - _BorderWidth) * (float)Math.Cos(_bAngel_));
            _To_.Y = _BasePoint.Y + (((_Radius - 5) - _BorderWidth) * (float)Math.Sin(_bAngel_));

            _Triangel_[2] = _To_;

            return _Triangel_;
        }

        private PointF GetPointOnRadius(float Angel, float Radius)
        {
            var _back_ = new PointF();
            var _bAngel_ = Grad2Bogen(Angel);
            _back_.X = _BasePoint.X + (Radius * (float)Math.Cos(_bAngel_));
            _back_.Y = _BasePoint.Y + (Radius * (float)Math.Sin(_bAngel_));
            return _back_;
        }

        private void RecalcSize()
        {
            float _widthMax_ = Width - (base.Padding.Right + base.Padding.Left);
            float _heightMax_ = Height - (base.Padding.Bottom + base.Padding.Top);

            var _width_ = _widthMax_;
            var _height_ = _heightMax_;

            if (_width_ > _height_)
                _width_ = _height_;
            else
                _height_ = _width_;

            _Radius = _width_ / 2;

            var _InstrumentSize_ = GetInstrumentSize(_Radius);

            var _iHeight_ = (int)(_InstrumentSize_.Height - _InstrumentSize_.Top);
            var _iWidth_ = (int)(_InstrumentSize_.Width - _InstrumentSize_.Left);

            var _WidthDiff_ = _widthMax_ - _iWidth_;
            var _HeightDiff_ = _heightMax_ - _iHeight_;

            var _Zoom_ = 1f;

            if (((_iHeight_ < _height_ - 10) && (_WidthDiff_ != 0)) || ((_iWidth_ < _width_ - 10) && (_HeightDiff_ != 0)))
            {
                var _tZoom_ = _Zoom_ + (_WidthDiff_ / _iWidth_);
                if (((_InstrumentSize_.Height - _InstrumentSize_.Top) * _tZoom_) >= _heightMax_)
                    _tZoom_ = _Zoom_ + ((_heightMax_ - _iHeight_) / _iHeight_);

                _Zoom_ = _tZoom_;
            }

            _Radius = _Radius * _Zoom_;
            _iHeight_ = (int)((_InstrumentSize_.Height - _InstrumentSize_.Top) * _Zoom_);
            _iWidth_ = (int)((_InstrumentSize_.Width - _InstrumentSize_.Left) * _Zoom_);

            _BasePoint.X = base.Padding.Left;
            _BasePoint.Y = base.Padding.Top;

            _BasePoint.X -= (int)(_InstrumentSize_.Left * _Zoom_);
            _BasePoint.Y -= (int)(_InstrumentSize_.Top * _Zoom_);
        }

        private RectangleF GetInstrumentSize(float Radius)
        {
            float _LowX_ = 0;
            float _LowY_ = 0;
            float _HighX_ = 0;
            float _HighY_ = 0;

            for (var _Angel_ = 0; _Angel_ <= 360; _Angel_++)
            {
                if ((_Angel_ >= (_StartAngel - 1)) && (_Angel_ <= (_StopAngel + 1)))
                {
                    var _bAngel_ = Grad2Bogen(_Angel_ + _AngelOffset);

                    var _X_ = (Radius * (float)Math.Cos(_bAngel_));
                    var _Y_ = (Radius * (float)Math.Sin(_bAngel_));

                    if (_X_ > _HighX_)
                        _HighX_ = _X_;

                    if (_X_ < _LowX_)
                        _LowX_ = _X_;

                    if (_Y_ > _HighY_)
                        _HighY_ = _Y_;

                    if (_Y_ < _LowY_)
                        _LowY_ = _Y_;
                }
            }

            return new RectangleF(_LowX_, _LowY_, _HighX_, _HighY_);
        }

        #region Make background
        private void MakeBackGround()
        {
            if ((Width > 0) && (Height > 0))
            {
                _Backgound = new Bitmap(Width, Height);
                using (var _Canvas_ = Graphics.FromImage(_Backgound))
                {
                    _Canvas_.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    _Canvas_.SmoothingMode = SmoothingMode.AntiAlias;

                    //Draw background
                    using (var _BackgroundBrush_ = new SolidBrush(BackColor))
                    {
                        _Canvas_.FillRectangle(_BackgroundBrush_, ClientRectangle);

                        if (base.BackgroundImage != null)
                            _Canvas_.DrawImageUnscaled(base.BackgroundImage, new Point(0, 0));
                    }

                    //draw Secundary scale
                    MakeBackGroundSecScale(_Canvas_);

                    //draw main scale
                    MakeBackGroundMainScale(_Canvas_);

                    //Draw border
                    if (_BorderEnabled)
                        using (var _BorderPen_ = new Pen(_BorderColor, _BorderWidth))
                        {
                            for (var _Angel_ = _StartAngel + 1; _Angel_ <= _StopAngel; _Angel_++)
                                _Canvas_.DrawLine(_BorderPen_, GetPointOnRadius((_Angel_ - 1) + _AngelOffset, _Radius), GetPointOnRadius(_Angel_ + _AngelOffset, _Radius));
                        }

                    _Canvas_.Flush(FlushIntention.Flush);
                }

                Invalidate();
            }
            else
                _Backgound = new Bitmap(1, 1);
        }

        private void MakeBackGroundMainScale(Graphics Canvas)
        {
            if (_ScaleMainEnabled || _ScaleMainTextEnabled)
                using (var _ScaleMainPen_ = new Pen(_ScaleMainColor, _ScaleMainLineWidth))
                {
                    using (var _ScaleTextBrush_ = new SolidBrush(_ScaleMainTextColor))
                    {
                        float _Scale_;

                        if (_ScaleReverse)
                            _Scale_ = _ScaleMax;
                        else
                            _Scale_ = _ScaleMin;

                        for (float _Angel_ = _StartAngel; _Angel_ <= _StopAngel; _Angel_ += (((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)) * _ScaleMainStep))
                        {
                            if (_ScaleMainEnabled)
                                Canvas.DrawLine(_ScaleMainPen_, GetPointOnRadius(_Angel_ + _AngelOffset, _Radius - _ScaleMainLineLenght), GetPointOnRadius(_Angel_ + _AngelOffset, _Radius));

                            if (_ScaleMainTextEnabled)
                            {
                                var _TextImage_ = GetRotatedText(string.Format(_ScaleMainTextFormat, _Scale_), _ScaleTextBrush_, _ScaleMainTextFont, _Angel_ + _AngelOffset + 90);
                                var _TextImageSize_ = _TextImage_.Size;

                                var _To_ = GetPointOnRadius(_Angel_ + _AngelOffset, _Radius - _ScaleMainLineLenght - _ScaleMainTextOffset);
                                _To_.X -= _TextImageSize_.Width / 2;
                                _To_.Y -= _TextImageSize_.Height / 2;

                                Canvas.DrawImageUnscaled(_TextImage_, (int)_To_.X, (int)_To_.Y);

                                if (_ScaleReverse)
                                    _Scale_ -= _ScaleMainStep;
                                else
                                    _Scale_ += _ScaleMainStep;
                            }
                        }
                    }
                }
        }

        private void MakeBackGroundSecScale(Graphics Canvas)
        {
            if (_ScaleSecundaryEnabled)
                using (var _ScaleSecundaryPen_ = new Pen(_ScaleSecundaryColor, _ScaleSecundaryLineWidth))
                {
                    for (float _Angel_ = _StartAngel; _Angel_ <= _StopAngel; _Angel_ += (((_StopAngel - _StartAngel) / (_ScaleMax - _ScaleMin)) * _ScaleSecundaryStep))
                        Canvas.DrawLine(_ScaleSecundaryPen_, GetPointOnRadius(_Angel_ + _AngelOffset, _Radius - _ScaleSecundaryLineLenght), GetPointOnRadius(_Angel_ + _AngelOffset, _Radius));
                }
        }
        #endregion

        private static Image GetRotatedText(string _Text, Brush _Brush, Font _Font, float _Angel)
        {
            var _back_ = new Bitmap(1, 1);
            SizeF _TextSize_;

            using (var _Canvas_ = Graphics.FromImage(_back_))
            {
                _Canvas_.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                _Canvas_.SmoothingMode = SmoothingMode.AntiAlias;

                _TextSize_ = _Canvas_.MeasureString(_Text, _Font);
            }

            _back_ = new Bitmap((int)_TextSize_.Width, (int)_TextSize_.Height);
            using (var _Canvas_ = Graphics.FromImage(_back_))
            {
                _Canvas_.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                _Canvas_.SmoothingMode = SmoothingMode.AntiAlias;

                _Canvas_.DrawString(_Text, _Font, _Brush, new PointF(0, 0));
            }

            //return back;
            return rotateImage(_back_, _Angel);
        }

        private static Bitmap rotateImage(Bitmap b, float angle)
        {
            //fix angle
            while (angle < 0)
                angle += 360;

            while (angle > 360)
                angle -= 360;

            var _NewSize_ = GetRotatedSize(b.Size, angle);

            var _back_ = new Bitmap((int)_NewSize_.Width, (int)_NewSize_.Height);
            //make a graphics object from the empty bitmap
            var _canvas_ = Graphics.FromImage(_back_);
            //move rotation point to center of image
            _canvas_.TranslateTransform(_NewSize_.Width / 2, _NewSize_.Height / 2);
            //rotate
            _canvas_.RotateTransform(angle);
            //move image back
            _canvas_.TranslateTransform(-(_NewSize_.Width / 2), -(_NewSize_.Height / 2));
            //draw passed in image onto graphics object
            _canvas_.DrawImage(b, new PointF((_NewSize_.Width - b.Width) / 2, (_NewSize_.Height - b.Height) / 2));
            return _back_;
        }

        private static SizeF GetRotatedSize(Size BaseSize, float angle)
        {
            var _Width_ = BaseSize.Width / 2f;
            var _Height_ = BaseSize.Height / 2f;

            var _w_ = GetAngelInBogen(angle);

            var _c_ = (float)Math.Cos(_w_);
            var _s_ = (float)Math.Sin(_w_);

            _Width_ = Math.Abs((_Width_ * _c_) + (_Height_ * _s_)) + Math.Abs((-_Width_ * _c_) + (-_Height_ * _s_));
            _Height_ = Math.Abs(-((_Width_ * _s_) + (_Height_ * _c_))) + Math.Abs(-((-_Width_ * _s_) + (-_Height_ * _c_)));

            return new Size((int)_Width_, (int)_Height_);
        }

        private static double GetAngelInBogen(float angle)
        {
            var _back_ = 0d;
            if ((angle >= 0) && (angle <= 90))
                _back_ = Grad2Bogen(angle);
            if ((angle > 90) && (angle <= 180))
                _back_ = Grad2Bogen(90 - (angle - 90));
            if ((angle > 180) && (angle <= 270))
                _back_ = Grad2Bogen((angle - 180));
            if ((angle > 270) && (angle <= 360))
                _back_ = Grad2Bogen(270 - (angle - 270));

            return _back_;
        }

        private static double Grad2Bogen(float Angel)
        {
            return Angel * Math.PI / 180;
        }

        #region overrides
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                MakeBackGround();
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                MakeBackGround();
            }
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                MakeBackGround();
            }
        }

        //public override Padding
        public new Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
                RecalcSize();
                MakeBackGround();
            }
        }

        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
                MakeBackGround();
            }
        }
        #endregion

        #region properties
        [DefaultValue(2)]
        public int BorderWidth
        {
            get
            {
                return _BorderWidth;
            }
            set
            {
                if (value > 0)
                {
                    _BorderWidth = value;
                    MakeBackGround();
                }
            }
        }

        public Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                _BorderColor = value;
                MakeBackGround();
            }
        }

        [DefaultValue(true)]
        public bool BorderEnabled
        {
            get
            {
                return _BorderEnabled;
            }
            set
            {
                _BorderEnabled = value;
                MakeBackGround();
            }
        }

        [DefaultValue(0)]
        public int AngelStart
        {
            get
            {
                return _StartAngel;
            }
            set
            {
                if ((value >= 0) && (value < _StopAngel))
                {
                    _StartAngel = value;
                    RecalcSize();
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(360)]
        public int AngelStop
        {
            get
            {
                return _StopAngel;
            }
            set
            {
                if ((value <= 360) && (value > _StartAngel))
                {
                    _StopAngel = value;
                    RecalcSize();
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(0)]
        public int AngelOffset
        {
            get
            {
                return _AngelOffset;
            }
            set
            {
                if ((value >= -180) && (value <= 180))
                {
                    _AngelOffset = value;
                    RecalcSize();
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(0)]
        public float ScaleMin
        {
            get
            {
                return _ScaleMin;
            }
            set
            {
                if (value < _ScaleMax)
                {
                    _ScaleMin = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(10F)]
        public float ScaleMax
        {
            get
            {
                return _ScaleMax;
            }
            set
            {
                if (value > _ScaleMin)
                {
                    _ScaleMax = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(false)]
        public bool ScaleReverse
        {
            get
            {
                return _ScaleReverse;
            }
            set
            {
                _ScaleReverse = value;
                MakeBackGround();
            }
        }

        [DefaultValue(1F)]
        public float ScaleMainStep
        {
            get
            {
                return _ScaleMainStep;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleMainStep = value;
                    MakeBackGround();
                }
            }
        }

        public Color ScaleMainColor
        {
            get
            {
                return _ScaleMainColor;
            }
            set
            {
                _ScaleMainColor = value;
                MakeBackGround();
            }
        }

        [DefaultValue(2)]
        public int ScaleMainLineWidth
        {
            get
            {
                return _ScaleMainLineWidth;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleMainLineWidth = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(10)]
        public int ScaleMainLineLenght
        {
            get
            {
                return _ScaleMainLineLenght;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleMainLineLenght = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(true)]
        public bool ScaleMainEnabled
        {
            get
            {
                return _ScaleMainEnabled;
            }
            set
            {
                _ScaleMainEnabled = value;
                MakeBackGround();
            }
        }

        [DefaultValue(10)]
        public int ScaleMainTextOffset
        {
            get
            {
                return _ScaleMainTextOffset;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleMainTextOffset = value;
                    MakeBackGround();
                }
            }
        }

        public Color ScaleMainTextColor
        {
            get
            {
                return _ScaleMainTextColor;
            }
            set
            {
                _ScaleMainTextColor = value;
                MakeBackGround();
            }
        }

        public Font ScaleMainTextFont
        {
            get
            {
                return _ScaleMainTextFont;
            }
            set
            {
                _ScaleMainTextFont = value;
                MakeBackGround();
            }
        }

        [DefaultValue("{0}")]
        public string ScaleMainTextFormat
        {
            get
            {
                return _ScaleMainTextFormat;
            }
            set
            {
                if (value.Length > 0)
                {
                    _ScaleMainTextFormat = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(true)]
        public bool ScaleMainTextEnabled
        {
            get
            {
                return _ScaleMainTextEnabled;
            }
            set
            {
                _ScaleMainTextEnabled = value;
                MakeBackGround();
            }
        }

        [DefaultValue(1F)]
        public float ScaleSecundaryStep
        {
            get
            {
                return _ScaleSecundaryStep;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleSecundaryStep = value;
                    MakeBackGround();
                }
            }
        }

        public Color ScaleSecundaryColor
        {
            get
            {
                return _ScaleSecundaryColor;
            }
            set
            {
                _ScaleSecundaryColor = value;
                MakeBackGround();
            }
        }

        [DefaultValue(1)]
        public int ScaleSecundaryLineWidth
        {
            get
            {
                return _ScaleSecundaryLineWidth;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleSecundaryLineWidth = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(5)]
        public int ScaleSecundaryLineLenght
        {
            get
            {
                return _ScaleSecundaryLineLenght;
            }
            set
            {
                if (value > 0)
                {
                    _ScaleSecundaryLineLenght = value;
                    MakeBackGround();
                }
            }
        }

        [DefaultValue(true)]
        public bool ScaleSecundaryEnabled
        {
            get
            {
                return _ScaleSecundaryEnabled;
            }
            set
            {
                _ScaleSecundaryEnabled = value;
                MakeBackGround();
            }
        }

        [DefaultValue(0)]
        public float Value
        {
            get
            {
                return _DestValue;
            }
            set
            {
                if ((value >= _ScaleMin) && (value <= _ScaleMax))
                {
                    _DestValue = value;
                    if (!_UseSmother)
                        _Value = value;

                    _LastValues.Add(value);

                    Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public float Value2
        {
            get
            {
                return _Value2;
            }
            set
            {
                if ((value >= _ScaleMin) && (value <= _ScaleMax))
                {
                    _Value2 = value;
                    Invalidate();
                }
            }
        }

        [DefaultValue(2)]
        public int PointerWidth
        {
            get
            {
                return _PointerWidth;
            }
            set
            {
                if (value > 0)
                {
                    _PointerWidth = value;
                    Invalidate();
                }
            }
        }

        public Color PointerBaseColor
        {
            get
            {
                return _PointerBaseColor;
            }
            set
            {
                _PointerBaseColor = value;
                Invalidate();
            }
        }

        public Color PointerColor
        {
            get
            {
                return _PointerColor;
            }
            set
            {
                _PointerColor = value;
                Invalidate();
            }
        }

        [DefaultValue(20)]
        public int PointerBaseSize
        {
            get
            {
                return _PointerBaseSize;
            }
            set
            {
                if (value > 1)
                {
                    _PointerBaseSize = value;
                    Invalidate();
                }
            }
        }

        [DefaultValue(true)]
        public bool PointerBaseEnabled
        {
            get
            {
                return _PointerBaseEnabled;
            }
            set
            {
                _PointerBaseEnabled = value;
                Invalidate();
            }
        }

        public Color BorderIndicatorColor
        {
            get
            {
                return _BorderIndicatorColor;
            }
            set
            {
                _BorderIndicatorColor = value;
                Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool SmotherActive
        {
            get
            {
                return _UseSmother;
            }
            set
            {
                _UseSmother = value;
                if (_UseSmother)
                    _UpdateSmother.Start();
                else
                    _UpdateSmother.Stop();
            }
        }

        [DefaultValue(5F)]
        public float SmotherMaxUpdateValue
        {
            get
            {
                return _MaxUpdateValue;
            }
            set
            {
                if (value > 0)
                    _MaxUpdateValue = value;
            }
        }

        [DefaultValue(50)]
        public int SmotherUpdateInterval
        {
            get
            {
                return _UpdateSmother.Interval;
            }
            set
            {
                if (value > 0)
                    _UpdateSmother.Interval = value;
            }
        }

        [DefaultValue(100)]
        public int StatisticHistoryCount
        {
            get
            {
                return _LastValues.Capacity;
            }
            set
            {
                if (value > 0)
                    _LastValues = new C_RingBuffer<float>(value);
            }
        }

        public Color StatisticMaxColor
        {
            get
            {
                return _StatMaxColor;
            }
            set
            {
                _StatMaxColor = value;
                Invalidate();
            }
        }

        public Color StatisticMinColor
        {
            get
            {
                return _StatMinColor;
            }
            set
            {
                _StatMinColor = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool StatisticMaxVisible
        {
            get
            {
                return _StatMaxVisible;
            }
            set
            {
                _StatMaxVisible = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool StatisticMinVisible
        {
            get
            {
                return _StatMinVisible;
            }
            set
            {
                _StatMinVisible = value;
                Invalidate();
            }
        }
        #endregion
    }
}

