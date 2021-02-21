using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Essentials.Options
{
    public class OptionOnValueChangedEventArgs : CancelEventArgs
    {
        public object Value { get; set; }
        public object OldValue { get; private set; }

        public OptionOnValueChangedEventArgs(object value, object oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }
    }

    public class ToggleOptionOnValueChangedEventArgs : OptionOnValueChangedEventArgs
    {
        public new bool Value { get { return (bool)base.Value; } set { Value = value; } }
        public new bool OldValue { get { return (bool)base.OldValue; } }

        public ToggleOptionOnValueChangedEventArgs(object value, object oldValue) : base(value, oldValue)
        {
        }
    }

    public class NumberOptionOnValueChangedEventArgs : OptionOnValueChangedEventArgs
    {
        public new float Value { get { return (float)base.Value; } set { Value = value; } }
        public new float OldValue { get { return (float)base.OldValue; } }

        public NumberOptionOnValueChangedEventArgs(object value, object oldValue) : base(value, oldValue)
        {
        }
    }

    public class StringOptionOnValueChangedEventArgs : OptionOnValueChangedEventArgs
    {
        public new int Value { get { return (int)base.Value; } set { Value = value; } }
        public new int OldValue { get { return (int)base.OldValue; } }

        public StringOptionOnValueChangedEventArgs(object value, object oldValue) : base(value, oldValue)
        {
        }
    }

    public class OptionValueChangedEventArgs : EventArgs
    {
        public readonly object OldValue;
        public readonly object Value;

        public OptionValueChangedEventArgs(object value, object oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }
    }

    public class ToggleOptionValueChangedEventArgs : OptionValueChangedEventArgs
    {
        public new bool OldValue { get { return (bool)base.OldValue; } }
        public new bool Value { get { return (bool)base.Value; } }

        public ToggleOptionValueChangedEventArgs(object value, object oldValue) : base(value, oldValue)
        {
        }
    }

    public class NumberOptionValueChangedEventArgs : OptionValueChangedEventArgs
    {
        public new float OldValue { get { return (float)base.OldValue; } }
        public new float Value { get { return (float)base.Value; } }

        public NumberOptionValueChangedEventArgs(object value, object oldValue) : base(value, oldValue)
        {
        }
    }

    public class StringOptionValueChangedEventArgs : OptionValueChangedEventArgs
    {
        public new int OldValue { get { return (int)base.OldValue; } }
        public new int Value { get { return (int)base.Value; } }

        public StringOptionValueChangedEventArgs(object value, object oldValue) : base(value, oldValue)
        {
        }
    }
}
