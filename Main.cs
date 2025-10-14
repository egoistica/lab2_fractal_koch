using Godot;
using System.Collections.Generic;

namespace Fractal
{
	public partial class Main : Control
	{
		private UniversalKochFractal _fractal;
		private VBoxContainer _uiContainer;
		
		private HSlider _iterationsSlider;
		private HSlider _scaleSlider;
		private HSlider _angleSlider;
		private HSlider _shapeSideSlider;  
		private ColorPickerButton _colorPicker;
		
		private Label _iterationsLabel;
		private Label _scaleLabel;
		private Label _angleLabel;   
		private Label _shapeSideLabel;     
		
		private Button _resetButton;
		private Button _resetPositionButton;
		private Button _animateButton;
		
		private Timer _animationTimer;
		private bool _isAnimating = false;

		//private OptionButton _shapeSelector;

		public override void _Ready()
		{
			GD.Print("Универсальный фрактал Коха запущен!");
			SetupUI();
			SetupFractal();
		}

		private void SetupUI()
		{
			_uiContainer = new VBoxContainer();
			_uiContainer.Position = new Vector2(20, 20);
			_uiContainer.Size = new Vector2(320, 500);
			AddChild(_uiContainer);

			var title = new Label();
			title.Text = "Универсальный фрактал Коха";
			title.AddThemeFontSizeOverride("font_size", 24);
			_uiContainer.AddChild(title);

			// --- Итерации ---
			_iterationsLabel = new Label { Text = "Итерации: 0" };
			_uiContainer.AddChild(_iterationsLabel);

			_iterationsSlider = new HSlider
			{
				MinValue = 0,
				MaxValue = 8,
				Value = 0,
				Step = 1
			};
			_iterationsSlider.ValueChanged += OnIterationsChanged;
			_uiContainer.AddChild(_iterationsSlider);

			// --- Масштаб ---
			_scaleLabel = new Label { Text = "Масштаб: 2.0" };
			_uiContainer.AddChild(_scaleLabel);

			_scaleSlider = new HSlider
			{
				MinValue = 0.1f,
				MaxValue = 10.0f,
				Value = 2.0f,
				Step = 0.1f
			};
			_scaleSlider.ValueChanged += OnScaleChanged;
			_uiContainer.AddChild(_scaleSlider);

			// --- Базовая фигура ---
			_shapeSideLabel = new Label { Text = "Кол-во сторон фигуры: 3" };
			_uiContainer.AddChild(_shapeSideLabel);

			_shapeSideSlider = new HSlider
			{
				MinValue = 3,
				MaxValue = 30,
				Value = 0,
				Step = 1
			};
			_shapeSideSlider.ValueChanged += OnShapeChanged;
			_uiContainer.AddChild(_shapeSideSlider);

			// --- 🔹 Ползунок угла ---
			_angleLabel = new Label { Text = "Угол выступа: 120°" };
			_uiContainer.AddChild(_angleLabel);

			_angleSlider = new HSlider
			{
				MinValue = 0,
				MaxValue = 180,
				Value = 120,
				Step = 1
			};
			_angleSlider.ValueChanged += OnAngleChanged;
			_uiContainer.AddChild(_angleSlider);

			// --- Цвет ---
			var colorLabel = new Label { Text = "Цвет фрактала:" };
			_uiContainer.AddChild(colorLabel);

			_colorPicker = new ColorPickerButton { Color = Colors.White };
			_colorPicker.ColorChanged += OnColorChanged;
			_uiContainer.AddChild(_colorPicker);

			// --- Кнопки управления ---
			var buttonContainer = new HBoxContainer();
			_uiContainer.AddChild(buttonContainer);

			_resetButton = new Button { Text = "Сброс" };
			_resetButton.Pressed += OnResetPressed;
			buttonContainer.AddChild(_resetButton);

			_resetPositionButton = new Button { Text = "Центр" };
			_resetPositionButton.Pressed += OnResetPositionPressed;
			buttonContainer.AddChild(_resetPositionButton);

			_animateButton = new Button { Text = "Анимация" };
			_animateButton.Pressed += OnAnimatePressed;
			buttonContainer.AddChild(_animateButton);

			// --- Таймер ---
			_animationTimer = new Timer { WaitTime = 0.5 };
			_animationTimer.Timeout += OnAnimationTick;
			AddChild(_animationTimer);
		}

		private void SetupFractal()
		{
			_fractal = new UniversalKochFractal();
			AddChild(_fractal);

			float centerX = GetViewport().GetVisibleRect().Size.X / 2;
			float centerY = GetViewport().GetVisibleRect().Size.Y / 2;
			_fractal.Position = new Vector2(centerX, centerY);
			_fractal.ScaleChanged += OnFractalScaleChanged;
		}

		// --- События UI ---
		private void OnIterationsChanged(double value)
		{
			int i = (int)value;
			_iterationsLabel.Text = $"Итерации: {i}";
			_fractal.SetIterations(i);
		}

		private void OnScaleChanged(double value)
		{
			float s = (float)value;
			_scaleLabel.Text = $"Масштаб: {s:F1}";
			_fractal.SetScale(s);
		}

		private void OnAngleChanged(double value)
		{
			float angle = (float)value;
			_angleLabel.Text = $"Угол выступа: {angle:F0}°";
			_fractal.SetPatternAngle(angle);
		}

		private void OnShapeChanged(double value)
		{
			int sides = (int)value;
			_shapeSideLabel.Text = $"Кол-во сторон фигуры: {sides}";
			_fractal.SetBaseSides(sides);
		}

		private void OnColorChanged(Color color)
		{
			_fractal.SetColor(color);
		}

		private void OnResetPressed()
		{
			_iterationsSlider.Value = 0;
			_scaleSlider.Value = 2.0f;
			_angleSlider.Value = 60;
			_colorPicker.Color = Colors.White;
			_shapeSideSlider.Value = 3;

			_fractal.BaseSides = 3;
			_fractal.SetPatternAngle(120f);
			_fractal.SetIterations(0);
			_fractal.SetScale(2.0f);
			_fractal.SetColor(Colors.White);
		}

		private void OnResetPositionPressed()
		{
			// Центрируем Node2D в середине вьюпорта
			float centerX = GetViewport().GetVisibleRect().Size.X / 2;
			float centerY = GetViewport().GetVisibleRect().Size.Y / 2;
			_fractal.Position = new Vector2(centerX, centerY);

			// Сбрасываем внутреннее смещение (offset), чтобы рисунок действительно оказался по центру
			_fractal.ResetPosition();
		}

		private void OnAnimatePressed()
		{
			if (_isAnimating)
			{
				_isAnimating = false;
				_animationTimer.Stop();
				_animateButton.Text = "Анимация";
			}
			else
			{
				_isAnimating = true;
				_animationTimer.Start();
				_animateButton.Text = "Стоп";
			}
		}

		private void OnAnimationTick()
		{
			if (!_isAnimating) return;

			int current = (int)_iterationsSlider.Value;
			if (current < 6) _iterationsSlider.Value = current + 1;
			else _iterationsSlider.Value = 0;
		}

		private void OnFractalScaleChanged(float newScale)
		{
			_scaleSlider.Value = newScale;
			_scaleLabel.Text = $"Масштаб: {newScale:F2}";
		}
	}
}
