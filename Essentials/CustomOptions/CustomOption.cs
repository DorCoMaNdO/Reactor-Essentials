using BepInEx.Configuration;
using System;

namespace Essentials.CustomOptions
{
    [Obsolete("Everything to do with custom options has moved from Essentials.CustomOptions to Essentials.Options, please update the namespace reference.", true)]
    public enum CustomOptionType
    {
        Toggle,
        Number,
        String
    }

    [Obsolete("Everything to do with custom options has moved from Essentials.CustomOptions to Essentials.Options, please update the namespace reference.", true)]
    public partial class CustomOption
    {
        private protected Options.CustomOption NewOption;

        public static bool ShamelessPlug = true;
        public static bool Debug = false;

        public readonly string PluginID;
        public readonly string ConfigID;
        public readonly string ID;
        public readonly string Name;

        protected readonly bool SaveValue;
        public readonly CustomOptionType Type;
        public readonly object DefaultValue;

        public event EventHandler<OptionOnValueChangedEventArgs> OnValueChanged;
        public event EventHandler<OptionValueChangedEventArgs> ValueChanged;

        public OptionBehaviour GameSetting { get; private protected set; }

        public Func<CustomOption, object, string> StringFormat;

        private protected CustomOption(Options.CustomOption newOption)
        {
            NewOption = newOption;

            PluginID = NewOption.PluginID;
            ConfigID = NewOption.ConfigID;
            Name = NewOption.Name;

            SaveValue = NewOption.SaveValue;

            Type = (CustomOptionType)NewOption.Type;
            DefaultValue = NewOption.DefaultValue;
        }

        protected CustomOption(string id, string name, bool saveValue, CustomOptionType type, object value) : this(new Options.CustomOption(id, name, saveValue, (Options.CustomOptionType)type, value))
        {
        }

        public void RaiseIfNonDefault()
        {
            NewOption.RaiseIfNonDefault();
        }

        public static CustomToggleOption AddToggle(string id, string name, bool saveValue, bool value)
        {
            return new CustomToggleOption(id, name, saveValue, value);
        }

        public static CustomToggleOption AddToggle(string id, string name, bool value)
        {
            return AddToggle(id, name, true, value);
        }

        public static CustomToggleOption AddToggle(string name, bool saveValue, bool value)
        {
            return AddToggle(name, name, saveValue, value);
        }

        public static CustomToggleOption AddToggle(string name, bool value)
        {
            return AddToggle(name, name, value);
        }

        public static CustomNumberOption AddNumber(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return new CustomNumberOption(id, name, saveValue, value, min, max, increment);
        }

        public static CustomNumberOption AddNumber(string id, string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(id, name, true, value, min, max, increment);
        }

        public static CustomNumberOption AddNumber(string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, name, saveValue, value, min, max, increment);
        }

