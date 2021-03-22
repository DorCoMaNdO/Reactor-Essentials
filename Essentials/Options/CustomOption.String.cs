using BepInEx.Configuration;

namespace Essentials.Options
{
    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling string options.
    /// </summary>
    public class CustomStringOption : CustomOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.SaveValue"/> is false.
        /// </remarks>
        public readonly ConfigEntry<int> ConfigEntry;

        /// <summary>
        /// The text values the option can present.
        /// </summary>
        public readonly string[] Values;

        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="values">The string values that may be displayed, initial/default value is index 0</param>
        public CustomStringOption(string id, string name, bool saveValue, string[] values) : base(id, name, saveValue, CustomOptionType.String, 0)
        {
            Values = values;

            ValueChanged += (sender, args) =>
            {
                if (GameSetting is StringOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer && ConfigEntry != null) ConfigEntry.Value = (int)Value;
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, (int)DefaultValue) : null;
            SetValue(ConfigEntry == null ? GetDefaultValue() : ConfigEntry.Value, false);

            StringFormat = (sender, value) => Values[(int)value];
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionValueChangedEventArgs(value, Value);
        }

        protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is not StringOption str) return;

            str.TitleText.Text = GetFormattedName();
            str.Value = str.oldValue = GetValue();
            str.ValueText.Text = GetFormattedValue();
        }

        /// <summary>
        /// Increases <see cref="CustomOption.Value"/> by 1 while it's lower than the length of <see cref="Values"/> or sets it back to 0 once the length is exceeded.
        /// </summary>
        public void Increase()
        {
            int next = GetValue() + 1;
            if (next >= Values.Length) next = 0;

            SetValue(next);
        }

        /// <summary>
        /// Decreases <see cref="CustomOption.Value"/> by 1 while it's higher than 0 or sets it back to the length of <see cref="Values"/>-1.
        /// </summary>
        public void Decrease()
        {
            int next = GetValue() - 1;
            if (next < 0) next = Values.Length - 1;

            SetValue(next);
        }

        private void SetValue(int value, bool raiseEvents)
        {
            if (value < 0 || value >= Values.Length) value = (int)DefaultValue;

            base.SetValue(value, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetValue(int value)
        {
            SetValue(value, true);
        }

        /// <returns>The int-casted default value.</returns>
        public int GetDefaultValue()
        {
            return (int)DefaultValue;
        }

        /// <returns>The int-casted old value.</returns>
        public int GetOldValue()
        {
            return (int)OldValue;
        }

        /// <returns>The int-casted current value.</returns>
        public int GetValue()
        {
            return (int)Value;
        }

        /// <returns>The text at index <paramref name="value"/>.</returns>
        public string GetText(int value)
        {
            return Values[value];
        }

        /// <returns>The current text.</returns>
        public string GetText()
        {
            return GetText(GetValue());
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="values">The string values that may be displayed, initial/default value is index 0</param>
        public static CustomStringOption AddString(string id, string name, bool saveValue, params string[] values)
        {
            return new CustomStringOption(id, name, saveValue, values);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="values">The string values that may be displayed, initial/default value is index 0</param>
        public static CustomStringOption AddString(string id, string name, params string[] values)
        {
            return AddString(id, name, true, values);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="values">The string values that may be displayed, initial/default value is index 0</param>
        public static CustomStringOption AddString(string name, bool saveValue, params string[] values)
        {
            return AddString(name, name, saveValue, values);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="name">The name/title of the option</param>
        /// <param name="values">The string values that may be displayed, initial/default value is index 0</param>
        public static CustomStringOption AddString(string name, params string[] values)
        {
            return AddString(name, name, values);
        }
    }
}