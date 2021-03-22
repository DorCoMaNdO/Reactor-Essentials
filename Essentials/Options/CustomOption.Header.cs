namespace Essentials.Options
{
    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling option headers.
    /// </summary>
    public class CustomOptionHeader : CustomOption
    {
        public override bool SendRpc { get { return false; } }

        /// <summary>
        /// Adds an option header.
        /// </summary>
        /// <param name="title">The title of the header</param>
        /// <param name="menu">The header will be visible in the lobby options menu</param>
        /// <param name="hud">The header will appear in the option list in the lobby</param>
        public CustomOptionHeader(string title, bool menu = true, bool hud = true) : base(title, title, false, CustomOptionType.Toggle, false)
        {
            OnValueChanged += (sender, args) =>
            {
                args.Cancel = true;
            };

            ToStringFormat = (_, name, _) => $"{name}[]";

            MenuVisible = menu;
            HudVisible = hud;
        }

        protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is not ToggleOption toggle) return;

            toggle.TitleText.Text = GetFormattedName();

            toggle.CheckMark.enabled = toggle.oldValue = false;

            toggle.transform.FindChild("Background")?.gameObject?.SetActive(false);
            toggle.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds an option header.
        /// </summary>
        /// <param name="title">The title of the header</param>
        /// <param name="menu">The header will be visible in the lobby options menu</param>
        /// <param name="hud">The header will appear in the option list in the lobby</param>
        public static CustomOptionHeader AddHeader(string title, bool menu = true, bool hud = true)
        {
            return new CustomOptionHeader(title, menu, hud);
        }
    }
}