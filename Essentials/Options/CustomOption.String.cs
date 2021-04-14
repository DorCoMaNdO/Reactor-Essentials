using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace Essentials.Options
{
    public interface IStringOption
    {
        public void Increase();
        public void Decrease();
        public string GetText();
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling string options.
    /// </summary>
    public class CustomStringOption : CustomOption, IStringOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.SaveValue"/> is false.
        /// </remarks>
        public readonly ConfigEntry<int> ConfigEntry;

        protected readonly string[] _values;
        /// <summary>
        /// The text values the option can present.
        /// </summary>
        public IReadOnlyCollection<string> Values { get { return Array.AsReadOnly(_values); } }

        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="values">The string values that may be displayed, initial/default value is index 0</param>
        public CustomStringOption(string id, string name, bool saveValue, string[] values) : base(id, name, saveValue, CustomOptionType.String, 0)
        {
            _values = values;

            ValueChanged += (sender, args) =>
            {
                if (ConfigEntry != null && GameObject is StringOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) ConfigEntry.Value = GetValue();
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, GetDefaultValue()) : null;
            SetValue(ConfigEntry?.Value ?? GetDefaultValue(), false);

            ValueStringFormat = (sender, value) => _values[(int)value];
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionValueChangedEventArgs(value, Value);
        }

        /// <summary>
        /// Increases <see cref="CustomOption.Value"/> by 1 while it's lower than the length of <see cref="_values"/> or sets it back to 0 once the length is exceeded.
        /// </summary>
        public virtual void Increase()
        {
            SetValue((GetValue() + 1) % _values.Length);
        }

        /// <summary>
        /// Decreases <see cref="CustomOption.Value"/> by 1 while it's higher than 0 or sets it back to the length of <see cref="_values"/>-1.
        /// </summary>
        public virtual void Decrease()
        {
            SetValue((GetValue() + (_values.Length - 1)) % _values.Length);
        }

        protected virtual void SetValue(int value, bool raiseEvents)
        {
            if (value < 0 || value >= _values.Length) value = GetDefaultValue();

            base.SetValue(value, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void SetValue(int value)
        {
            SetValue(value, true);
        }

        /// <returns>The int-casted default value.</returns>
        public virtual int GetDefaultValue()
        {
            return GetDefaultValue<int>();
        }

        /// <returns>The int-casted old value.</returns>
        public virtual int GetOldValue()
        {
            return GetOldValue<int>();
        }

        /// <returns>The int-casted current value.</returns>
        public virtual int GetValue()
        {
            return GetValue<int>();
        }

        /// <returns>The text at index <paramref name="value"/>.</returns>
        public virtual string GetText(int value)
        {
            return _values[value];
        }

        /// <returns>The current text.</returns>
        public virtual string GetText()
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