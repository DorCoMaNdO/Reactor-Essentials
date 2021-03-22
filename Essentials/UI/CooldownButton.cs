using Essentials.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Essentials.UI
{
    public partial class CooldownButton : GameplayButton
    {
        protected static List<CooldownButton> CooldownButtons = new List<CooldownButton>();

        protected float _initialCooldownDuration = 0F;
        /// <summary>
        /// The button's initial (match start) cooldown duration. Set to 0 for no cooldown.
        /// </summary>
        /// <remarks>Can be used to match kill button behaviour when set to 10.</remarks>
        public virtual float InitialCooldownDuration { get { return _initialCooldownDuration; } set { _initialCooldownDuration = Mathf.Max(0F, value); } }
        protected float _cooldownDuration = 0F;
        /// <summary>
        /// The button's cooldown duration. Set to 0 for no cooldown.
        /// </summary>
        public virtual float CooldownDuration { get { return _cooldownDuration; } set { _cooldownDuration = Mathf.Max(0F, value); } }
        protected float _cooldownTime = 0F;
        /// <summary>
        /// The button's current cooldown or effect time remaining.
        /// </summary>
        public virtual float CooldownTime { get { return _cooldownTime; } protected set { _cooldownTime = Mathf.Max(0F, value); ; } }
        /// <summary>
        /// Whether or not <see cref="CooldownTime"/> is currently larger than 0.
        /// </summary>
        public virtual bool IsCoolingDown { get { return CooldownTime > 0F; } }

        protected float _effectDuration = 0F;
        /// <summary>
        /// The duration of the "effect" applied once the button is pressed.
        /// </summary>
        public virtual float EffectDuration { get { return _effectDuration; } set { _effectDuration = Mathf.Max(0F, value); } }
        /// <summary>
        /// Whether or not <see cref="EffectDuration"/> is larger than 0.
        /// </summary>
        public virtual bool HasEffect { get { return EffectDuration > 0F; } }
        /// <summary>
        /// Whether or not the button's effect duration is currently ongoing.
        /// </summary>
        public virtual bool IsEffectActive { get; protected set; } = false; // TODO: Refactor by adding EffectTime property instead of reusing CooldownTime.

        /// <summary>
        /// Whether the remaining effect duration will only decrease when the cooldown can decrease (as opposed to always decrease when set to false).
        /// <para>See <see cref="CanUpdateCooldown"/> for when the cooldown can decrease.</para>
        /// </summary>
        public virtual bool EffectCanPause { get; set; } = false;
        /// <summary>
        /// Whether the effect will end early when a meeting is called.
        /// </summary>
        public virtual bool MeetingsEndEffect { get; set; } = true;
        /// <summary>
        /// Whether cooldown will be applied after meetings.
        /// </summary>
        public virtual bool CooldownAfterMeetings { get; set; } = true;

        /// <summary>
        /// Whether the remaining cooldown duration should decrease.
        /// </summary>
        public virtual bool CanUpdateCooldown { get { return Visible && !IntroCutscene.Instance && !MeetingHud.Instance && !ExileController.Instance; } }

        /// <summary>
        /// Whether or not the button is shown.
        /// </summary>
        /// <remarks>
        /// The button cannot be clicked.
        /// <para>Cooldown does not decrease when false, but effect duration does.</para>
        /// </remarks>
        public override bool Visible { get; set; } = true;
        /// <summary>
        /// Whether or not the button can be clicked.
        /// </summary>
        /// <remarks>
        /// Affects button appearance, appears desaturated and translucent when not clickable.
        /// <para>Does not override cooldown.</para>
        /// </remarks>
        public override bool Clickable { get; set; } = true;

        /// <summary>
        /// Whether or not the button can currently be used.
        /// </summary>
        public override bool IsUsable { get { return !IsCoolingDown && base.IsUsable; } }

        /// <summary>
        /// Raised when the effect duration starts, when <see cref="HasEffect"/> is true and after the button has been clicked and the click hasn't been cancelled.
        /// </summary>
        public virtual event EventHandler<EventArgs> EffectStarted;
        /// <summary>
        /// Raised after the effect duration ends, or when the effect ends early (due to a meeting).
        /// </summary>
        public virtual event EventHandler<EventArgs> EffectEnded;
        /// <summary>
        /// Raised when the cooldown starts, either after the button has been clicked and the click hasn't been cancelled, or after the effect duration if <see cref="HasEffect"/> is true.
        /// </summary>
        public virtual event EventHandler<EventArgs> CooldownStarted;
        /// <summary>
        /// Raised after the cooldown ends.
        /// </summary>
        public virtual event EventHandler<EventArgs> CooldownEnded;

        public CooldownButton(Sprite sprite, Vector2 positionOffset, float cooldown, float effectDuration = 0F, float initialCooldown = 0F) :
            base(sprite, positionOffset)
        {
            Setup(cooldown, effectDuration, initialCooldown);
        }

        public CooldownButton(byte[] imageData, Vector2 positionOffset, float cooldown, float effectDuration = 0F, float initialCooldown = 0F) :
            base(imageData, positionOffset)
        {
            Setup(cooldown, effectDuration, initialCooldown);
        }

        public CooldownButton(string imageEmbededResourcePath, Vector2 positionOffset, float cooldown, float effectDuration = 0F, float initialCooldown = 0F) :
            this(GetBytesFromEmbeddedResource(Assembly.GetCallingAssembly(), imageEmbededResourcePath), positionOffset, cooldown, effectDuration, initialCooldown)
        {
        }

        private void Setup(float cooldown, float effectDuration = 0F, float initialCooldown = 0F)
        {
            CooldownDuration = cooldown;
            EffectDuration = effectDuration;
            InitialCooldownDuration = initialCooldown;

            CooldownTime = InitialCooldownDuration;

            CooldownButtons.Add(this);
        }

        public override bool PerformClick()
        {
            if (!base.PerformClick()) return false;

            if (HasEffect)
            {
                StartEffect();
            }
            else
            {
                ApplyCooldown();
            }

            return true;
        }

        protected override void Update()
        {
            if (!Exists)
            {
                IsEffectActive = false;

                //CooldownTime = 0F;

                if (KillButtonManager) SetVisible(false);

                //RaiseOnUpdate();

                return;
            }

            UpdatePosition();

            if (ButtonSprite) KillButtonManager.renderer.sprite = ButtonSprite;

            UpdateCooldown();

            RaiseOnUpdate();

            if (IsDisposed) return; // Dispose may be called during OnUpdate, resulting exceptions.

            SetVisible(HudVisible && Visible);
        }

        protected void RaiseEffectStarted()
        {
            EffectStarted?.SafeInvoke(this, EventArgs.Empty, nameof(EffectStarted));
        }

        protected void RaiseEffectEnded()
        {
            EffectEnded?.SafeInvoke(this, EventArgs.Empty, nameof(EffectEnded));
        }

        /// <summary>
        /// Starts the effect duration and raises the <see cref="EffectStarted"/> event.
        /// </summary>
        public virtual void StartEffect()
        {
            bool wasEffectActive = IsEffectActive;

            IsEffectActive = true;

            CooldownTime = EffectDuration;

            KillButtonManager.TimerText.Color = new Color(0F, 0.8F, 0F);

            if (!wasEffectActive) RaiseEffectStarted();
        }

        /// <summary>
        /// Ends the effect duration and raises the <see cref="EffectEnd"/> event.
        /// </summary>
        /// <param name="startCooldown">Whether or not to start the cooldown</param>
        public virtual void EndEffect(bool startCooldown = true)
        {
            if (IsEffectActive)
            {
                IsEffectActive = false;

                KillButtonManager.TimerText.Color = Palette.EnabledColor;

                RaiseEffectEnded();
            }

            if (startCooldown) ApplyCooldown();
        }

        protected virtual void UpdateCooldown()
        {
            if (!IsCoolingDown || (!IsEffectActive || EffectCanPause) && !CanUpdateCooldown) return;

            //if (IsCoolingDown && (IsEffectActive || Visible && PlayerControl.LocalPlayer.CanMove))
            //{
            CooldownTime -= Time.deltaTime;

            if (!IsCoolingDown)
            {
                if (IsEffectActive)
                {
                    EndEffect();
                }
                else
                {
                    RaiseCooldownEnded();
                }
            }
            //}
        }

        protected override void SetVisible(bool visible)
        {
            base.SetVisible(visible);

            UpdateCooldownVisuals(visible);
        }

        protected void RaiseCooldownStarted()
        {
            CooldownStarted?.SafeInvoke(this, EventArgs.Empty, nameof(CooldownStarted));
        }

        protected void RaiseCooldownEnded()
        {
            CooldownEnded?.SafeInvoke(this, EventArgs.Empty, nameof(CooldownEnded));
        }

        /// <summary>
        /// Sets the button on cooldown. Defaults to <see cref="CooldownDuration"/>.
        /// </summary>
        /// <remarks>Raises the <see cref="CooldownStarted"/> or <see cref="CooldownEnded"/> events depending on whether the button wasn't or was on cooldown, respectively.</remarks>
        /// <remarks>Ends effect duration if the effect is active when called.</remarks>
        /// <param name="customCooldown">Optional custom cooldown duration (does not affect <see cref="CooldownDuration"/>, may be longer or shorter than <see cref="CooldownDuration"/>)</param>
        public virtual void ApplyCooldown(float? customCooldown = null)
        {
            if (IsEffectActive) EndEffect(false);

            bool wasCoolingDown = IsCoolingDown;

            CooldownTime = customCooldown ?? CooldownDuration;

            if (!wasCoolingDown && IsCoolingDown)
            {
                RaiseCooldownStarted();
            }
            else if (wasCoolingDown && !IsCoolingDown)
            {
                RaiseCooldownEnded();
            }
        }

        /// <summary>
        /// Updates the button cooldown text and progress visual.
        /// </summary>
        protected virtual void UpdateCooldownVisuals(bool visible)
        {
            if (visible)
            {
                float cooldownRate = CooldownDuration == 0F ? IsCoolingDown ? 1F : 0F :
                    Mathf.Clamp(CooldownTime / (IsEffectActive ? EffectDuration : CooldownDuration), 0f, 1f);
                KillButtonManager.renderer?.material?.SetFloat("_Percent", cooldownRate);

                //KillButtonManager.isCoolingDown = IsCoolingDown;

                KillButtonManager.TimerText.Text = Mathf.CeilToInt(CooldownTime).ToString();
            }

            KillButtonManager.TimerText.enabled = visible && IsCoolingDown;
            KillButtonManager.TimerText.gameObject.SetActive(visible && IsCoolingDown);
        }
    }
}