using Essentials.Extensions;
using Reactor.Extensions;
#if S20201209
using Reactor.Unstrip;
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
    /// <summary>
    /// THIS CLASS IS STILL UNDER REFACTORING AND MAY BREAK IN THE FUTURE, BEWARE.
    /// </summary>
    //[Obsolete("This class is under refactoring, future versions will likely be incompatible and require code tweaks")]
    public partial class CooldownButton : IDisposable
    {
        /// <summary>
        /// The game's state for UI elements like buttons. Stored to change the visibility of custom buttons too.
        /// </summary>
        public static bool HudVisible { get; private set; } = true;

        private static List<CooldownButton> Buttons = new List<CooldownButton>();

        private KillButtonManager KillButtonManager;

        private Vector2 _positionOffset;
        public Vector2 PositionOffset { get { return _positionOffset; } set { _positionOffset = value; PositionOffsetV3 = _positionOffset.ToVector3(); } }
        private Vector3 PositionOffsetV3;

        private float _initialCooldownDuration = 0F;
        /// <summary>
        /// The button's initial (match start) cooldown duration. Set to 0 for no cooldown.
        /// </summary>
        public float InitialCooldownDuration { get { return _initialCooldownDuration; } set { _initialCooldownDuration = Mathf.Max(0F, value); } }
        private float _cooldownDuration = 0F;
        /// <summary>
        /// The button's cooldown duration. Set to 0 for no cooldown.
        /// </summary>
        public float CooldownDuration { get { return _cooldownDuration; } set { _cooldownDuration = Mathf.Max(0F, value); } }
        private float _cooldownTime = 0F;
        /// <summary>
        /// The button's current cooldown or effect time remaining.
        /// </summary>
        public float CooldownTime { get { return _cooldownTime; } private set { _cooldownTime = Mathf.Max(0F, value); } }
        /// <summary>
        /// Whether or not <see cref="CooldownTime"/> is currently larger than 0.
        /// </summary>
        public bool IsCoolingDown { get { return CooldownTime > 0F; } }

        private float _effectDuration = 0F;
        /// <summary>
        /// The duration of the "effect" applied once the button is pressed.
        /// </summary>
        public float EffectDuration { get { return _effectDuration; } set { _effectDuration = Mathf.Max(0F, value); } }
        /// <summary>
        /// Whether or not <see cref="EffectDuration"/> is larger than 0.
        /// </summary>
        public bool HasEffect { get { return EffectDuration > 0F; } }
        /// <summary>
        /// Whether or not the button's effect duration is currently ongoing.
        /// </summary>
        public bool IsEffectActive { get; private set; } = false;

        /// <summary>
        /// Whether or not the button is shown.
        /// </summary>
        /// <remarks>
        /// The button cannot be clicked.
        /// <para>Cooldown does not decrease when false, but effect duration does.</para>
        /// </remarks>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Whether or not the button can be clicked.
        /// </summary>
        /// <remarks>
        /// Affects button appearance, appears desaturated and translucent when not clickable.
        /// <para>Does not override cooldown.</para>
        /// </remarks>
        public bool Clickable { get; set; } = true;

        /// <summary>
        /// Whether or not the button can currently be used.
        /// </summary>
        public bool IsUsable { get { return HudVisible && Visible && Clickable && !IsCoolingDown && GameData.Instance; } }

        /// <summary>
        /// Raised when the button is clicked when IsCoolingDown is false and Visible and Clickable are both true.
        /// <para>Can be cancelled.</para>
        /// </summary>
        public event EventHandler<CancelEventArgs> OnClick;
        /// <summary>
        /// Raised every hud frame, can be used to control the button's state.
        /// </summary>
        public event EventHandler<EventArgs> OnUpdate;
        /// <summary>
        /// Raised when the effect duration starts, when <see cref="HasEffect"/> is true and after the button has been clicked and the click hasn't been cancelled.
        /// </summary>
        public event EventHandler<EventArgs> EffectStart;
        /// <summary>
        /// Raised after the effect duration ends, or when the effect ends early (due to a meeting).
        /// </summary>
        public event EventHandler<EventArgs> EffectEnd;
        /// <summary>
        /// Raised when the cooldown starts, either after the button has been clicked and the click hasn't been cancelled, or after the effect duration if <see cref="HasEffect"/> is true.
        /// </summary>
        public event EventHandler<EventArgs> OnCooldownStart;
        /// <summary>
        /// Raised after the cooldown ends.
        /// </summary>
        public event EventHandler<EventArgs> OnCooldownEnd;

        private Vector3? BasePosition;

        private Sprite ButtonSprite;

        private bool IsDisposed;

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public CooldownButton(Sprite sprite, Vector2 positionOffset, float cooldown = 0F, float effectDuration = 0F, float initialCooldown = 0F)
        {
            PositionOffset = positionOffset;
            CooldownDuration = cooldown;
            EffectDuration = effectDuration;
            InitialCooldownDuration = initialCooldown;

            CooldownTime = InitialCooldownDuration;

            Buttons.Add(this);

            if (sprite) UpdateSprite(sprite);

            CreateButton();
        }

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public CooldownButton(byte[] imageData, Vector2 positionOffset, float cooldown = 0F, float effectDuration = 0F, float initialCooldown = 0F) :
            this(CreateSprite(imageData ?? throw new ArgumentNullException(nameof(imageData), $"An image asset is required.")),
                positionOffset, cooldown, effectDuration, initialCooldown)
        {
        }

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public CooldownButton(string imageEmbededResourcePath, Vector2 positionOffset, float cooldown = 0F, float effectDuration = 0F, float initialCooldown = 0F) :
            this(GetBytesFromEmbeddedResource(Assembly.GetCallingAssembly(), imageEmbededResourcePath), positionOffset, cooldown, effectDuration, initialCooldown)
        {
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
        private static byte[] GetBytesFromEmbeddedResource(Assembly asm, string embeddedResourcePath)
        {
            string embeddedResourceFullPath = asm.GetManifestResourceNames().FirstOrDefault(resourceName => resourceName.EndsWith(embeddedResourcePath, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(embeddedResourceFullPath)) throw new ArgumentNullException(nameof(embeddedResourcePath), $"The embedded resource \"{embeddedResourcePath}\" was not found in assembly \"{asm.GetName().Name}\".");

            return asm.GetManifestResourceStream(embeddedResourceFullPath).ReadFully();
        }

        private static Sprite CreateSprite(byte[] imageData)
        {
            Texture2D tex = GUIExtensions.CreateEmptyTexture();//.DontDestroy();
            ImageConversion.LoadImage(tex, imageData, false);

            Sprite sprite = tex.CreateSprite();//.DontDestroy();

            return sprite;
        }

        /// <summary>
        /// Updates the button's sprite.
        /// </summary>
        /// <remarks>
        /// The sprite is instantiated.
        /// </remarks>
        /// <param name="sprite">New sprite</param>
        public void UpdateSprite(Sprite sprite)
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
        private void CreateButton()
        {
            if (KillButtonManager || !HudManager.Instance?.KillButton) return;

            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);

            KillButtonManager.gameObject.SetActive(HudVisible && Visible);
            KillButtonManager.renderer.enabled = HudVisible && Visible;

            if (ButtonSprite) KillButtonManager.renderer.sprite = ButtonSprite;

            PassiveButton button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(new Action(PerformClick));
        }

        /// <summary>
        /// Performs button click.
        /// </summary>
        public void PerformClick()
        {
            if (!IsUsable) return;

            CancelEventArgs args = new CancelEventArgs();
            OnClick?.SafeInvoke(this, args, nameof(OnClick));

            if (args.Cancel) return; // Click was cancelled.

            if (HasEffect)
            {
                StartEffect();

                return;
            }

            ApplyCooldown();
        }

        /// <summary>
        /// Starts the effect duration and raises the <see cref="EffectStart"/> event.
        /// </summary>
        public void StartEffect()
        {
            bool wasEffectActive = IsEffectActive;

            IsEffectActive = true;

            CooldownTime = EffectDuration;

            KillButtonManager.TimerText.Color = new Color(0F, 0.8F, 0F);

            if (!wasEffectActive) EffectStart?.SafeInvoke(this, EventArgs.Empty, nameof(EffectStart));
        }

        /// <summary>
        /// Ends the effect duration and raises the <see cref="EffectEnd"/> event.
        /// </summary>
        /// <param name="startCooldown">Whether or not to start the cooldown</param>
        public void EndEffect(bool startCooldown = true)
        {
            if (IsEffectActive)
            {
                IsEffectActive = false;

                KillButtonManager.TimerText.Color = Palette.EnabledColor;

                EffectEnd?.SafeInvoke(this, EventArgs.Empty, nameof(EffectEnd));
            }

            if (startCooldown) ApplyCooldown();
        }


        private void Update()
        {
            if (!GameData.Instance || PlayerControl.LocalPlayer?.Data == null || KillButtonManager == null)
            {
                //EndEffect(false);
                IsEffectActive = false;

                //ApplyCooldown(0F);
                CooldownTime = 0F;

                return;
            }

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

            if (ButtonSprite) KillButtonManager.renderer.sprite = ButtonSprite;

#warning change canmove
            if (IsCoolingDown && (IsEffectActive || Visible && PlayerControl.LocalPlayer.CanMove))
            {
                CooldownTime -= Time.deltaTime;

                if (!IsCoolingDown)
                {
                    if (IsEffectActive)
                    {
                        EndEffect();
                    }
                    else
                    {
                        OnCooldownEnd?.SafeInvoke(this, EventArgs.Empty, nameof(OnCooldownEnd));
                    }
                }
            }

            OnUpdate?.SafeInvoke(this, EventArgs.Empty, nameof(OnUpdate)); // Implementing code can control visibility and appearance.

            if (IsDisposed) return; // Dispose may be called during OnUpdate, resulting exceptions.

            KillButtonManager.gameObject.SetActive(HudVisible && Visible);
            KillButtonManager.renderer.enabled = HudVisible && Visible;
            KillButtonManager.TimerText.enabled = HudVisible && Visible && IsCoolingDown;

            KillButtonManager.renderer.color = IsCoolingDown || !Clickable ? Palette.DisabledColor : Palette.EnabledColor;

            //KillButtonManager.renderer.material.SetFloat("_Desat", 0F);
            KillButtonManager.renderer.material.SetFloat("_Desat", Clickable ? 0F : 1F);

            //KillButtonManager.SetCoolDown(Timer, MaxTimer);
            UpdateCooldown();
        }

        /// <summary>
        /// Sets the button on cooldown. Defaults to <see cref="CooldownDuration"/>.
        /// </summary>
        /// <remarks>Raises the <see cref="OnCooldownStart"/> or <see cref="OnCooldownEnd"/> events depending on whether the button wasn't or was on cooldown, respectively.</remarks>
        /// <remarks>Ends effect duration if the effect is active when called.</remarks>
        /// <param name="customCooldown">Optional custom cooldown duration (does not affect <see cref="CooldownDuration"/>, may be longer or shorter than <see cref="CooldownDuration"/>)</param>
        public void ApplyCooldown(float? customCooldown = null)
        {
            if (IsEffectActive) EndEffect(false);

            bool wasCoolingDown = IsCoolingDown;

            CooldownTime = customCooldown ?? CooldownDuration;

            if (!wasCoolingDown && IsCoolingDown)
            {
                OnCooldownStart?.SafeInvoke(this, EventArgs.Empty, nameof(OnCooldownStart));
            }
            else if (wasCoolingDown && !IsCoolingDown)
            {
                OnCooldownEnd?.SafeInvoke(this, EventArgs.Empty, nameof(OnCooldownEnd));
            }
        }

        /// <summary>
        /// Updates the button cooldown text and progress visual.
        /// </summary>
        private void UpdateCooldown()
        {
            float cooldownRate = CooldownDuration == 0F ? IsCoolingDown ? 1F : 0F : Mathf.Clamp(CooldownTime / (IsEffectActive ? EffectDuration : CooldownDuration), 0f, 1f);
            KillButtonManager.renderer?.material?.SetFloat("_Percent", cooldownRate);

            KillButtonManager.isCoolingDown = IsCoolingDown;

            KillButtonManager.TimerText.Text = Mathf.CeilToInt(CooldownTime).ToString();
            KillButtonManager.TimerText.gameObject.SetActive(HudVisible && Visible && KillButtonManager.isCoolingDown);
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
                    /*finally
                    {
                        Buttons.Remove(this);
                    }*/ // Could cause collection modified exception if called during OnUpdate event.
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