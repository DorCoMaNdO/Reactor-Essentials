using BepInEx.Configuration;
using Essentials.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Essentials.Options
{
    public enum CustomOptionType
    {
        /// <summary>
        /// A checkmark toggle setting.
        /// </summary>
        Toggle,
        /// <summary>
        /// A float setting with increase/decrease arrows.
        /// </summary>
        Number,
        /// <summary>
        /// A string setting (underlying int) with forward/back arrows.
        /// </summary>
        String
    }

    /// <summary>
    /// A class wrapping all the nessecary logic to add custom lobby options.
    /// </summary>
    public partial class CustomOption
    {
        /// <summary>
        /// The list of all the added custom options.
        /// </summary>
        private static List<CustomOption> Options = new List<CustomOption>();

        /// <summary>
        /// Enables or disables the credit string appended to the option list in the lobby screen.
        /// Please provide credit or reference elsewhere if you disable this.
        /// </summary>
        public static bool ShamelessPlug = true;

        /// <summary>
        /// Enables debug logging messages.
        /// </summary>
        public static bool Debug = false;

        /// <summary>
        /// The ID of the plugin that created the option.
        /// </summary>
        public readonly string PluginID;
        /// <summary>
        /// The key value used in the config to store the option value (when SaveValue is true).
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Name">Name</see> when unspecified.
        /// </remarks>
        public readonly string ConfigID;
        /// <summary>
        /// Combines <see cref="PluginID">PluginID</see> and <see cref="ConfigID">ConfigID</see> with an underscore between.
        /// </summary>
        public readonly string ID;
        /// <summary>
        /// The name/title of the option.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Specifies whether this option saves it's value to be reloaded when the game is launched again.
        /// </summary>
        public readonly bool SaveValue;
        /// <summary>
        /// The option type.
        /// See <see cref="CustomOptionType"/>.
        /// </summary>
        public readonly CustomOptionType Type;
        /// <summary>
        /// The value provided to the option constructor.
        /// </summary>
        public readonly object DefaultValue;

        /// <summary>
        /// The previous value, may match <see cref="Value">Value</see> when it matches <see cref="DefaultValue">DefaultValue</see>
        /// </summary>
        protected object OldValue { get; private protected set; }
        /// <summary>
        /// The current value of the option.
        /// </summary>
        protected object Value { get; private protected set; }

        /// <summary>
        /// An event raised before a value change occurs, can alter the final value or cancel the value change. Only raised for the lobby host.
        /// See <see cref="OptionOnValueChangedEventArgs"/> and childs <seealso cref="ToggleOptionOnValueChangedEventArgs"/>, <seealso cref="NumberOptionOnValueChangedEventArgs"/> and <seealso cref="StringOptionOnValueChangedEventArgs"/>.
        /// </summary>
        public event EventHandler<OptionOnValueChangedEventArgs> OnValueChanged;
        /// <summary>
        /// An event raised after the option value has changed.
        /// See <see cref="OptionValueChangedEventArgs"/> and childs <seealso cref="ToggleOptionValueChangedEventArgs"/>, <seealso cref="NumberOptionValueChangedEventArgs"/> and<seealso cref="StringOptionValueChangedEventArgs"/>.
        /// </summary>
        public event EventHandler<OptionValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// The game object that represents the custom option in the lobby options list.
        /// </summary>
        public OptionBehaviour GameSetting { get; private protected set; }

        /// <summary>
        /// The string format that's applied to <see cref="ToString()"/>
        /// </summary>
        public Func<CustomOption, object, string> StringFormat;

        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="type"><see cref="Type"/></param>
        /// <param name="value">The default value</param>
        protected internal CustomOption(string id, string name, bool saveValue, CustomOptionType type, object value)
        {
            PluginID = PluginHelpers.GetCallingPluginId();
            ConfigID = id;

            //string Id = ID = $"{nameof(CustomOption)}_{PluginID}_{id}";
            string Id = ID = $"{PluginID}_{id}";
            Name = name;

            SaveValue = saveValue;

            Type = type;
            DefaultValue = OldValue = Value = value;

            int i = 0;
            while (Options.Any(option => option.ID.Equals(ID, StringComparison.Ordinal)))
            {
                ID = $"{Id}_{++i}";
                ConfigID = $"{id}_{i}";
            }

            Options.Add(this);
        }

        /// <summary>
        /// Returns event args of type <see cref="OptionOnValueChangedEventArgs"/> or a derivative.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="oldValue">The current value</param>
        /// <returns></returns>
        private protected virtual OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new OptionOnValueChangedEventArgs(value, Value);
        }

        /// <summary>
        /// Returns event args of type <see cref="OptionValueChangedEventArgs"/> or a derivative.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="oldValue">The current value</param>
        /// <returns></returns>
        private protected virtual OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new OptionValueChangedEventArgs(value, Value);
        }

        private void OnGameOptionCreated(OptionBehaviour o)
        {
            if (o == null) return;

            try
            {
                // Could move some logic to this callback, though pointless when the methods leading to this callback to be called are overriden.
                o.OnValueChanged = new Action<OptionBehaviour>((_) => { });

                o.name = o.gameObject.name = ID;

                GameOptionCreated(o);
            }
            catch (Exception e)
            {
                EssentialsPlugin.Logger.LogWarning($"Exception in OnGameOptionCreated for option \"{Name}\" ({Type}): {e}");
            }

            GameSetting = o;
        }

        /// <summary>
        /// Called when the game object is (re)created for this option.
        /// </summary>
        /// <param name="o">The game object that was created for this option</param>
        private protected virtual void GameOptionCreated(OptionBehaviour o)
        {
            // throw unimplemented?
        }

        /// <summary>
        /// Calls <see cref="SetValue"/> when <see cref="Value"/> differs from <see cref="DefaultValue"/>
        /// </summary>
        public void RaiseIfNonDefault()
        {
            if (Value != DefaultValue) SetValue(DefaultValue, true);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="value">The default value</param>
        public static CustomToggleOption AddToggle(string id, string name, bool saveValue, bool value)
        {
            return new CustomToggleOption(id, name, saveValue, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="value">The default value</param>
        public static CustomToggleOption AddToggle(string id, string name, bool value)
        {
            return AddToggle(id, name, true, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="value">The default value</param>
        public static CustomToggleOption AddToggle(string name, bool saveValue, bool value)
        {
            return AddToggle(name, name, saveValue, value);
        }

        /// <summary>
        /// Adds a toggle option.
        /// </summary>
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="value">The default value</param>
        public static CustomToggleOption AddToggle(string name, bool value)
        {
            return AddToggle(name, name, value);
        }

        /// <summary>
        /// Adds a number option.
        /// </summary>
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="value">The default value</param>
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
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="value">The default value</param>
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
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="value">The default value</param>
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
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="value">The default value</param>
        /// <param name="min">The lowest value permitted, may be overriden if <paramref name="value"/> is lower</param>
        /// <param name="max">The highest value permitted, may be overriden if <paramref name="value"/> is higher</param>
        /// <param name="increment">The increment or decrement steps when <see cref="CustomNumberOption.Increase"/> or <see cref="CustomNumberOption.Decrease"/> are called</param>
        public static CustomNumberOption AddNumber(string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, true, value, min, max, increment);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="values">The string values that may be displayed, default value is index 0</param>
        public static CustomStringOption AddString(string id, string name, bool saveValue, params string[] values)
        {
            return new CustomStringOption(id, name, saveValue, values);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="id"><see cref="ID"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="values">The string values that may be displayed, default value is index 0</param>
        public static CustomStringOption AddString(string id, string name, params string[] values)
        {
            return AddString(id, name, true, values);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="saveValue"><see cref="SaveValue"/></param>
        /// <param name="values">The string values that may be displayed, default value is index 0</param>
        public static CustomStringOption AddString(string name, bool saveValue, params string[] values)
        {
            return AddString(name, name, saveValue, values);
        }

        /// <summary>
        /// Adds a string option.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="values">The string values that may be displayed, default value is index 0</param>
        public static CustomStringOption AddString(string name, params string[] values)
        {
            return AddString(name, name, values);
        }

        /*public static CustomKeyValueOption AddKeyValue(string id, string name, string[] values, Action<int> callback)
        {
            CustomKeyValueOption option = new CustomKeyValueOption(id, name, values);
            
            void KeyValueCallback(object newVal, bool report)
            {
                int newValue = (int)newVal;
                
                //int oldValue = (int)option.Value;
                option.Value = newValue;
                
                if (option.GameSetting is KeyValueOption kv)
                {
                    if (report && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost)
                    {
                        Rpc.Instance.Send(new Rpc.Data(option));

                        option.ConfigEntry.Value = newValue;
                    }

                    //kv.oldValue = oldValue;
                    kv.Selected = kv.oldValue = newValue;
                    kv.ValueText.Text = option.ToString();
                }
                
                try
                {
                    callback?.Invoke(newValue);
                }
                catch (Exception e)
                {
                    EssentialsPlugin.Logger.LogInfo($"KeyValueCallback \"{name}\" failed: {e}");
                }
            }
            
            option.Callback = new Action<object, bool>(KeyValueCallback);
            
            if (option.Value != option.DefaultValue) option.Callback.Invoke(option.Value, false);
            
            return option;
        }

        public static CustomKeyValueOption AddKeyValue(string id, string name, params string[] values)
        {
            return AddKeyValue(id, name, values, null);
        }

        public static CustomKeyValueOption AddKeyValue(string name, params string[] values)
        {
            return AddKeyValue(name, name, values);
        }*/

        /// <summary>
        /// Restores the option to it's default value.
        /// </summary>
        public void SetToDefault(bool raiseEvents = true)
        {
            SetValue(DefaultValue, raiseEvents);
        }

        /// <summary>
        /// Sets the option's value, it's not recommended to call this directly, call derivatives instead.
        /// </summary>
        /// <remarks>
        /// Does nothing when the value type differs or when the value matches the current value.
        /// </remarks>
        /// <param name="value">The new value</param>
        /// <param name="raiseEvents">Whether or not to raise events</param>
        private protected void SetValue(object value, bool raiseEvents)
        {
            if (value?.GetType() != Value?.GetType() || Value == value) return; // Refuse value updates that don't match the option type

            if (raiseEvents && OnValueChanged != null && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost)
            {
                object lastValue = value;

                OptionOnValueChangedEventArgs args = OnValueChangedEventArgs(value, Value);
                foreach (EventHandler<OptionOnValueChangedEventArgs> handler in OnValueChanged.GetInvocationList())
                {
                    handler(this, args);

                    if (args.Value.GetType() != value.GetType())
                    {
                        args.Value = lastValue;
                        args.Cancel = false;

                        EssentialsPlugin.Logger.LogWarning($"A handler for option \"{Name}\" attempted to change value type, ignored.");
                    }

                    lastValue = args.Value;

                    if (args.Cancel) return; // Handler cancelled value change.
                }

                value = args.Value;
            }

            if (OldValue != Value) OldValue = Value;

            Value = value;

            if (GameSetting != null && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost) Rpc.Instance.Send(new Rpc.Data(this));

            try
            {
                if (GameSetting is ToggleOption toggle)
                {
                    bool newValue = (bool)Value;

                    toggle.oldValue = newValue;
                    if (toggle.CheckMark != null) toggle.CheckMark.enabled = newValue;
                }
                else if (GameSetting is NumberOption number)
                {
                    float newValue = (float)Value;

                    number.Value = number.oldValue = newValue;
                    number.ValueText.Text = ToString();
                }
                else if (GameSetting is StringOption str)
                {
                    int newValue = (int)Value;

                    str.Value = str.oldValue = newValue;
                    str.ValueText.Text = ToString();
                }
                else if (GameSetting is KeyValueOption kv)
                {
                    int newValue = (int)Value;

                    kv.Selected = kv.oldValue = newValue;
                    kv.ValueText.Text = ToString();
                }
            }
            catch (Exception e)
            {
                EssentialsPlugin.Logger.LogWarning($"Failed to update game setting value for option \"{Name}\": {e}");
            }

            if (raiseEvents) ValueChanged?.Invoke(this, ValueChangedEventArgs(value, Value));
            /*{
                OptionValueChangedEventArgs args = ValueChangedEventArgs(value, Value);
                foreach (EventHandler<OptionValueChangedEventArgs> handler in ValueChanged.GetInvocationList()) handler(this, args);
            }*/

            try
            {
                if (GameSetting != null) Object.FindObjectOfType<GameOptionsMenu>()?.Method_16(); // RefreshChildren();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the option's value, it's not recommended to call this directly, call derivatives instead.
        /// </summary>
        /// <remarks>
        /// Does nothing when the value type differs or when the value matches the current value.
        /// </remarks>
        /// <param name="value">The new value</param>
        public void SetValue(object value)
        {
            SetValue(value, true);
        }

        /// <summary>
        /// Gets the option value casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <returns>The casted value.</returns>
        public T GetValue<T>()
        {
            return (T)Value;
        }

        /// <returns><see cref="object.ToString()"/> or the return value of <see cref="StringFormat"/> when provided.</returns>
        public override string ToString()
        {
            if (StringFormat != null) return StringFormat(this, Value);

            return Value.ToString();
        }
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling toggle options.
    /// </summary>
    public class CustomToggleOption : CustomOption
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
        /// <param name="id"><see cref="CustomOption.ID"/></param>
        /// <param name="name"><see cref="CustomOption.Name"/></param>
        /// <param name="saveValue"><see cref="CustomOption.SaveValue"/></param>
        /// <param name="value">The default value</param>
        protected internal CustomToggleOption(string id, string name, bool saveValue, bool value) : base(id, name, saveValue, CustomOptionType.Toggle, value)
        {
            ValueChanged += (sender, args) =>
            {
                if (GameSetting is ToggleOption && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost && ConfigEntry != null) ConfigEntry.Value = (bool)Value;
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, (bool)DefaultValue) : null;
            SetValue(ConfigEntry == null ? GetDefaultValue() : ConfigEntry.Value, false);

            StringFormat = (sender, value) => ((bool)value) ? "On" : "Off";

            //RaiseIfNonDefault();
        }

        private protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionOnValueChangedEventArgs(value, Value);
        }

        private protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionValueChangedEventArgs(value, Value);
        }

        private protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is not ToggleOption toggle) return;

            toggle.TitleText.Text = Name;
            toggle.CheckMark.enabled = toggle.oldValue = GetValue();
        }

        /// <summary>
        /// Toggles the option value.
        /// </summary>
        public void Toggle()
        {
            SetValue(!GetValue());
        }

        private void SetValue(bool value, bool raiseEvents)
        {
            base.SetValue(value, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetValue(bool value)
        {
            SetValue(value, true);
        }

        /// <returns>The boolean-casted default value.</returns>
        public bool GetDefaultValue()
        {
            return (bool)DefaultValue;
        }

        /// <returns>The boolean-casted old value.</returns>
        public bool GetOldValue()
        {
            return (bool)OldValue;
        }

        /// <returns>The boolean-casted current value.</returns>
        public bool GetValue()
        {
            return (bool)Value;
        }
    }

    /// <summary>
    /// A derivative of <see cref="CustomOption"/>, handling number options.
    /// </summary>
    public class CustomNumberOption : CustomOption
    {
        /// <summary>
        /// The config entry used to store this option's value.
        /// </summary>
        /// <remarks>
        /// Can be null when <see cref="CustomOption.SaveValue"/> is false.
        /// </remarks>
        public readonly ConfigEntry<float> ConfigEntry;

        /// <summary>
        /// A "modifier" string format, simply appending an 'x' after the value.
        /// </summary>
        public static Func<CustomOption, object, string> ModifierStringFormat { get; } = (sender, value) => $"{value:0.0}x";

        //public new float Value { get { return (float)base.Value; } private protected set { base.Value = value; } }

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

        protected internal CustomNumberOption(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F) : base(id, name, saveValue, CustomOptionType.Number, value)
        {
            Min = Math.Min(value, min);
            Max = Math.Max(value, max);

            Increment = increment;

            ValueChanged += (sender, args) =>
            {
                if (GameSetting is NumberOption && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost && ConfigEntry != null) ConfigEntry.Value = (float)Value;
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, (float)DefaultValue) : null;
            SetValue(ConfigEntry == null ? GetDefaultValue() : ConfigEntry.Value, false);

            StringFormat = (sender, value) => value.ToString();

            //RaiseIfNonDefault();
        }

        private protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new NumberOptionOnValueChangedEventArgs(value, Value);
        }

        private protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new NumberOptionValueChangedEventArgs(value, Value);
        }

        private protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is not NumberOption number) return;

            number.TitleText.Text = Name;
            number.ValidRange = new FloatRange(Min, Max);
            number.Increment = Increment;
            number.Value = number.oldValue = GetValue();
            number.ValueText.Text = ToString();
        }

        /// <summary>
        /// Increases <see cref="CustomOption.Value"/> by <see cref="Increment"/> while it's lower or until it matches <see cref="Max"/>.
        /// </summary>
        public void Increase()
        {
            SetValue(GetValue() + Increment);
        }

        /// <summary>
        /// Decreases <see cref="CustomOption.Value"/> by <see cref="Increment"/> while it's higher or until it matches <see cref="Min"/>.
        /// </summary>
        public void Decrease()
        {
            SetValue(GetValue() - Increment);
        }

        private void SetValue(float value, bool raiseEvents)
        {
            value = Mathf.Clamp(value, Min, Max);

            base.SetValue(value, raiseEvents);
        }

        /// <summary>
        /// Sets a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetValue(float value)
        {
            SetValue(value, true);
        }

        /// <returns>The float-casted default value.</returns>
        public float GetDefaultValue()
        {
            return (float)DefaultValue;
        }

        /// <returns>The float-casted old value.</returns>
        public float GetOldValue()
        {
            return (float)OldValue;
        }

        /// <returns>The float-casted current value.</returns>
        public float GetValue()
        {
            return (float)Value;
        }
    }

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

        protected internal CustomStringOption(string id, string name, bool saveValue, string[] values) : base(id, name, saveValue, CustomOptionType.String, 0)
        {
            Values = values;

            ValueChanged += (sender, args) =>
            {
                if (GameSetting is StringOption && AmongUsClient.Instance && PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost && ConfigEntry != null) ConfigEntry.Value = (int)Value;
            };

            ConfigEntry = saveValue ? EssentialsPlugin.Instance.Config.Bind(PluginID, ConfigID, (int)DefaultValue) : null;
            SetValue(ConfigEntry == null ? GetDefaultValue() : ConfigEntry.Value, false);

            StringFormat = (sender, value) => Values[(int)value];

            //RaiseIfNonDefault();
        }

        private protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionOnValueChangedEventArgs(value, Value);
        }

        private protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionValueChangedEventArgs(value, Value);
        }

        private protected override void GameOptionCreated(OptionBehaviour o)
        {
            if (o is not StringOption str) return;

            str.TitleText.Text = Name;
            str.Value = str.oldValue = GetValue();
            str.ValueText.Text = ToString();
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

    //public class CustomKeyValueOption : CustomStringOption
    //{
    //    protected internal CustomKeyValueOption(string id, string name, bool saveValue, string[] values) : base(id, name, saveValue, values)
    //    {
    //        GameSettingCallback = new Action<OptionBehaviour>(GameOptionCreated);
    //    }

    //    private void GameOptionCreated(OptionBehaviour o)
    //    {
    //        if (!(o is KeyValueOption kv)) return;

    //        kv.OnValueChanged = new Action<OptionBehaviour>((option) => { });

    //        kv.name = kv.gameObject.name = ID;

    //        kv.TitleText.Text = Name;
    //        if(kv.Values!=null)
    //        {
    //            kv.Values.Clear();
    //            for (int i = 0; i < Values.Length; i++) kv.Values.Add(new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>(Values[i], i));
    //        }
    //        /*kv.Values = new List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
    //        for (int i = 0; i < Values.Length; i++) kv.Values.Add(new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>(Values[i], i));*/
    //        kv.Selected = kv.oldValue = GetValue();
    //        kv.ValueText.Text = ToString();

    //        GameSetting = kv;
    //    }
    //}
}