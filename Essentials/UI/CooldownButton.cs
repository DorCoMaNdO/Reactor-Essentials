/*
HOW TO USE: (reactor is recommended)
1. Copy the code in this file (not including this comment) to CooldownButton.cs
2. Get an image for your button (150 x 150 pixels is recommended) and add it to the VS solution. Make sure the 'Build Action' of the image in VS is 'Embedded resource'
3. Make a button patch. This one will make a button in the bottom left of the screen that prints 'PRESS' on press, everyone can press it and has a cooldown of 5 seconds.
```
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class ExampleButton
{
    private static CooldownButton btn;

    public static void Postfix(HudManager __instance)
    {
        btn = new CooldownButton(
            () =>
            {
                // Restores the cooldown
                btn.Timer = btn.MaxTimer;

                System.Console.WriteLine("PRESS");
            },
            5f,
            "<BUTTON NAMESPACE>.<IMAGE FILE NAME (with file extension)>",
            0.25f,
            new Vector2(0.125f, 0.125f),
            CooldownButton.Category.Everyone,
            __instance
        );
    }
}
```

If you've done everything correctly, a button should appear when going into a game/freeplay!
Credits to https://gist.github.com/gabriel-nsiqueira/827dea0a1cdc2210db6f9a045ec4ce0a for the actual code!! I only added some minor stuff.
*/

