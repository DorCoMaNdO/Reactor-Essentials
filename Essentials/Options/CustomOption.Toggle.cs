using BepInEx.Configuration;

namespace Essentials.Options
{
    public interface IToggleOption
    {
        public void Toggle();
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling toggle options.
    /// </summary>
    public class CustomToggleOption : CustomOption, IToggleOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.SaveValue"/> is false.
        /// </remarks>
        public readonly ConfigEntry<bool> ConfigEntry;

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="value">The initial/default value</param>
        public CustomToggleOption(string id, string name, bool saveValue, bool value) : base(id, name, saveValue, CustomOptionType.Toggle, value)
        {
            ValueChanged += (sender, args) =>
            {
                if (GameSetting is ToggleOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer && ConfigEntry != null) ConfigEntry.Value = GetValue();
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, GetDefaultValue()) : null;
            SetValue(ConfigEntry == null ? GetDefaultValue() : ConfigEntry.Value, false);

            StringFormat = (sender, value) => ((bool)value) ? "On" : "Off";
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionValueChangedEventArgs(value, Value);
        }

        protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is ToggleOption toggle)
            {
                toggle.TitleText.Text = GetFormattedName();
                toggle.CheckMark.enabled = toggle.oldValue = GetValue();
            }
            else if (o is StringOption str) // Display settings in menu for non-host
            {
                str.TitleText.Text = GetFormattedName();
                str.Value = str.oldValue = 0;
                str.ValueText.Text = GetFormattedValue();
            }
        }

        /// <summary>
        /// Toggles the option value.
        /// </summary>
        public virtual void Toggle()
        {
            SetValue(!GetValue());
        }

        protected virtual void SetValue(bool value, bool raiseEvents)
        {
            base.SetValue(value, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void SetValue(bool value)
        {
            SetValue(value, true);
        }

        /// <returns>The boolean-casted default value.</returns>
        public virtual bool GetDefaultValue()
        {
            return GetDefaultValue<bool>();
        }

        /// <returns>The boolean-casted old value.</returns>
        public virtual bool GetOldValue()
        {
            return GetOldValue<bool>();
        }

        /// <returns>The boolean-casted current value.</returns>
        public virtual bool GetValue()
        {
            return GetValue<bool>();
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="value">The initial/default value</param>
        public static CustomToggleOption AddToggle(string id, string name, bool saveValue, bool value)
        {
            return new CustomToggleOption(id, name, saveValue, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="value">The initial/default value</param>
        public static CustomToggleOption AddToggle(string id, string name, bool value)
        {
            return AddToggle(id, name, true, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="name">The name/title of the option, also used as the option's ID</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="value">The initial/default value</param>
        public static CustomToggleOption AddToggle(string name, bool saveValue, bool value)
        {
            return AddToggle(name, name, saveValue, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="name">The name/title of the option</param>
        /// <param name="value">The initial/default value</param>
        public static CustomToggleOption AddToggle(string name, bool value)
        {
            return AddToggle(name, name, value);
        }
    }
}