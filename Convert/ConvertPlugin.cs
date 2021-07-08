using BepInEx;
using BepInEx.IL2CPP;
using Essentials;
using Essentials.Options;
using Essentials.UI;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Convert
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(EssentialsPlugin.Id)]
    [ReactorPluginSide(PluginSide.Both)]
    internal partial class ConvertPlugin : BasePlugin
    {
        public const string Id = "com.comando.convert";

        public Harmony Harmony { get; } = new Harmony(Id);

        private static CustomNumberOption ImpostorConversions = CustomOption.AddNumber(nameof(ImpostorConversions), "Impostor conversions", 1, 0, 2, 1);
        private static CustomToggleOption ConversionCooldown = CustomOption.AddToggle(nameof(ConversionCooldown), "Conversion applies kill cooldown", true);
        private static CooldownButton ConvertButton;
        private static int Conversions = 0;

        public override void Load()
        {
            Harmony.PatchAll();

#if !S20210615 && !S20210630
            RegisterInIl2CppAttribute.Register();
            RegisterCustomRpcAttribute.Register(this);
#endif

            ConvertButton = new CooldownButton("Resources.Button.png", new HudPosition(GameplayButton.OffsetX, GameplayButton.OffsetY, HudAlignment.BottomRight), PlayerControl.GameOptions.KillCooldown, 0, 10)
            {
                Visible = false,
                Clickable = false
            };
            ConvertButton.OnClick += ConvertButton_OnClick;
            ConvertButton.OnUpdate += ConvertButton_OnUpdate;
        }

        private void ConvertButton_OnClick(object sender, CancelEventArgs e)
        {
            PlayerControl currentTarget = DestroyableSingleton<HudManager>.Instance?.KillButton?.CurrentTarget;
            if (!currentTarget)
            {
                e.Cancel = true;

                return;
            }

            Rpc.Instance.Send(currentTarget.PlayerId);

            if (ConversionCooldown.GetValue()) PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
        }

        private void ConvertButton_OnUpdate(object sender, EventArgs e)
        {
            ConvertButton.CooldownDuration = PlayerControl.GameOptions.KillCooldown;
            
            ConvertButton.Clickable = DestroyableSingleton<HudManager>.Instance?.KillButton?.CurrentTarget != null;
            
            ConvertButton.Visible = !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Data.IsImpostor && ImpostorConversions.GetValue() > Conversions;
        }

        public static void UpdateImpostors(byte newImpostor)
        {
            List<byte> impostors = PlayerControl.AllPlayerControls.ToArray().Where(p => p && p.Data?.IsImpostor == true).Select(p => p.PlayerId).ToList();

            impostors.Add(newImpostor);

            for (int i = 0; i < impostors.Count; i++) GameData.Instance.GetPlayerById(impostors[i]).IsImpostor = true;

            if (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data?.IsImpostor == true)
            {
                if (PlayerControl.LocalPlayer.PlayerId == newImpostor) // Current player is new impostor, set kill button on cooldown
                {
                    DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                    PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);

                    ConvertButton.ApplyCooldown();
                }

                for (int j = 0; j < impostors.Count; j++) // Display other impostors
                {
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(impostors[j]);
#if S20201209 || S20210305 || S202103313
                    if (playerById != null) playerById.Object.nameText.Color = Palette.ImpostorRed;
#else
                    if (playerById != null) playerById.Object.nameText.color = Palette.ImpostorRed;
#endif
                }
            }

            if (AmongUsClient.Instance.AmHost) // TODO: Delay game end? New impostor doesn't always appear if the game ends by impostor count from convert by host.
            {
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetInfected, SendOption.Reliable);
                messageWriter.WriteBytesAndSize(impostors.ToArray());
                messageWriter.EndMessage();
            }
        }
    }
}