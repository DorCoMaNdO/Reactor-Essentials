namespace Essentials.Options
{
    /// <summary>
    /// A derivative of <see cref="CustomOptionButton"/>, handling option headers.
    /// </summary>
    public class CustomOptionHeader : CustomOptionButton
    {
        /// <summary>
        /// Adds an option header.
        /// </summary>
        /// <param name="title">The title of the header</param>
        /// <param name="menu">The header will be visible in the lobby options menu</param>
        /// <param name="hud">The header will appear in the HUD (option list) in the lobby</param>
        /// <param name="initialValue">The header's initial (client sided) value, can be used to hide/show other options</param>
        public CustomOptionHeader(string title, bool menu = true, bool hud = true, bool initialValue = false) : base(title, menu, hud, initialValue)
        {
        }

        protected override bool GameObjectCreated(OptionBehaviour o)
        {
            o.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);
            o.transform.FindChild("Background")?.gameObject?.SetActive(false);

            return UpdateGameObject();
        }

        /// <summary>
        /// Toggles the option value (called when the header is pressed).
        /// </summary>
        public override void Toggle()
        {
            base.Toggle();
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds an option header.
        /// </summary>
        /// <param name="title">The title of the header</param>
        /// <param name="menu">The header will be visible in the lobby options menu</param>
        /// <param name="hud">The header will appear in the HUD (option list) in the lobby</param>
        /// <param name="initialValue">The header's initial (client sided) value, can be used to hide/show other options</param>
        public static CustomOptionHeader AddHeader(string title, bool menu = true, bool hud = true, bool initialValue = false)
        {
            return new CustomOptionHeader(title, menu, hud, initialValue);
        }
    }
}