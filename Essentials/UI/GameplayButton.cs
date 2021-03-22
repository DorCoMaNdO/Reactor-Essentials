using Essentials.Extensions;
using Reactor.Extensions;
#if S20201209
using Reactor.Unstrip;
#elif S20210305
using InnerNet;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Essentials.UI
{
    public partial class GameplayButton : IDisposable
    {
        protected static List<GameplayButton> Buttons = new List<GameplayButton>();

        /// <summary>
        /// The game's state for UI elements like buttons. Stored to change the visibility of custom buttons too.
        /// </summary>
        public static bool HudVisible { get; private set; } = true;

        protected static bool GameStarted
        {
            get
            {
                return GameData.Instance && ShipStatus.Instance && AmongUsClient.Instance && /*!IntroCutscene.Instance &&*/
                    (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameMode == GameModes.FreePlay);
            }
        }

        static GameplayButton()
        {
            Events.HudStateChanged += (sender, e) => HudVisible = e.Active;
        }

        protected KillButtonManager KillButtonManager;

        public virtual bool Exists { get { return GameStarted && PlayerControl.LocalPlayer?.Data != null && KillButtonManager; } }

        protected Vector2 _positionOffset;
        public Vector2 PositionOffset { get { return _positionOffset; } set { _positionOffset = value; PositionOffsetV3 = _positionOffset.ToVector3(); } }
        protected Vector3 PositionOffsetV3;

        /// <summary>
        /// Whether or not the button is shown.
        /// </summary>
        /// <remarks>
        /// The button cannot be clicked.
        /// </remarks>
        public virtual bool Visible { get; set; } = true;
        /// <summary>
        /// Whether or not the button can be clicked.
        /// </summary>
        /// <remarks>
        /// Affects button appearance, appears desaturated and translucent when not clickable.
        /// </remarks>
        public virtual bool Clickable { get; set; } = true;

        /// <summary>
        /// Whether or not the button can currently be used.
        /// </summary>
        public virtual bool IsUsable { get { return HudVisible && Visible && Clickable && GameStarted; } }

        /// <summary>
        /// Raised every hud frame, can be used to control the button's state.
        /// </summary>
        public event EventHandler<EventArgs> OnUpdate;
        /// <summary>
        /// Raised when the button is clicked (when Visible and Clickable are both true).
        /// <para>Can be cancelled.</para>
        /// </summary>
        public event EventHandler<CancelEventArgs> OnClick;
        /// <summary>
        /// Raised after the button is clicked, if not cancelled by <see cref="OnClick"/>.
        /// </summary>
        public event EventHandler<EventArgs> Clicked;

        protected Vector3? BasePosition;

        protected Sprite ButtonSprite;

        protected bool IsDisposed;

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public GameplayButton(Sprite sprite, Vector2 positionOffset)
        {
            PositionOffset = positionOffset;

            Buttons.Add(this);

            if (sprite) UpdateSprite(sprite);

            CreateButton();

            Events.HudUpdate += HudUpdate;
        }

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public GameplayButton(byte[] imageData, Vector2 positionOffset) :
            this(CreateSprite(imageData ?? throw new ArgumentNullException(nameof(imageData), $"An image asset is required.")), positionOffset)
        {
        }

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public GameplayButton(string imageEmbededResourcePath, Vector2 positionOffset) :
            this(GetBytesFromEmbeddedResource(Assembly.GetCallingAssembly(), imageEmbededResourcePath), positionOffset)
        {
        }

        protected virtual void HudUpdate(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                Events.HudUpdate -= HudUpdate;

                Buttons.Remove(this);

                return;
            }

            try
            {
                CreateButton();

                Update();
            }
            catch (Exception ex)
            {
                EssentialsPlugin.Logger.LogWarning($"An exception has occurred when creating or updating a button: {ex}");
            }
        }

        /// <summary>
        /// Gets embedded resource as byte array.
        /// </summary>
        /// <param name="asm">The assembly that contains the resource.</param>
        /// <param name="embeddedResourcePath">Accepts partial resource names as long as the resource name ends with the specified value.</param>
        /// <example>
        /// "Resources.Button.png" can be simplified to "Button.png"
        /// <code>
        /// GetBytesFromEmbeddedResource(Assembly.GetCallingAssembly(), "Button.png");
        /// </code>
        /// </example>
        protected static byte[] GetBytesFromEmbeddedResource(Assembly asm, string embeddedResourcePath)
        {
            string embeddedResourceFullPath = asm.GetManifestResourceNames().FirstOrDefault(resourceName => resourceName.EndsWith(embeddedResourcePath, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(embeddedResourceFullPath)) throw new ArgumentNullException(nameof(embeddedResourcePath), $"The embedded resource \"{embeddedResourcePath}\" was not found in assembly \"{asm.GetName().Name}\".");

            return asm.GetManifestResourceStream(embeddedResourceFullPath).ReadFully();
        }

        protected static Sprite CreateSprite(byte[] imageData, bool dontDestroy = false)
        {
            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            ImageConversion.LoadImage(tex, imageData, false);

            Sprite sprite = tex.CreateSprite();

            if (dontDestroy)
            {
                tex.DontDestroy();
                sprite.DontDestroy();
            }

            return sprite;
        }

        /// <summary>
        /// Updates the button's sprite.
        /// </summary>
        /// <remarks>
        /// The sprite is instantiated.
        /// </remarks>
        /// <param name="sprite">New sprite</param>
        public virtual void UpdateSprite(Sprite sprite)
        {
            if (sprite == null) throw new ArgumentNullException(nameof(sprite), $"A sprite image is required.");

            try
            {
                ButtonSprite?.texture?.Destroy();
                ButtonSprite?.Destroy();
            }
            catch
            {
            }

            ButtonSprite = Object.Instantiate(sprite).DontDestroy();
            ButtonSprite.texture.DontDestroy();
        }

        /// <summary>
        /// Creates an instance of <see cref="KillButtonManager"/> when one does not exist.
        /// </summary>
        protected virtual void CreateButton()
        {
            if (KillButtonManager || !HudManager.Instance?.KillButton) return;

            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);

            KillButtonManager.gameObject.SetActive(HudVisible && Visible);

            KillButtonManager.renderer.enabled = HudVisible && Visible;
            if (ButtonSprite) KillButtonManager.renderer.sprite = ButtonSprite;

            KillButtonManager.TimerText.enabled = false;
            KillButtonManager.TimerText.gameObject.SetActive(false);

            PassiveButton button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(new Action(() => PerformClick()));
        }

        /// <summary>
        /// Raises <see cref="OnClick"/> event.
        /// </summary>
        /// <returns>False if the click was cancelled.</returns>
        protected bool RaiseOnClick()
        {
            CancelEventArgs args = new CancelEventArgs();
            OnClick?.SafeInvoke(this, args, nameof(OnClick));

            return !args.Cancel;
        }

        /// <summary>
        /// Raises <see cref="Clicked"/> event.
        /// </summary>
        protected void RaiseClicked()
        {
            Clicked?.SafeInvoke(this, EventArgs.Empty, nameof(Clicked));
        }

        /// <summary>
        /// Performs button click.
        /// </summary>
        /// <returns>Click success.</returns>
        public virtual bool PerformClick()
        {
            if (!IsUsable || !RaiseOnClick()) return false; // Button is not usable or click was cancelled.

            RaiseClicked();

            return true;
        }

        /// <summary>
        /// Raises <see cref="OnUpdate"/> event.
        /// </summary>
        protected void RaiseOnUpdate()
        {
            OnUpdate?.SafeInvoke(this, EventArgs.Empty, nameof(OnUpdate)); // Implementing code can control visibility and appearance.
        }

        protected virtual void Update()
        {
            if (!Exists)
            {
                if (KillButtonManager) SetVisible(false);

                //RaiseOnUpdate();

                return;
            }

            UpdatePosition();

            if (ButtonSprite) KillButtonManager.renderer.sprite = ButtonSprite;

            RaiseOnUpdate();

            if (IsDisposed) return; // Dispose may be called during OnUpdate, resulting exceptions.

            SetVisible(HudVisible && Visible);
        }

        protected virtual void UpdatePosition()
        {
            if (BasePosition == null && KillButtonManager.transform.localPosition.x > 0F)
            {
                Vector3 v = KillButtonManager.transform.localPosition;
                v.x = -v.x;// - 1.3F;

                BasePosition = v;
            }

            if (BasePosition.HasValue)
            {
                Vector3 vector = BasePosition.Value + PositionOffsetV3;

                KillButtonManager.transform.localPosition = vector;
            }
        }

        protected virtual void SetVisible(bool visible)
        {
            KillButtonManager.gameObject.SetActive(visible);

            KillButtonManager.renderer.enabled = visible;
            KillButtonManager.renderer.color = !Clickable ? Palette.DisabledColor : Palette.EnabledColor;
            KillButtonManager.renderer.material.SetFloat("_Desat", Clickable ? 0F : 1F);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (KillButtonManager)
                        {
                            KillButtonManager.renderer.enabled = false;
                            KillButtonManager.TimerText.enabled = false;

                            KillButtonManager.Destroy();
                        }

                        ButtonSprite?.texture?.Destroy();
                        ButtonSprite?.Destroy();
                    }
                    catch
                    {
                    }
                }

                IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CooldownButton()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Removes the button and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}