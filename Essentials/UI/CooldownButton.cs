using Essentials.Extensions;
using Reactor.Extensions;
using Reactor.Unstrip;
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

        public Vector2 PositionOffset { get; private set; }

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
        /// Raised every hud frame, can be used to control the button's state. Not called when <see cref="HudVisible"/> is false.
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

        private byte[] ImageData;

        private bool IsDisposed;

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public CooldownButton(byte[] imageData, Vector2 positionOffset, float cooldown = 0F, float effectDuration = 0F)
        {
            PositionOffset = positionOffset;
            EffectDuration = effectDuration;
            CooldownDuration = cooldown;
            CooldownTime = CooldownDuration;

            Buttons.Add(this);

            ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData), $"An image asset is required.");

            CreateButton();
        }

        /// <summary>
        /// Call in Plugin.Load
        /// </summary>
        public CooldownButton(string imageEmbededResourcePath, Vector2 positionOffset, float cooldown = 0F, float effectDuration = 0F) : this(GetBytesFromEmbeddedResource(Assembly.GetCallingAssembly(), imageEmbededResourcePath), positionOffset, cooldown, effectDuration)
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

        /// <summary>
        /// Creates an instance of <see cref="KillButtonManager"/> when one does not exist.
        /// </summary>
        private void CreateButton()
        {
            if (KillButtonManager != null || !HudManager.Instance?.KillButton || ImageData == null) return;

            KillButtonManager = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);

            KillButtonManager.gameObject.SetActive(HudVisible && Visible);
            KillButtonManager.renderer.enabled = HudVisible && Visible;

            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            ImageConversion.LoadImage(tex, ImageData, false);

            KillButtonManager.renderer.sprite = GUIExtensions.CreateSprite(tex);

            PassiveButton button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(new Action(() =>
            {
                if (!IsUsable) return;

                CancelEventArgs args = new CancelEventArgs();
                OnClick?.SafeInvoke(this, args);

                if (args.Cancel) return; // Click was cancelled.

                if (HasEffect)
                {
                    StartEffect();

                    return;
                }

                ApplyCooldown();
            }));
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

            if (!wasEffectActive) EffectStart?.SafeInvoke(this, EventArgs.Empty);
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

                EffectEnd?.SafeInvoke(this, EventArgs.Empty);
            }

            if (startCooldown) ApplyCooldown();
        }


        private void Update()
        {
            if (!GameData.Instance || PlayerControl.LocalPlayer?.Data == null || KillButtonManager == null)
            {
                EndEffect(false);

                ApplyCooldown(0F);

                return;
            }

            if (KillButtonManager.transform.localPosition.x > 0F)
            {
                Vector3 vector = KillButtonManager.transform.localPosition;
                vector.x = (vector.x + 1.3F) * -1;

                vector += new Vector3(PositionOffset.x, PositionOffset.y);

                KillButtonManager.transform.localPosition = vector;
            }

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
                        OnCooldownEnd?.SafeInvoke(this, EventArgs.Empty);
                    }
                }
            }

            if (HudVisible) OnUpdate?.SafeInvoke(this, EventArgs.Empty); // Implementing code can control visibility and appearance.

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
                OnCooldownStart?.SafeInvoke(this, EventArgs.Empty);
            }
            else if (wasCoolingDown && !IsCoolingDown)
            {
                OnCooldownEnd?.SafeInvoke(this, EventArgs.Empty);
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
                        ImageData = null;

                        KillButtonManager.renderer.enabled = false;
                        KillButtonManager.TimerText.enabled = false;

                        Object.Destroy(KillButtonManager);
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