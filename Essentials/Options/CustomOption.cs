using Essentials.Extensions;
using Essentials.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
#if !S20201209 && !S20210305 && !S202103313
using TMPro;
#endif
using Object = UnityEngine.Object;

namespace Essentials.Options
{
    //[Serializable]
    public enum CustomOptionType : byte
    {
        /// <summary>
        /// A checkmark toggle option.
        /// </summary>
        Toggle,
        /// <summary>
        /// A float number option with increase/decrease buttons.
        /// </summary>
        Number,
        /// <summary>
        /// A string option (underlying int) with forward/back arrows.
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
        /// Enables or disables the credit string appended to the HUD (option list) in the lobby.
        /// Please provide credit or reference elsewhere if you disable this.
        /// </summary>
        public static bool ShamelessPlug { get; set; } = true;

        /// <summary>
        /// Enables debug logging messages.
        /// </summary>
        public static bool Debug { get; set; } = true;

#if S20201209 || S20210305 || S202103313
        /// <summary>
        /// The size of HUD (lobby options) text, game default is 0.65F, Essentials default is 0.5F.
        /// </summary>
        public static float HudTextScale { get; set; } = 0.5F;
#else
        /// <summary>
        /// The size of HUD (lobby options) text, game default is 2, Essentials default is 1.4F.
        /// </summary>
        public static float HudTextFontSize { get; set; } = 1.4F;
#endif

        /// <summary>
        /// Enables or disables the HUD (lobby options) text scroller.
        /// </summary>
        public static bool HudTextScroller { get; set; } = true;

        /// <summary>
        /// Clear the game's default options list before listing custom options in the lobby HUD.
        /// </summary>
        public static bool ClearDefaultHudText { get; set; } = false;

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
        /// Used transmit the value of the option between players.
        /// </summary>
        /// <remarks>
        /// Combines <see cref="PluginID">PluginID</see> and <see cref="ConfigID">ConfigID</see> with an underscore between.
        /// </remarks>
        public readonly string ID;
        /// <summary>
        /// The name/title of the option.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Specifies whether this option saves it's value to be reloaded when the game is launched again (only applies for the lobby host).
        /// </summary>
        protected readonly bool SaveValue;
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
        protected virtual object OldValue { get; set; }
        /// <summary>
        /// The current value of the option.
        /// </summary>
        protected virtual object Value { get; set; }

        protected readonly byte[] SHA1;

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
        /// The game object that represents the custom option in the lobby options menu.
        /// </summary>
        public virtual OptionBehaviour GameObject { get; protected set; }

        public static Func<CustomOption, string, string> DefaultNameStringFormat = (_, name) => name;
        /// <summary>
        /// The string format reflecting the option name, result returned by <see cref="GetFormattedName"/>.
        /// <para>Arguments: the sending custom option, option name.</para>
        /// </summary>
        public virtual Func<CustomOption, string, string> NameStringFormat { get; set; } = DefaultNameStringFormat;

        public static Func<CustomOption, object, string> DefaultValueStringFormat = (_, value) => value.ToString();
        /// <summary>
        /// The string format reflecting the value, result returned by <see cref="GetFormattedValue"/>.
        /// <para>Arguments: the sending custom option, current value.</para>
        /// </summary>
        public virtual Func<CustomOption, object, string> ValueStringFormat { get; set; } = DefaultValueStringFormat;

#if S20201209 || S20210305 || S202103313
        public static Func<CustomOption, string, string, string> DefaultHudStringFormat = (_, name, value) => $"{name}: {value}[]";
#else
        public static Func<CustomOption, string, string, string> DefaultHudStringFormat = (_, name, value) => $"{name}: {value}";
#endif
        /// <summary>
        /// The string format reflecting the option name and value, result returned by <see cref="ToString"/>.
        /// Used when displaying the option in the lobby HUD (option list).
        /// <para>Arguments: the sending custom option, formatted name, formatted value.</para>
        /// </summary>
        public virtual Func<CustomOption, string, string, string> HudStringFormat { get; set; } = DefaultHudStringFormat;

        /// <summary>
        /// Affects whether the custom option will be visible in the lobby options menu.
        /// </summary>
        public virtual bool MenuVisible { get; set; } = true;
        /// <summary>
        /// Affects whether the custom option will appear in the HUD (option list) in the lobby.
        /// </summary>
        public virtual bool HudVisible { get; set; } = true;

