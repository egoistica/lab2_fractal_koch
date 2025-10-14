using Godot;
using System;
using System.Collections.Generic;

namespace Fractal
{
	public partial class UniversalKochFractal : Node2D
	{
		[Signal]
		public delegate void ScaleChangedEventHandler(float newScale);

		private List<Vector2> _points = new List<Vector2>();
		private int _iterations = 0;
		private int _baseSides = 3; // кол-во сторон базовой фигуры
		private float _length = 300.0f;
		private Color _color = Colors.White;
		private float _scale = 2.0f;
		private Vector2 _offset = Vector2.Zero;
		private float _patternAngle = 60f; // угол выступа
		
		public void SetPatternAngle(float angle)
		{
			if (angle > 60 && angle < 120)
			{
				// Если угол в запрещенном диапазоне, устанавливаем ближайшее допустимое значение
				_patternAngle = (angle <= 60) ? 60f : 120f;
			}
			else
			{
				_patternAngle = Mathf.Clamp(angle, 0f, 180f);
			}
			GenerateFractal();
		}

		private bool _isDragging = false;
		private Vector2 _lastMousePosition;

		// 🔹 Угловой паттерн для средней части (в градусах)
		// По умолчанию — треугольник Коха
		private List<float> _patternAngles = new() { 0, 60, -120, 60, 0 };

		public int Iterations
		{
			get => _iterations;
			set
			{
				_iterations = Mathf.Clamp(value, 0, 8);
				GenerateFractal();
			}
		}

		public int BaseSides
		{
			get => _baseSides;
			set
			{
				_baseSides = Mathf.Clamp(value, 3, 30);
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
				Vector2 a = _points[i] * _scale + _offset;
				Vector2 b = _points[i + 1] * _scale + _offset;
				DrawLine(a, b, _color, 2.0f);
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb)
			{
				if (mb.ButtonIndex == MouseButton.WheelUp)
				{
					Scale *= 1.1f;
					EmitSignal(SignalName.ScaleChanged, Scale);
					QueueRedraw();
				}
				else if (mb.ButtonIndex == MouseButton.WheelDown)
				{
					Scale /= 1.1f;
					EmitSignal(SignalName.ScaleChanged, Scale);
					QueueRedraw();
				}

				if (mb.ButtonIndex == MouseButton.Left)
				{
					if (mb.Pressed)
					{
						Vector2 mousePos = GetGlobalMousePosition();
						if (!IsPointOverUI(mousePos))
						{
							_isDragging = true;
							_lastMousePosition = mousePos;
						}
					}
					else _isDragging = false;
				}
			}
			else if (@event is InputEventMouseMotion mm && _isDragging)
			{
				Vector2 cur = GetGlobalMousePosition();
				_offset += cur - _lastMousePosition;
				_lastMousePosition = cur;
				QueueRedraw();
			}
		}

		private bool IsPointOverUI(Vector2 p)
		{
			return p.X >= 0 && p.X <= 400 && p.Y >= 0 && p.Y <= 350;
		}

		private void GenerateFractal()
		{
			_points.Clear();

			// создаём базовую фигуру
			var polygon = CreateBasePolygon(_baseSides, _length);
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector2 start = polygon[i];
				Vector2 end = polygon[(i + 1) % polygon.Count];
				GenerateSegment(start, end, _iterations);
			}

			QueueRedraw();
		}

		// 🔹 Создаёт вершины исходного многоугольника
		private List<Vector2> CreateBasePolygon(int sides, float size)
		{
			List<Vector2> pts = new();
			float angleStep = Mathf.Tau / sides;
			for (int i = 0; i < sides; i++)
			{
				float a = i * angleStep - Mathf.Pi / 2; // вершиной вверх
				pts.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * size / 2);
			}
			return pts;
		}

		private void GenerateSegment(Vector2 start, Vector2 end, int depth)
		{
			if (depth == 0)
			{
				if (_points.Count == 0)
					_points.Add(start);
				_points.Add(end);
				return;
			}

			// 1️⃣ Точки деления
			Vector2 a = start + (end - start) / 3f;
			Vector2 b = start + 2f * (end - start) / 3f;

			// 2️⃣ Вектор направления
			Vector2 dir = (end - start).Normalized();
			float segLen = (end - start).Length() / 3f;

			// 3️⃣ Перпендикуляр наружу
			Vector2 perp = new Vector2(-dir.Y, dir.X);

			// 4️⃣ Высота выступа
			float radians = Mathf.DegToRad(_patternAngle);
			float height = Mathf.Tan(radians) * segLen / 2f;

			// 5️⃣ Вершина "пика" наружу
			Vector2 mid = (a + b) / 2f;
			Vector2 peak = mid + perp * height;

			// 6️⃣ Рекурсивное построение четырёх сегментов
			GenerateSegment(start, a, depth - 1);
			GenerateSegment(a, peak, depth - 1);
			GenerateSegment(peak, b, depth - 1);
			GenerateSegment(b, end, depth - 1);
		}



		// 🔹 Установка паттерна (например: треугольник, квадрат, ромб)
		public void SetPattern(List<float> angles)
		{
			if (angles.Count < 2)
				return;

			_patternAngles = new List<float>(angles);
			GenerateFractal();
		}

		// 🔹 Служебные методы
		public void SetIterations(int i) => Iterations = i;
		public void SetLength(float l) => Length = l;
		public void SetColor(Color c) { FractalColor = c; QueueRedraw(); }
		public void SetScale(float s) => Scale = s;
		public void SetBaseSides(int i) => BaseSides = i;
		public void ResetPosition() { _offset = Vector2.Zero; QueueRedraw(); }
		public void SetPosition(Vector2 p) { _offset = p; QueueRedraw(); }
	}
}
