#nullable enable
using System;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.Demos.Managers
{
    // handles the 3 menu screens: main menu (play/audio/controls/exit), the audio submenu
    // (music + sfx sliders) and the controls submenu (just an image of the keybinds + back button).
    // this is a GameComponent rather than living inside a Scene because it needs its own dedicated
    // "menu scene" that's separate from whatever level is loaded - keeps menu UI alive across level loads
    public sealed class MenuManager : GameComponent
    {
        #region Fields
        private Scene? _menuScene;

        // the 3 panels
        private UIMenuPanel? _mainMenuPanel;
        private UIMenuPanel? _audioMenuPanel;
        private UIMenuPanel? _controlsMenuPanel;

        // main menu buttons
        private UIButton? _playButton;
        private UIButton? _audioButton;
        private UIButton? _controlsButton;
        private UIButton? _exitButton;

        // audio menu controls
        private UIButton? _audioBackButton;
        private UISlider? _musicSlider;
        private UISlider? _sfxSlider;

        // controls menu controls
        private UIButton? _controlsBackButton;
        private UITexture? _controlsLayoutTexture;

        // assets passed in from Initialize()
        private Texture2D? _buttonTexture;
        private Texture2D? _sliderTrackTexture;
        private Texture2D? _sliderHandleTexture;
        private Texture2D? _controlsLayout;
        private SpriteFont? _font;

        private bool _configured;
        private bool _built;
        #endregion

        #region Properties
        // hook these up in your game class to actually respond to menu clicks
        public event Action? PlayRequested;
        public event Action? ExitRequested;
        public event Action<float>? MusicVolumeChanged;
        public event Action<float>? SfxVolumeChanged;
        #endregion

        #region Constructors
        // remember to add this to Game.Components, doesn't do anything on its own otherwise
        public MenuManager(Game game)
            : base(game)
        {
        }
        #endregion

        #region Methods
        // pass in the menu scene + all the ui textures/font, this kicks off building the actual panels
        public void Initialize(
            Scene menuScene,
            Texture2D buttonTexture,
            Texture2D sliderTrackTexture,
            Texture2D sliderHandleTexture,
            Texture2D controlsLayoutTexture,
            SpriteFont font)
        {
            if (menuScene == null)
                throw new ArgumentNullException(nameof(menuScene));
            if (buttonTexture == null)
                throw new ArgumentNullException(nameof(buttonTexture));
            if (sliderTrackTexture == null)
                throw new ArgumentNullException(nameof(sliderTrackTexture));
            if (sliderHandleTexture == null)
                throw new ArgumentNullException(nameof(sliderHandleTexture));
            if (controlsLayoutTexture == null)
                throw new ArgumentNullException(nameof(controlsLayoutTexture));
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            _menuScene = menuScene;
            _buttonTexture = buttonTexture;
            _sliderTrackTexture = sliderTrackTexture;
            _sliderHandleTexture = sliderHandleTexture;
            _controlsLayout = controlsLayoutTexture;
            _font = font;

            _configured = true;

            TryBuildMenus();
        }

        // shows main menu, hides the other two. assumes the menu scene is the active one right now
        public void ShowMainMenu()
        {
            if (_mainMenuPanel == null ||
                _audioMenuPanel == null ||
                _controlsMenuPanel == null)
                return;

            SetActivePanel(_mainMenuPanel, _audioMenuPanel, _controlsMenuPanel);
        }

        public void ShowAudioMenu()
        {
            if (_mainMenuPanel == null ||
                _audioMenuPanel == null ||
                _controlsMenuPanel == null)
                return;

            SetActivePanel(_audioMenuPanel, _mainMenuPanel, _controlsMenuPanel);
        }

        public void ShowControlsMenu()
        {
            if (_mainMenuPanel == null ||
                _audioMenuPanel == null ||
                _controlsMenuPanel == null)
                return;

            SetActivePanel(_controlsMenuPanel, _mainMenuPanel, _audioMenuPanel);
        }

        private void TryBuildMenus()
        {
            if (_built)
                return;

            if (!_configured)
                return;

            if (_menuScene == null)
                return;

            if (_buttonTexture == null ||
                _sliderTrackTexture == null ||
                _sliderHandleTexture == null ||
                _controlsLayout == null ||
                _font == null)
                return;

            BuildPanels(_menuScene);
            _built = true;

            ShowMainMenu();
        }

        private void BuildPanels(Scene scene)
        {
            // just eyeballed these numbers until the layout looked decent, nothing fancy
            Vector2 panelPosition = new Vector2(100f, 100f);
            Vector2 itemSize = new Vector2(260f, 64f);
            float spacing = 12f;

            // ---------- main menu panel ----------
            GameObject mainRoot = new GameObject("UI_MainMenuPanel");
            scene.Add(mainRoot);

            _mainMenuPanel = mainRoot.AddComponent<UIMenuPanel>();
            _mainMenuPanel.PanelPosition = panelPosition;
            _mainMenuPanel.ItemSize = itemSize;
            _mainMenuPanel.VerticalSpacing = spacing;
            _mainMenuPanel.IsVisible = true;

            _playButton = _mainMenuPanel.AddButton(
                "Play",
                _buttonTexture!,
                _font!,
                OnPlayClicked);

            _audioButton = _mainMenuPanel.AddButton(
                "Audio",
                _buttonTexture!,
                _font!,
                OnAudioClicked);

            _controlsButton = _mainMenuPanel.AddButton(
                "Controls",
                _buttonTexture!,
                _font!,
                OnControlsClicked);

            _exitButton = _mainMenuPanel.AddButton(
                "Exit",
                _buttonTexture!,
                _font!,
                OnExitClicked);

            // ---------- audio menu panel ----------
            GameObject audioRoot = new GameObject("UI_AudioMenuPanel");
            scene.Add(audioRoot);

            _audioMenuPanel = audioRoot.AddComponent<UIMenuPanel>();
            _audioMenuPanel.PanelPosition = panelPosition;
            _audioMenuPanel.ItemSize = itemSize;
            _audioMenuPanel.VerticalSpacing = spacing;
            _audioMenuPanel.IsVisible = false;

            _musicSlider = _audioMenuPanel.AddSlider(
                "Music",
                _sliderTrackTexture!,
                _sliderHandleTexture!,
                _font!,
                0f,
                1f,
                0.8f,
                OnMusicSliderChanged);

            _sfxSlider = _audioMenuPanel.AddSlider(
                "SFX",
                _sliderTrackTexture!,
                _sliderHandleTexture!,
                _font!,
                0f,
                1f,
                0.8f,
                OnSfxSliderChanged);

            _audioBackButton = _audioMenuPanel.AddButton(
                "Back",
                _buttonTexture!,
                _font!,
                OnBackToMainFromAudio);

            // ---------- controls menu panel ----------
            GameObject controlsRoot = new GameObject("UI_ControlsMenuPanel");
            scene.Add(controlsRoot);

            _controlsMenuPanel = controlsRoot.AddComponent<UIMenuPanel>();
            _controlsMenuPanel.PanelPosition = panelPosition;
            _controlsMenuPanel.ItemSize = itemSize;
            _controlsMenuPanel.VerticalSpacing = spacing;
            _controlsMenuPanel.IsVisible = false;

            // just a static image showing the keybinds, made it a bit bigger than a normal button
            GameObject controlsImageGO = new GameObject("ControlsLayout");
            scene.Add(controlsImageGO);
            controlsImageGO.Transform.SetParent(_controlsMenuPanel.Transform);

            _controlsLayoutTexture = controlsImageGO.AddComponent<UITexture>();
            _controlsLayoutTexture.Texture = _controlsLayout!;
            _controlsLayoutTexture.Size = new Vector2(itemSize.X * 1.5f, itemSize.Y * 2.0f);
            _controlsLayoutTexture.Position = panelPosition + new Vector2(0f, 0f);
            _controlsLayoutTexture.Tint = Color.White;
            _controlsLayoutTexture.LayerDepth = UILayer.Menu;

            _controlsBackButton = _controlsMenuPanel.AddButton(
                "Back",
                _buttonTexture!,
                _font!,
                OnBackToMainFromControls);

            _controlsMenuPanel.RefreshChildren();
        }

        private static void SetActivePanel(
            UIMenuPanel toShow,
            UIMenuPanel toHideA,
            UIMenuPanel toHideB)
        {
            toShow.IsVisible = true;
            toHideA.IsVisible = false;
            toHideB.IsVisible = false;
        }

        private void OnPlayClicked()
        {
            PlayRequested?.Invoke();
        }

        private void OnAudioClicked()
        {
            ShowAudioMenu();
        }

        private void OnControlsClicked()
        {
            ShowControlsMenu();
        }

        private void OnExitClicked()
        {
            ExitRequested?.Invoke();
        }

        private void OnBackToMainFromAudio()
        {
            ShowMainMenu();
        }

        private void OnBackToMainFromControls()
        {
            ShowMainMenu();
        }

        private void OnMusicSliderChanged(float value)
        {
            MusicVolumeChanged?.Invoke(value);
        }

        private void OnSfxSliderChanged(float value)
        {
            SfxVolumeChanged?.Invoke(value);
        }
        #endregion

        #region Lifecycle Methods
        public override void Update(GameTime gameTime)
        {
            // note: this doesn't drive the menu scene's Update/Draw itself, that's SceneManager's
            // job. this Update is basically just here to catch the "configured but not built yet" case
            if (!_built && _configured)
                TryBuildMenus();

            base.Update(gameTime);
        }
        #endregion

        #region Housekeeping Methods
        public override string ToString()
        {
            return "UIManager(MenuScene=" + (_menuScene?.Name ?? "null") + ", Built=" + (_built ? "true" : "false") + ")";
        }
        #endregion
    }
}
