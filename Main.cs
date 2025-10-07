using Godot;

namespace Fractal
{
	public partial class Main : Control
	{
		private KochFractal _kochFractal;
		private VBoxContainer _uiContainer;
		private HSlider _iterationsSlider;
		private HSlider _scaleSlider;
		private ColorPickerButton _colorPicker;
		private Label _iterationsLabel;
		private Label _scaleLabel;
		private Button _resetButton;
		private Button _resetPositionButton;
		private Button _animateButton;
		private Timer _animationTimer;
		private bool _isAnimating = false;

		public override void _Ready()
		{
			GD.Print("Добро пожаловать в программу отрисовки фракталов!");
			SetupUI();
			SetupFractal();
		}

		private void SetupUI()
		{
			// Создаем контейнер для UI
			_uiContainer = new VBoxContainer();
			_uiContainer.Position = new Vector2(20, 20);
			_uiContainer.Size = new Vector2(300, 400);
			AddChild(_uiContainer);

			// Заголовок
			var title = new Label();
			title.Text = "Фрактал: Снежинка Коха";
			title.AddThemeFontSizeOverride("font_size", 24);
			_uiContainer.AddChild(title);

			// Слайдер для итераций
			_iterationsLabel = new Label();
			_iterationsLabel.Text = "Итерации: 0";
			_uiContainer.AddChild(_iterationsLabel);

			_iterationsSlider = new HSlider();
			_iterationsSlider.MinValue = 0;
			_iterationsSlider.MaxValue = 6;
			_iterationsSlider.Value = 0;
			_iterationsSlider.Step = 1;
			_iterationsSlider.ValueChanged += OnIterationsChanged;
			_uiContainer.AddChild(_iterationsSlider);


			// Слайдер для масштаба
			_scaleLabel = new Label();
			_scaleLabel.Text = "Масштаб: 1.5";
			_uiContainer.AddChild(_scaleLabel);

			_scaleSlider = new HSlider();
			_scaleSlider.MinValue = 0.01f;
			_scaleSlider.MaxValue = 20.0f;
			_scaleSlider.Value = 1.5f;
			_scaleSlider.Step = 0.01f;
			_scaleSlider.ValueChanged += OnScaleChanged;
			_uiContainer.AddChild(_scaleSlider);

			// Выбор цвета
			var colorLabel = new Label();
			colorLabel.Text = "Цвет фрактала:";
			_uiContainer.AddChild(colorLabel);

			_colorPicker = new ColorPickerButton();
			_colorPicker.Color = Colors.White;
			_colorPicker.ColorChanged += OnColorChanged;
			_uiContainer.AddChild(_colorPicker);

			// Кнопки управления
			var buttonContainer = new HBoxContainer();
			_uiContainer.AddChild(buttonContainer);

			_resetButton = new Button();
			_resetButton.Text = "Сброс";
			_resetButton.Pressed += OnResetPressed;
			buttonContainer.AddChild(_resetButton);

			_resetPositionButton = new Button();
			_resetPositionButton.Text = "Центр";
			_resetPositionButton.Pressed += OnResetPositionPressed;
			buttonContainer.AddChild(_resetPositionButton);

			_animateButton = new Button();
			_animateButton.Text = "Анимация";
			_animateButton.Pressed += OnAnimatePressed;
			buttonContainer.AddChild(_animateButton);

			// Таймер для анимации
			_animationTimer = new Timer();
			_animationTimer.WaitTime = 0.5;
			_animationTimer.Timeout += OnAnimationTick;
			AddChild(_animationTimer);
		}

		private void SetupFractal()
		{
			_kochFractal = new KochFractal();
			// Центрируем фрактал по горизонтали относительно всего окна, но размещаем внизу
			float centerX = GetViewport().GetVisibleRect().Size.X / 2;
			float bottomY = GetViewport().GetVisibleRect().Size.Y / 2;
			_kochFractal.Position = new Vector2(centerX, bottomY);
			AddChild(_kochFractal);
			
			_kochFractal.ScaleChanged += OnFractalScaleChanged;
		}

		private void OnIterationsChanged(double value)
		{
			int iterations = (int)value;
			_iterationsLabel.Text = $"Итерации: {iterations}";
			_kochFractal.SetIterations(iterations);
		}


		private void OnScaleChanged(double value)
		{
			float scale = (float)value;
			_scaleLabel.Text = $"Масштаб: {scale:F1}";
			_kochFractal.SetScale(scale);
		}

		private void OnColorChanged(Color color)
		{
			_kochFractal.SetColor(color);
		}

		private void OnResetPressed()
		{
			_iterationsSlider.Value = 0;
			_scaleSlider.Value = 1.5f;
			_colorPicker.Color = Colors.White;
			_kochFractal.SetIterations(0);
			_kochFractal.SetScale(1.5f);
			_kochFractal.SetColor(Colors.White);
		}

		private void OnResetPositionPressed()
		{
			// Центрируем фрактал по горизонтали относительно всего окна, но размещаем внизу
			float centerX = GetViewport().GetVisibleRect().Size.X / 2;
			float bottomY = GetViewport().GetVisibleRect().Size.Y / 2; 
			
			// Вычисляем смещение относительно текущей позиции фрактала
			Vector2 currentPos = _kochFractal.Position;
			Vector2 targetOffset = new Vector2(centerX, bottomY) - currentPos;
			_kochFractal.SetPosition(targetOffset);
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

			int currentIterations = (int)_iterationsSlider.Value;
			int maxIterations = 6;
			
			if (currentIterations < maxIterations)
			{
				_iterationsSlider.Value = currentIterations + 1;
			}
			else
			{
				_iterationsSlider.Value = 0;
			}
		}
		
		private void OnFractalScaleChanged(float newScale)
		{
			_scaleSlider.Value = newScale; // обновляем ползунок
			_scaleLabel.Text = $"Масштаб: {newScale:F2}";
		}
	}
}
