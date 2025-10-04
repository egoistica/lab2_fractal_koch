using Godot;
using System.Collections.Generic;

namespace Fractal
{
    public partial class KochFractal : Node2D
    {
        private List<Vector2> _points = new List<Vector2>();
        private int _iterations = 0;
        private float _length = 400.0f;
        private Color _color = Colors.White;
        private float _scale = 4.0f;
        private Vector2 _offset = Vector2.Zero;
        private bool _isDragging = false;
        private Vector2 _lastMousePosition;

        public int Iterations
        {
            get => _iterations;
            set
            {
                _iterations = Mathf.Clamp(value, 0, 6);
                GenerateFractal();
            }
        }

        public float Length
        {
            get => _length;
            set
            {
                _length = Mathf.Max(value, 50.0f);
                GenerateFractal();
            }
        }

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = Mathf.Max(value, 0.1f);
                QueueRedraw();
            }
        }

        public Color FractalColor
        {
            get => _color;
            set => _color = value;
        }

        public override void _Ready()
        {
            GenerateFractal();
        }

        public override void _Draw()
        {
            if (_points.Count < 2) return;

            for (int i = 0; i < _points.Count - 1; i++)
            {
                Vector2 start = _points[i] * _scale + _offset;
                Vector2 end = _points[i + 1] * _scale + _offset;
                DrawLine(start, end, _color, 2.0f);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    if (mouseButton.Pressed)
                    {
                        // Проверяем, что клик НЕ по UI элементам (ползункам)
                        Vector2 mousePos = GetGlobalMousePosition();
                        if (!IsPointOverUI(mousePos))
                        {
                            _isDragging = true;
                            _lastMousePosition = mousePos;
                        }
                    }
                    else
                    {
                        _isDragging = false;
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
            {
                Vector2 currentMousePosition = GetGlobalMousePosition();
                Vector2 delta = currentMousePosition - _lastMousePosition;
                _offset += delta;
                _lastMousePosition = currentMousePosition;
                QueueRedraw();
            }
        }

        private bool IsPointOverUI(Vector2 point)
        {
            // Проверяем, находится ли точка в области UI элементов (левая панель)
            // UI панель находится в левой части экрана
            return point.X >= 0 && point.X <= 350 && point.Y >= 0 && point.Y <= 600;
        }

        private void GenerateFractal()
        {
            _points.Clear();
            
            if (_iterations == 0)
            {
                // Базовый случай - прямая линия
                _points.Add(new Vector2(-_length / 2, 0));
                _points.Add(new Vector2(_length / 2, 0));
            }
            else
            {
                // Начинаем с базовой линии
                Vector2 start = new Vector2(-_length / 2, 0);
                Vector2 end = new Vector2(_length / 2, 0);
                
                GenerateKochCurve(start, end, _iterations);
            }
            
            QueueRedraw();
        }



        private void GenerateKochCurve(Vector2 start, Vector2 end, int iterations)
        {
            if (iterations == 0)
            {
                // Базовый случай - добавляем точки линии
                if (_points.Count == 0)
                    _points.Add(start);
                _points.Add(end);
                return;
            }

            // Вычисляем точки для кривой Коха
            Vector2 p1 = start;
            Vector2 p2 = start + (end - start) / 3;
            Vector2 p3 = CalculateKochPeak(start, end);
            Vector2 p4 = start + 2 * (end - start) / 3;
            Vector2 p5 = end;

            // Рекурсивно генерируем кривую для каждого сегмента
            GenerateKochCurve(p1, p2, iterations - 1);
            GenerateKochCurve(p2, p3, iterations - 1);
            GenerateKochCurve(p3, p4, iterations - 1);
            GenerateKochCurve(p4, p5, iterations - 1);
        }



        private Vector2 CalculateKochPeak(Vector2 start, Vector2 end)
        {
            Vector2 direction = (end - start).Normalized();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            float segmentLength = (end - start).Length() / 3;
            float height = segmentLength * Mathf.Sqrt(3) / 2;
            
            Vector2 midpoint = start + (end - start) / 2;
            // Изменяем направление на противоположное, чтобы треугольник строился вверх
            return midpoint - perpendicular * height;
        }


        public void SetIterations(int iterations)
        {
            Iterations = iterations;
        }

        public void SetLength(float length)
        {
            Length = length;
        }

        public void SetColor(Color color)
        {
            FractalColor = color;
            QueueRedraw();
        }

        public void SetScale(float scale)
        {
            Scale = scale;
        }

        public void ResetPosition()
        {
            _offset = Vector2.Zero;
            QueueRedraw();
        }

        public void SetPosition(Vector2 position)
        {
            _offset = position;
            QueueRedraw();
        }
    }
}