using HarmonyLib;
using Reactor.Extensions;
using Reactor.Unstrip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Essentials.UI
{
    public enum ButtonCategory
    {
        Everyone,
        OnlyCrewmate,
        OnlyImpostor
    }

    /// <summary>
    /// THIS CLASS IS STILL UNDER REFACTORING AND MAY BREAK IN THE FUTURE, BEWARE.
    /// </summary>
    public class CooldownButton : IDisposable
    {
        public static List<CooldownButton> Buttons = new List<CooldownButton>();
        public KillButtonManager KillButtonManager;
        //private Color StartColorButton = new Color(255, 255, 255);
        private Color StartColorText = new Color(255, 255, 255);
        public Vector2 PositionOffset = Vector2.zero;
        public float MaxTimer = 0F;
        public float Timer = 0F;
        public float EffectDuration = 0F;
        public bool IsEffectActive = false, enabled = true;
        public ButtonCategory Category;
        //private float PixelsPerUnit;
        private bool CanUse;
        private Action OnEffectEnd; // rework to events
        private bool IsDisposed;

        /// <summary>
        /// Call in HudManager.Start
        /// </summary>
        private CooldownButton(Assembly asm, Action onClick, float cooldown, string embeddedResourcePath/*, float pixelsPerUnit*/, Vector2 positionOffset, ButtonCategory category, HudManager hudManager, bool hasEffectDuration, float effectDuration, Action onEffectEnd)
        {
            OnEffectEnd = onEffectEnd;
            PositionOffset = positionOffset;
            EffectDuration = effectDuration;
            Category = category;
            //PixelsPerUnit = pixelsPerUnit;
            MaxTimer = cooldown;
            Timer = MaxTimer;

            Buttons.Add(this);

            string embeddedResourceFullPath = asm.GetManifestResourceNames().FirstOrDefault(resourceName => resourceName.EndsWith(embeddedResourcePath, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(embeddedResourceFullPath)) throw new ArgumentNullException(nameof(embeddedResourcePath), $"The embedded resource \"{embeddedResourcePath}\" was not found in assembly \"{asm.GetName().Name}\".");

            Stream resourceStream = asm.GetManifestResourceStream(embeddedResourceFullPath);

            KillButtonManager = Object.Instantiate(hudManager.KillButton, hudManager.transform);

            //StartColorButton = killButtonManager.renderer.color;
            StartColorText = KillButtonManager.TimerText.Color;

            KillButtonManager.gameObject.SetActive(true);
            KillButtonManager.renderer.enabled = true;

            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            byte[] buttonTexture = Reactor.Extensions.Extensions.ReadFully(resourceStream);
            ImageConversion.LoadImage(tex, buttonTexture, false);
            KillButtonManager.renderer.sprite = GUIExtensions.CreateSprite(tex);

            PassiveButton button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(new Action(() =>
            {
                if (Timer < 0f && CanUse)
                {
                    KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);

                    if (hasEffectDuration)
                    {
                        IsEffectActive = true;

                        Timer = EffectDuration;

                        KillButtonManager.TimerText.Color = new Color(0, 255, 0);
                    }

                    onClick();
                }
            }));
        }

        /// <summary>
        /// Call in HudManager.Start
        /// </summary>
        public CooldownButton(Action onClick, float cooldown, string imageEmbededResourcePath, Vector2 positionOffset, ButtonCategory category, HudManager hudManager, float effectDuration, Action onEffectEnd) : this(Assembly.GetCallingAssembly(), onClick, cooldown, imageEmbededResourcePath, positionOffset, category, hudManager, true, effectDuration, onEffectEnd)
        {
        }

        /// <summary>
        /// Call in HudManager.Start
        /// </summary>
        public CooldownButton(Action onClick, float cooldown, string imageEmbededResourcePath, Vector2 positionOffset, ButtonCategory category, HudManager hudManager) : this(Assembly.GetCallingAssembly(), onClick, cooldown, imageEmbededResourcePath, positionOffset, category, hudManager, false, 0F, null)
        {
        }

        public bool IsUsable()
        {
            if (PlayerControl.LocalPlayer?.Data == null) return false;

            switch (Category)
            {
                case ButtonCategory.Everyone:
                {
                    CanUse = true;

                    break;
                }
                case ButtonCategory.OnlyCrewmate:
                {
                    CanUse = !PlayerControl.LocalPlayer.Data.IsImpostor;

                    break;
                }
                case ButtonCategory.OnlyImpostor:
                {
                    CanUse = PlayerControl.LocalPlayer.Data.IsImpostor;

                    break;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(HudManager), "Update")]
        private static class HudUpdatePatch
        {
            public static void Postfix()
            {
                Buttons.RemoveAll(item => item.KillButtonManager == null);
                for (int i = 0; i < Buttons.Count; i++)
                {
                    try
                    {
                        if (Buttons[i].IsUsable()) Buttons[i].Update();
                    }
                    catch (NullReferenceException)
                    {
                        System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().CanUse(), if theres only one warning its fine");
                    }
                }
            }
        }

        private void Update()
        {
            if (!GameData.Instance || PlayerControl.LocalPlayer.Data == null) return;

            if (KillButtonManager.transform.localPosition.x > 0F)
            {
                Vector3 vector = KillButtonManager.transform.localPosition;
                vector.x = (vector.x + 1.3F) * -1;

                vector += new Vector3(PositionOffset.x, PositionOffset.y);

                KillButtonManager.transform.localPosition = vector;
            }

            if (Timer < 0F)
            {
                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);

                if (IsEffectActive)
                {
                    KillButtonManager.TimerText.Color = StartColorText;

                    Timer = MaxTimer;

                    IsEffectActive = false;

                    OnEffectEnd();
                }
            }
            else
            {
                if (CanUse) Timer -= Time.deltaTime;

                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
            }

            KillButtonManager.gameObject.SetActive(CanUse);
            KillButtonManager.renderer.enabled = CanUse;

            if (CanUse)
            {
                KillButtonManager.renderer.material.SetFloat("_Desat", 0f);

                KillButtonManager.SetCoolDown(Timer, MaxTimer);
            }
        }

        public void ApplyCooldown()
        {
            Timer = MaxTimer;
        }


        /*private void UpdateCooldown()
        {
            KillButtonManager.SetCoolDown(Timer, MaxTimer);
        }*/

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        KillButtonManager.renderer.enabled = false;
                        KillButtonManager.TimerText.enabled = false;

                        Object.Destroy(KillButtonManager);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Buttons.Remove(this);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}