        public static CustomNumberOption AddNumber(string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, true, value, min, max, increment);
        }

        public static CustomStringOption AddString(string id, string name, bool saveValue, params string[] values)
        {
            return new CustomStringOption(id, name, saveValue, values);
        }

        public static CustomStringOption AddString(string id, string name, params string[] values)
        {
            return AddString(id, name, true, values);
        }

        public static CustomStringOption AddString(string name, bool saveValue, params string[] values)
        {
            return AddString(name, name, saveValue, values);
        }

        public static CustomStringOption AddString(string name, params string[] values)
        {
            return AddString(name, name, values);
        }

        public void SetToDefault(bool raiseEvents = true)
        {
            NewOption.SetToDefault(raiseEvents);
        }

        public void SetValue(object value)
        {
            NewOption.SetValue(value);
        }

        public T GetValue<T>()
        {
            return NewOption.GetValue<T>();
        }

        public override string ToString()
        {
            return NewOption.ToString();
        }

        protected void RaiseOnValueChanged(OptionOnValueChangedEventArgs newArgs)
        {
            if (OnValueChanged == null || !AmongUsClient.Instance || !PlayerControl.LocalPlayer || !AmongUsClient.Instance.AmHost) return;

            object value = newArgs.Value;
            object lastValue = value;

            OptionOnValueChangedEventArgs args;
            if (newArgs is ToggleOptionOnValueChangedEventArgs)
            {
                args = new ToggleOptionOnValueChangedEventArgs(value, newArgs.OldValue);
            }
            else if (newArgs is NumberOptionOnValueChangedEventArgs)
            {
                args = new NumberOptionOnValueChangedEventArgs(value, newArgs.OldValue);
            }
            else if (newArgs is StringOptionOnValueChangedEventArgs)
            {
                args = new StringOptionOnValueChangedEventArgs(value, newArgs.OldValue);
            }
            else
            {
                args = new OptionOnValueChangedEventArgs(value, newArgs.OldValue);
            }

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

                if (args.Cancel)
                {
                    args.Cancel = true;

                    return; // Handler cancelled value change.
                }
            }

            newArgs.Value = args.Value;
        }

        protected void RaiseValueChanged(OptionValueChangedEventArgs args)
        {
            ValueChanged?.Invoke(this, args);
        }
    }

    [Obsolete("Everything to do with custom options has moved from Essentials.CustomOptions to Essentials.Options, please update the namespace reference.", true)]
    public class CustomToggleOption : CustomOption
    {
        public readonly ConfigEntry<bool> ConfigEntry;

        private CustomToggleOption(Options.CustomToggleOption newOption) : base(newOption)
        {
            NewOption.OnValueChanged += (sender, args) =>
            {
                RaiseOnValueChanged(new ToggleOptionOnValueChangedEventArgs(args.Value, args.OldValue));
            };

            NewOption.ValueChanged += (sender, args) =>
            {
                RaiseValueChanged(new ToggleOptionValueChangedEventArgs(args.Value, args.OldValue));
            };

            ConfigEntry = newOption.ConfigEntry;
        }

        protected internal CustomToggleOption(string id, string name, bool saveValue, bool value) : this(new Options.CustomToggleOption(id, name, saveValue, value))
        {
        }

        public void Toggle()
        {
            ((Options.CustomToggleOption)NewOption).Toggle();
        }

        public void SetValue(bool value)
        {
            ((Options.CustomToggleOption)NewOption).SetValue(value);
        }

        public bool GetDefaultValue()
        {
            return ((Options.CustomToggleOption)NewOption).GetDefaultValue();
        }

        public bool GetOldValue()
        {
            return ((Options.CustomToggleOption)NewOption).GetOldValue();
        }

        public bool GetValue()
        {
            return ((Options.CustomToggleOption)NewOption).GetValue();
        }
    }

    [Obsolete("Everything to do with custom options has moved from Essentials.CustomOptions to Essentials.Options, please update the namespace reference.", true)]
    public class CustomNumberOption : CustomOption
    {
        public readonly ConfigEntry<float> ConfigEntry;

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


        private CustomNumberOption(Options.CustomNumberOption newOption) : base(newOption)
        {
            Min = newOption.Min;
            Max = newOption.Max;

            Increment = newOption.Increment;

            NewOption.OnValueChanged += (sender, args) =>
            {
                RaiseOnValueChanged(new NumberOptionOnValueChangedEventArgs(args.Value, args.OldValue));
            };

            NewOption.ValueChanged += (sender, args) =>
            {
                RaiseValueChanged(new NumberOptionValueChangedEventArgs(args.Value, args.OldValue));
            };

            ConfigEntry = newOption.ConfigEntry;
        }

        protected internal CustomNumberOption(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F) : this(new Options.CustomNumberOption(id, name, saveValue, value, min, max, increment))
        {
        }

        public void Increase()
        {
            ((Options.CustomNumberOption)NewOption).Increase();
        }

        public void Decrease()
        {
            ((Options.CustomNumberOption)NewOption).Decrease();
        }

        public void SetValue(float value)
        {
            ((Options.CustomNumberOption)NewOption).SetValue(value);
        }

        public float GetDefaultValue()
        {
            return ((Options.CustomNumberOption)NewOption).GetDefaultValue();
        }

        public float GetOldValue()
        {
            return ((Options.CustomNumberOption)NewOption).GetOldValue();
        }

        public float GetValue()
        {
            return ((Options.CustomNumberOption)NewOption).GetValue();
        }
    }

    [Obsolete("Everything to do with custom options has moved from Essentials.CustomOptions to Essentials.Options, please update the namespace reference.")]
    public class CustomStringOption : CustomOption
    {
        public readonly ConfigEntry<int> ConfigEntry;

        public readonly string[] Values;

        private CustomStringOption(Options.CustomStringOption newOption) : base(newOption)
        {
            Values = newOption.Values;

            NewOption.OnValueChanged += (sender, args) =>
            {
                RaiseOnValueChanged(new StringOptionOnValueChangedEventArgs(args.Value, args.OldValue));
            };

            NewOption.ValueChanged += (sender, args) =>
            {
                RaiseValueChanged(new NumberOptionValueChangedEventArgs(args.Value, args.OldValue));
            };

            ConfigEntry = newOption.ConfigEntry;
        }

        public CustomStringOption(string id, string name, bool saveValue, string[] values) : this(new Options.CustomStringOption(id, name, saveValue, values))
        {
        }

        public void Increase()
        {
            ((Options.CustomStringOption)NewOption).Increase();
        }

        public void Decrease()
        {
            ((Options.CustomStringOption)NewOption).Decrease();
        }

        public void SetValue(int value)
        {
            ((Options.CustomStringOption)NewOption).SetValue(value);
        }

        public int GetDefaultValue()
        {
            return ((Options.CustomStringOption)NewOption).GetDefaultValue();
        }

        public int GetOldValue()
        {
            return ((Options.CustomStringOption)NewOption).GetOldValue();
        }

        public int GetValue()
        {
            return ((Options.CustomStringOption)NewOption).GetValue();
        }

        public string GetText(int value)
        {
            return ((Options.CustomStringOption)NewOption).GetText(value);
        }

        public string GetText()
        {
            return ((Options.CustomStringOption)NewOption).GetText();
        }
    }
}