        /// <summary>
        /// Whether the custom option and it's value changes will be sent through RPC.
        /// </summary>
        public virtual bool SendRpc { get { return true; } }

        /// <param name="id">The ID of the option, used to maintain the last value when <paramref name="saveValue"/> is true and to transmit the value between players</param>
        /// <param name="name">The name/title of the option</param>
        /// <param name="saveValue">Saves the last value of the option to apply again when the game is reopened (only applies for the lobby host)</param>
        /// <param name="type">The option type. See <see cref="CustomOptionType"/>.</param>
        /// <param name="value">The initial/default value</param>
        protected CustomOption(string id, string name, bool saveValue, CustomOptionType type, object value)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id), "Option id cannot be null or empty.");

            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Option name cannot be null or empty.");

            if (value == null) throw new ArgumentNullException(nameof(value), "Value cannot be null");

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

            SHA1 = SHA1Helper.Create(ID);

            Options.Add(this);
        }

        /// <summary>
        /// Returns event args of type <see cref="OptionOnValueChangedEventArgs"/> or a derivative.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="oldValue">The current value</param>
        protected virtual OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new OptionOnValueChangedEventArgs(value, Value);
        }

        /// <summary>
        /// Returns event args of type <see cref="OptionValueChangedEventArgs"/> or a derivative.
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="oldValue">The current value</param>
        protected virtual OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new OptionValueChangedEventArgs(value, Value);
        }

        private bool OnGameObjectCreated(OptionBehaviour o)
        {
            if (o == null) return false;

            try
            {
                o.OnValueChanged = new Action<OptionBehaviour>((_) => { });

                o.name = o.gameObject.name = ID;

                GameObject = o;

#if S20201209 || S20210305 || S202103313
                TextRenderer title = null;
#else
                TextMeshPro title = null;
#endif

                if (GameObject is ToggleOption toggle) title = toggle.TitleText;
                else if (GameObject is NumberOption number) title = number.TitleText;
                else if (GameObject is StringOption str) title = str.TitleText;
                else if (GameObject is KeyValueOption kv) title = kv.TitleText;

#if S20201209 || S20210305 || S202103313
                if (title != null) title.Text = GetFormattedName();
#else
                if (title != null) title.text = GetFormattedName();
#endif

                if (!GameObjectCreated(o))
                {
                    GameObject = null;

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                EssentialsPlugin.Logger.LogWarning($"Exception in {nameof(OnGameObjectCreated)} for option \"{Name}\" ({Type}): {e}");
            }

            return false;
        }

        /// <summary>
        /// Called when the game object is (re)created for this option.
        /// <para>Depends on <see cref="UpdateGameObject"/> by default.</para>
        /// </summary>
        /// <param name="o">The game object that was created for this option</param>
        /// <returns>Whether the object is valid, false to destroy.</returns>
        protected virtual bool GameObjectCreated(OptionBehaviour o)
        {
            //EssentialsPlugin.Logger.LogWarning($"Base handler for {nameof(CustomOption)}.{nameof(GameObjectCreated)} was called, game object for option \"{Name}\" from plugin \"{PluginID}\" will be destroyed.");

            //return false; // Destroy unhandled objects

            return UpdateGameObject();
        }

        /// <summary>
        /// Called when the <see cref="GameObject"/>'s components need to be updated to reflect the value visually.
        /// </summary>
        /// <returns>Update success.</returns>
        protected virtual bool UpdateGameObject()
        {
            try
            {
                if (GameObject is ToggleOption toggle)
                {
                    if (Value is not bool newValue) return false;

                    toggle.oldValue = newValue;
                    if (toggle.CheckMark != null) toggle.CheckMark.enabled = newValue;

                    return true;
                }
                else if (GameObject is NumberOption number)
                {
#if S20210305
                    if (Value is float newValue) number.Value = number.Field_3 = newValue;
#else
                    if (Value is float newValue) number.Value = number.oldValue = newValue;
#endif
                    //else if (Value is bool newBoolValue) number.Value = number.oldValue = newBoolValue ? 1 : 0;

#if S20201209 || S20210305 || S202103313
                    if (number.ValueText != null) number.ValueText.Text = GetFormattedValue();
#else
                    if (number.ValueText != null) number.ValueText.text = GetFormattedValue();
#endif

                    return true;
                }
                else if (GameObject is StringOption str)
                {
                    if (Value is int newValue) str.Value = str.oldValue = newValue;
                    else if (Value is bool newBoolValue) str.Value = str.oldValue = newBoolValue ? 1 : 0;

#if S20201209 || S20210305 || S202103313
                    if (str.ValueText != null) str.ValueText.Text = GetFormattedValue();
#else
                    if (str.ValueText != null) str.ValueText.text = GetFormattedValue();
#endif

                    return true;
                }
                else if (GameObject is KeyValueOption kv)
                {
                    if (Value is int newValue) kv.Selected = kv.oldValue = newValue;
                    else if (Value is bool newBoolValue) kv.Selected = kv.oldValue = newBoolValue ? 1 : 0;

#if S20201209 || S20210305 || S202103313
                    if (kv.ValueText != null) kv.ValueText.Text = GetFormattedValue();
#else
                    if (kv.ValueText != null) kv.ValueText.text = GetFormattedValue();
#endif

                    return true;
                }
            }
            catch (Exception e)
            {
                EssentialsPlugin.Logger.LogWarning($"Failed to update game setting value for option \"{Name}\": {e}");
            }

            return false;
        }

        /// <summary>
        /// Raises <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="nonDefault">Only raise the event when the current value isn't default</param>
        public void RaiseValueChanged(bool nonDefault = true)
        {
            if (!nonDefault || Value != DefaultValue) ValueChanged?.Invoke(this, ValueChangedEventArgs(Value, DefaultValue));
        }

        /// <summary>
        /// Restores the option to it's default value.
        /// </summary>
        public void SetToDefault(bool raiseEvents = true)
        {
            SetValue(DefaultValue, raiseEvents);
        }

        /// <summary>
        /// Sets the option's value.
        /// </summary>
        /// <remarks>
        /// Does nothing when the value type differs or when the value matches the current value.
        /// </remarks>
        /// <param name="value">The new value</param>
        /// <param name="raiseEvents">Whether or not to raise events</param>
        protected virtual void SetValue(object value, bool raiseEvents)
        {
            if (value?.GetType() != Value?.GetType() || Value == value) return; // Refuse value updates that don't match the option type.

            if (raiseEvents && OnValueChanged != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
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

            //if (SendRpc && GameSetting != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) Rpc.Send(new (string, CustomOptionType, object)[] { this });
            if (SendRpc && GameObject != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) Rpc.Instance.Send(this);

            UpdateGameObject();

            if (raiseEvents) ValueChanged?.SafeInvoke(this, ValueChangedEventArgs(value, Value), nameof(ValueChanged));

            if (GameObject == null) return; // Game object does not exist, menu is closed

            // Refresh the value of all options in the menu, in case an option affects another.
            try
            {
                GameOptionsMenu optionsMenu = Object.FindObjectOfType<GameOptionsMenu>();

                if (optionsMenu == null) return;

                for (int i = 0; i < optionsMenu.Children.Length; i++)
                {
                    OptionBehaviour optionBehaviour = optionsMenu.Children[i];
                    optionBehaviour.enabled = false;
                    optionBehaviour.enabled = true;
                }
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

        /// <summary>
        /// Gets the default option value casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <returns>The casted default value.</returns>
        public T GetDefaultValue<T>()
        {
            return (T)DefaultValue;
        }

        /// <summary>
        /// Gets the old option value casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to cast the value to</typeparam>
        /// <returns>The casted old value.</returns>
        public T GetOldValue<T>()
        {
            return (T)OldValue;
        }

        /// <returns><see cref="Name"/> passed through <see cref="NameStringFormat"/>.</returns>
        public string GetFormattedName()
        {
            return (NameStringFormat ?? DefaultNameStringFormat).Invoke(this, Name);
        }

        /// <returns><see cref="Value"/> passed through <see cref="ValueStringFormat"/>.</returns>
        public string GetFormattedValue()
        {
            return (ValueStringFormat ?? DefaultValueStringFormat).Invoke(this, Value);
        }

        /// <returns><see cref="object.ToString()"/> or the return value of <see cref="ValueStringFormat"/> when provided.</returns>
        public override string ToString()
        {
            return (HudStringFormat ?? DefaultHudStringFormat).Invoke(this, GetFormattedName(), GetFormattedValue());
        }
    }
}