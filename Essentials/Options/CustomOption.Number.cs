using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Essentials.Options
{
    public interface INumberOption
    {
        public void Increase();
        public void Decrease();
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling number options.
    /// </summary>
    public class CustomNumberOption : CustomOption, INumberOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.SaveValue"/> is false.
        /// </remarks>
        public readonly ConfigEntry<float> ConfigEntry;

        /// <summary>
        /// A "modifier" string format, simply appending 'x' after the value.
        /// </summary>
        public static Func<CustomOption, object, string> ModifierStringFormat { get; } = (sender, value) => $"{value:0.0}x";

        /// <summary>
        /// A "seconds" string format, simply appending 's' after the value.
        /// </summary>
        public static Func<CustomOption, object, string> SecondsStringFormat { get; } = (sender, value) => $"{value:0.0}s";

        //public new float Value { get { return (float)base.Value; } protected set { base.Value = value; } }

        /// <summary>
        /// The lowest permitted value.
        /// </summary>
        public readonly float Min;
        /// <summary>
        /// The highest permitted value.
        /// </summary>
        public readonly float Max;
        /// <summary>
        /// The increment or decrement steps when <see cref="Increase"/> or <see cref="Decrease"/> are called.
        /// </summary>
        public readonly float Increment;

        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="value">The initial/default value</param>
        /// <param name="min">The lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">The highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">The increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        public CustomNumberOption(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F) : base(id, name, saveValue, CustomOptionType.Number, value)
        {
            Min = Mathf.Min(value, min);
            Max = Mathf.Max(value, max);

            Increment = increment;

            ValueChanged += (sender, args) =>
            {
                if (GameSetting is NumberOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer && ConfigEntry != null) ConfigEntry.Value = (float)Value;
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, (float)DefaultValue) : null;
            SetValue(ConfigEntry == null ? GetDefaultValue() : ConfigEntry.Value, false);

            StringFormat = (sender, value) => value.ToString();
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new NumberOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new NumberOptionValueChangedEventArgs(value, Value);
        }

        protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is not NumberOption number) return;

            number.TitleText.Text = GetFormattedName();
            number.ValidRange = new FloatRange(Min, Max);
            number.Increment = Increment;
#if S20201209
            number.Value = number.oldValue = GetValue();
#elif S20210305
            number.Value = number.Field_3 = GetValue();
#endif
            number.ValueText.Text = GetFormattedValue();
        }

        /// <summary>
        /// Increases <see cref="CustomOption.Value"/> by <see cref="Increment"/> while it's lower or until it matches <see cref="Max"/>.
        /// </summary>
        public virtual void Increase()
        {
            SetValue(GetValue() + Increment);
        }

        /// <summary>
        /// Decreases <see cref="CustomOption.Value"/> by <see cref="Increment"/> while it's higher or until it matches <see cref="Min"/>.
        /// </summary>
        public virtual void Decrease()
        {
            SetValue(GetValue() - Increment);
        }

        protected virtual void SetValue(float value, bool raiseEvents)
        {
            value = Mathf.Clamp(value, Min, Max);

            base.SetValue(value, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void SetValue(float value)
        {
            SetValue(value, true);
        }

        /// <returns>The float-casted default value.</returns>
        public virtual float GetDefaultValue()
        {
            return (float)DefaultValue;
        }

        /// <returns>The float-casted old value.</returns>
        public virtual float GetOldValue()
        {
            return (float)OldValue;
        }

        /// <returns>The float-casted current value.</returns>
        public virtual float GetValue()
        {
            return (float)Value;
        }
    }

    public partial class CustomOption
    {
        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="value">The initial/default value</param>
        /// <param name="min">The lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">The highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">The increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        public static CustomNumberOption AddNumber(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return new CustomNumberOption(id, name, saveValue, value, min, max, increment);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="value">The initial/default value</param>
        /// <param name="min">The lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">The highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">The increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        public static CustomNumberOption AddNumber(string id, string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(id, name, true, value, min, max, increment);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="value">The initial/default value</param>
        /// <param name="min">The lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">The highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">The increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        public static CustomNumberOption AddNumber(string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, name, saveValue, value, min, max, increment);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="name">The name/title of the option</param>
        /// <param name="value">The initial/default value</param>
        /// <param name="min">The lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">The highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">The increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        public static CustomNumberOption AddNumber(string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, true, value, min, max, increment);
        }
    }
}